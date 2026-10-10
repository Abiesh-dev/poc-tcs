import { Button } from 'primereact/button';
import { Message } from 'primereact/message';

interface ErrorMessageProps {
  message: string;
  onRetry?: () => void;
}

export const ErrorMessage = ({ message, onRetry }: ErrorMessageProps) => (
  <div className="form-error">
    <Message severity="error" text={message} />
    {onRetry && (
      <Button
        type="button"
        label="Try again"
        icon="pi pi-refresh"
        severity="danger"
        outlined
        onClick={onRetry}
      />
    )}
  </div>
);
