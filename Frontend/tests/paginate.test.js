import test from 'node:test';
import assert from 'node:assert/strict';
import { paginate } from '../src/utils/paginate.js';

test('un catálogo de 5000 elementos se recorre completo sin duplicados ni omisiones', () => {
  const catalogo = Array.from({ length: 5000 }, (_, id) => ({ id }));
  const recorridos = [];
  for (let page = 1; page <= 200; page++) {
    const result = paginate(catalogo, page, 25);
    assert.equal(result.items.length, 25);
    recorridos.push(...result.items);
  }
  assert.deepEqual(recorridos, catalogo);
});

test('filtrar desde una página alta y vaciar resultados mantiene una página válida', () => {
  assert.deepEqual(paginate([1, 2, 3], 100), { page: 1, items: [1, 2, 3] });
  assert.deepEqual(paginate([], 100), { page: 1, items: [] });
  assert.deepEqual(paginate([1, 2, 3], -2), { page: 1, items: [1, 2, 3] });
  assert.deepEqual(paginate(Array.from({ length: 21 }, (_, i) => i), 3).items, [20]);
});
