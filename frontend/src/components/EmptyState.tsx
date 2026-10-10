import { useNavigate } from 'react-router-dom';
import { Button } from 'primereact/button';

export const EmptyState = () => {
  const navigate = useNavigate();


  return (
    <div className="empty-state">
      <i className="pi pi-users empty-state__icon" aria-hidden="true" />
      <h2>No users yet</h2>
      <p className="muted">The directory is empty. Add the first person to get started.</p>
      <Button label="Add a user" icon="pi pi-plus" onClick={() => navigate('/add')} />
    </div>
  );
};
