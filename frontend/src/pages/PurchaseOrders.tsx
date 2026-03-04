import { useEffect, useState } from 'react'
import axios from 'axios'
import { Plus, SlidersHorizontal } from 'lucide-react'

interface Supplier {
  id: number
  name: string
}

interface Product {
  id: number
  name: string
  cost: number
}

interface TaxCode {
  id?: number
  code: string
  name: string
  rate: number
  isExempt: boolean
}

interface PurchasingSettings {
  defaultTaxRate: number
}

interface PurchaseOrderLine {
  id?: number
  productId: number
  quantity: number
  unitCost: number
  total: number
  product?: Product
}

interface PurchaseOrder {
  id: number
  orderNumber: string
  orderDate: string
  supplierId: number
  supplier?: Supplier
  subtotal: number
  tax: number
  total: number
  status: string
  lines: PurchaseOrderLine[]
}

function PurchaseOrders() {
  const [orders, setOrders] = useState<PurchaseOrder[]>([])
  const [suppliers, setSuppliers] = useState<Supplier[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [taxCodes, setTaxCodes] = useState<TaxCode[]>([])
  const [settings, setSettings] = useState<PurchasingSettings>({ defaultTaxRate: 13 })
  const [showModal, setShowModal] = useState(false)
  const [formData, setFormData] = useState({
    supplierId: '',
    taxCode: '',
    lines: [] as { productId: string; quantity: string; unitCost: string }[]
  })

  useEffect(() => {
    fetchOrders()
    fetchSuppliers()
    fetchProducts()
    fetchTaxCodes()
    fetchSettings()
  }, [])

  const fetchOrders = async () => {
    try {
      const response = await axios.get('/api/purchaseorders')
      setOrders(response.data)
    } catch (error) {
      console.error('Error fetching orders:', error)
    }
  }

  const fetchSuppliers = async () => {
    try {
      const response = await axios.get('/api/suppliers')
      setSuppliers(response.data)
    } catch (error) {
      console.error('Error fetching suppliers:', error)
    }
  }

  const fetchProducts = async () => {
    try {
      const response = await axios.get('/api/products')
      setProducts(response.data)
    } catch (error) {
      console.error('Error fetching products:', error)
    }
  }

  const fetchTaxCodes = async () => {
    try {
      const response = await axios.get('/api/taxcodes')
      setTaxCodes(response.data)
    } catch (error) {
      console.error('Error fetching tax codes:', error)
    }
  }

  const fetchSettings = async () => {
    try {
      const response = await axios.get('/api/purchasingsettings')
      setSettings(response.data)
    } catch (error) {
      console.error('Error fetching settings:', error)
    }
  }

  const getTaxRate = () => {
    const tax = taxCodes.find(t => t.code === formData.taxCode)
    if (tax) {
      return tax.isExempt ? 0 : tax.rate
    }
    return settings.defaultTaxRate ?? 13
  }

  const addLine = () => {
    setFormData({
      ...formData,
      lines: [...formData.lines, { productId: '', quantity: '1', unitCost: '' }]
    })
  }

  const updateLine = (index: number, field: string, value: string) => {
    const newLines = [...formData.lines]
    newLines[index] = { ...newLines[index], [field]: value }

    if (field === 'productId') {
      const product = products.find(p => p.id === parseInt(value))
      if (product) {
        newLines[index].unitCost = product.cost.toString()
      }
    }

    setFormData({ ...formData, lines: newLines })
  }

  const removeLine = (index: number) => {
    setFormData({
      ...formData,
      lines: formData.lines.filter((_, i) => i !== index)
    })
  }

  const calculateTotals = () => {
    let subtotal = 0
    formData.lines.forEach(line => {
      const qty = parseFloat(line.quantity) || 0
      const cost = parseFloat(line.unitCost) || 0
      subtotal += qty * cost
    })
    const taxRate = getTaxRate()
    const tax = subtotal * (taxRate / 100)
    return { subtotal, tax, total: subtotal + tax }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const { subtotal, tax, total } = calculateTotals()

      const data = {
        supplierId: parseInt(formData.supplierId),
        subtotal,
        tax,
        total,
        lines: formData.lines.map(line => ({
          productId: parseInt(line.productId),
          quantity: parseFloat(line.quantity),
          unitCost: parseFloat(line.unitCost),
          total: parseFloat(line.quantity) * parseFloat(line.unitCost)
        }))
      }

      await axios.post('/api/purchaseorders', data)

      setShowModal(false)
      setFormData({ supplierId: '', taxCode: '', lines: [] })
      fetchOrders()
    } catch (error) {
      console.error('Error saving order:', error)
      alert('Error saving the order')
    }
  }

  const statusClass = (status: string) => {
    if (status?.toLowerCase() === 'received') return 'pill pill-success'
    if (status?.toLowerCase() === 'approved') return 'pill pill-warning'
    return 'pill'
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Purchase Orders</h2>
          <div className="page-subtitle">Enterprise Resource Planning</div>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={16} style={{ marginRight: '8px' }} />
          Create Purchase Order
        </button>
      </div>

      <div className="page-toolbar">
        <input className="search-input" placeholder="Search purchase orders..." />
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
              <th>Order ID</th>
              <th>Supplier</th>
              <th>Date</th>
              <th>Status</th>
              <th>Total</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {orders.map(order => (
              <tr key={order.id}>
                <td>{order.orderNumber}</td>
                <td>{order.supplier?.name || '-'}</td>
                <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                <td>
                  <span className={statusClass(order.status)}>
                    {order.status || 'Pending'}
                  </span>
                </td>
                <td>${order.total.toFixed(2)}</td>
                <td>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }}>View</button>
                  <button className="btn btn-outline">Edit</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" style={{ maxWidth: '700px' }} onClick={e => e.stopPropagation()}>
            <h3>New Purchase Order</h3>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Order Number</label>
                <input type="text" className="form-control" value="Auto-generated" readOnly />
              </div>

              <div className="form-group">
                <label>Supplier</label>
                <select className="form-control" value={formData.supplierId} onChange={e => setFormData({ ...formData, supplierId: e.target.value })} required>
                  <option value="">Select a supplier</option>
                  {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                </select>
              </div>

              <div className="form-group">
                <label>Tax</label>
                <select className="form-control" value={formData.taxCode} onChange={e => setFormData({ ...formData, taxCode: e.target.value })}>
                  <option value="">Use default ({getTaxRate().toFixed(2)}%)</option>
                  {taxCodes.map(t => (
                    <option key={t.id ?? t.code} value={t.code}>
                      {t.code} - {t.name} ({t.isExempt ? 'Exempt' : `${t.rate.toFixed(2)}%`})
                    </option>
                  ))}
                </select>
              </div>

              <h4 style={{ marginTop: '20px', marginBottom: '10px' }}>Lines</h4>
              {formData.lines.map((line, index) => (
                <div key={index} style={{ display: 'flex', gap: '10px', marginBottom: '10px', alignItems: 'flex-end' }}>
                  <div style={{ flex: 2 }}>
                    <label>Product</label>
                    <select className="form-control" value={line.productId} onChange={e => updateLine(index, 'productId', e.target.value)} required>
                      <option value="">Select</option>
                      {products.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                    </select>
                  </div>
                  <div style={{ flex: 1 }}>
                    <label>Quantity</label>
                    <input type="number" className="form-control" value={line.quantity} onChange={e => updateLine(index, 'quantity', e.target.value)} required />
                  </div>
                  <div style={{ flex: 1 }}>
                    <label>Cost</label>
                    <input type="number" step="0.01" className="form-control" value={line.unitCost} onChange={e => updateLine(index, 'unitCost', e.target.value)} required />
                  </div>
                  <button type="button" className="btn btn-danger" onClick={() => removeLine(index)}>X</button>
                </div>
              ))}

              <button type="button" className="btn btn-primary" onClick={addLine} style={{ marginBottom: '20px' }}>
                + Add Line
              </button>

              {formData.lines.length > 0 && (
                <div style={{ textAlign: 'right', marginBottom: '20px', padding: '15px', background: '#f8f9fa', borderRadius: '5px' }}>
                  <div>Subtotal: ${calculateTotals().subtotal.toFixed(2)}</div>
                  <div>Tax ({getTaxRate().toFixed(2)}%): ${calculateTotals().tax.toFixed(2)}</div>
                  <div style={{ fontSize: '18px', fontWeight: 'bold', marginTop: '5px' }}>
                    Total: ${calculateTotals().total.toFixed(2)}
                  </div>
                </div>
              )}

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

export default PurchaseOrders
