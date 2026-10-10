import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from 'primereact/button';
import { Column } from 'primereact/column';
import { DataTable } from 'primereact/datatable';
import { IconField } from 'primereact/iconfield';
import { InputIcon } from 'primereact/inputicon';
import { InputText } from 'primereact/inputtext';
import { Tag } from 'primereact/tag';
import { ApiError } from '../api/ApiError';
import { getUsers } from '../api/users';
import { EmptyState } from '../components/EmptyState';
import { ErrorMessage } from '../components/ErrorMessage';
import { Spinner } from '../components/Spinner';
import type { User } from '../types/user';

type Status = 'loading' | 'ready' | 'error';

const PAGE_SIZE = 10;

export const UserListPage = () => {
  const navigate = useNavigate();

  const [users, setUsers] = useState<User[]>([]);
  const [status, setStatus] = useState<Status>('loading');
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [reloadToken, setReloadToken] = useState(0);

  const reload = useCallback(() => setReloadToken((token) => token + 1), []);

  useEffect(() => {
    const controller = new AbortController();
    let active = true;

    setStatus('loading');
    setError('');

    getUsers(controller.signal)
      .then((result) => {
        if (!active) {
          return;
        }
        setUsers(result);
        setStatus('ready');
      })
      .catch((caught: unknown) => {
        if (!active || controller.signal.aborted) {
          return;
        }
        setError(caught instanceof ApiError ? caught.message : 'Unable to load users.');
        setStatus('error');
      });

    return () => {
      active = false;
      controller.abort();
    };
  }, [reloadToken]);

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase();

    if (!term) {
      return users;
    }

    return users.filter((user) =>
      [user.name, user.city, user.state, user.pincode].some((value) =>
        value.toLowerCase().includes(term),
      ),
    );
  }, [users, search]);

  return (
    <section>
      <div className="page-header">
        <div>
          <h1>Users</h1>
          <p className="muted">Everyone currently stored in the directory.</p>
        </div>
        <div className="page-header__actions">
          <Button
            type="button"
            label="Refresh"
            icon="pi pi-refresh"
            outlined
            onClick={reload}
            disabled={status === 'loading'}
          />
          <Button
            type="button"
            label="Add user"
            icon="pi pi-plus"
            onClick={() => navigate('/add')}
          />
        </div>
      </div>

      {status === 'loading' && <Spinner label="Loading users" />}

      {status === 'error' && <ErrorMessage message={error} onRetry={reload} />}

      {status === 'ready' && users.length === 0 && <EmptyState />}

      {status === 'ready' && users.length > 0 && (
        <div className="surface-card">
          <div className="table-toolbar">
            <IconField iconPosition="left">
              <InputIcon className="pi pi-search" />
              <InputText
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Search name, city, state or pincode"
                aria-label="Search users"
              />
            </IconField>
            <Tag
              value={`${filtered.length} of ${users.length}`}
              severity="info"
              rounded
            />
          </div>

          <DataTable
            value={filtered}
            dataKey="id"
            stripedRows
            removableSort
            paginator={filtered.length > PAGE_SIZE}
            rows={PAGE_SIZE}
            emptyMessage="No users match your search."
            aria-label="List of users in the directory"
          >
            <Column field="name" header="Name" sortable body={(user: User) => (
              <div className="cell-name">
                <span className="cell-name__avatar" aria-hidden="true">
                  {user.name.charAt(0).toUpperCase()}
                </span>
                {user.name}
              </div>
            )} />
            <Column field="age" header="Age" sortable style={{ width: '6rem' }} />
            <Column field="city" header="City" sortable />
            <Column field="state" header="State" sortable />
            <Column
              field="pincode"
              header="Pincode"
              sortable
              body={(user: User) => <span className="cell-pincode">{user.pincode}</span>}
            />
          </DataTable>
        </div>
      )}
    </section>
  );
};
