import { useEffect, useState } from 'react'
import axios from 'axios'
import { Plus, SlidersHorizontal } from 'lucide-react'
import { useLocation, useNavigate } from 'react-router-dom'

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
  unitPrice: number
  discount: number
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
  const location = useLocation()
  const navigate = useNavigate()
  const [orders, setOrders] = useState<PurchaseOrder[]>([])
  const [suppliers, setSuppliers] = useState<Supplier[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [taxCodes, setTaxCodes] = useState<TaxCode[]>([])
  const [settings, setSettings] = useState<PurchasingSettings>({ defaultTaxRate: 13 })
  const [showModal, setShowModal] = useState(false)
  const [showDetailModal, setShowDetailModal] = useState(false)
  const [selectedOrder, setSelectedOrder] = useState<PurchaseOrder | null>(null)
  const [showReceiveModal, setShowReceiveModal] = useState(false)
  const [receiveOrder, setReceiveOrder] = useState<PurchaseOrder | null>(null)
  const [receiveLines, setReceiveLines] = useState<Array<{
    productId: number
    productName: string
    orderedQty: number
    quantity: string
    unitCost: string
  }>>([])
  const [receiveIsFinal, setReceiveIsFinal] = useState(true)
  const [receiving, setReceiving] = useState(false)
  const [workingOrderId, setWorkingOrderId] = useState<number | null>(null)
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

  useEffect(() => {
    const params = new URLSearchParams(location.search)
    if (params.get('create') === '1') {
      setShowModal(true)
      params.delete('create')
      const next = params.toString()
      navigate(`${location.pathname}${next ? `?${next}` : ''}`, { replace: true })
    }
  }, [location.pathname, location.search, navigate])

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
          unitPrice: parseFloat(line.unitCost),
          discount: 0,
          total: parseFloat(line.quantity) * parseFloat(line.unitCost)
        }))
      }

      await axios.post('/api/purchaseorders', data)

      setShowModal(false)
      setFormData({ supplierId: '', taxCode: '', lines: [] })
      fetchOrders()
    } catch (error) {
      console.error('Error saving order:', error)
      if (axios.isAxiosError(error)) {
        const status = error.response?.status
        const payload = error.response?.data as { message?: string; detail?: string; title?: string } | undefined
        const message = payload?.message || payload?.title || 'Error saving the order'
        const detail = payload?.detail
        alert(`${message}${status ? ` (${status})` : ''}${detail ? `\n${detail}` : ''}`)
      } else {
        alert('Error saving the order')
      }
    }
  }

  const statusClass = (status: string) => {
    if (status?.toLowerCase() === 'received') return 'pill pill-success'
    if (status?.toLowerCase() === 'billed') return 'pill pill-success'
    if (status?.toLowerCase() === 'partiallyreceived') return 'pill pill-warning'
    if (status?.toLowerCase() === 'approved') return 'pill pill-warning'
    return 'pill'
  }

  const getOrder = async (id: number) => {
    const response = await axios.get(`/api/purchaseorders/${id}`)
    return response.data as PurchaseOrder
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

  const handleEditStatus = async (order: PurchaseOrder) => {
    const nextStatus = window.prompt('Set status (Pending, Approved, Received, Billed):', order.status || 'Pending')
    if (!nextStatus) return

    try {
      await axios.put(`/api/purchaseorders/${order.id}/status`, JSON.stringify(nextStatus), {
        headers: { 'Content-Type': 'application/json' }
      })
      await fetchOrders()
    } catch (error) {
      console.error('Error updating order status:', error)
      alert('Error updating order status')
    }
  }

  const handleCreateBill = async (order: PurchaseOrder) => {
    if (order.status?.toLowerCase() === 'billed') {
      alert('This purchase order is already billed.')
      return
    }

    try {
      setWorkingOrderId(order.id)
      await axios.post(`/api/purchaseinvoices/from-purchaseorder/${order.id}`)
      await fetchOrders()
      alert('Purchase invoice created successfully.')
    } catch (error) {
      console.error('Error creating purchase invoice:', error)
      if (axios.isAxiosError(error)) {
        const status = error.response?.status
        const payload = error.response?.data as { message?: string; detail?: string; title?: string } | undefined
        const message = payload?.message || payload?.title || 'Error creating purchase invoice'
        const detail = payload?.detail
        alert(`${message}${status ? ` (${status})` : ''}${detail ? `\n${detail}` : ''}`)
      } else {
        alert('Error creating purchase invoice')
      }
    } finally {
      setWorkingOrderId(null)
    }
  }

  const handleOpenReceive = async (order: PurchaseOrder) => {
    try {
      const fullOrder = await getOrder(order.id)
      setReceiveOrder(fullOrder)
      setReceiveLines(
        fullOrder.lines.map(line => ({
          productId: line.productId,
          productName: line.product?.name || `Product #${line.productId}`,
          orderedQty: line.quantity,
          quantity: line.quantity.toString(),
          unitCost: line.unitPrice.toString()
        }))
      )
      setReceiveIsFinal(true)
      setShowReceiveModal(true)
    } catch (error) {
      console.error('Error loading order for receipt:', error)
      alert('Error loading order for receipt')
    }
  }

  const updateReceiveLine = (index: number, field: 'quantity' | 'unitCost', value: string) => {
    const next = [...receiveLines]
    next[index] = { ...next[index], [field]: value }
    setReceiveLines(next)
  }

  const submitReceive = async () => {
    if (!receiveOrder) return

    const lines = receiveLines
      .map(line => ({
        productId: line.productId,
        quantity: Number(line.quantity),
        unitCost: Number(line.unitCost),
        orderedQty: line.orderedQty
      }))
      .filter(line => line.quantity > 0)

    if (lines.length === 0) {
      alert('Enter at least one received line quantity.')
      return
    }

    const invalid = lines.find(line => !Number.isFinite(line.unitCost) || line.unitCost <= 0 || line.quantity > line.orderedQty)
    if (invalid) {
      alert('Validate quantities and unit costs. Quantity cannot exceed ordered quantity.')
      return
    }

    try {
      setReceiving(true)
      await axios.post(`/api/purchaseorders/${receiveOrder.id}/receive`, {
        isFinal: receiveIsFinal,
        lines: lines.map(l => ({
          productId: l.productId,
          quantity: l.quantity,
          unitCost: l.unitCost
        }))
      })
      setShowReceiveModal(false)
      setReceiveOrder(null)
      setReceiveLines([])
      await fetchOrders()
      alert('Receipt posted and inventory updated.')
    } catch (error) {
      console.error('Error posting receipt:', error)
      if (axios.isAxiosError(error)) {
        const status = error.response?.status
        const payload = error.response?.data as { message?: string; detail?: string; title?: string } | undefined
        const message = payload?.message || payload?.title || 'Error posting receipt'
        const detail = payload?.detail
        alert(`${message}${status ? ` (${status})` : ''}${detail ? `\n${detail}` : ''}`)
      } else {
        alert('Error posting receipt')
      }
    } finally {
      setReceiving(false)
    }
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
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => handleView(order.id)}>View</button>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => handleEditStatus(order)}>Edit</button>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => handleOpenReceive(order)}>
                    Receive
                  </button>
                  <button
                    className="btn btn-primary"
                    onClick={() => handleCreateBill(order)}
                    disabled={workingOrderId === order.id || order.status?.toLowerCase() === 'billed'}
                  >
                    {workingOrderId === order.id ? 'Creating...' : 'Bill'}
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

      {showDetailModal && selectedOrder && (
        <div className="modal-overlay" onClick={() => setShowDetailModal(false)}>
          <div className="modal" style={{ maxWidth: '900px' }} onClick={e => e.stopPropagation()}>
            <h3>Purchase Order {selectedOrder.orderNumber}</h3>
            <div style={{ marginBottom: '16px' }}>
              <strong>Supplier:</strong> {selectedOrder.supplier?.name || selectedOrder.supplierId}
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
                  <th>Unit Cost</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                {selectedOrder.lines.map((line, idx) => (
                  <tr key={line.id ?? idx}>
                    <td>{line.product?.name || line.productId}</td>
                    <td>{line.quantity}</td>
                    <td>${line.unitPrice.toFixed(2)}</td>
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
              <button type="button" className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => handleOpenReceive(selectedOrder)}>
                Receive
              </button>
              <button type="button" className="btn btn-primary" style={{ marginRight: '8px' }} onClick={() => handleCreateBill(selectedOrder)}>
                Register Bill
              </button>
              <button type="button" className="btn" style={{ background: '#95a5a6', color: 'white' }} onClick={() => setShowDetailModal(false)}>
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {showReceiveModal && receiveOrder && (
        <div className="modal-overlay" onClick={() => setShowReceiveModal(false)}>
          <div className="modal" style={{ maxWidth: '900px' }} onClick={e => e.stopPropagation()}>
            <h3>Receive Purchase Order {receiveOrder.orderNumber}</h3>
            <div style={{ marginBottom: '12px' }}>
              <strong>Supplier:</strong> {receiveOrder.supplier?.name || receiveOrder.supplierId}
            </div>
            <table className="table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Ordered Qty</th>
                  <th>Receive Qty</th>
                  <th>Unit Cost</th>
                </tr>
              </thead>
              <tbody>
                {receiveLines.map((line, idx) => (
                  <tr key={`${line.productId}-${idx}`}>
                    <td>{line.productName}</td>
                    <td>{line.orderedQty}</td>
                    <td>
                      <input
                        type="number"
                        min="0"
                        max={line.orderedQty}
                        step="0.0001"
                        className="form-control"
                        value={line.quantity}
                        onChange={e => updateReceiveLine(idx, 'quantity', e.target.value)}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min="0"
                        step="0.01"
                        className="form-control"
                        value={line.unitCost}
                        onChange={e => updateReceiveLine(idx, 'unitCost', e.target.value)}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="form-group" style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
              <input
                id="receive-final"
                type="checkbox"
                checked={receiveIsFinal}
                onChange={e => setReceiveIsFinal(e.target.checked)}
              />
              <label htmlFor="receive-final" style={{ marginBottom: 0 }}>
                Mark order as fully received
              </label>
            </div>
            <div className="modal-actions">
              <button
                type="button"
                className="btn"
                style={{ background: '#95a5a6', color: 'white' }}
                onClick={() => setShowReceiveModal(false)}
                disabled={receiving}
              >
                Cancel
              </button>
              <button type="button" className="btn btn-success" onClick={submitReceive} disabled={receiving}>
                {receiving ? 'Posting...' : 'Post Receipt'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default PurchaseOrders
