import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import './index.css'
import axios from 'axios'

const resolveTenantId = () => {
  const stored = localStorage.getItem('tenantId')
  if (stored) return stored

  // Local fallback for single-tenant dev setups.
  if (window.location.hostname === 'localhost') {
    localStorage.setItem('tenantId', '1')
    return '1'
  }

  return null
}

axios.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  const tenantId = resolveTenantId()

  if (token) {
    config.headers = config.headers || {}
    config.headers['Authorization'] = `Bearer ${token}`
  }

  if (tenantId) {
    config.headers = config.headers || {}
    config.headers['X-Tenant-ID'] = tenantId
  }
  return config
})

axios.interceptors.response.use(
  response => response,
  error => {
    if (error?.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('username')
      localStorage.removeItem('role')
    }
    return Promise.reject(error)
  }
)

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <BrowserRouter>
      <App />
    </BrowserRouter>
  </React.StrictMode>,
)
