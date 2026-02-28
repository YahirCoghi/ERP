import { useEffect, useState } from 'react'
import axios from 'axios'

interface TaxCode {
  id?: number
  code: string
  name: string
  rate: number
  isExempt: boolean
}

function TaxCodes() {
  const [items, setItems] = useState<TaxCode[]>([])
  const [form, setForm] = useState<TaxCode>({
    code: '',
    name: '',
    rate: 13,
    isExempt: false
  })

  const fetchItems = async () => {
    const response = await axios.get('/api/taxcodes')
    setItems(response.data)
  }

  useEffect(() => {
    fetchItems()
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await axios.post('/api/taxcodes', form)
    setForm({ code: '', name: '', rate: 13, isExempt: false })
    fetchItems()
  }

  return (
    <div>
      <div className="page-header">
        <h2>Impuestos</h2>
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
            <label>Tarifa (%)</label>
            <input type="number" step="0.01" className="form-control" value={form.rate} onChange={e => setForm({ ...form, rate: parseFloat(e.target.value) })} />
          </div>
          <label style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <input type="checkbox" checked={form.isExempt} onChange={e => setForm({ ...form, isExempt: e.target.checked })} />
            Exento
          </label>
          <button className="btn btn-success" type="submit">Guardar</button>
        </form>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Código</th>
              <th>Nombre</th>
              <th>Tarifa</th>
              <th>Exento</th>
            </tr>
          </thead>
          <tbody>
            {items.map(i => (
              <tr key={i.id ?? i.code}>
                <td>{i.code}</td>
                <td>{i.name}</td>
                <td>{i.rate.toFixed(2)}%</td>
                <td>{i.isExempt ? 'Sí' : 'No'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default TaxCodes
