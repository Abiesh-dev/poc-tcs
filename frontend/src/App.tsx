import { Navigate, Route, Routes } from 'react-router-dom';
import { NavBar } from './components/NavBar';
import { AddUserPage } from './pages/AddUserPage';
import { UserListPage } from './pages/UserListPage';

export const App = () => (
  <>
    <NavBar />
    <main className="page">
      <Routes>
        <Route path="/" element={<UserListPage />} />
        <Route path="/add" element={<AddUserPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </main>
  </>
);
