import { useEffect, useState } from 'react'
import axios from 'axios'
import { Plus, SlidersHorizontal } from 'lucide-react'

interface Customer {
  id: number
  name: string
}

interface Product {
  id: number
  name: string
  price: number
}

interface TaxCode {
  id?: number
  code: string
  name: string
  rate: number
  isExempt: boolean
}

interface SalesSettings {
  defaultTaxRate: number
}

interface SalesOrderLine {
  id?: number
  productId: number
  quantity: number
  unitPrice: number
  discount: number
  total: number
  product?: Product
}

interface SalesOrder {
  id: number
  orderNumber: string
  orderDate: string
  customerId: number
  customer?: Customer
  subtotal: number
  tax: number
  total: number
  status: string
  lines: SalesOrderLine[]
}

function SalesOrders() {
  const [orders, setOrders] = useState<SalesOrder[]>([])
  const [customers, setCustomers] = useState<Customer[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [taxCodes, setTaxCodes] = useState<TaxCode[]>([])
  const [settings, setSettings] = useState<SalesSettings>({ defaultTaxRate: 13 })
  const [showModal, setShowModal] = useState(false)
  const [showDetailModal, setShowDetailModal] = useState(false)
  const [selectedOrder, setSelectedOrder] = useState<SalesOrder | null>(null)
  const [workingOrderId, setWorkingOrderId] = useState<number | null>(null)
  const [formData, setFormData] = useState({
    customerId: '',
    taxCode: '',
    lines: [] as { productId: string; quantity: string; unitPrice: string; discount: string }[]
  })

  useEffect(() => {
    fetchOrders()
    fetchCustomers()
    fetchProducts()
    fetchTaxCodes()
    fetchSettings()
  }, [])

  const fetchOrders = async () => {
    try {
      const response = await axios.get('/api/salesorders')
      setOrders(response.data)
    } catch (error) {
      console.error('Error fetching orders:', error)
    }
  }

  const fetchCustomers = async () => {
    try {
      const response = await axios.get('/api/customers')
      setCustomers(response.data)
    } catch (error) {
      console.error('Error fetching customers:', error)
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
      const response = await axios.get('/api/salessettings')
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
      lines: [...formData.lines, { productId: '', quantity: '1', unitPrice: '', discount: '0' }]
    })
  }

  const updateLine = (index: number, field: string, value: string) => {
    const newLines = [...formData.lines]
    newLines[index] = { ...newLines[index], [field]: value }

    if (field === 'productId') {
      const product = products.find(p => p.id === parseInt(value))
      if (product) {
        newLines[index].unitPrice = product.price.toString()
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
      const price = parseFloat(line.unitPrice) || 0
      const discount = parseFloat(line.discount) || 0
      subtotal += (qty * price) - discount
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
        customerId: parseInt(formData.customerId),
        subtotal,
        tax,
        total,
        lines: formData.lines.map(line => ({
          productId: parseInt(line.productId),
          quantity: parseFloat(line.quantity),
          unitPrice: parseFloat(line.unitPrice),
          discount: parseFloat(line.discount),
          total: (parseFloat(line.quantity) * parseFloat(line.unitPrice)) - parseFloat(line.discount)
        }))
      }

      await axios.post('/api/salesorders', data)

      setShowModal(false)
      setFormData({ customerId: '', taxCode: '', lines: [] })
      fetchOrders()
    } catch (error) {
      console.error('Error saving order:', error)
      if (axios.isAxiosError(error)) {
        const status = error.response?.status
        const payload = error.response?.data as { message?: string; detail?: string; title?: string; errors?: Record<string, string[]> } | undefined
        const message = payload?.message || payload?.title || 'Error saving the order'
        const detail = payload?.detail
        alert(`${message}${status ? ` (${status})` : ''}${detail ? `\n${detail}` : ''}`)
      } else {
        alert('Error saving the order')
      }
    }
  }

  const statusClass = (status: string) => {
    if (status?.toLowerCase() === 'approved') return 'pill pill-success'
    if (status?.toLowerCase() === 'invoiced') return 'pill pill-warning'
    return 'pill'
  }

  const getOrder = async (id: number) => {
    const response = await axios.get(`/api/salesorders/${id}`)
    return response.data as SalesOrder
  }

  const handleView = async (id: number) => {
    try {
      const order = await getOrder(id)
      setSelectedOrder(order)
      setShowDetailModal(true)
    } catch (error) {
      console.error('Error loading order:', error)
      alert('Error loading order details')
    }
  }

  const handleEditStatus = async (order: SalesOrder) => {
    const nextStatus = window.prompt('Set status (Pending, Approved, Invoiced):', order.status || 'Pending')
    if (!nextStatus) return

    try {
      await axios.put(`/api/salesorders/${order.id}/status`, JSON.stringify(nextStatus), {
        headers: { 'Content-Type': 'application/json' }
      })
      fetchOrders()
    } catch (error) {
      console.error('Error updating status:', error)
      alert('Error updating order status')
    }
  }

  const handleCreateInvoice = async (order: SalesOrder) => {
    if (order.status?.toLowerCase() === 'invoiced') {
      alert('This order is already invoiced.')
      return
    }

    try {
      setWorkingOrderId(order.id)
      await axios.post(`/api/invoices/from-salesorder/${order.id}`)
      await fetchOrders()
      alert('Invoice created successfully. You can review it in Invoices.')
    } catch (error) {
      console.error('Error creating invoice:', error)
      if (axios.isAxiosError(error)) {
        const status = error.response?.status
        const payload = error.response?.data as { message?: string; detail?: string; title?: string } | undefined
        const message = payload?.message || payload?.title || 'Error creating invoice'
        const detail = payload?.detail
        alert(`${message}${status ? ` (${status})` : ''}${detail ? `\n${detail}` : ''}`)
      } else {
        alert('Error creating invoice')
      }
    } finally {
      setWorkingOrderId(null)
    }
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Sales Orders</h2>
          <div className="page-subtitle">Enterprise Resource Planning</div>
        </div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={16} style={{ marginRight: '8px' }} />
          Create New Sale
        </button>
      </div>

      <div className="page-toolbar">
        <input className="search-input" placeholder="Search orders..." />
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
              <th>Customer</th>
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
                <td>{order.customer?.name || '-'}</td>
                <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                <td>
                  <span className={statusClass(order.status)}>
                    {order.status || 'Pending'}
                  </span>
                </td>
                <td>${order.total.toFixed(2)}</td>
                <td>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => handleView(order.id)}>View</button>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => handleEditStatus(order)}>Edit</button>
                  <button
                    className="btn btn-primary"
                    onClick={() => handleCreateInvoice(order)}
                    disabled={workingOrderId === order.id || order.status?.toLowerCase() === 'invoiced'}
                  >
                    {workingOrderId === order.id ? 'Creating...' : 'Invoice'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" style={{ maxWidth: '700px' }} onClick={e => e.stopPropagation()}>
            <h3>New Sales Order</h3>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Order Number</label>
                <input type="text" className="form-control" value="Auto-generated" readOnly />
              </div>

              <div className="form-group">
                <label>Customer</label>
                <select className="form-control" value={formData.customerId} onChange={e => setFormData({ ...formData, customerId: e.target.value })} required>
                  <option value="">Select a customer</option>
                  {customers.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
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
                    <label>Price</label>
                    <input type="number" step="0.01" className="form-control" value={line.unitPrice} onChange={e => updateLine(index, 'unitPrice', e.target.value)} required />
                  </div>
                  <div style={{ flex: 1 }}>
                    <label>Discount</label>
                    <input type="number" step="0.01" className="form-control" value={line.discount} onChange={e => updateLine(index, 'discount', e.target.value)} />
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

      {showDetailModal && selectedOrder && (
        <div className="modal-overlay" onClick={() => setShowDetailModal(false)}>
          <div className="modal" style={{ maxWidth: '900px' }} onClick={e => e.stopPropagation()}>
            <h3>Sales Order {selectedOrder.orderNumber}</h3>
            <div style={{ marginBottom: '16px' }}>
              <strong>Customer:</strong> {selectedOrder.customer?.name || selectedOrder.customerId}
              <br />
              <strong>Date:</strong> {new Date(selectedOrder.orderDate).toLocaleString()}
              <br />
              <strong>Status:</strong> {selectedOrder.status}
            </div>
            <table className="table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Unit Price</th>
                  <th>Discount</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                {selectedOrder.lines.map((line, idx) => (
                  <tr key={line.id ?? idx}>
                    <td>{line.product?.name || line.productId}</td>
                    <td>{line.quantity}</td>
                    <td>${line.unitPrice.toFixed(2)}</td>
                    <td>${line.discount.toFixed(2)}</td>
                    <td>${line.total.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div style={{ textAlign: 'right', marginTop: '10px' }}>
              <div>Subtotal: ${selectedOrder.subtotal.toFixed(2)}</div>
              <div>Tax: ${selectedOrder.tax.toFixed(2)}</div>
              <div style={{ fontWeight: 'bold', fontSize: '18px' }}>Total: ${selectedOrder.total.toFixed(2)}</div>
            </div>
            <div className="modal-actions">
              <button type="button" className="btn" style={{ background: '#95a5a6', color: 'white' }} onClick={() => setShowDetailModal(false)}>
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default SalesOrders
