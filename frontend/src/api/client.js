import axios from 'axios'

const baseURL = process.env.NEXT_PUBLIC_API_BASE_URL || '/api'

const client = axios.create({ baseURL })

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('smc_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

client.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401 && window.location.pathname === '/demand-application/officer') {
      localStorage.removeItem('smc_token')
      localStorage.removeItem('smc_user')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

export default client
