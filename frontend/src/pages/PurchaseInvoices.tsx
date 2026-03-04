import { useEffect, useState } from 'react'
import axios from 'axios'

interface Supplier {
  id: number
  name: string
}

interface Product {
  id: number
  name: string
}

interface PurchaseInvoiceLine {
  id: number
  productId: number
  product?: Product
  quantity: number
  unitCost: number
  discount: number
  total: number
}

interface PurchaseInvoice {
  id: number
  invoiceNumber: string
  invoiceDate: string
  dueDate?: string
  supplierId: number
  supplier?: Supplier
  purchaseOrderId?: number
  subtotal: number
  tax: number
  total: number
  paidAmount: number
  balance: number
  lastPaymentDate?: string
  status: string
  lines: PurchaseInvoiceLine[]
}

function PurchaseInvoices() {
  const [invoices, setInvoices] = useState<PurchaseInvoice[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedInvoice, setSelectedInvoice] = useState<PurchaseInvoice | null>(null)
  const [showModal, setShowModal] = useState(false)
  const [showPaymentModal, setShowPaymentModal] = useState(false)
  const [paymentInvoice, setPaymentInvoice] = useState<PurchaseInvoice | null>(null)
  const [paymentAmount, setPaymentAmount] = useState('')
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().slice(0, 10))
  const [paymentLoading, setPaymentLoading] = useState(false)

  useEffect(() => {
    fetchInvoices()
  }, [])

  const fetchInvoices = async () => {
    try {
      const response = await axios.get('/api/purchaseinvoices')
      setInvoices(response.data)
    } catch (error) {
      console.error('Error loading purchase invoices:', error)
      alert('Error loading purchase invoices')
    } finally {
      setLoading(false)
    }
  }

  const viewInvoice = async (id: number) => {
    try {
      const response = await axios.get(`/api/purchaseinvoices/${id}`)
      setSelectedInvoice(response.data)
      setShowModal(true)
    } catch (error) {
      console.error('Error loading purchase invoice:', error)
      alert('Error loading purchase invoice')
    }
  }

  const updateStatus = async (invoice: PurchaseInvoice) => {
    const nextStatus = window.prompt('Set status (Issued, Pending, Paid, Cancelled):', invoice.status || 'Issued')
    if (!nextStatus) return

    try {
      await axios.put(`/api/purchaseinvoices/${invoice.id}/status`, JSON.stringify(nextStatus), {
        headers: { 'Content-Type': 'application/json' }
      })
      await fetchInvoices()
    } catch (error) {
      console.error('Error updating purchase invoice status:', error)
      alert('Error updating purchase invoice status')
    }
  }

  const statusClass = (status: string) => {
    const normalized = status?.toLowerCase()
    if (normalized === 'paid') return 'pill pill-success'
    if (normalized === 'issued') return 'pill pill-warning'
    if (normalized === 'partial') return 'pill pill-warning'
    return 'pill'
  }

  const openPaymentModal = (invoice: PurchaseInvoice) => {
    if ((invoice.balance ?? 0) <= 0) {
      alert('Invoice is already fully paid.')
      return
    }
    setPaymentInvoice(invoice)
    setPaymentAmount(invoice.balance.toFixed(2))
    setPaymentDate(new Date().toISOString().slice(0, 10))
    setShowPaymentModal(true)
  }

  const registerPayment = async () => {
    if (!paymentInvoice) return
    const amount = Number(paymentAmount)
    if (!amount || amount <= 0) {
      alert('Enter a valid payment amount.')
      return
    }

    try {
      setPaymentLoading(true)
      await axios.post(`/api/purchaseinvoices/${paymentInvoice.id}/payments`, {
        amount,
        paymentDate: paymentDate ? `${paymentDate}T00:00:00` : null
      })
      setShowPaymentModal(false)
      setPaymentInvoice(null)
      await fetchInvoices()
      alert('Payment registered successfully.')
    } catch (error) {
      console.error('Error registering payment:', error)
      if (axios.isAxiosError(error)) {
        const status = error.response?.status
        const payload = error.response?.data as { message?: string; detail?: string; title?: string } | undefined
        const message = payload?.message || payload?.title || 'Error registering payment'
        const detail = payload?.detail
        alert(`${message}${status ? ` (${status})` : ''}${detail ? `\n${detail}` : ''}`)
      } else {
        alert('Error registering payment')
      }
    } finally {
      setPaymentLoading(false)
    }
  }

  if (loading) return <div>Loading purchase invoices...</div>

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Purchase Invoices</h2>
          <div className="page-subtitle">Enterprise Resource Planning</div>
        </div>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Invoice Number</th>
              <th>Supplier</th>
              <th>Issue Date</th>
              <th>Due Date</th>
              <th>Total</th>
              <th>Paid</th>
              <th>Balance</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {invoices.map(invoice => (
              <tr key={invoice.id}>
                <td>{invoice.invoiceNumber}</td>
                <td>{invoice.supplier?.name || `Supplier #${invoice.supplierId}`}</td>
                <td>{new Date(invoice.invoiceDate).toLocaleDateString()}</td>
                <td>{invoice.dueDate ? new Date(invoice.dueDate).toLocaleDateString() : '-'}</td>
                <td>${invoice.total.toFixed(2)}</td>
                <td>${(invoice.paidAmount ?? 0).toFixed(2)}</td>
                <td>${(invoice.balance ?? invoice.total).toFixed(2)}</td>
                <td>
                  <span className={statusClass(invoice.status)}>{invoice.status || 'Issued'}</span>
                </td>
                <td>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => viewInvoice(invoice.id)}>
                    View
                  </button>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => updateStatus(invoice)}>
                    Edit
                  </button>
                  <button className="btn btn-primary" onClick={() => openPaymentModal(invoice)}>
                    Pay
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && selectedInvoice && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" style={{ maxWidth: '900px' }} onClick={e => e.stopPropagation()}>
            <h3>Purchase Invoice {selectedInvoice.invoiceNumber}</h3>
            <div style={{ marginBottom: '16px' }}>
              <strong>Supplier:</strong> {selectedInvoice.supplier?.name || selectedInvoice.supplierId}
              <br />
              <strong>Issue date:</strong> {new Date(selectedInvoice.invoiceDate).toLocaleString()}
              <br />
              <strong>Status:</strong> {selectedInvoice.status}
            </div>
            <table className="table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Unit Cost</th>
                  <th>Discount</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                {selectedInvoice.lines.map((line, idx) => (
                  <tr key={line.id ?? idx}>
                    <td>{line.product?.name || line.productId}</td>
                    <td>{line.quantity}</td>
                    <td>${line.unitCost.toFixed(2)}</td>
                    <td>${line.discount.toFixed(2)}</td>
                    <td>${line.total.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div style={{ textAlign: 'right', marginTop: '10px' }}>
              <div>Subtotal: ${selectedInvoice.subtotal.toFixed(2)}</div>
              <div>Tax: ${selectedInvoice.tax.toFixed(2)}</div>
              <div>Paid: ${(selectedInvoice.paidAmount ?? 0).toFixed(2)}</div>
              <div>Balance: ${(selectedInvoice.balance ?? selectedInvoice.total).toFixed(2)}</div>
              <div style={{ fontWeight: 'bold', fontSize: '18px' }}>Total: ${selectedInvoice.total.toFixed(2)}</div>
            </div>
            <div className="modal-actions">
              <button type="button" className="btn btn-primary" style={{ marginRight: '8px' }} onClick={() => openPaymentModal(selectedInvoice)}>
                Register Payment
              </button>
              <button type="button" className="btn" style={{ background: '#95a5a6', color: 'white' }} onClick={() => setShowModal(false)}>
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {showPaymentModal && paymentInvoice && (
        <div className="modal-overlay" onClick={() => setShowPaymentModal(false)}>
          <div className="modal" style={{ maxWidth: '560px' }} onClick={e => e.stopPropagation()}>
            <h3>Register Payment</h3>
            <div className="form-group">
              <label>Invoice</label>
              <input className="form-control" value={paymentInvoice.invoiceNumber} readOnly />
            </div>
            <div className="form-group">
              <label>Current balance</label>
              <input className="form-control" value={paymentInvoice.balance.toFixed(2)} readOnly />
            </div>
            <div className="form-group">
              <label>Amount</label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                max={paymentInvoice.balance}
                className="form-control"
                value={paymentAmount}
                onChange={e => setPaymentAmount(e.target.value)}
              />
            </div>
            <div className="form-group">
              <label>Payment date</label>
              <input
                type="date"
                className="form-control"
                value={paymentDate}
                onChange={e => setPaymentDate(e.target.value)}
              />
            </div>
            <div className="modal-actions">
              <button
                type="button"
                className="btn"
                style={{ background: '#95a5a6', color: 'white' }}
                onClick={() => setShowPaymentModal(false)}
                disabled={paymentLoading}
              >
                Cancel
              </button>
              <button type="button" className="btn btn-success" onClick={registerPayment} disabled={paymentLoading}>
                {paymentLoading ? 'Saving...' : 'Save payment'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default PurchaseInvoices
