import { NavLink } from 'react-router-dom'
import { ReactNode } from 'react'
import {
  LayoutDashboard,
  ShoppingCart,
  Truck,
  Package,
  Boxes,
  Users,
  User,
  FileText,
  Settings,
  Bell,
  CircleUser,
  Filter,
  ShieldCheck
} from 'lucide-react'

interface LayoutProps {
  children: ReactNode
  onLogout: () => void
  username: string
  role: string
}

function Layout({ children, onLogout, username, role }: LayoutProps) {
  const hasAnyRole = (...roles: string[]) => roles.includes(role)
  const initials = (username || 'U').slice(0, 2).toUpperCase()

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">NEX</div>
          <div className="brand-sub">ERP System v2.4</div>
        </div>
        <nav className="nav">
          <NavLink to="/" end>
            <span className="nav-icon"><LayoutDashboard size={18} /></span>
            Dashboard
          </NavLink>
          {hasAnyRole('Owner', 'Admin', 'Sales') && (
            <NavLink to="/sales-orders">
              <span className="nav-icon"><ShoppingCart size={18} /></span>
              Sales
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin', 'Purchasing') && (
            <NavLink to="/purchase-orders">
              <span className="nav-icon"><Truck size={18} /></span>
              Purchasing
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin', 'Inventory') && (
            <NavLink to="/inventory">
              <span className="nav-icon"><Package size={18} /></span>
              Inventory
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin', 'Sales', 'Purchasing', 'Inventory') && (
            <NavLink to="/products">
              <span className="nav-icon"><Boxes size={18} /></span>
              Products
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin', 'Sales') && (
            <NavLink to="/customers">
              <span className="nav-icon"><Users size={18} /></span>
              Customers
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin', 'Purchasing') && (
            <NavLink to="/suppliers">
              <span className="nav-icon"><User size={18} /></span>
              Suppliers
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin', 'Accounting') && (
            <NavLink to="/invoices">
              <span className="nav-icon"><FileText size={18} /></span>
              Invoices
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing') && (
            <NavLink to="/catalogs/taxes">
              <span className="nav-icon"><Settings size={18} /></span>
              Settings
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin') && (
            <NavLink to="/users">
              <span className="nav-icon"><ShieldCheck size={18} /></span>
              Users
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin') && (
            <NavLink to="/tenants">
              <span className="nav-icon"><Filter size={18} /></span>
              Tenants
            </NavLink>
          )}
          {hasAnyRole('Owner', 'Admin') && (
            <NavLink to="/signup">
              <span className="nav-icon"><CircleUser size={18} /></span>
              Signup
            </NavLink>
          )}
        </nav>
        <div className="sidebar-footer">
          <button onClick={onLogout} className="btn btn-ghost">
            Cerrar sesion
          </button>
        </div>
      </aside>

      <div className="content-shell">
        <header className="topbar">
          <div className="topbar-title">Enterprise Resource Planning</div>
          <div className="topbar-actions">
            <button className="icon-btn" type="button">
              <Bell size={18} />
            </button>
            <div className="user-pill">
              <span className="user-avatar">{initials}</span>
              {username} ({role})
            </div>
          </div>
        </header>
        <main className="main-content">{children}</main>
      </div>
    </div>
  )
}

export default Layout
