import { useEffect, useState } from 'react'
import axios from 'axios'

interface SaleCondition {
  id?: number
  code: string
  name: string
}

function SaleConditions() {
  const [items, setItems] = useState<SaleCondition[]>([])
  const [form, setForm] = useState<SaleCondition>({ code: '', name: '' })

  const fetchItems = async () => {
    const response = await axios.get('/api/saleconditions')
    setItems(response.data)
  }

  useEffect(() => {
    fetchItems()
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await axios.post('/api/saleconditions', form)
    setForm({ code: '', name: '' })
    fetchItems()
  }

  return (
    <div>
      <div className="page-header">
        <h2>Condiciones de Venta</h2>
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
          <button className="btn btn-success" type="submit">Guardar</button>
        </form>
      </div>

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Código</th>
              <th>Nombre</th>
            </tr>
          </thead>
          <tbody>
            {items.map(i => (
              <tr key={i.id ?? i.code}>
                <td>{i.code}</td>
                <td>{i.name}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default SaleConditions
