import { Routes, Route, Navigate } from 'react-router-dom'
import { useState, useEffect } from 'react'
import axios from 'axios'
import Login from './components/Login'
import Layout from './components/Layout'
import Dashboard from './pages/Dashboard'
import Products from './pages/Products'
import Customers from './pages/Customers'
import Suppliers from './pages/Suppliers'
import SalesOrders from './pages/SalesOrders'
import PurchaseOrders from './pages/PurchaseOrders'
import PurchaseInvoices from './pages/PurchaseInvoices'
import Invoices from './pages/Invoices'
import InventoryMovements from './pages/InventoryMovements'
import TaxCodes from './pages/TaxCodes'
import SaleConditions from './pages/SaleConditions'
import PaymentMethods from './pages/PaymentMethods'
import PaymentTerms from './pages/PaymentTerms'
import Signup from './pages/Signup'
import TenantSelector from './pages/TenantSelector'
import UsersManagement from './pages/UsersManagement'
import SubscriptionStatus from './pages/SubscriptionStatus'
import Billing from './pages/Billing'
import ExchangeRates from './pages/ExchangeRates'
import Approvals from './pages/Approvals'
import Alerts from './pages/Alerts'
import FinanceDashboard from './pages/FinanceDashboard'
import RecurringTemplates from './pages/RecurringTemplates'
import Addons from './pages/Addons'
import Workflows from './pages/Workflows'
import Campaigns from './pages/Campaigns'
import FixedAssets from './pages/FixedAssets'
import Intrastat from './pages/Intrastat'
import PickPack from './pages/PickPack'
import KnowledgeBase from './pages/KnowledgeBase'
import LicenseManagement from './pages/LicenseManagement'

const hasAnyRole = (role: string, roles: string[]) => roles.includes(role)

