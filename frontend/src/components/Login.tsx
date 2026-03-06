import { useState } from 'react'
import axios from 'axios'

interface LoginProps {
  onLogin: (token: string, username: string, role: string) => void
}

function Login({ onLogin }: LoginProps) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      const response = await axios.post('/api/auth/login', {
        username,
        password
      })

      const { token, username: loginUser, role } = response.data
      onLogin(token, loginUser, role)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Usuario o contraseña incorrectos')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-screen">
      <div className="login-card">
        <div className="login-brand">NEX</div>
        <div className="login-subtitle">Enterprise Resource Planning</div>

        {error && (
          <div className="alert">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <label className="field-label">Username</label>
          <input
            type="text"
            className="field-input"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="admin"
            required
          />

          <label className="field-label">Password</label>
          <input
            type="password"
            className="field-input"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="admin"
            required
          />

          <button
            type="submit"
            className="btn btn-primary btn-block"
            disabled={loading}
          >
            {loading ? 'Iniciando...' : 'Login'}
          </button>
        </form>

        <div className="login-footer">
          © 2026 NEX ERP System. All rights reserved.
        </div>
      </div>
    </div>
  )
}

export default Login
