import { useEffect, useState } from 'react'
import axios from 'axios'

type Assignment = {
  id: number
  licenseCode: string
  scope: string
  userId?: number
  enabled: boolean
  expiresAt?: string
}

function LicenseManagement() {
  const [rows, setRows] = useState<Assignment[]>([])
  const [error, setError] = useState('')
  const [form, setForm] = useState({ licenseCode: '', scope: 'Tenant', userId: '', enabled: true, expiresAt: '' })

  const load = async () => {
    const response = await axios.get('/api/licensemanagement')
    setRows(response.data || [])
  }

  useEffect(() => {
    load().catch(() => setError('No se pudieron cargar licencias.'))
  }, [])

  const create = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await axios.post('/api/licensemanagement', {
        id: 0,
        licenseCode: form.licenseCode,
        scope: form.scope,
        userId: form.userId ? Number(form.userId) : null,
        enabled: form.enabled,
        expiresAt: form.expiresAt || null
      })
      setForm({ licenseCode: '', scope: 'Tenant', userId: '', enabled: true, expiresAt: '' })
      await load()
    } catch {
      setError('No se pudo guardar la licencia.')
    }
  }

  return (
    <div>
      <div className="page-header"><h2>Gestion de Licencias</h2></div>
      {error && <div className="alert">{error}</div>}
      <div className="card">
        <form className="page-toolbar" onSubmit={create}>
          <input className="form-control" placeholder="Codigo licencia" value={form.licenseCode} onChange={e => setForm({ ...form, licenseCode: e.target.value })} required />
          <select className="form-control" value={form.scope} onChange={e => setForm({ ...form, scope: e.target.value })}>
            <option value="Tenant">Tenant</option>
            <option value="User">User</option>
          </select>
          <input className="form-control" placeholder="UserId (opcional)" value={form.userId} onChange={e => setForm({ ...form, userId: e.target.value })} />
          <input className="form-control" type="date" value={form.expiresAt} onChange={e => setForm({ ...form, expiresAt: e.target.value })} />
          <button className="btn btn-primary" type="submit">Asignar</button>
        </form>
      </div>
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Codigo</th>
              <th>Scope</th>
              <th>UserId</th>
              <th>Estado</th>
              <th>Expira</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(row => (
              <tr key={row.id}>
                <td>{row.licenseCode}</td>
                <td>{row.scope}</td>
                <td>{row.userId || '-'}</td>
                <td>{row.enabled ? 'Activa' : 'Inactiva'}</td>
                <td>{row.expiresAt ? new Date(row.expiresAt).toLocaleDateString() : '-'}</td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={5}>Sin licencias.</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default LicenseManagement