function decodeJwtRole(token: string): string {
  try {
    const payload = token.split('.')[1]
    const decoded = JSON.parse(atob(payload))
    return (
      decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      decoded.role ||
      'User'
    )
  } catch {
    return 'User'
  }
}

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  const [loading, setLoading] = useState(true)
  const [username, setUsername] = useState('')
  const [role, setRole] = useState('User')
  const [moduleAccess, setModuleAccess] = useState<Record<string, string>>({})

  const loadPlanAccess = async () => {
    try {
      const response = await axios.get('/api/subscription/current')
      const modules = response.data?.modules || []
      const map: Record<string, string> = {}
      modules.forEach((m: any) => {
        if (m?.module) {
          map[m.module] = String(m.accessLevel || 'None')
        }
      })
      setModuleAccess(map)
    } catch {
      setModuleAccess({})
    }
  }

  const hasPlanAccess = (moduleName: string, required: 'Read' | 'Full') => {
    const current = (moduleAccess[moduleName] || 'Full').toLowerCase()
    const currentRank = current === 'full' ? 2 : current === 'read' ? 1 : 0
    const requiredRank = required === 'Full' ? 2 : 1
    return currentRank >= requiredRank
  }

  useEffect(() => {
    // Verificar si hay un token guardado
    const token = localStorage.getItem('token')
    const storedRole = localStorage.getItem('role')
    const storedUsername = localStorage.getItem('username')
    if (token) {
      axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
      setRole(storedRole || decodeJwtRole(token))
      setUsername(storedUsername || 'User')
      setIsAuthenticated(true)
      loadPlanAccess()
    }
    setLoading(false)
  }, [])

  const handleLogin = (token: string, loginUsername: string, loginRole: string) => {
    localStorage.setItem('token', token)
    localStorage.setItem('username', loginUsername)
    localStorage.setItem('role', loginRole)
    axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
    setUsername(loginUsername)
    setRole(loginRole)
    setIsAuthenticated(true)
    loadPlanAccess()
  }

  const handleLogout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('role')
    delete axios.defaults.headers.common['Authorization']
    setModuleAccess({})
    setIsAuthenticated(false)
  }

  if (loading) {
    return <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>Cargando...</div>
  }

  if (!isAuthenticated) {
    return <Login onLogin={handleLogin} />
  }

  return (
    <Layout onLogout={handleLogout} username={username} role={role} moduleAccess={moduleAccess}>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/products" element={hasAnyRole(role, ['Owner', 'Admin', 'Sales', 'Purchasing', 'Inventory']) && hasPlanAccess('Products', 'Read') ? <Products /> : <Navigate to="/" replace />} />
        <Route path="/customers" element={hasAnyRole(role, ['Owner', 'Admin', 'Sales']) && hasPlanAccess('Customers', 'Read') ? <Customers /> : <Navigate to="/" replace />} />
        <Route path="/suppliers" element={hasAnyRole(role, ['Owner', 'Admin', 'Purchasing']) ? <Suppliers /> : <Navigate to="/" replace />} />
        <Route path="/sales-orders" element={hasAnyRole(role, ['Owner', 'Admin', 'Sales']) && hasPlanAccess('SalesOrders', 'Read') ? <SalesOrders /> : <Navigate to="/" replace />} />
        <Route path="/purchase-orders" element={hasAnyRole(role, ['Owner', 'Admin', 'Purchasing']) && hasPlanAccess('PurchaseOrders', 'Read') ? <PurchaseOrders /> : <Navigate to="/" replace />} />
        <Route path="/purchase-invoices" element={hasAnyRole(role, ['Owner', 'Admin', 'Purchasing']) && hasPlanAccess('PurchaseOrders', 'Read') ? <PurchaseInvoices /> : <Navigate to="/" replace />} />
        <Route path="/invoices" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting']) && hasPlanAccess('Accounting', 'Read') ? <Invoices /> : <Navigate to="/" replace />} />
        <Route path="/inventory" element={hasAnyRole(role, ['Owner', 'Admin', 'Inventory']) && hasPlanAccess('Inventory', 'Read') ? <InventoryMovements /> : <Navigate to="/" replace />} />
        <Route path="/catalogs/taxes" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing']) ? <TaxCodes /> : <Navigate to="/" replace />} />
        <Route path="/catalogs/sale-conditions" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing']) ? <SaleConditions /> : <Navigate to="/" replace />} />
        <Route path="/catalogs/payment-methods" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing']) ? <PaymentMethods /> : <Navigate to="/" replace />} />
        <Route path="/catalogs/payment-terms" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing']) ? <PaymentTerms /> : <Navigate to="/" replace />} />
        <Route path="/signup" element={hasAnyRole(role, ['Owner', 'Admin']) ? <Signup /> : <Navigate to="/" replace />} />
        <Route path="/tenants" element={hasAnyRole(role, ['Owner', 'Admin']) ? <TenantSelector /> : <Navigate to="/" replace />} />
        <Route path="/subscription" element={hasAnyRole(role, ['Owner', 'Admin']) ? <SubscriptionStatus /> : <Navigate to="/" replace />} />
        <Route path="/billing" element={hasAnyRole(role, ['Owner', 'Admin']) ? <Billing /> : <Navigate to="/" replace />} />
        <Route path="/exchange-rates" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting']) && hasPlanAccess('ExchangeRates', 'Read') ? <ExchangeRates /> : <Navigate to="/" replace />} />
        <Route path="/approvals" element={hasAnyRole(role, ['Owner', 'Admin']) && hasPlanAccess('Approvals', 'Read') ? <Approvals /> : <Navigate to="/" replace />} />
        <Route path="/alerts" element={hasAnyRole(role, ['Owner', 'Admin']) && hasPlanAccess('Alerts', 'Read') ? <Alerts /> : <Navigate to="/" replace />} />
        <Route path="/finance-dashboard" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting']) && hasPlanAccess('FinancialReports', 'Read') ? <FinanceDashboard /> : <Navigate to="/" replace />} />
        <Route path="/recurring-templates" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting']) && hasPlanAccess('JournalEntries', 'Read') ? <RecurringTemplates /> : <Navigate to="/" replace />} />
        <Route path="/addons" element={hasAnyRole(role, ['Owner', 'Admin']) && hasPlanAccess('Addons', 'Read') ? <Addons /> : <Navigate to="/" replace />} />
        <Route path="/workflows" element={hasAnyRole(role, ['Owner', 'Admin']) && hasPlanAccess('WorkflowManager', 'Read') ? <Workflows /> : <Navigate to="/" replace />} />
        <Route path="/license-management" element={hasAnyRole(role, ['Owner', 'Admin']) && hasPlanAccess('LicenseManagement', 'Read') ? <LicenseManagement /> : <Navigate to="/" replace />} />
        <Route path="/campaigns" element={hasAnyRole(role, ['Owner', 'Admin', 'Sales']) && hasPlanAccess('Campaigns', 'Read') ? <Campaigns /> : <Navigate to="/" replace />} />
        <Route path="/fixed-assets" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting']) && hasPlanAccess('FixedAssets', 'Read') ? <FixedAssets /> : <Navigate to="/" replace />} />
        <Route path="/intrastat" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting']) && hasPlanAccess('Intrastat', 'Read') ? <Intrastat /> : <Navigate to="/" replace />} />
        <Route path="/pick-pack" element={hasAnyRole(role, ['Owner', 'Admin', 'Purchasing', 'Inventory']) && hasPlanAccess('PickPack', 'Read') ? <PickPack /> : <Navigate to="/" replace />} />
        <Route path="/knowledge-base" element={hasAnyRole(role, ['Owner', 'Admin']) && hasPlanAccess('KnowledgeBase', 'Read') ? <KnowledgeBase /> : <Navigate to="/" replace />} />
        <Route path="/users" element={hasAnyRole(role, ['Owner', 'Admin']) ? <UsersManagement /> : <Navigate to="/" replace />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  )
}

export default App
