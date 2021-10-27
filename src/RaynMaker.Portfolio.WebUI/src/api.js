import axios from 'axios'

// const hostURL = process.env.VUE_APP_BASE_URL ? process.env.VUE_APP_BASE_URL : window.location.origin
const hostURL = process.env.NODE_ENV === 'production' ? '' : 'http://localhost:2525'

const API = axios.create({
  // withCredentials: true,
  baseURL: hostURL + '/api'
})

API.interceptors.response.use(response => {
  return response
}, error => {
  console.log('response:', { ...error })

  // check if it's a server error
  if (!error.response) {
    console.log(`Network/Server error: ${error.message}`)
    return Promise.reject(error)
  }

  if (error.response.status === 500) {
    console.log(`${error.response.statusText}: ${error.response.data}`)
    return Promise.reject(error)
  }

  return Promise.reject(error)
})

API.interceptors.request.use(request => {
  return request
}, error => {
  console.log('request:', { ...error })

  return Promise.reject(error)
})

export default API
