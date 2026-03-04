import { NavLink, useLocation, useNavigate } from 'react-router-dom'
import { ReactNode, useMemo, useState } from 'react'
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
  ShieldCheck,
  Search,
  Plus
} from 'lucide-react'

interface LayoutProps {
  children: ReactNode
  onLogout: () => void
  username: string
  role: string
}

function Layout({ children, onLogout, username, role }: LayoutProps) {
  const location = useLocation()
  const navigate = useNavigate()
  const hasAnyRole = (...roles: string[]) => roles.includes(role)
  const initials = (username || 'U').slice(0, 2).toUpperCase()
  const [searchText, setSearchText] = useState('')
  const [showNotifications, setShowNotifications] = useState(false)
  const [showQuickOrderMenu, setShowQuickOrderMenu] = useState(false)
  const [notifications, setNotifications] = useState([
    { id: 1, text: '3 facturas de compra pendientes de pago', read: false, route: '/purchase-invoices' },
    { id: 2, text: '2 órdenes de venta pendientes de facturar', read: false, route: '/sales-orders' },
    { id: 3, text: '5 productos con stock bajo', read: true, route: '/inventory' }
  ])

  const searchItems = useMemo(() => {
    const items: Array<{ label: string; route: string; roles: string[] }> = [
      { label: 'Dashboard', route: '/', roles: ['Owner', 'Admin', 'Sales', 'Purchasing', 'Inventory', 'Accounting'] },
      { label: 'Sales Orders', route: '/sales-orders', roles: ['Owner', 'Admin', 'Sales'] },
      { label: 'Purchase Orders', route: '/purchase-orders', roles: ['Owner', 'Admin', 'Purchasing'] },
      { label: 'Purchase Bills', route: '/purchase-invoices', roles: ['Owner', 'Admin', 'Purchasing'] },
      { label: 'Invoices', route: '/invoices', roles: ['Owner', 'Admin', 'Accounting'] },
      { label: 'Products', route: '/products', roles: ['Owner', 'Admin', 'Sales', 'Purchasing', 'Inventory'] },
      { label: 'Customers', route: '/customers', roles: ['Owner', 'Admin', 'Sales'] },
      { label: 'Suppliers', route: '/suppliers', roles: ['Owner', 'Admin', 'Purchasing'] },
      { label: 'Inventory', route: '/inventory', roles: ['Owner', 'Admin', 'Inventory'] },
      { label: 'Tax Catalog', route: '/catalogs/taxes', roles: ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing'] },
      { label: 'Users', route: '/users', roles: ['Owner', 'Admin'] },
      { label: 'Tenants', route: '/tenants', roles: ['Owner', 'Admin'] }
    ]

    return items.filter(i => i.roles.includes(role))
  }, [role])

  const filteredSearchItems = useMemo(() => {
    const q = searchText.trim().toLowerCase()
    if (!q) return []
    return searchItems.filter(i => i.label.toLowerCase().includes(q)).slice(0, 6)
  }, [searchItems, searchText])

  const unreadCount = notifications.filter(n => !n.read).length

  const currentPage = location.pathname === '/'
    ? 'Dashboard'
    : location.pathname
      .replace('/', '')
      .split('-')
      .map(w => w.charAt(0).toUpperCase() + w.slice(1))
      .join(' ')

  const goTo = (route: string) => {
    setSearchText('')
    setShowNotifications(false)
    setShowQuickOrderMenu(false)
    navigate(route)
  }

  const openNewSalesOrder = () => goTo('/sales-orders?create=1')
  const openNewPurchaseOrder = () => goTo('/purchase-orders?create=1')

  const openNotification = (id: number, route: string) => {
    setNotifications(prev => prev.map(n => (n.id === id ? { ...n, read: true } : n)))
    goTo(route)
  }

  const markAllRead = () => {
    setNotifications(prev => prev.map(n => ({ ...n, read: true })))
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-logo">
            <div className="brand-icon">NX</div>
            <div className="brand-content">
              <div className="brand-mark">NEX</div>
              <div className="brand-sub">Systems · ERP SaaS</div>
            </div>
          </div>
          <div className="tenant-badge">
            <span className="tenant-dot"></span>
            <span className="tenant-name">Tenant Activo</span>
          </div>
        </div>
        <nav className="nav">
          <div className="nav-label">Principal</div>
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
          {hasAnyRole('Owner', 'Admin', 'Purchasing') && (
            <NavLink to="/purchase-invoices">
              <span className="nav-icon"><FileText size={18} /></span>
              Purchase Bills
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
            <NavLink to="/subscription">
              <span className="nav-icon"><ShieldCheck size={18} /></span>
              Subscription
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
          <div className="topbar-breadcrumb">
            <span className="breadcrumb-root">MINIERP</span>
            <span className="breadcrumb-sep">/</span>
            <span className="breadcrumb-current">{currentPage}</span>
          </div>
          <div className="topbar-actions">
            <div className="search-bar">
              <Search size={14} className="search-icon" />
              <input
                className="search-input-top"
                placeholder="Buscar módulos y pantallas..."
                value={searchText}
                onChange={e => setSearchText(e.target.value)}
              />
            </div>
            {filteredSearchItems.length > 0 && (
              <div className="search-results-popover">
                {filteredSearchItems.map(item => (
                  <button key={item.route} className="search-result-btn" onClick={() => goTo(item.route)}>
                    {item.label}
                  </button>
                ))}
              </div>
            )}
            <button className="icon-btn" type="button" onClick={() => setShowNotifications(v => !v)}>
              <Bell size={18} />
              {unreadCount > 0 && <span className="notif-dot"></span>}
            </button>
            {showNotifications && (
              <div className="popover-panel notifications-panel">
                <div className="popover-head">
                  <span>Notificaciones</span>
                  <button className="popover-link" onClick={markAllRead}>Marcar todo leído</button>
                </div>
                {notifications.map(n => (
                  <button key={n.id} className={`notification-item ${n.read ? '' : 'unread'}`} onClick={() => openNotification(n.id, n.route)}>
                    {n.text}
                  </button>
                ))}
              </div>
            )}
            <button className="new-btn" type="button" onClick={() => setShowQuickOrderMenu(v => !v)}>
              <Plus size={14} />
              Nueva Orden
            </button>
            {showQuickOrderMenu && (
              <div className="popover-panel new-order-panel">
                <button className="popover-item" onClick={openNewSalesOrder}>Nueva orden de venta</button>
                <button className="popover-item" onClick={openNewPurchaseOrder}>Nueva orden de compra</button>
              </div>
            )}
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
