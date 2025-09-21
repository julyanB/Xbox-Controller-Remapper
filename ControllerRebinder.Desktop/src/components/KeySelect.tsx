import { VIRTUAL_KEY_GROUPS } from '../data/virtualKeyOptions';

interface KeySelectProps {
  label: string;
  value: string | null | undefined;
  onChange: (value: string | null) => void;
  description?: string;
  disabled?: boolean;
  allowEmpty?: boolean;
}

const KeySelect = ({ label, value, onChange, description, disabled, allowEmpty = true }: KeySelectProps) => {
  return (
    <label className={`field field--select${disabled ? ' field--disabled' : ''}`}>
      <span className="field__label">{label}</span>
      <select
        value={value ?? ''}
        onChange={(event) => {
          const next = event.target.value;
          onChange(next === '' ? null : next);
        }}
        disabled={disabled}
      >
        {allowEmpty ? <option value="">Unassigned</option> : null}
        {VIRTUAL_KEY_GROUPS.map((group) => (
          <optgroup key={group.label} label={group.label}>
            {group.options.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label} ({option.value})
              </option>
            ))}
          </optgroup>
        ))}
      </select>
      {description ? <span className="field__description">{description}</span> : null}
    </label>
  );
};

export default KeySelect;
