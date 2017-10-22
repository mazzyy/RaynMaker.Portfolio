
export function formatValue (value) {
  if (typeof value === 'string' || value instanceof String) return value
  let val = (value / 1).toFixed(2).replace('.', ',')
  return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.')
}

export function randomColor () {
  function randomInt (min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min
  }

  var h = randomInt(0, 360)
  var s = randomInt(42, 98)
  var l = randomInt(40, 90)
  return 'hsl(' + h + ',' + s + '%,' + l + '%)'
}
