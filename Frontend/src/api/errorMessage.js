export function errorMessage(error, fallback = 'No se pudo completar la operación') {
  const data = error?.response?.data;
  if (typeof data === 'string' && data.trim()) return data;
  if (typeof data?.mensaje === 'string') return data.mensaje;
  if (data?.errors && typeof data.errors === 'object') {
    const messages = Object.entries(data.errors).flatMap(([field, values]) => {
      const label = field.replace(/^\$\./, '').toLowerCase();
      if (label === 'telefono') return ['El teléfono debe enviarse como texto. Recargá la página e intentá nuevamente.'];
      if (label === 'dni') return ['El DNI debe ser un número entero válido entre 1 y 2147483647.'];
      return (Array.isArray(values) ? values : [values])
        .filter(value => typeof value === 'string')
        .map(value => `${field}: ${value}`);
    });
    if (messages.length) return messages.join(' ');
  }
  return fallback;
}
