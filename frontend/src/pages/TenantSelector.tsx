import { useEffect, useState } from 'react'
import axios from 'axios'

interface Tenant {
  id: number
  name: string
  status: string
}

function TenantSelector() {
  const [tenants, setTenants] = useState<Tenant[]>([])
  const [tenantId, setTenantId] = useState<number | null>(() => {
    const raw = localStorage.getItem('tenantId')
    return raw ? parseInt(raw) : null
  })

  const fetchTenants = async () => {
    const response = await axios.get('/api/tenants')
    setTenants(response.data)
  }

  useEffect(() => {
    fetchTenants()
  }, [])

  const handleSelect = (id: number) => {
    localStorage.setItem('tenantId', id.toString())
    setTenantId(id)
  }

  return (
    <div>
      <div className="page-header">
        <h2>Seleccionar Tenant</h2>
      </div>
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Nombre</th>
              <th>Estado</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {tenants.map(t => (
              <tr key={t.id}>
                <td>{t.id}</td>
                <td>{t.name}</td>
                <td>{t.status}</td>
                <td>
                  <button className="btn btn-primary" onClick={() => handleSelect(t.id)}>
                    Usar
                  </button>
                </td>
              </tr>
            ))}
            {tenants.length === 0 && (
              <tr>
                <td colSpan={4}>No hay tenants</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {tenantId && (
        <div className="card" style={{ marginTop: '10px', background: '#e8f5e9' }}>
          Tenant seleccionado: {tenantId}
        </div>
      )}
    </div>
  )
}

export default TenantSelector
