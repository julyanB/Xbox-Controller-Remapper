interface ToggleFieldProps {
  label: string;
  checked: boolean;
  onChange: (value: boolean) => void;
  description?: string;
  disabled?: boolean;
}

const ToggleField = ({ label, checked, onChange, description, disabled }: ToggleFieldProps) => {
  return (
    <div className="field field--toggle">
      <div className="field__label-row">
        <span className="field__label">{label}</span>
        <label className={`switch${disabled ? ' switch--disabled' : ''}`}>
          <input
            type="checkbox"
            checked={checked}
            onChange={(event) => onChange(event.target.checked)}
            disabled={disabled}
          />
          <span className="switch__slider" />
        </label>
      </div>
      {description ? <p className="field__description">{description}</p> : null}
    </div>
  );
};

export default ToggleField;
