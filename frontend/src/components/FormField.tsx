import { InputText, type InputTextProps } from 'primereact/inputtext';
import { classNames } from 'primereact/utils';

interface FormFieldProps extends Omit<InputTextProps, 'id' | 'className'> {
  id: string;
  label: string;
  error?: string;
  hint?: string;
}

export const FormField = ({ id, label, error, hint, ...inputProps }: FormFieldProps) => {
  const errorId = `${id}-error`;
  const hintId = `${id}-hint`;
  const describedBy = [error ? errorId : undefined, hint ? hintId : undefined]
    .filter(Boolean)
    .join(' ');

  return (
    <div className="field">
      <label className="field__label" htmlFor={id}>
        {label}
      </label>
      <InputText
        {...inputProps}
        id={id}
        className={classNames('field__input', { 'p-invalid': Boolean(error) })}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy || undefined}
      />
      {hint && !error && (
        <small className="field__hint" id={hintId}>
          {hint}
        </small>
      )}
      {error && (
        <small className="field__error" id={errorId} role="alert">
          <i className="pi pi-exclamation-circle" aria-hidden="true" />
          {error}
        </small>
      )}
    </div>
  );
};
