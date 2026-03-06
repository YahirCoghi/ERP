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
  Plus,
  CreditCard,
  ChevronDown
} from 'lucide-react'

interface LayoutProps {
  children: ReactNode
  onLogout: () => void
  username: string
  role: string
  moduleAccess: Record<string, string>
}

function Layout({ children, onLogout, username, role, moduleAccess }: LayoutProps) {
  const location = useLocation()
  const navigate = useNavigate()
  const hasAnyRole = (...roles: string[]) => roles.includes(role)
  const initials = (username || 'U').slice(0, 2).toUpperCase()
  const [searchText, setSearchText] = useState('')
  const [showNotifications, setShowNotifications] = useState(false)
  const [showQuickOrderMenu, setShowQuickOrderMenu] = useState(false)
  const [openSections, setOpenSections] = useState({
    sales: true,
    purchasing: true,
    inventory: true,
    finance: true,
    admin: true
  })
  const [notifications, setNotifications] = useState([
    { id: 1, text: '3 facturas de compra pendientes de pago', read: false, route: '/purchase-invoices' },
    { id: 2, text: '2 órdenes de venta pendientes de facturar', read: false, route: '/sales-orders' },
    { id: 3, text: '5 productos con stock bajo', read: true, route: '/inventory' }
  ])

  const hasPlanAccess = (moduleName: string, required: 'Read' | 'Full' = 'Read') => {
    const current = String(moduleAccess[moduleName] || 'Full').toLowerCase()
    const currentRank = current === 'full' ? 2 : current === 'read' ? 1 : 0
    const requiredRank = required === 'Full' ? 2 : 1
    return currentRank >= requiredRank
  }

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
      { label: 'Exchange Rates', route: '/exchange-rates', roles: ['Owner', 'Admin', 'Accounting'] },
      { label: 'Approvals', route: '/approvals', roles: ['Owner', 'Admin'] },
      { label: 'Alerts', route: '/alerts', roles: ['Owner', 'Admin'] },
      { label: 'Finance Dashboard', route: '/finance-dashboard', roles: ['Owner', 'Admin', 'Accounting'] },
      { label: 'Recurring Templates', route: '/recurring-templates', roles: ['Owner', 'Admin', 'Accounting'] },
      { label: 'Fixed Assets', route: '/fixed-assets', roles: ['Owner', 'Admin', 'Accounting'] },
      { label: 'Intrastat', route: '/intrastat', roles: ['Owner', 'Admin', 'Accounting'] },
      { label: 'Campaigns', route: '/campaigns', roles: ['Owner', 'Admin', 'Sales'] },
      { label: 'Pick Pack', route: '/pick-pack', roles: ['Owner', 'Admin', 'Purchasing', 'Inventory'] },
      { label: 'Knowledge Base', route: '/knowledge-base', roles: ['Owner', 'Admin'] },
      { label: 'Addons', route: '/addons', roles: ['Owner', 'Admin'] },
      { label: 'Workflows', route: '/workflows', roles: ['Owner', 'Admin'] },
      { label: 'License Management', route: '/license-management', roles: ['Owner', 'Admin'] },
      { label: 'Users', route: '/users', roles: ['Owner', 'Admin'] },
      { label: 'Tenants', route: '/tenants', roles: ['Owner', 'Admin'] },
      { label: 'Billing', route: '/billing', roles: ['Owner', 'Admin'] },
    ]

    return items
      .filter(i => i.roles.includes(role))
      .filter(i => {
        if (i.route === '/customers') return hasPlanAccess('Customers')
        if (i.route === '/products') return hasPlanAccess('Products')
        if (i.route === '/sales-orders') return hasPlanAccess('SalesOrders')
        if (i.route === '/purchase-orders' || i.route === '/purchase-invoices') return hasPlanAccess('PurchaseOrders')
        if (i.route === '/inventory') return hasPlanAccess('Inventory')
        if (i.route === '/invoices') return hasPlanAccess('Accounting')
        if (i.route === '/exchange-rates') return hasPlanAccess('ExchangeRates')
        if (i.route === '/approvals') return hasPlanAccess('Approvals')
        if (i.route === '/alerts') return hasPlanAccess('Alerts')
        if (i.route === '/finance-dashboard') return hasPlanAccess('FinancialReports')
        if (i.route === '/recurring-templates') return hasPlanAccess('JournalEntries')
        if (i.route === '/fixed-assets') return hasPlanAccess('FixedAssets')
        if (i.route === '/intrastat') return hasPlanAccess('Intrastat')
        if (i.route === '/campaigns') return hasPlanAccess('Campaigns')
        if (i.route === '/pick-pack') return hasPlanAccess('PickPack')
        if (i.route === '/knowledge-base') return hasPlanAccess('KnowledgeBase')
        if (i.route === '/addons') return hasPlanAccess('Addons')
        if (i.route === '/workflows') return hasPlanAccess('WorkflowManager')
        if (i.route === '/license-management') return hasPlanAccess('LicenseManagement')
        return true
      })
  }, [role, moduleAccess])

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

  const toggleSection = (section: keyof typeof openSections) => {
    setOpenSections(prev => ({ ...prev, [section]: !prev[section] }))
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
          <button className="nav-group-btn" type="button" aria-expanded={openSections.sales} onClick={() => toggleSection('sales')}>
            <span className="nav-group-title"><ShoppingCart size={16} /> Ventas</span>
            <ChevronDown size={14} className={`nav-group-chevron ${openSections.sales ? 'open' : ''}`} />
          </button>
          {openSections.sales && (
            <div className="nav-group-items">
              {hasAnyRole('Owner', 'Admin', 'Sales') && hasPlanAccess('SalesOrders') && (
                <NavLink to="/sales-orders"><span className="nav-icon"><ShoppingCart size={18} /></span>Ordenes de venta</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Sales') && hasPlanAccess('Customers') && (
                <NavLink to="/customers"><span className="nav-icon"><Users size={18} /></span>Clientes</NavLink>
              )}
            </div>
          )}

          <button className="nav-group-btn" type="button" aria-expanded={openSections.purchasing} onClick={() => toggleSection('purchasing')}>
            <span className="nav-group-title"><Truck size={16} /> Compras</span>
            <ChevronDown size={14} className={`nav-group-chevron ${openSections.purchasing ? 'open' : ''}`} />
          </button>
          {openSections.purchasing && (
            <div className="nav-group-items">
              {hasAnyRole('Owner', 'Admin', 'Purchasing') && hasPlanAccess('PurchaseOrders') && (
                <NavLink to="/purchase-orders"><span className="nav-icon"><Truck size={18} /></span>Ordenes de compra</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Purchasing') && hasPlanAccess('PurchaseOrders') && (
                <NavLink to="/purchase-invoices"><span className="nav-icon"><FileText size={18} /></span>Facturas de compra</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Purchasing') && (
                <NavLink to="/suppliers"><span className="nav-icon"><User size={18} /></span>Proveedores</NavLink>
              )}
            </div>
          )}

          <button className="nav-group-btn" type="button" aria-expanded={openSections.inventory} onClick={() => toggleSection('inventory')}>
            <span className="nav-group-title"><Package size={16} /> Inventario</span>
            <ChevronDown size={14} className={`nav-group-chevron ${openSections.inventory ? 'open' : ''}`} />
          </button>
          {openSections.inventory && (
            <div className="nav-group-items">
              {hasAnyRole('Owner', 'Admin', 'Inventory') && hasPlanAccess('Inventory') && (
                <NavLink to="/inventory"><span className="nav-icon"><Package size={18} /></span>Movimientos</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Sales', 'Purchasing', 'Inventory') && hasPlanAccess('Products') && (
                <NavLink to="/products"><span className="nav-icon"><Boxes size={18} /></span>Productos</NavLink>
              )}
            </div>
          )}

          <button className="nav-group-btn" type="button" aria-expanded={openSections.finance} onClick={() => toggleSection('finance')}>
            <span className="nav-group-title"><FileText size={16} /> Finanzas</span>
            <ChevronDown size={14} className={`nav-group-chevron ${openSections.finance ? 'open' : ''}`} />
          </button>
          {openSections.finance && (
            <div className="nav-group-items">
              {hasAnyRole('Owner', 'Admin', 'Accounting') && hasPlanAccess('Accounting') && (
                <NavLink to="/invoices"><span className="nav-icon"><FileText size={18} /></span>Facturas de venta</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Accounting') && hasPlanAccess('ExchangeRates') && (
                <NavLink to="/exchange-rates"><span className="nav-icon"><Settings size={18} /></span>Tipos de cambio</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Accounting') && hasPlanAccess('FinancialReports') && (
                <NavLink to="/finance-dashboard"><span className="nav-icon"><Settings size={18} /></span>Dashboard financiero</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Accounting') && hasPlanAccess('JournalEntries') && (
                <NavLink to="/recurring-templates"><span className="nav-icon"><Settings size={18} /></span>Plantillas recurrentes</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Accounting') && hasPlanAccess('FixedAssets') && (
                <NavLink to="/fixed-assets"><span className="nav-icon"><Settings size={18} /></span>Activos fijos</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Accounting') && hasPlanAccess('Intrastat') && (
                <NavLink to="/intrastat"><span className="nav-icon"><Settings size={18} /></span>Intrastat</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing') && (
                <NavLink to="/catalogs/taxes"><span className="nav-icon"><Settings size={18} /></span>Catalogos</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Sales') && hasPlanAccess('Campaigns') && (
                <NavLink to="/campaigns"><span className="nav-icon"><Settings size={18} /></span>Campanas</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin', 'Purchasing', 'Inventory') && hasPlanAccess('PickPack') && (
                <NavLink to="/pick-pack"><span className="nav-icon"><Settings size={18} /></span>Pick Pack</NavLink>
              )}
            </div>
          )}

          <button className="nav-group-btn" type="button" aria-expanded={openSections.admin} onClick={() => toggleSection('admin')}>
            <span className="nav-group-title"><ShieldCheck size={16} /> Administracion</span>
            <ChevronDown size={14} className={`nav-group-chevron ${openSections.admin ? 'open' : ''}`} />
          </button>
          {openSections.admin && (
            <div className="nav-group-items">
              {hasAnyRole('Owner', 'Admin') && hasPlanAccess('Approvals') && (
                <NavLink to="/approvals"><span className="nav-icon"><ShieldCheck size={18} /></span>Aprobaciones</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && hasPlanAccess('Alerts') && (
                <NavLink to="/alerts"><span className="nav-icon"><Bell size={18} /></span>Alertas</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && hasPlanAccess('KnowledgeBase') && (
                <NavLink to="/knowledge-base"><span className="nav-icon"><FileText size={18} /></span>Knowledge base</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && hasPlanAccess('Addons') && (
                <NavLink to="/addons"><span className="nav-icon"><Settings size={18} /></span>Add-ons</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && hasPlanAccess('WorkflowManager') && (
                <NavLink to="/workflows"><span className="nav-icon"><Settings size={18} /></span>Workflows</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && hasPlanAccess('LicenseManagement') && (
                <NavLink to="/license-management"><span className="nav-icon"><ShieldCheck size={18} /></span>Licencias</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && (
                <NavLink to="/users"><span className="nav-icon"><ShieldCheck size={18} /></span>Usuarios</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && (
                <NavLink to="/tenants"><span className="nav-icon"><Filter size={18} /></span>Tenants</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && (
                <NavLink to="/subscription"><span className="nav-icon"><ShieldCheck size={18} /></span>Suscripcion</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && (
                <NavLink to="/billing"><span className="nav-icon"><CreditCard size={18} /></span>Billing</NavLink>
              )}
              {hasAnyRole('Owner', 'Admin') && (
                <NavLink to="/signup"><span className="nav-icon"><CircleUser size={18} /></span>Nuevo tenant</NavLink>
              )}
            </div>
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
