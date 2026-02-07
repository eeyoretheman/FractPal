import { useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

interface LoginResponse {
  jwt: any
  refreshToken: string
}

interface RegistrationResponse {
  id: string
  email: string
  roles: string[]
}

type View = 'login' | 'register'

function App() {
  const [view, setView] = useState<View>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loginSuccess, setLoginSuccess] = useState<LoginResponse | null>(null)
  const [registerSuccess, setRegisterSuccess] = useState<RegistrationResponse | null>(null)

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)
    setLoginSuccess(null)

    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email,
          password,
        }),
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        if (response.status === 401) {
          throw new Error(errorData?.message || 'Invalid email or password')
        }
        throw new Error(errorData?.message || `Login failed: ${response.statusText}`)
      }

      const data: LoginResponse = await response.json()
      setLoginSuccess(data)
      console.log('Login successful:', data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred')
      console.error('Login error:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleRegister = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    setError(null)
    setRegisterSuccess(null)

    try {
      const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email,
          password,
        }),
      })

      if (!response.ok) {
        const errorData = await response.json().catch(() => null)
        throw new Error(errorData?.message || `Registration failed: ${response.statusText}`)
      }

      const data: RegistrationResponse = await response.json()
      setRegisterSuccess(data)
      console.log('Registration successful:', data)

      // Clear form
      setEmail('')
      setPassword('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred')
      console.error('Registration error:', err)
    } finally {
      setLoading(false)
    }
  }

  const switchView = (newView: View) => {
    setView(newView)
    setError(null)
    setLoginSuccess(null)
    setRegisterSuccess(null)
  }

  return (
    <div className="app-container">
      <h1>FractPal Auth Test</h1>

      <div className="view-toggle">
        <button
          className={view === 'login' ? 'active' : ''}
          onClick={() => switchView('login')}
        >
          Login
        </button>
        <button
          className={view === 'register' ? 'active' : ''}
          onClick={() => switchView('register')}
        >
          Register
        </button>
      </div>

      {view === 'login' ? (
        <form onSubmit={handleLogin} className="auth-form">
          <h2>Login</h2>
          <div className="form-group">
            <label htmlFor="email">Email:</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="user@example.com"
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password:</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="Enter your password"
              disabled={loading}
            />
          </div>

          <button type="submit" disabled={loading}>
            {loading ? 'Logging in...' : 'Login'}
          </button>
        </form>
      ) : (
        <form onSubmit={handleRegister} className="auth-form">
          <h2>Register</h2>
          <div className="form-group">
            <label htmlFor="email">Email:</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="user@example.com"
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password:</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="Minimum 6 characters"
              disabled={loading}
              minLength={6}
            />
          </div>

          <button type="submit" disabled={loading}>
            {loading ? 'Registering...' : 'Register'}
          </button>
        </form>
      )}

      {error && (
        <div className="message error">
          <strong>Error:</strong> {error}
        </div>
      )}

      {loginSuccess && (
        <div className="message success">
          <h3>Login Successful!</h3>
          <div className="token-display">
            <p><strong>JWT Token:</strong></p>
            <pre>{loginSuccess.jwt.substring(0, 50)}...</pre>
            <p><strong>Refresh Token:</strong></p>
            <pre>{loginSuccess.refreshToken.substring(0, 50)}...</pre>
          </div>
        </div>
      )}

      {registerSuccess && (
        <div className="message success">
          <h3>Registration Successful!</h3>
          <div className="user-info">
            <p><strong>User ID:</strong> {registerSuccess.id}</p>
            <p><strong>Email:</strong> {registerSuccess.email}</p>
            <p><strong>Roles:</strong> {registerSuccess.roles.join(', ')}</p>
          </div>
          <p className="switch-hint">You can now login with your credentials</p>
        </div>
      )}
    </div>
  )
}

export default App
