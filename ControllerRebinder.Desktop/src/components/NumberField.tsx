interface NumberFieldProps {
  label: string;
  value: number;
  onChange: (value: number) => void;
  min?: number;
  max?: number;
  step?: number;
  description?: string;
  disabled?: boolean;
  suffix?: string;
}

const NumberField = ({
  label,
  value,
  onChange,
  min,
  max,
  step,
  description,
  disabled,
  suffix
}: NumberFieldProps) => {
  return (
    <label className={`field field--input${disabled ? ' field--disabled' : ''}`}>
      <span className="field__label">{label}</span>
      <div className="field__control">
        <input
          type="number"
          value={Number.isFinite(value) ? value : ''}
          min={min}
          max={max}
          step={step ?? 1}
          onChange={(event) => {
            const next = event.target.value;
            const numeric = next === '' ? Number.NaN : Number(next);
            onChange(Number.isNaN(numeric) ? 0 : numeric);
          }}
          disabled={disabled}
        />
        {suffix ? <span className="field__suffix">{suffix}</span> : null}
      </div>
      {description ? <span className="field__description">{description}</span> : null}
    </label>
  );
};

export default NumberField;
