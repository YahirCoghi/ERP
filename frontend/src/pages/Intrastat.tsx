import { useEffect, useState } from 'react'
import axios from 'axios'

type Declaration = {
  id: number
  declarationNumber: string
  year: number
  month: number
  status: string
}

function Intrastat() {
  const [rows, setRows] = useState<Declaration[]>([])
  const [error, setError] = useState('')
  const [form, setForm] = useState({
    declarationNumber: '',
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1
  })

  const load = async () => {
    const response = await axios.get('/api/financialadvanced/intrastat')
    setRows(response.data || [])
  }

  useEffect(() => {
    load().catch(() => setError('No se pudo cargar Intrastat.'))
  }, [])

  const create = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      await axios.post('/api/financialadvanced/intrastat', { id: 0, ...form, status: 'Draft' })
      setForm({
        declarationNumber: '',
        year: new Date().getFullYear(),
        month: new Date().getMonth() + 1
      })
      await load()
    } catch {
      setError('No se pudo crear la declaracion.')
    }
  }

  const exportCsv = async (id: number) => {
    try {
      const response = await axios.get(`/api/financialadvanced/intrastat/${id}/export`, { responseType: 'blob' })
      const url = window.URL.createObjectURL(response.data)
      const a = document.createElement('a')
      a.href = url
      a.download = `intrastat_${id}.csv`
      a.click()
      window.URL.revokeObjectURL(url)
    } catch {
      setError('No se pudo exportar el archivo Intrastat.')
    }
  }

  return (
    <div>
      <div className="page-header"><h2>Intrastat</h2></div>
      {error && <div className="alert">{error}</div>}
      <div className="card">
        <form className="page-toolbar" onSubmit={create}>
          <input className="form-control" placeholder="Numero declaracion" value={form.declarationNumber} onChange={e => setForm({ ...form, declarationNumber: e.target.value })} required />
          <input className="form-control" type="number" value={form.year} onChange={e => setForm({ ...form, year: Number(e.target.value) })} required />
          <input className="form-control" type="number" value={form.month} min={1} max={12} onChange={e => setForm({ ...form, month: Number(e.target.value) })} required />
          <button className="btn btn-primary" type="submit">Crear</button>
        </form>
      </div>
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Numero</th>
              <th>Periodo</th>
              <th>Estado</th>
              <th>Accion</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(row => (
              <tr key={row.id}>
                <td>{row.declarationNumber}</td>
                <td>{row.month}/{row.year}</td>
                <td>{row.status}</td>
                <td><button className="btn btn-outline" onClick={() => exportCsv(row.id)}>Exportar CSV</button></td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={4}>Sin declaraciones.</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default Intrastat
