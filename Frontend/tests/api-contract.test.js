import test from 'node:test';
import assert from 'node:assert/strict';
import { readdirSync, readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const root = fileURLToPath(new URL('../../', import.meta.url));
const read = file => readFileSync(file, 'utf8');
function files(dir) {
  return readdirSync(dir, { withFileTypes: true }).flatMap(entry => {
    const file = path.join(dir, entry.name);
    return entry.isDirectory() ? files(file) : [file];
  });
}
const sources = files(path.join(root, 'Frontend/src')).filter(f => /\.(js|jsx)$/.test(f));
const normalize = route => route.split('?')[0]
  .replace(/\$\{[^}]+\}/g, '{}').replace(/\{[^}]*\}/g, '{}').toLowerCase();

test('los métodos y rutas HTTP del frontend existen en los controladores', () => {
  const routes = new Set();
  for (const file of files(path.join(root, 'Backend/CarFix.API/Controllers'))) {
    if (!file.endsWith('.cs')) continue;
    const source = read(file);
    const controller = source.match(/class\s+(\w+)Controller\b/)?.[1];
    if (!controller) continue;
    const prefix = source.match(/\[Route\("([^"]+)"\)\]/)?.[1]
      .replace('[controller]', controller).replace(/^api\//, '');
    if (!prefix) continue;
    for (const match of source.matchAll(/\[Http(Get|Post|Put|Delete|Patch)(?:\("([^"]*)"\))?\]/g)) {
      routes.add(`${match[1].toLowerCase()} ${normalize('/' + prefix + (match[2] ? '/' + match[2] : ''))}`);
    }
  }
  let checked = 0;
  for (const file of sources) {
    for (const match of read(file).matchAll(/\b(?:api|axiosClient)\.(get|post|put|delete|patch)\(\s*(['"`])([^'"`]+)\2/g)) {
      const key = `${match[1]} ${normalize(match[3])}`;
      assert.ok(routes.has(key), `${path.relative(root, file)}: ${key}`);
      checked++;
    }
  }
  assert.ok(checked > 50, 'Debe revisar los llamados de todos los módulos');
});

test('las funciones llamadas de los servicios importados están definidas', () => {
  let checked = 0;
  for (const file of sources) {
    const source = read(file);
    for (const imported of source.matchAll(/import\s*\{\s*(\w+Service)\s*\}\s*from\s*['"]([^'"]+)['"]/g)) {
      const serviceFile = path.resolve(path.dirname(file), imported[2] + (imported[2].endsWith('.js') ? '' : '.js'));
      const service = read(serviceFile);
      for (const call of source.matchAll(new RegExp(`\\b${imported[1]}\\.(\\w+)\\(`, 'g'))) {
        assert.match(service, new RegExp(`\\b${call[1]}\\s*:`), `${path.relative(root, file)} llama a ${imported[1]}.${call[1]}`);
        checked++;
      }
    }
  }
  assert.ok(checked > 10);
});
