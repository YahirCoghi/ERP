import { useEffect, useState } from 'react'
import axios from 'axios'

type AddonDefinition = {
  id: number
  code: string
  name: string
  version: string
}

type AddonActivation = {
  id: number
  addonDefinitionId: number
  addonCode: string
  addonName: string
  enabled: boolean
}

function Addons() {
  const [definitions, setDefinitions] = useState<AddonDefinition[]>([])
  const [activations, setActivations] = useState<AddonActivation[]>([])
  const [form, setForm] = useState({ code: '', name: '', version: '1.0.0' })
  const [error, setError] = useState('')

  const load = async () => {
    const [defs, acts] = await Promise.all([
      axios.get('/api/addons/definitions'),
      axios.get('/api/addons/activations')
    ])
    setDefinitions(defs.data || [])
    setActivations(acts.data || [])
  }

  useEffect(() => {
    load().catch(() => setError('No se pudo cargar add-ons.'))
  }, [])

  const createDefinition = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    try {
      await axios.post('/api/addons/definitions', { id: 0, ...form, configurationSchemaJson: null })
      setForm({ code: '', name: '', version: '1.0.0' })
      await load()
    } catch {
      setError('No se pudo crear el add-on.')
    }
  }

  const toggleAddon = async (row: AddonActivation) => {
    setError('')
    try {
      await axios.post('/api/addons/activations', {
        id: row.id,
        addonDefinitionId: row.addonDefinitionId,
        enabled: !row.enabled,
        configurationJson: null,
        enabledAt: null
      })
      await load()
    } catch {
      setError('No se pudo actualizar el add-on.')
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Add-ons Manager</h2>
      </div>
      {error && <div className="alert">{error}</div>}

      <div className="card">
        <h3>Nuevo Add-on</h3>
        <form className="page-toolbar" onSubmit={createDefinition}>
          <input className="form-control" placeholder="Codigo" value={form.code} onChange={e => setForm({ ...form, code: e.target.value })} required />
          <input className="form-control" placeholder="Nombre" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
          <input className="form-control" placeholder="Version" value={form.version} onChange={e => setForm({ ...form, version: e.target.value })} required />
          <button className="btn btn-primary" type="submit">Crear</button>
        </form>
      </div>

      <div className="card">
        <h3>Activaciones</h3>
        <table className="table">
          <thead>
            <tr>
              <th>Codigo</th>
              <th>Nombre</th>
              <th>Estado</th>
              <th>Accion</th>
            </tr>
          </thead>
          <tbody>
            {activations.map(row => (
              <tr key={row.id}>
                <td>{row.addonCode}</td>
                <td>{row.addonName}</td>
                <td>{row.enabled ? 'Activo' : 'Inactivo'}</td>
                <td>
                  <button className="btn btn-outline" onClick={() => toggleAddon(row)}>
                    {row.enabled ? 'Desactivar' : 'Activar'}
                  </button>
                </td>
              </tr>
            ))}
            {activations.length === 0 && (
              <tr>
                <td colSpan={4}>No hay activaciones registradas.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="card">
        <h3>Definiciones registradas</h3>
        <table className="table">
          <thead>
            <tr>
              <th>Codigo</th>
              <th>Nombre</th>
              <th>Version</th>
            </tr>
          </thead>
          <tbody>
            {definitions.map(row => (
              <tr key={row.id}>
                <td>{row.code}</td>
                <td>{row.name}</td>
                <td>{row.version}</td>
              </tr>
            ))}
            {definitions.length === 0 && (
              <tr>
                <td colSpan={3}>No hay add-ons.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default Addons
