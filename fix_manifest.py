import json

path = 'D:/Repositorio/jonatha1992/corvux-desafio/Corvux/bin/Debug/net9.0/Corvux.staticwebassets.runtime.json'

with open(path, 'r') as f:
    data = json.load(f)

print("Old roots:", data['ContentRoots'])

new_roots = []
for r in data['ContentRoots']:
    new_r = r.replace('d:\\Repositorio\\corvux-desafio\\', 'D:\\Repositorio\\jonatha1992\\corvux-desafio\\')
    new_roots.append(new_r)

data['ContentRoots'] = new_roots
print("New roots:", data['ContentRoots'])

with open(path, 'w') as f:
    json.dump(data, f, separators=(',', ':'))

print("Done")
