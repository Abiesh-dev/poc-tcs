import { ProgressSpinner } from 'primereact/progressspinner';

export const Spinner = ({ label = 'Loading' }: { label?: string }) => (
  <div className="spinner" role="status" aria-live="polite">
    <ProgressSpinner
      style={{ width: '38px', height: '38px' }}
      strokeWidth="4"
      animationDuration="0.8s"
    />
    <span>{label}…</span>
  </div>
);
