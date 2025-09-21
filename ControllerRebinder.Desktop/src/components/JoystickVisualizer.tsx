import { useMemo } from 'react';
import type { JoystickKeyBindings } from '../desktop-api';
import { VIRTUAL_KEY_GROUPS } from '../data/virtualKeyOptions';
import './JoystickVisualizer.css';

const AREA_MULTIPLIER = 10_000_000;
const HALF_PI = Math.PI / 2;
const EPSILON = 0.0001;
const SVG_SIZE = 260;
const CENTER = SVG_SIZE / 2;
const OUTER_RADIUS = 110;

const KEY_LABEL_LOOKUP = new Map<string, string>();
for (const group of VIRTUAL_KEY_GROUPS) {
  for (const option of group.options) {
    KEY_LABEL_LOOKUP.set(option.value, option.label);
  }
}

type SegmentType = 'horizontal' | 'diagonal' | 'vertical';

type RawSegment = {
  start: number;
  end: number;
  type: SegmentType;
  label: string;
  quadrant: string;
};

type Segment = RawSegment & {
  path: string;
  centerAngle: number;
};

export interface JoystickVisualizerProps {
  title: string;
  deadZone: number;
  threshold: number;
  maxValue: number;
  forwardDown: number;
  leftRight: number;
  keys: JoystickKeyBindings;
}

const clamp = (value: number, min: number, max: number) => {
  if (Number.isNaN(value)) {
    return min;
  }

  return Math.min(Math.max(value, min), max);
};

const resolveKeyLabel = (value?: string | null) => {
  if (!value) {
    return 'Unassigned';
  }

  return KEY_LABEL_LOOKUP.get(value) ?? value;
};

const polarPoint = (angle: number, radius: number) => ({
  x: CENTER + radius * Math.cos(angle),
  y: CENTER - radius * Math.sin(angle)
});

const createSegmentPath = (start: number, end: number) => {
  const radius = OUTER_RADIUS;
  const startPoint = polarPoint(start, radius);
  const endPoint = polarPoint(end, radius);
  const largeArc = end - start > Math.PI ? 1 : 0;

  return [
    `M ${CENTER.toFixed(1)} ${CENTER.toFixed(1)}`,
    `L ${startPoint.x.toFixed(1)} ${startPoint.y.toFixed(1)}`,
    `A ${radius.toFixed(1)} ${radius.toFixed(1)} 0 ${largeArc} 0 ${endPoint.x.toFixed(1)} ${endPoint.y.toFixed(1)}`,
    'Z'
  ].join(' ');
};

const formatAngle = (radians: number) => `${((radians * 180) / Math.PI).toFixed(1)}°`;

