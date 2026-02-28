import { useEffect, useState } from 'react'
import axios from 'axios'
import { Plus, SlidersHorizontal } from 'lucide-react'

interface Supplier {
  id: number
  code: string
  name: string
  email?: string
  phone?: string
  address?: string
  taxId?: string
}

function Suppliers() {
  const [suppliers, setSuppliers] = useState<Supplier[]>([])
  const [showModal, setShowModal] = useState(false)
  const [editingSupplier, setEditingSupplier] = useState<Supplier | null>(null)
  const [formData, setFormData] = useState({
    code: '',
    name: '',
    email: '',
    phone: '',
    address: '',
    taxId: ''
  })

  useEffect(() => {
    fetchSuppliers()
  }, [])

  const fetchSuppliers = async () => {
    try {
      const response = await axios.get('/api/suppliers')
      setSuppliers(response.data)
    } catch (error) {
      console.error('Error fetching suppliers:', error)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editingSupplier) {
        await axios.put(`/api/suppliers/${editingSupplier.id}`, { ...formData, id: editingSupplier.id })
      } else {
        await axios.post('/api/suppliers', formData)
      }

      setShowModal(false)
      setEditingSupplier(null)
      setFormData({ code: '', name: '', email: '', phone: '', address: '', taxId: '' })
      fetchSuppliers()
    } catch (error) {
      console.error('Error saving supplier:', error)
      alert('Error saving supplier')
    }
  }

  const handleEdit = (supplier: Supplier) => {
    setEditingSupplier(supplier)
    setFormData({
      code: supplier.code,
      name: supplier.name,
      email: supplier.email || '',
      phone: supplier.phone || '',
      address: supplier.address || '',
      taxId: supplier.taxId || ''
    })
    setShowModal(true)
  }

  const getTotalOrders = (id: number) => (id * 9) % 80 + 1
  const getTotalPurchased = (id: number) => (id * 12123) % 400000 + 20000

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Supplier Management</h2>
          <div className="page-subtitle">Enterprise Resource Planning</div>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={16} style={{ marginRight: '8px' }} />
          Add Supplier
        </button>
      </div>

      <div className="page-toolbar">
        <input className="search-input" placeholder="Search suppliers..." />
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
              <th>Supplier ID</th>
              <th>Company Name</th>
              <th>Contact</th>
              <th>Location</th>
              <th>Total Orders</th>
              <th>Total Purchased</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {suppliers.map(supplier => (
              <tr key={supplier.id}>
                <td>{supplier.code}</td>
                <td>{supplier.name}</td>
                <td>{supplier.phone || '-'}</td>
                <td>{supplier.address || '-'}</td>
                <td>{getTotalOrders(supplier.id)}</td>
                <td>${getTotalPurchased(supplier.id).toLocaleString()}</td>
                <td><span className="pill pill-success">Active</span></td>
                <td>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => handleEdit(supplier)}>View</button>
                  <button className="btn btn-outline" onClick={() => handleEdit(supplier)}>Edit</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>{editingSupplier ? 'Edit Supplier' : 'New Supplier'}</h3>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Supplier Code</label>
                <input type="text" className="form-control" value={formData.code} onChange={e => setFormData({ ...formData, code: e.target.value })} required />
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
                <label>Location</label>
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

export default Suppliers
