import { useState, useEffect } from 'react'
import axios from 'axios'
import { Plus, SlidersHorizontal } from 'lucide-react'

interface Product {
  id: number
  name: string
  sku: string
  stock: number
}

interface InventoryMovement {
  id: number
  productId: number
  productName: string
  productSku: string
  type: string
  quantity: number
  unitCost: number
  totalCost: number
  reference: string
  referenceId: number
  notes: string
  createdAt: string
}

function InventoryMovements() {
  const [movements, setMovements] = useState<InventoryMovement[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [formData, setFormData] = useState({
    productId: '',
    type: 'IN',
    quantity: '',
    unitCost: '',
    notes: ''
  })

  useEffect(() => {
    fetchMovements()
    fetchProducts()
  }, [])

  const fetchMovements = async () => {
    try {
      const response = await axios.get('/api/inventorymovements')
      setMovements(response.data)
    } catch (error) {
      console.error('Error loading movements:', error)
    } finally {
      setLoading(false)
    }
  }

  const fetchProducts = async () => {
    try {
      const response = await axios.get('/api/products')
      setProducts(response.data)
    } catch (error) {
      console.error('Error loading products:', error)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await axios.post('/api/inventorymovements', {
        productId: parseInt(formData.productId),
        type: formData.type,
        quantity: parseInt(formData.quantity),
        unitCost: parseFloat(formData.unitCost),
        notes: formData.notes
      })
      setShowModal(false)
      setFormData({ productId: '', type: 'IN', quantity: '', unitCost: '', notes: '' })
      fetchMovements()
    } catch (error) {
      console.error('Error creating movement:', error)
      alert('Error creating movement')
    }
  }

  const getTypeBadge = (type: string) => {
    if (type === 'IN') return 'pill pill-success'
    if (type === 'OUT') return 'pill pill-danger'
    return 'pill pill-warning'
  }

  if (loading) return <div>Loading...</div>

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Inventory Management</h2>
          <div className="page-subtitle">Enterprise Resource Planning</div>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={16} style={{ marginRight: '8px' }} />
          Add Product Movement
        </button>
      </div>

      <div className="page-toolbar">
        <input className="search-input" placeholder="Search inventory..." />
        <div className="toolbar-actions">
          <button className="btn btn-outline">
            <SlidersHorizontal size={16} style={{ marginRight: '6px' }} />
            More Filters
          </button>
        </div>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>SKU</th>
              <th>Product</th>
              <th>Type</th>
              <th>Quantity</th>
              <th>Unit Cost</th>
              <th>Total Cost</th>
              <th>Reference</th>
              <th>Notes</th>
            </tr>
          </thead>
          <tbody>
            {movements.map(movement => (
              <tr key={movement.id}>
                <td>{movement.productSku}</td>
                <td>{movement.productName}</td>
                <td>
                  <span className={getTypeBadge(movement.type)}>
                    {movement.type === 'IN' ? 'Normal' : movement.type === 'OUT' ? 'Low Stock' : 'Adjusted'}
                  </span>
                </td>
                <td>{movement.quantity}</td>
                <td>${movement.unitCost.toFixed(2)}</td>
                <td>${movement.totalCost.toFixed(2)}</td>
                <td>{movement.reference || '-'}</td>
                <td>{movement.notes || '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>New Inventory Movement</h3>

            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Product</label>
                <select
                  className="form-control"
                  value={formData.productId}
                  onChange={(e) => setFormData({ ...formData, productId: e.target.value })}
                  required
                >
                  <option value="">Select a product</option>
                  {products.map(product => (
                    <option key={product.id} value={product.id}>
                      {product.name} (Stock: {product.stock})
                    </option>
                  ))}
                </select>
              </div>

              <div className="form-group">
                <label>Movement Type</label>
                <select
                  className="form-control"
                  value={formData.type}
                  onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                  required
                >
                  <option value="IN">Inbound</option>
                  <option value="OUT">Outbound</option>
                  <option value="ADJUSTMENT">Adjustment</option>
                </select>
              </div>

              <div className="form-group">
                <label>Quantity</label>
                <input
                  type="number"
                  className="form-control"
                  value={formData.quantity}
                  onChange={(e) => setFormData({ ...formData, quantity: e.target.value })}
                  min="1"
                  required
                />
              </div>

              <div className="form-group">
                <label>Unit Cost</label>
                <input
                  type="number"
                  step="0.01"
                  className="form-control"
                  value={formData.unitCost}
                  onChange={(e) => setFormData({ ...formData, unitCost: e.target.value })}
                  min="0"
                  required
                />
              </div>

              <div className="form-group">
                <label>Notes</label>
                <textarea
                  className="form-control"
                  value={formData.notes}
                  onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                  rows={3}
                />
              </div>

              <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
                Save Movement
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default InventoryMovements
