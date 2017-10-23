
export function formatValue (value) {
  if (typeof value === 'string' || value instanceof String) return value
  let val = (value / 1).toFixed(2).replace('.', ',')
  return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.')
}
