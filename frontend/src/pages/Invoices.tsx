import { useEffect, useState } from 'react'
import axios from 'axios'

interface Customer {
  id: number
  name: string
}

interface Product {
  id: number
  name: string
}

interface InvoiceLine {
  id: number
  productId: number
  product?: Product
  quantity: number
  unitPrice: number
  discount: number
  taxRate: number
  taxAmount: number
  total: number
}

interface Invoice {
  id: number
  invoiceNumber: string
  invoiceDate: string
  dueDate?: string
  customerId: number
  customer?: Customer
  salesOrderId?: number
  subtotal: number
  tax: number
  total: number
  status: string
  lines: InvoiceLine[]
  referenceReason?: string
}

function Invoices() {
  const [invoices, setInvoices] = useState<Invoice[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null)
  const [showModal, setShowModal] = useState(false)
  const [showCreditNoteModal, setShowCreditNoteModal] = useState(false)
  const [creditNoteTarget, setCreditNoteTarget] = useState<Invoice | null>(null)
  const [creditReason, setCreditReason] = useState('')
  const [creditCode, setCreditCode] = useState('01')
  const [creditRestock, setCreditRestock] = useState(false)
  const [creditLoading, setCreditLoading] = useState(false)

  useEffect(() => {
    fetchInvoices()
  }, [])

  const fetchInvoices = async () => {
    try {
      const response = await axios.get('/api/invoices')
      setInvoices(response.data)
    } catch (error) {
      console.error('Error loading invoices:', error)
      alert('Error loading invoices')
    } finally {
      setLoading(false)
    }
  }

  const viewInvoice = async (id: number) => {
    try {
      const response = await axios.get(`/api/invoices/${id}`)
      setSelectedInvoice(response.data)
      setShowModal(true)
    } catch (error) {
      console.error('Error loading invoice:', error)
      alert('Error loading invoice')
    }
  }

  const updateStatus = async (invoice: Invoice) => {
    const nextStatus = window.prompt('Set status (Pending, Issued, Paid, Cancelled):', invoice.status || 'Pending')
    if (!nextStatus) return
    try {
      await axios.put(`/api/invoices/${invoice.id}/status`, JSON.stringify(nextStatus), {
        headers: { 'Content-Type': 'application/json' }
      })
      await fetchInvoices()
    } catch (error) {
      console.error('Error updating invoice status:', error)
      alert('Error updating invoice status')
    }
  }

  const openCreditNoteModal = (invoice: Invoice) => {
    if (invoice.invoiceNumber?.toUpperCase().startsWith('NC-')) {
      alert('Cannot create a credit note from another credit note.')
      return
    }
    setCreditNoteTarget(invoice)
    setCreditReason(`Credit note for invoice ${invoice.invoiceNumber}`)
    setCreditCode('01')
    setCreditRestock(false)
    setShowCreditNoteModal(true)
  }

  const createCreditNote = async () => {
    if (!creditNoteTarget) return
    if (!creditReason.trim()) {
      alert('Reason is required.')
      return
    }

    try {
      setCreditLoading(true)
      await axios.post(`/api/invoices/${creditNoteTarget.id}/credit-note`, {
        reason: creditReason.trim(),
        code: creditCode,
        restock: creditRestock
      })

      setShowCreditNoteModal(false)
      setCreditNoteTarget(null)
      setCreditReason('')
      await fetchInvoices()
      alert('Credit note created successfully.')
    } catch (error) {
      console.error('Error creating credit note:', error)
      if (axios.isAxiosError(error)) {
        const status = error.response?.status
        const payload = error.response?.data as { message?: string; detail?: string; title?: string } | undefined
        const message = payload?.message || payload?.title || 'Error creating credit note'
        const detail = payload?.detail
        alert(`${message}${status ? ` (${status})` : ''}${detail ? `\n${detail}` : ''}`)
      } else {
        alert('Error creating credit note')
      }
    } finally {
      setCreditLoading(false)
    }
  }

  const getStatusBadge = (status: string) => {
    const normalized = status?.toLowerCase()
    if (normalized === 'paid') return <span className="pill pill-success">{status}</span>
    if (normalized === 'issued' || normalized === 'invoiced') return <span className="pill pill-warning">{status}</span>
    if (normalized === 'cancelled') return <span className="pill">{status}</span>
    return <span className="pill">{status || 'Pending'}</span>
  }

  if (loading) return <div>Loading invoices...</div>

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Invoices</h2>
          <div className="page-subtitle">Enterprise Resource Planning</div>
        </div>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Invoice Number</th>
              <th>Customer</th>
              <th>Issue Date</th>
              <th>Due Date</th>
              <th>Total</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {invoices.map(invoice => (
              <tr key={invoice.id}>
                <td>{invoice.invoiceNumber}</td>
                <td>{invoice.customer?.name || `Customer #${invoice.customerId}`}</td>
                <td>{new Date(invoice.invoiceDate).toLocaleDateString()}</td>
                <td>{invoice.dueDate ? new Date(invoice.dueDate).toLocaleDateString() : '-'}</td>
                <td>${invoice.total.toFixed(2)}</td>
                <td>{getStatusBadge(invoice.status)}</td>
                <td>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => viewInvoice(invoice.id)}>
                    View
                  </button>
                  <button className="btn btn-outline" style={{ marginRight: '8px' }} onClick={() => updateStatus(invoice)}>
                    Edit
                  </button>
                  <button className="btn btn-primary" onClick={() => openCreditNoteModal(invoice)}>
                    Credit Note
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
            <h3>Invoice {selectedInvoice.invoiceNumber}</h3>
            <div style={{ marginBottom: '16px' }}>
              <strong>Customer:</strong> {selectedInvoice.customer?.name || selectedInvoice.customerId}
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
                  <th>Unit Price</th>
                  <th>Discount</th>
                  <th>Tax</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                {selectedInvoice.lines.map((line, idx) => (
                  <tr key={line.id ?? idx}>
                    <td>{line.product?.name || line.productId}</td>
                    <td>{line.quantity}</td>
                    <td>${line.unitPrice.toFixed(2)}</td>
                    <td>${line.discount.toFixed(2)}</td>
                    <td>${line.taxAmount.toFixed(2)}</td>
                    <td>${line.total.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div style={{ textAlign: 'right', marginTop: '10px' }}>
              <div>Subtotal: ${selectedInvoice.subtotal.toFixed(2)}</div>
              <div>Tax: ${selectedInvoice.tax.toFixed(2)}</div>
              <div style={{ fontWeight: 'bold', fontSize: '18px' }}>Total: ${selectedInvoice.total.toFixed(2)}</div>
            </div>
            {selectedInvoice.referenceReason && (
              <div style={{ marginTop: '12px' }}>
                <strong>Reference reason:</strong> {selectedInvoice.referenceReason}
              </div>
            )}
            <div className="modal-actions">
              <button type="button" className="btn btn-primary" style={{ marginRight: '8px' }} onClick={() => openCreditNoteModal(selectedInvoice)}>
                Create Credit Note
              </button>
              <button type="button" className="btn" style={{ background: '#95a5a6', color: 'white' }} onClick={() => setShowModal(false)}>
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {showCreditNoteModal && creditNoteTarget && (
        <div className="modal-overlay" onClick={() => setShowCreditNoteModal(false)}>
          <div className="modal" style={{ maxWidth: '650px' }} onClick={e => e.stopPropagation()}>
            <h3>Create Credit Note</h3>
            <div className="form-group">
              <label>Original invoice</label>
              <input className="form-control" value={creditNoteTarget.invoiceNumber} readOnly />
            </div>
            <div className="form-group">
              <label>Reference code</label>
              <select className="form-control" value={creditCode} onChange={e => setCreditCode(e.target.value)}>
                <option value="01">01 - Total cancellation</option>
                <option value="02">02 - Partial cancellation</option>
                <option value="03">03 - Amount correction</option>
                <option value="04">04 - Description correction</option>
                <option value="05">05 - Return of goods</option>
              </select>
            </div>
            <div className="form-group">
              <label>Reason</label>
              <textarea
                className="form-control"
                value={creditReason}
                onChange={e => setCreditReason(e.target.value)}
                rows={4}
              />
            </div>
            <div className="form-group" style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
              <input
                id="restock-checkbox"
                type="checkbox"
                checked={creditRestock}
                onChange={e => setCreditRestock(e.target.checked)}
              />
              <label htmlFor="restock-checkbox" style={{ marginBottom: 0 }}>Return quantities to inventory</label>
            </div>
            <div className="modal-actions">
              <button
                type="button"
                className="btn"
                style={{ background: '#95a5a6', color: 'white' }}
                onClick={() => setShowCreditNoteModal(false)}
                disabled={creditLoading}
              >
                Cancel
              </button>
              <button type="button" className="btn btn-success" onClick={createCreditNote} disabled={creditLoading}>
                {creditLoading ? 'Creating...' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default Invoices
