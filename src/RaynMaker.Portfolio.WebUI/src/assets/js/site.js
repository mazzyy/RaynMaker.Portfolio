
export const webApi = {
  methods: {
    get: (that, url, data, onDone) => {
      const baseUrl = process.env.NODE_ENV === 'production' ? '' : 'http://localhost:2525'

      fetch(baseUrl + url)
        .then(res => res.json())
        .then(response => {
          if (response.error) {
            console.err('ERROR: ' + response.error)
          } else {
            onDone(that, response)
          }
        })
        .catch(err => {
          console.error('EXCEPTION: ' + err)
        })
    }
  }
}
