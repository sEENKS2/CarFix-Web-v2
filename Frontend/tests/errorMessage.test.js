import test from 'node:test';
import assert from 'node:assert/strict';
import { errorMessage } from '../src/api/errorMessage.js';

test('errores de la API siempre se convierten en texto para React', () => {
  for (const data of [undefined, null, {}, { title: 'Bad Request', status: 400 }, { mensaje: {} }, { errors: { campo: [{}] } }]) {
    assert.equal(errorMessage({ response: { data } }, 'Error'), 'Error');
  }
  assert.equal(errorMessage({ response: { data: 'Cliente duplicado' } }), 'Cliente duplicado');
  assert.equal(errorMessage({ response: { data: { mensaje: 'Revisá los datos' } } }), 'Revisá los datos');
  assert.equal(errorMessage({ response: { data: { errors: { Email: ['Correo inválido'] } } } }), 'Email: Correo inválido');
  assert.match(errorMessage({ response: { data: { errors: { '$.telefono': ['Conversion failed'] } } } }), /teléfono.*texto/);
});
