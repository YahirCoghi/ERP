import { useState } from 'react'
import axios from 'axios'

function Signup() {
  const [form, setForm] = useState({
    companyName: '',
    email: '',
    subdomain: '',
    trialDays: 14,
    maxUsers: 5,
    planCode: ''
  })
  const [result, setResult] = useState<any>(null)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      const response = await axios.post('/api/signup', {
        companyName: form.companyName,
        email: form.email,
        subdomain: form.subdomain || null,
        trialDays: Number(form.trialDays),
        maxUsers: Number(form.maxUsers),
        planCode: form.planCode || null
      })
      setResult(response.data)
    } catch (err: any) {
      setError(err?.response?.data ?? 'Error al registrar')
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Registro de Empresa</h2>
      </div>

      <div className="card" style={{ marginBottom: '20px' }}>
        <form onSubmit={handleSubmit} style={{ display: 'grid', gap: '10px' }}>
          <div className="form-group">
            <label>Empresa</label>
            <input className="form-control" value={form.companyName} onChange={e => setForm({ ...form, companyName: e.target.value })} required />
          </div>
          <div className="form-group">
            <label>Email dueño</label>
            <input className="form-control" type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} required />
          </div>
          <div className="form-group">
            <label>Subdominio (opcional)</label>
            <input className="form-control" value={form.subdomain} onChange={e => setForm({ ...form, subdomain: e.target.value })} />
          </div>
          <div className="form-group">
            <label>Días de prueba</label>
            <input type="number" className="form-control" value={form.trialDays} onChange={e => setForm({ ...form, trialDays: parseInt(e.target.value) })} />
          </div>
          <div className="form-group">
            <label>Usuarios máximos</label>
            <input type="number" className="form-control" value={form.maxUsers} onChange={e => setForm({ ...form, maxUsers: parseInt(e.target.value) })} />
          </div>
          <div className="form-group">
            <label>Plan</label>
            <input className="form-control" value={form.planCode} onChange={e => setForm({ ...form, planCode: e.target.value })} />
          </div>
          <button className="btn btn-success" type="submit">Crear tenant</button>
        </form>
      </div>

      {error && (
        <div className="card" style={{ background: '#f8d7da', color: '#842029' }}>
          {typeof error === 'string' ? error : JSON.stringify(error)}
        </div>
      )}

      {result && (
        <div className="card" style={{ background: '#e8f5e9' }}>
          <div><strong>Tenant ID:</strong> {result.tenant?.id}</div>
          <div><strong>BD:</strong> {result.tenant?.databaseName}</div>
          <div><strong>Licencia:</strong> {result.license?.licenseKey}</div>
        </div>
      )}
    </div>
  )
}

export default Signup
