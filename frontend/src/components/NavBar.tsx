import { NavLink } from 'react-router-dom';
import { Menubar } from 'primereact/menubar';
import type { MenuItem } from 'primereact/menuitem';

const navLink = (label: string, to: string, icon: string, end = false): MenuItem => ({
  label,
  template: () => (
    <NavLink
      to={to}
      end={end}
      className={({ isActive }) =>
        isActive ? 'nav__link nav__link--active' : 'nav__link'
      }
    >
      <i className={icon} aria-hidden="true" />
      <span>{label}</span>
    </NavLink>
  ),
});

const items: MenuItem[] = [
  navLink('List', '/', 'pi pi-list', true),
  navLink('Add', '/add', 'pi pi-user-plus'),
];

const brand = (
  <span className="nav__brand">
    <i className="pi pi-id-card" aria-hidden="true" />
    User Directory
  </span>
);

export const NavBar = () => (
  <nav className="nav" aria-label="Main">
    <Menubar model={items} start={brand} />
  </nav>
);
