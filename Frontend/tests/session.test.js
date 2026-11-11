import test from 'node:test';
import assert from 'node:assert/strict';
import { hasRole, validSessionToken } from '../src/security/session.js';

test('nombre admin no otorga permisos', () => {
  assert.equal(hasRole({ nombreUsuario: 'admin', roles: ['Tecnicos'] }, []), false);
  assert.equal(hasRole({ roles: ['Administradores'] }, []), true);
  assert.equal(hasRole({ roles: ['Tecnicos'] }, ['Operadores']), false);
});
test('token ausente, ficticio, mal formado o vencido no inicia sesión', () => {
  for (const value of [null, '', 'invalid-session', 'x.y.z']) assert.equal(validSessionToken(value), false);
  const token = exp => 'header.' + Buffer.from(JSON.stringify({ exp })).toString('base64url') + '.signature';
  assert.equal(validSessionToken(token(1)), false);
  assert.equal(validSessionToken(token(Date.now() / 1000 + 60)), true);
});
