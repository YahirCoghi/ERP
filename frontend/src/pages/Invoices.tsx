import { useState, useEffect } from 'react'
import axios from 'axios'

interface InvoiceLine {
  id: number
  productId: number
  productName: string
  quantity: number
  unitPrice: number
  discount: number
  total: number
}

interface Invoice {
  id: number
  invoiceNumber: string
  salesOrderId: number
  salesOrderNumber: string
  customerId: number
  customerName: string
  issueDate: string
  dueDate: string
  subtotal: number
  taxAmount: number
  total: number
  status: string
  notes: string
  lines: InvoiceLine[]
}

function Invoices() {
  const [invoices, setInvoices] = useState<Invoice[]>([])
  const [loading, setLoading] = useState(true)
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null)
  const [showModal, setShowModal] = useState(false)

  useEffect(() => {
    fetchInvoices()
  }, [])

  const fetchInvoices = async () => {
    try {
      const response = await axios.get('/api/invoices')
      setInvoices(response.data)
    } catch (error) {
      console.error('Error al cargar facturas:', error)
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
      console.error('Error al cargar factura:', error)
    }
  }

  const getStatusBadge = (status: string) => {
    const styles: { [key: string]: { background: string; color: string } } = {
      'Draft': { background: '#95a5a6', color: 'white' },
      'Issued': { background: '#3498db', color: 'white' },
      'Paid': { background: '#27ae60', color: 'white' },
      'Overdue': { background: '#e74c3c', color: 'white' },
      'Cancelled': { background: '#7f8c8d', color: 'white' }
    }
    const style = styles[status] || styles['Draft']
    return (
      <span style={{
        padding: '4px 8px',
        borderRadius: '4px',
        fontSize: '12px',
        fontWeight: 'bold',
        ...style
      }}>
        {status}
      </span>
    )
  }

  if (loading) return <div>Cargando...</div>

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h1>Facturas</h1>
      </div>

      <table className="table">
        <thead>
          <tr>
            <th>Número</th>
            <th>Cliente</th>
            <th>Fecha Emisión</th>
            <th>Fecha Vencimiento</th>
            <th>Total</th>
            <th>Estado</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {invoices.map(invoice => (
            <tr key={invoice.id}>
              <td>{invoice.invoiceNumber}</td>
              <td>{invoice.customerName}</td>
              <td>{new Date(invoice.issueDate).toLocaleDateString()}</td>
              <td>{new Date(invoice.dueDate).toLocaleDateString()}</td>
              <td>${invoice.total.toFixed(2)}</td>
              <td>{getStatusBadge(invoice.status)}</td>
              <td>
                <button className="btn btn-primary" onClick={() => viewInvoice(invoice.id)}>
                  Ver
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {showModal && selectedInvoice && (
        <div className="modal" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: '800px' }}>
            <div className="modal-header">
              <h2>Factura {selectedInvoice.invoiceNumber}</h2>
              <button className="close-btn" onClick={() => setShowModal(false)}>×</button>
            </div>
            
            <div style={{ marginBottom: '20px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <div>
                  <p><strong>Cliente:</strong> {selectedInvoice.customerName}</p>
                  <p><strong>Orden de Venta:</strong> {selectedInvoice.salesOrderNumber}</p>
                </div>
                <div style={{ textAlign: 'right' }}>
                  <p><strong>Fecha Emisión:</strong> {new Date(selectedInvoice.issueDate).toLocaleDateString()}</p>
                  <p><strong>Fecha Vencimiento:</strong> {new Date(selectedInvoice.dueDate).toLocaleDateString()}</p>
                  <p><strong>Estado:</strong> {getStatusBadge(selectedInvoice.status)}</p>
                </div>
              </div>
            </div>

            <table className="table">
              <thead>
                <tr>
                  <th>Producto</th>
                  <th>Cantidad</th>
                  <th>Precio Unit.</th>
                  <th>Descuento</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                {selectedInvoice.lines.map(line => (
                  <tr key={line.id}>
                    <td>{line.productName}</td>
                    <td>{line.quantity}</td>
                    <td>${line.unitPrice.toFixed(2)}</td>
                    <td>${line.discount.toFixed(2)}</td>
                    <td>${line.total.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div style={{ textAlign: 'right', marginTop: '20px' }}>
              <p><strong>Subtotal:</strong> ${selectedInvoice.subtotal.toFixed(2)}</p>
              <p><strong>IVA (16%):</strong> ${selectedInvoice.taxAmount.toFixed(2)}</p>
              <p style={{ fontSize: '18px', fontWeight: 'bold' }}>
                <strong>Total:</strong> ${selectedInvoice.total.toFixed(2)}
              </p>
            </div>

            {selectedInvoice.notes && (
              <div style={{ marginTop: '20px', padding: '10px', background: '#f8f9fa', borderRadius: '5px' }}>
                <strong>Notas:</strong> {selectedInvoice.notes}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

export default Invoices
