import { useEffect, useState } from 'react'
import axios from 'axios'

type PickPackTask = {
  id: number
  taskNumber: string
  taskType: string
  status: string
  warehouse?: string
}

function PickPack() {
  const [rows, setRows] = useState<PickPackTask[]>([])
  const [error, setError] = useState('')
  const [form, setForm] = useState({ taskNumber: '', taskType: 'Pick', warehouse: '' })

  const load = async () => {
    const response = await axios.get('/api/productionadvanced/pick-pack')
    setRows(response.data || [])
  }

  useEffect(() => {
    load().catch(() => setError('No se pudo cargar Pick Pack.'))
  }, [])

  const create = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await axios.post('/api/productionadvanced/pick-pack', {
        id: 0,
        ...form,
        status: 'Open'
      })
      setForm({ taskNumber: '', taskType: 'Pick', warehouse: '' })
      await load()
    } catch {
      setError('No se pudo crear la tarea.')
    }
  }

  const complete = async (id: number) => {
    try {
      await axios.post(`/api/productionadvanced/pick-pack/${id}/complete`)
      await load()
    } catch {
      setError('No se pudo completar la tarea.')
    }
  }

  return (
    <div>
      <div className="page-header"><h2>Pick Pack & Production</h2></div>
      {error && <div className="alert">{error}</div>}
      <div className="card">
        <form className="page-toolbar" onSubmit={create}>
          <input className="form-control" placeholder="Numero tarea" value={form.taskNumber} onChange={e => setForm({ ...form, taskNumber: e.target.value })} required />
          <select className="form-control" value={form.taskType} onChange={e => setForm({ ...form, taskType: e.target.value })}>
            <option value="Pick">Pick</option>
            <option value="Pack">Pack</option>
            <option value="Production">Production</option>
          </select>
          <input className="form-control" placeholder="Bodega" value={form.warehouse} onChange={e => setForm({ ...form, warehouse: e.target.value })} />
          <button className="btn btn-primary" type="submit">Crear tarea</button>
        </form>
      </div>
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Numero</th>
              <th>Tipo</th>
              <th>Bodega</th>
              <th>Estado</th>
              <th>Accion</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(row => (
              <tr key={row.id}>
                <td>{row.taskNumber}</td>
                <td>{row.taskType}</td>
                <td>{row.warehouse || '-'}</td>
                <td>{row.status}</td>
                <td>
                  {row.status !== 'Completed' ? (
                    <button className="btn btn-outline" onClick={() => complete(row.id)}>Completar</button>
                  ) : '-'}
                </td>
              </tr>
            ))}
            {rows.length === 0 && <tr><td colSpan={5}>Sin tareas.</td></tr>}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default PickPack
