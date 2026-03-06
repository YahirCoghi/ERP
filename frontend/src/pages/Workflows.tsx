import { useEffect, useState } from 'react'
import axios from 'axios'

type Workflow = {
  id: number
  code: string
  name: string
  module: string
  isPublished: boolean
}

function Workflows() {
  const [rows, setRows] = useState<Workflow[]>([])
  const [instances, setInstances] = useState<any[]>([])
  const [error, setError] = useState('')
  const [form, setForm] = useState({ code: '', name: '', module: 'Sales', isPublished: true })

  const load = async () => {
    const [defs, inst] = await Promise.all([
      axios.get('/api/workflowmanager/definitions'),
      axios.get('/api/workflowmanager/instances')
    ])
    setRows(defs.data || [])
    setInstances(inst.data || [])
  }

  useEffect(() => {
    load().catch(() => setError('No se pudieron cargar los workflows.'))
  }, [])

  const create = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      await axios.post('/api/workflowmanager/definitions', { id: 0, ...form })
      setForm({ code: '', name: '', module: 'Sales', isPublished: true })
      await load()
    } catch {
      setError('No se pudo crear el workflow.')
    }
  }

  const run = async (workflowId: number) => {
    setError('')
    try {
      await axios.post('/api/workflowmanager/instances', {
        id: 0,
        workflowDefinitionId: workflowId,
        referenceType: 'Manual',
        referenceId: Date.now() % 100000,
        status: 'Running',
        startedAt: new Date().toISOString(),
        completedAt: null
      })
      await load()
    } catch {
      setError('No se pudo iniciar la instancia.')
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Workflow Manager</h2>
      </div>
      {error && <div className="alert">{error}</div>}

      <div className="card">
        <h3>Nuevo Workflow</h3>
        <form className="page-toolbar" onSubmit={create}>
          <input className="form-control" placeholder="Codigo" value={form.code} onChange={e => setForm({ ...form, code: e.target.value })} required />
          <input className="form-control" placeholder="Nombre" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
          <input className="form-control" placeholder="Modulo" value={form.module} onChange={e => setForm({ ...form, module: e.target.value })} required />
          <button className="btn btn-primary" type="submit">Crear</button>
        </form>
      </div>

      <div className="card">
        <h3>Definiciones</h3>
        <table className="table">
          <thead>
            <tr>
              <th>Codigo</th>
              <th>Nombre</th>
              <th>Modulo</th>
              <th>Publicado</th>
              <th>Accion</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(r => (
              <tr key={r.id}>
                <td>{r.code}</td>
                <td>{r.name}</td>
                <td>{r.module}</td>
                <td>{r.isPublished ? 'Si' : 'No'}</td>
                <td><button className="btn btn-outline" onClick={() => run(r.id)}>Iniciar instancia</button></td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={5}>Sin workflows.</td></tr>}
          </tbody>
        </table>
      </div>

      <div className="card">
        <h3>Instancias</h3>
        <table className="table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Workflow</th>
              <th>Referencia</th>
              <th>Estado</th>
              <th>Inicio</th>
            </tr>
          </thead>
          <tbody>
            {instances.map((r: any) => (
              <tr key={r.id}>
                <td>{r.id}</td>
                <td>{r.workflowDefinitionId}</td>
                <td>{r.referenceType} #{r.referenceId}</td>
                <td>{r.status}</td>
                <td>{new Date(r.startedAt).toLocaleString()}</td>
              </tr>
            ))}
            {instances.length === 0 && <tr><td colSpan={5}>Sin instancias.</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default Workflows
