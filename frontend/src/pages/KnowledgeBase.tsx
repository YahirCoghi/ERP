import { useEffect, useState } from 'react'
import axios from 'axios'

type Article = {
  id: number
  title: string
  category: string
  content: string
  tags?: string
}

function KnowledgeBase() {
  const [rows, setRows] = useState<Article[]>([])
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [form, setForm] = useState({ title: '', category: 'General', content: '', tags: '' })

  const load = async (q = '') => {
    const response = await axios.get('/api/serviceandtools/knowledge-base', { params: { q } })
    setRows(response.data || [])
  }

  useEffect(() => {
    load().catch(() => setError('No se pudo cargar la base de conocimiento.'))
  }, [])

  const save = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await axios.post('/api/serviceandtools/knowledge-base', { id: 0, ...form })
      setForm({ title: '', category: 'General', content: '', tags: '' })
      await load(search)
    } catch {
      setError('No se pudo guardar el articulo.')
    }
  }

  const runSearch = async (e: React.FormEvent) => {
    e.preventDefault()
    await load(search)
  }

  return (
    <div>
      <div className="page-header"><h2>Knowledge Base</h2></div>
      {error && <div className="alert">{error}</div>}
      <div className="card">
        <form className="page-toolbar" onSubmit={runSearch}>
          <input className="form-control" placeholder="Buscar articulo..." value={search} onChange={e => setSearch(e.target.value)} />
          <button className="btn btn-outline" type="submit">Buscar</button>
        </form>
      </div>

      <div className="card">
        <h3>Nuevo articulo</h3>
        <form onSubmit={save}>
          <div className="form-group"><label>Titulo</label><input className="form-control" value={form.title} onChange={e => setForm({ ...form, title: e.target.value })} required /></div>
          <div className="form-group"><label>Categoria</label><input className="form-control" value={form.category} onChange={e => setForm({ ...form, category: e.target.value })} required /></div>
          <div className="form-group"><label>Contenido</label><textarea className="form-control" rows={4} value={form.content} onChange={e => setForm({ ...form, content: e.target.value })} required /></div>
          <div className="form-group"><label>Tags</label><input className="form-control" value={form.tags} onChange={e => setForm({ ...form, tags: e.target.value })} /></div>
          <button className="btn btn-primary" type="submit">Guardar</button>
        </form>
      </div>

      <div className="card">
        <h3>Articulos</h3>
        <table className="table">
          <thead>
            <tr>
              <th>Titulo</th>
              <th>Categoria</th>
              <th>Tags</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(row => (
              <tr key={row.id}>
                <td>{row.title}</td>
                <td>{row.category}</td>
                <td>{row.tags || '-'}</td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={3}>Sin articulos.</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default KnowledgeBase
