import { useEffect, useState } from 'react'
import axios from 'axios'

interface SubscriptionData {
  id: number
  name: string
  status: string
  planCode?: string
  trialEndsAt?: string
  paidUntil?: string
  maxUsers: number
  activeUsers: number
  remainingUsers: number
  license?: {
    id: number
    licenseKey: string
    status: string
    expiresAt?: string
  } | null
}

function SubscriptionStatus() {
  const [data, setData] = useState<SubscriptionData | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const load = async () => {
      try {
        const response = await axios.get('/api/subscription/current')
        setData(response.data)
      } catch (error) {
        console.error('Error loading subscription:', error)
      } finally {
        setLoading(false)
      }
    }

    load()
  }, [])

  const formatDate = (value?: string) => (value ? new Date(value).toLocaleDateString() : '-')

  if (loading) return <div>Loading subscription...</div>
  if (!data) return <div>Subscription data not available.</div>

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Subscription</h2>
          <div className="page-subtitle">Tenant access and licensing status</div>
        </div>
      </div>

      <div className="stats-grid">
        <div className="stat-card">
          <h3>Tenant</h3>
          <div className="number">{data.name}</div>
        </div>
        <div className="stat-card">
          <h3>Status</h3>
          <div className="number">{data.status}</div>
        </div>
        <div className="stat-card">
          <h3>Plan</h3>
          <div className="number">{data.planCode || 'N/A'}</div>
        </div>
      </div>

      <div className="card">
        <table className="table">
          <tbody>
            <tr>
              <th>Trial Ends</th>
              <td>{formatDate(data.trialEndsAt)}</td>
            </tr>
            <tr>
              <th>Paid Until</th>
              <td>{formatDate(data.paidUntil)}</td>
            </tr>
            <tr>
              <th>Max Users</th>
              <td>{data.maxUsers}</td>
            </tr>
            <tr>
              <th>Active Users</th>
              <td>{data.activeUsers}</td>
            </tr>
            <tr>
              <th>Remaining Users</th>
              <td>{data.remainingUsers}</td>
            </tr>
            <tr>
              <th>License Key</th>
              <td>{data.license?.licenseKey || '-'}</td>
            </tr>
            <tr>
              <th>License Status</th>
              <td>{data.license?.status || '-'}</td>
            </tr>
            <tr>
              <th>License Expires</th>
              <td>{formatDate(data.license?.expiresAt)}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default SubscriptionStatus