const JoystickVisualizer = ({
  title,
  deadZone,
  threshold,
  maxValue,
  forwardDown,
  leftRight,
  keys
}: JoystickVisualizerProps) => {
  const safeMax = maxValue > 0 ? maxValue : 1;
  const safeDeadZone = clamp(deadZone, 0, safeMax);
  const safeThreshold = clamp(threshold, safeDeadZone, safeMax);

  const deadZoneRadius = (safeDeadZone / safeMax) * OUTER_RADIUS;
  const thresholdRadius = (safeThreshold / safeMax) * OUTER_RADIUS;

  const { segments, hasDiagonal, horizontalLimit, verticalLimit } = useMemo(() => {
    const denominator = threshold > 0 ? threshold * threshold : 0;
    let horizontalLimit = denominator > 0 ? (leftRight * AREA_MULTIPLIER * 2) / denominator : 0;
    let verticalLimit = denominator > 0 ? (forwardDown * AREA_MULTIPLIER * 2) / denominator : 0;

    if (!Number.isFinite(horizontalLimit)) {
      horizontalLimit = 0;
    }

    if (!Number.isFinite(verticalLimit)) {
      verticalLimit = 0;
    }

    horizontalLimit = clamp(horizontalLimit, 0, HALF_PI);
    verticalLimit = clamp(verticalLimit, 0, HALF_PI);

    const rawSegments: RawSegment[] = [];
    let diagonalFound = false;

    const quadrants = [
      { id: 'top-right', base: 0, horizontalLabel: 'Right', verticalLabel: 'Up' },
      { id: 'top-left', base: HALF_PI, horizontalLabel: 'Left', verticalLabel: 'Up' },
      { id: 'bottom-left', base: Math.PI, horizontalLabel: 'Left', verticalLabel: 'Down' },
      { id: 'bottom-right', base: 3 * HALF_PI, horizontalLabel: 'Right', verticalLabel: 'Down' }
    ];

    for (const quadrant of quadrants) {
      const quadrantEnd = quadrant.base + HALF_PI;

      if (horizontalLimit < verticalLimit) {
        if (horizontalLimit > EPSILON) {
          rawSegments.push({
            start: quadrant.base,
            end: quadrant.base + horizontalLimit,
            type: 'horizontal',
            label: quadrant.horizontalLabel,
            quadrant: quadrant.id
          });
        }

        if (verticalLimit - horizontalLimit > EPSILON) {
          rawSegments.push({
            start: quadrant.base + horizontalLimit,
            end: quadrant.base + verticalLimit,
            type: 'diagonal',
            label: `${quadrant.verticalLabel} + ${quadrant.horizontalLabel}`,
            quadrant: quadrant.id
          });
          diagonalFound = true;
        }

        if (quadrantEnd - verticalLimit > EPSILON) {
          rawSegments.push({
            start: quadrant.base + verticalLimit,
            end: quadrantEnd,
            type: 'vertical',
            label: quadrant.verticalLabel,
            quadrant: quadrant.id
          });
        }
      } else {
        const horizontalEnd = Math.min(horizontalLimit, verticalLimit);

        if (horizontalEnd > EPSILON) {
          rawSegments.push({
            start: quadrant.base,
            end: quadrant.base + horizontalEnd,
            type: 'horizontal',
            label: quadrant.horizontalLabel,
            quadrant: quadrant.id
          });
        }

        if (quadrantEnd - verticalLimit > EPSILON) {
          rawSegments.push({
            start: quadrant.base + verticalLimit,
            end: quadrantEnd,
            type: 'vertical',
            label: quadrant.verticalLabel,
            quadrant: quadrant.id
          });
        }
      }
    }

    const segments: Segment[] = rawSegments.map((segment) => ({
      ...segment,
      path: createSegmentPath(segment.start, segment.end),
      centerAngle: (segment.start + segment.end) / 2
    }));

    return { segments, hasDiagonal: diagonalFound, horizontalLimit, verticalLimit };
  }, [forwardDown, leftRight, threshold]);

  const mixSpanRadians = horizontalLimit < verticalLimit ? verticalLimit - horizontalLimit : 0;
  const horizontalAngleText = formatAngle(horizontalLimit);
  const verticalAngleText = formatAngle(verticalLimit);
  const mixAngleText = mixSpanRadians > 0 ? formatAngle(mixSpanRadians) : '0.0°';

  const deadZonePercent = ((safeDeadZone / safeMax) * 100).toFixed(1);
  const thresholdPercent = ((safeThreshold / safeMax) * 100).toFixed(1);

  const bindings = useMemo(
    () => [
      { label: 'Up', value: resolveKeyLabel(keys.Up) },
      { label: 'Right', value: resolveKeyLabel(keys.Right) },
      { label: 'Down', value: resolveKeyLabel(keys.Down) },
      { label: 'Left', value: resolveKeyLabel(keys.Left) }
    ],
    [keys.Down, keys.Left, keys.Right, keys.Up]
  );

  return (
    <div className="joystick-visualizer" aria-label={`${title} visualisation`}>
      <div className="joystick-visualizer__heading">
        <span className="joystick-visualizer__title">{title} response</span>
        <span className="joystick-visualizer__subtitle">Live preview updates as you tweak values.</span>
      </div>
      <div className="joystick-visualizer__stage">
        <svg
          className="joystick-visualizer__canvas"
          viewBox={`0 0 ${SVG_SIZE} ${SVG_SIZE}`}
          role="img"
          aria-label={`${title} dead zone and directional preview`}
        >
          <title>{`${title} dead zone and directional preview`}</title>
          {segments.map((segment) => (
            <path
              key={`${segment.quadrant}-${segment.type}`}
              d={segment.path}
              className={`joystick-visualizer__segment joystick-visualizer__segment--${segment.type}`}
            >
              <title>{`${segment.label} priority`}</title>
            </path>
          ))}
          <circle
            className="joystick-visualizer__ring joystick-visualizer__ring--max"
            cx={CENTER}
            cy={CENTER}
            r={OUTER_RADIUS}
          />
          <circle
            className="joystick-visualizer__ring joystick-visualizer__ring--threshold"
            cx={CENTER}
            cy={CENTER}
            r={Math.max(thresholdRadius, 0)}
          />
          <circle
            className="joystick-visualizer__deadzone"
            cx={CENTER}
            cy={CENTER}
            r={Math.max(deadZoneRadius, 0)}
          />
        </svg>
      </div>
      <div className="joystick-visualizer__meta">
        <dl className="joystick-visualizer__stats">
          <div className="joystick-visualizer__stat">
            <dt>Dead zone radius</dt>
            <dd>
              {safeDeadZone.toLocaleString()} (<span>{deadZonePercent}%</span> of max)
            </dd>
          </div>
          <div className="joystick-visualizer__stat">
            <dt>Actuation threshold</dt>
            <dd>
              {safeThreshold.toLocaleString()} (<span>{thresholdPercent}%</span> of max)
            </dd>
          </div>
          <div className="joystick-visualizer__stat">
            <dt>Horizontal priority</dt>
            <dd>Until ≈ {horizontalAngleText} from axis</dd>
          </div>
          <div className="joystick-visualizer__stat">
            <dt>Vertical priority</dt>
            <dd>From ≈ {verticalAngleText} toward pole</dd>
          </div>
          <div className="joystick-visualizer__stat">
            <dt>Blend window</dt>
            <dd>{hasDiagonal ? `≈ ${mixAngleText} shared arc` : 'No shared zone'}</dd>
          </div>
        </dl>
        <div className="joystick-visualizer__legend">
          <span className="joystick-visualizer__legend-item">
            <span className="joystick-visualizer__chip joystick-visualizer__chip--horizontal" /> Horizontal
          </span>
          <span className="joystick-visualizer__legend-item">
            <span className="joystick-visualizer__chip joystick-visualizer__chip--diagonal" /> Horizontal + Vertical
          </span>
          <span className="joystick-visualizer__legend-item">
            <span className="joystick-visualizer__chip joystick-visualizer__chip--vertical" /> Vertical
          </span>
        </div>
      </div>
      <ul className="joystick-visualizer__bindings">
        {bindings.map((binding) => (
          <li key={binding.label}>
            <span>{binding.label}</span>
            <span>{binding.value}</span>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default JoystickVisualizer;
