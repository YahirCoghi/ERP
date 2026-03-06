import { useEffect, useState } from 'react'
import axios from 'axios'

interface AppUser {
  id: number
  username: string
  email: string
  role: string
  isActive: boolean
  createdAt: string
  lastLoginAt?: string
}

const AVAILABLE_ROLES = ['Owner', 'Admin', 'Accounting', 'Sales', 'Purchasing', 'Inventory']

function UsersManagement() {
  const [users, setUsers] = useState<AppUser[]>([])
  const [loading, setLoading] = useState(true)
  const [newPassword, setNewPassword] = useState<Record<number, string>>({})

  const fetchUsers = async () => {
    try {
      const response = await axios.get('/api/users')
      setUsers(response.data)
    } catch (error) {
      console.error('Error fetching users:', error)
      const message =
        (error as any)?.response?.data?.detail ||
        (error as any)?.response?.data?.message ||
        `Error loading users (${(error as any)?.response?.status ?? 'unknown'})`
      alert(message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchUsers()
  }, [])

  const updateRole = async (id: number, role: string) => {
    await axios.put(`/api/users/${id}/role`, { role })
    setUsers(prev => prev.map(u => (u.id === id ? { ...u, role } : u)))
  }

  const updateStatus = async (id: number, isActive: boolean) => {
    await axios.put(`/api/users/${id}/status`, { isActive })
    setUsers(prev => prev.map(u => (u.id === id ? { ...u, isActive } : u)))
  }

  const resetPassword = async (id: number) => {
    const password = (newPassword[id] || '').trim()
    if (password.length < 8) {
      alert('Password must be at least 8 characters')
      return
    }

    await axios.put(`/api/users/${id}/password`, { newPassword: password })
    setNewPassword(prev => ({ ...prev, [id]: '' }))
    alert('Password updated')
  }

  if (loading) return <div>Loading users...</div>

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>User Management</h2>
          <div className="page-subtitle">Owner/Admin only</div>
        </div>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Username</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Last Login</th>
              <th>Password Reset</th>
            </tr>
          </thead>
          <tbody>
            {users.map(user => (
              <tr key={user.id}>
                <td>{user.username}</td>
                <td>{user.email}</td>
                <td>
                  <select
                    className="form-control"
                    value={user.role}
                    onChange={e => updateRole(user.id, e.target.value)}
                  >
                    {AVAILABLE_ROLES.map(role => <option key={role} value={role}>{role}</option>)}
                  </select>
                </td>
                <td>
                  <button
                    className={`btn ${user.isActive ? 'btn-success' : 'btn-danger'}`}
                    onClick={() => updateStatus(user.id, !user.isActive)}
                  >
                    {user.isActive ? 'Active' : 'Inactive'}
                  </button>
                </td>
                <td>{user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleString() : '-'}</td>
                <td>
                  <div style={{ display: 'flex', gap: '8px' }}>
                    <input
                      className="form-control"
                      type="password"
                      placeholder="new password"
                      value={newPassword[user.id] || ''}
                      onChange={e => setNewPassword(prev => ({ ...prev, [user.id]: e.target.value }))}
                    />
                    <button className="btn btn-primary" onClick={() => resetPassword(user.id)}>
                      Reset
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default UsersManagement
