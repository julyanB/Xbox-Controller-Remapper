import { useEffect, useRef } from 'react';
import type { LogEntry } from '../desktop-api';

interface LogConsoleProps {
  entries: LogEntry[];
  onClear: () => void;
  className?: string;
}

const LogConsole = ({ entries, onClear, className }: LogConsoleProps) => {
  const containerRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const element = containerRef.current;
    if (!element) {
      return;
    }

    element.scrollTop = element.scrollHeight;
  }, [entries]);

  return (
    <section className={`panel log-panel ${className ?? ''}`}>
      <header className="panel__header">
        <div>
          <h2>Live Telemetry</h2>
          <p className="panel__subtitle">Streaming output from the controller remapper service.</p>
        </div>
        <button className="ghost-button" onClick={onClear} type="button">
          Clear
        </button>
      </header>
      <div ref={containerRef} className="log-panel__body">
        {entries.length === 0 ? (
          <p className="log-panel__empty">No log messages yet.</p>
        ) : (
          entries.map((entry, index) => (
            <article key={`${entry.timestamp}-${index}`} className={`log-entry log-entry--${entry.level}`}>
              <time>{new Date(entry.timestamp).toLocaleTimeString()}</time>
              <p>{entry.message}</p>
            </article>
          ))
        )}
      </div>
    </section>
  );
};

export default LogConsole;
