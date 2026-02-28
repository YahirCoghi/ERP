import { useEffect, useState } from 'react'
import axios from 'axios'

interface PaymentTerm {
  id?: number
  code: string
  name: string
  days: number
  appliesTo: 'Sales' | 'Purchasing' | 'Both'
}

function PaymentTerms() {
  const [items, setItems] = useState<PaymentTerm[]>([])
  const [form, setForm] = useState<PaymentTerm>({
    code: '',
    name: '',
    days: 0,
    appliesTo: 'Both'
  })

  const fetchItems = async () => {
    const response = await axios.get('/api/paymentterms')
    setItems(response.data)
  }

  useEffect(() => {
    fetchItems()
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await axios.post('/api/paymentterms', form)
    setForm({ code: '', name: '', days: 0, appliesTo: 'Both' })
    fetchItems()
  }

  return (
    <div>
      <div className="page-header">
        <h2>Plazos de Pago</h2>
      </div>

      <div className="card" style={{ marginBottom: '20px' }}>
        <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '10px' }}>
          <div className="form-group">
            <label>Código</label>
            <input className="form-control" value={form.code} onChange={e => setForm({ ...form, code: e.target.value })} required />
          </div>
          <div className="form-group">
            <label>Nombre</label>
            <input className="form-control" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
          </div>
          <div className="form-group">
            <label>Días</label>
            <input type="number" className="form-control" value={form.days} onChange={e => setForm({ ...form, days: parseInt(e.target.value) })} />
          </div>
          <div className="form-group">
            <label>Aplica a</label>
            <select className="form-control" value={form.appliesTo} onChange={e => setForm({ ...form, appliesTo: e.target.value as PaymentTerm['appliesTo'] })}>
              <option value="Both">Ventas y Compras</option>
              <option value="Sales">Ventas</option>
              <option value="Purchasing">Compras</option>
            </select>
          </div>
          <button className="btn btn-success" type="submit">Guardar</button>
        </form>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Código</th>
              <th>Nombre</th>
              <th>Días</th>
              <th>Aplica</th>
            </tr>
          </thead>
          <tbody>
            {items.map(i => (
              <tr key={i.id ?? i.code}>
                <td>{i.code}</td>
                <td>{i.name}</td>
                <td>{i.days}</td>
                <td>{i.appliesTo}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default PaymentTerms
