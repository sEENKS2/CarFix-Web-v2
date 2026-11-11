#!/usr/bin/env python3
"""Guardia ligera del árbol actual. No imprime valores detectados ni sustituye una auditoría del historial."""
from pathlib import Path
import re
import subprocess
import sys

root = Path(__file__).resolve().parents[1]
# También sirve al extraer el ZIP, donde no existe .git.
try:
    paths = [root / f for f in subprocess.check_output(['git', '-C', str(root), 'ls-files', '--cached', '--others', '--exclude-standard'], text=True).splitlines()]
except subprocess.CalledProcessError:
    paths = [p for p in root.rglob('*') if p.is_file()]
patterns = {
    'clave Google': r'AIza[0-9A-Za-z_-]{30,}',
    'token GitHub': r'(?:ghp_|github_pat_)[A-Za-z0-9_]{20,}',
    'clave OpenAI': r'sk-(?:proj-)?[A-Za-z0-9_-]{25,}',
    'clave AWS': r'(?:AKIA|ASIA)[A-Z0-9]{16}',
    'clave privada': r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',
    'contraseña fija': r'\.EstablecerPassword\("[^"\n]+"\)',
    'respaldo JWT fijo': r'(?:Jwt:Key"\]\s*\?\?\s*"[^"\n]+"|jwtKey\s*=\s*"[^"\n]+")',
    'hash precargado': r'PasswordHash\s*=\s*"(?!RESET_REQUIRED")[^"\n]+"',
    'registro sensible': r'\.EnableSensitiveDataLogging\(\)',
}
errors = []
for path in dict.fromkeys(paths):
    relative = path.relative_to(root)
    if any(p in {'.git', 'node_modules', 'bin', 'obj', 'dist'} for p in relative.parts) or path == Path(__file__): continue
    if path.name.startswith('.env') and not path.name.endswith('.example'):
        errors.append((relative, 0, 'archivo de entorno privado versionado')); continue
    if path.suffix.lower() in {'.pfx', '.p12', '.pem', '.key', '.bak', '.db', '.sqlite'}:
        errors.append((relative, 0, 'archivo potencialmente privado')); continue
    try: text = path.read_text(encoding='utf-8-sig')
    except (OSError, UnicodeError): continue
    for label, pattern in patterns.items():
        for match in re.finditer(pattern, text):
            errors.append((relative, text.count('\n', 0, match.start()) + 1, label))
for path, line, label in errors: print(f'{path}:{line}: {label}')
if errors: sys.exit(1)
print('Sin patrones sensibles detectados en el árbol de archivos revisado.')
