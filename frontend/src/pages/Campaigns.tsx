import { useEffect, useState } from 'react'
import axios from 'axios'

type Campaign = {
  id: number
  name: string
  segmentCriteriaJson: string
  scheduledAt?: string
  status: string
}

function Campaigns() {
  const [rows, setRows] = useState<Campaign[]>([])
  const [error, setError] = useState('')
  const [form, setForm] = useState({ name: '', segmentCriteriaJson: '{}', status: 'Draft' })

  const load = async () => {
    const response = await axios.get('/api/campaigns')
    setRows(response.data || [])
  }

  useEffect(() => {
    load().catch(() => setError('No se pudieron cargar las campanas.'))
  }, [])

  const create = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      await axios.post('/api/campaigns', { id: 0, ...form, scheduledAt: null })
      setForm({ name: '', segmentCriteriaJson: '{}', status: 'Draft' })
      await load()
    } catch {
      setError('No se pudo crear la campana.')
    }
  }

  const segment = async (id: number) => {
    try {
      await axios.post(`/api/campaigns/${id}/wizard/segment`)
      await load()
    } catch {
      setError('No se pudo segmentar la campana.')
    }
  }

  const execute = async (id: number) => {
    try {
      await axios.post(`/api/campaigns/${id}/run`)
      await load()
    } catch {
      setError('No se pudo ejecutar la campana.')
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Campaign Manager</h2>
      </div>
      {error && <div className="alert">{error}</div>}
      <div className="card">
        <form className="page-toolbar" onSubmit={create}>
          <input className="form-control" placeholder="Nombre de campana" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
          <input className="form-control" placeholder="Segmento (json)" value={form.segmentCriteriaJson} onChange={e => setForm({ ...form, segmentCriteriaJson: e.target.value })} />
          <button className="btn btn-primary" type="submit">Crear campana</button>
        </form>
      </div>
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Estado</th>
              <th>Programada</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(row => (
              <tr key={row.id}>
                <td>{row.name}</td>
                <td>{row.status}</td>
                <td>{row.scheduledAt ? new Date(row.scheduledAt).toLocaleString() : '-'}</td>
                <td>
                  <button className="btn btn-outline" onClick={() => segment(row.id)}>Segmentar</button>
                  <button className="btn btn-primary" style={{ marginLeft: 8 }} onClick={() => execute(row.id)}>Ejecutar</button>
                </td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={4}>Sin campanas.</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default Campaigns
