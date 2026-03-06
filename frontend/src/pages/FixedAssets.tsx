import { useEffect, useState } from 'react'
import axios from 'axios'

type Asset = {
  id: number
  assetCode: string
  name: string
  acquisitionDate: string
  acquisitionCost: number
  residualValue: number
  usefulLifeMonths: number
  status: string
}

function FixedAssets() {
  const [rows, setRows] = useState<Asset[]>([])
  const [error, setError] = useState('')
  const [form, setForm] = useState({
    assetCode: '',
    name: '',
    acquisitionDate: new Date().toISOString().slice(0, 10),
    acquisitionCost: 0,
    residualValue: 0,
    usefulLifeMonths: 60
  })

  const load = async () => {
    const response = await axios.get('/api/financialadvanced/fixed-assets')
    setRows(response.data || [])
  }

  useEffect(() => {
    load().catch(() => setError('No se pudieron cargar activos fijos.'))
  }, [])

  const create = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await axios.post('/api/financialadvanced/fixed-assets', {
        id: 0,
        ...form,
        status: 'Active'
      })
      setForm({
        assetCode: '',
        name: '',
        acquisitionDate: new Date().toISOString().slice(0, 10),
        acquisitionCost: 0,
        residualValue: 0,
        usefulLifeMonths: 60
      })
      await load()
    } catch {
      setError('No se pudo crear el activo fijo.')
    }
  }

  const depreciate = async (id: number) => {
    try {
      await axios.post(`/api/financialadvanced/fixed-assets/${id}/depreciate`)
      alert('Depreciacion generada.')
    } catch {
      setError('No se pudo ejecutar depreciacion.')
    }
  }

  return (
    <div>
      <div className="page-header"><h2>Activos Fijos</h2></div>
      {error && <div className="alert">{error}</div>}
      <div className="card">
        <form className="page-toolbar" onSubmit={create}>
          <input className="form-control" placeholder="Codigo" value={form.assetCode} onChange={e => setForm({ ...form, assetCode: e.target.value })} required />
          <input className="form-control" placeholder="Nombre" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
          <input className="form-control" type="date" value={form.acquisitionDate} onChange={e => setForm({ ...form, acquisitionDate: e.target.value })} required />
          <input className="form-control" type="number" value={form.acquisitionCost} onChange={e => setForm({ ...form, acquisitionCost: Number(e.target.value) })} required />
          <input className="form-control" type="number" value={form.residualValue} onChange={e => setForm({ ...form, residualValue: Number(e.target.value) })} required />
          <button className="btn btn-primary" type="submit">Registrar</button>
        </form>
      </div>
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Codigo</th>
              <th>Nombre</th>
              <th>Costo</th>
              <th>Vida util</th>
              <th>Accion</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(row => (
              <tr key={row.id}>
                <td>{row.assetCode}</td>
                <td>{row.name}</td>
                <td>{row.acquisitionCost.toFixed(2)}</td>
                <td>{row.usefulLifeMonths} meses</td>
                <td><button className="btn btn-outline" onClick={() => depreciate(row.id)}>Depreciar mes</button></td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={5}>Sin activos.</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default FixedAssets
