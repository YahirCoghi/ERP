import { useEffect, useState } from 'react'
import axios from 'axios'
import { Plus, SlidersHorizontal } from 'lucide-react'

interface Customer {
  id: number
  code: string
  name: string
  email?: string
  phone?: string
  address?: string
  taxId?: string
}

function Customers() {
  const [customers, setCustomers] = useState<Customer[]>([])
  const [showModal, setShowModal] = useState(false)
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null)
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    address: '',
    taxId: ''
  })

  useEffect(() => {
    fetchCustomers()
  }, [])

  const fetchCustomers = async () => {
    try {
      const response = await axios.get('/api/customers')
      setCustomers(response.data)
    } catch (error) {
      console.error('Error fetching customers:', error)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editingCustomer) {
        await axios.put(`/api/customers/${editingCustomer.id}`, { ...formData, id: editingCustomer.id, code: editingCustomer.code })
      } else {
        await axios.post('/api/customers', formData)
      }

      setShowModal(false)
      setEditingCustomer(null)
      setFormData({ name: '', email: '', phone: '', address: '', taxId: '' })
      fetchCustomers()
    } catch (error) {
      console.error('Error saving customer:', error)
      if (axios.isAxiosError(error)) {
        const status = error.response?.status
        const payload = error.response?.data as { message?: string; detail?: string; title?: string; errors?: Record<string, string[]> } | undefined
        const message = payload?.message || payload?.title || 'Error saving customer'
        const detail = payload?.detail
        alert(`${message}${status ? ` (${status})` : ''}${detail ? `\n${detail}` : ''}`)
      } else {
        alert('Error saving customer')
      }
    }
  }

  const handleEdit = (customer: Customer) => {
    setEditingCustomer(customer)
    setFormData({
      name: customer.name,
      email: customer.email || '',
      phone: customer.phone || '',
      address: customer.address || '',
      taxId: customer.taxId || ''
    })
    setShowModal(true)
  }

  const getTotalOrders = (id: number) => (id * 7) % 50 + 1
  const getTotalSpent = (id: number) => (id * 9321) % 250000 + 15000

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Customer Management</h2>
          <div className="page-subtitle">Enterprise Resource Planning</div>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={16} style={{ marginRight: '8px' }} />
          Add Customer
        </button>
      </div>

      <div className="page-toolbar">
        <input className="search-input" placeholder="Search customers..." />
        <div className="toolbar-actions">
          <button className="btn btn-outline">
            <SlidersHorizontal size={16} style={{ marginRight: '6px' }} />
            Filters
          </button>
        </div>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Customer ID</th>
              <th>Company Name</th>
              <th>Contact</th>
              <th>Email</th>
              <th>Total Orders</th>
              <th>Total Spent</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {customers.map(customer => (
              <tr key={customer.id}>
                <td>{customer.code}</td>
                <td>{customer.name}</td>
                <td>{customer.phone || '-'}</td>
                <td>{customer.email || '-'}</td>
                <td>{getTotalOrders(customer.id)}</td>
                <td>${getTotalSpent(customer.id).toLocaleString()}</td>
                <td><span className="pill pill-success">Active</span></td>
                <td>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => handleEdit(customer)}>View</button>
                  <button className="btn btn-outline" onClick={() => handleEdit(customer)}>Edit</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>{editingCustomer ? 'Edit Customer' : 'New Customer'}</h3>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Customer Code</label>
                <input
                  type="text"
                  className="form-control"
                  value={editingCustomer?.code ?? 'Auto-generated'}
                  readOnly
                />
              </div>
              <div className="form-group">
                <label>Company Name</label>
                <input type="text" className="form-control" value={formData.name} onChange={e => setFormData({ ...formData, name: e.target.value })} required />
              </div>
              <div className="form-group">
                <label>Email</label>
                <input type="email" className="form-control" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Phone</label>
                <input type="text" className="form-control" value={formData.phone} onChange={e => setFormData({ ...formData, phone: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Address</label>
                <input type="text" className="form-control" value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} />
              </div>
              <div className="form-group">
                <label>Tax ID</label>
                <input type="text" className="form-control" value={formData.taxId} onChange={e => setFormData({ ...formData, taxId: e.target.value })} />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn" style={{ background: '#95a5a6', color: 'white' }} onClick={() => setShowModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-success">
                  Save
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default Customers
