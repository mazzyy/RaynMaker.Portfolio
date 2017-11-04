
export function formatValue (value) {
  console.log(value + ' -> ' + (typeof value))
  if (typeof value === 'string' || value instanceof String) return value
  let val = (value / 1).toFixed(2).replace('.', ',')
  return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.')
}

export const webApi = {
  methods: {
    get: (that, url, data, onDone) => {
      let baseUrl = process.env.NODE_ENV === 'production' ? '' : 'http://localhost:2525'

      $.ajax({
        url: baseUrl + url,
        data: data,
        dataType: 'json',
        method: 'GET'
      }).then(response => {
        if (response.error) {
          console.err('ERROR: ' + response.error)
        } else {
          onDone(that, response)
        }
      }).catch(err => {
        console.error('EXCEPTION: ' + err)
      })
    }
  }
}
