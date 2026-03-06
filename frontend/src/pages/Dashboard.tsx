import { useEffect, useState } from 'react'
import axios from 'axios'

interface Stats {
  products: number
  customers: number
  suppliers: number
  salesOrders: number
}

function Dashboard() {
  const [stats, setStats] = useState<Stats>({
    products: 0,
    customers: 0,
    suppliers: 0,
    salesOrders: 0
  })

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const [products, customers, suppliers, salesOrders] = await Promise.all([
          axios.get('/api/products'),
          axios.get('/api/customers'),
          axios.get('/api/suppliers'),
          axios.get('/api/salesorders')
        ])

        setStats({
          products: products.data.length,
          customers: customers.data.length,
          suppliers: suppliers.data.length,
          salesOrders: salesOrders.data.length
        })
      } catch (error) {
        console.error('Error fetching stats:', error)
      }
    }

    fetchStats()
  }, [])

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Dashboard</h2>
          <div className="page-subtitle">Enterprise Resource Planning</div>
        </div>
        <div className="page-subtitle">Last updated: February 11, 2026</div>
      </div>

      <div className="stats-grid">
        <div className="stat-card">
          <h3>Total Sales</h3>
          <div className="number">{stats.products}</div>
        </div>
        <div className="stat-card">
          <h3>Inventory Status</h3>
          <div className="number">{stats.customers}</div>
        </div>
        <div className="stat-card">
          <h3>Outstanding Invoices</h3>
          <div className="number">{stats.suppliers}</div>
        </div>
        <div className="stat-card">
          <h3>Monthly Revenue</h3>
          <div className="number">{stats.salesOrders}</div>
        </div>
      </div>

      <div className="card">
        <h3>Sales Performance</h3>
        <p style={{ marginTop: '10px', color: '#7f8c8d' }}>
          Dashboard widgets will be connected to analytics once the reporting data is ready.
        </p>
      </div>
    </div>
  )
}

export default Dashboard
