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
  }

  const handleLogout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('role')
    delete axios.defaults.headers.common['Authorization']
    setIsAuthenticated(false)
  }

  if (loading) {
    return <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>Cargando...</div>
  }

  if (!isAuthenticated) {
    return <Login onLogin={handleLogin} />
  }

  return (
    <Layout onLogout={handleLogout} username={username} role={role}>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/products" element={hasAnyRole(role, ['Owner', 'Admin', 'Sales', 'Purchasing', 'Inventory']) ? <Products /> : <Navigate to="/" replace />} />
        <Route path="/customers" element={hasAnyRole(role, ['Owner', 'Admin', 'Sales']) ? <Customers /> : <Navigate to="/" replace />} />
        <Route path="/suppliers" element={hasAnyRole(role, ['Owner', 'Admin', 'Purchasing']) ? <Suppliers /> : <Navigate to="/" replace />} />
        <Route path="/sales-orders" element={hasAnyRole(role, ['Owner', 'Admin', 'Sales']) ? <SalesOrders /> : <Navigate to="/" replace />} />
        <Route path="/purchase-orders" element={hasAnyRole(role, ['Owner', 'Admin', 'Purchasing']) ? <PurchaseOrders /> : <Navigate to="/" replace />} />
        <Route path="/purchase-invoices" element={hasAnyRole(role, ['Owner', 'Admin', 'Purchasing']) ? <PurchaseInvoices /> : <Navigate to="/" replace />} />
        <Route path="/invoices" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting']) ? <Invoices /> : <Navigate to="/" replace />} />
        <Route path="/inventory" element={hasAnyRole(role, ['Owner', 'Admin', 'Inventory']) ? <InventoryMovements /> : <Navigate to="/" replace />} />
        <Route path="/catalogs/taxes" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing']) ? <TaxCodes /> : <Navigate to="/" replace />} />
        <Route path="/catalogs/sale-conditions" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing']) ? <SaleConditions /> : <Navigate to="/" replace />} />
        <Route path="/catalogs/payment-methods" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing']) ? <PaymentMethods /> : <Navigate to="/" replace />} />
        <Route path="/catalogs/payment-terms" element={hasAnyRole(role, ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing']) ? <PaymentTerms /> : <Navigate to="/" replace />} />
        <Route path="/signup" element={hasAnyRole(role, ['Owner', 'Admin']) ? <Signup /> : <Navigate to="/" replace />} />
        <Route path="/tenants" element={hasAnyRole(role, ['Owner', 'Admin']) ? <TenantSelector /> : <Navigate to="/" replace />} />
        <Route path="/subscription" element={hasAnyRole(role, ['Owner', 'Admin']) ? <SubscriptionStatus /> : <Navigate to="/" replace />} />
        <Route path="/users" element={hasAnyRole(role, ['Owner', 'Admin']) ? <UsersManagement /> : <Navigate to="/" replace />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  )
}

export default App
