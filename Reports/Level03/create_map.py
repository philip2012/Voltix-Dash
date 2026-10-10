import json,html
from pathlib import Path
r=json.loads(Path('Reports/Level03/Layout.json').read_text());s=1.8
x=lambda v:35+v*s
y=lambda v:410-v*5.5
out=['<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1570 505" width="1570" height="505"><rect width="100%" height="100%" fill="#080b19"/><style>text{font-family:Arial,sans-serif;fill:#e6edff}</style><text x="30" y="30" font-size="23">VOLTIX DASH / 03 — REACTOR HEART</text><text x="30" y="53" font-size="12">Cyan = safe decks · blue = moving decks · green = launch pads/checkpoint · red = hazards · yellow = bots · magenta = sentries</text>']
sections={}
for p in r['platforms']:
 a,b,t=p['minX'],p['maxX'],p['top']; col='#5499ff' if p.get('moving') else '#51ebf6'; opacity='.4' if p.get('optional') else '1'
 out.append(f'<rect x="{x(a)}" y="{y(t)}" width="{(b-a)*s}" height="{max(2,p["thickness"]*5.5)}" fill="{col}" opacity="{opacity}"/>')
 if p.get('moving'):
  dx,dy=p['pointB'];out.append(f'<path d="M{x((a+b)/2)} {y(t)} l{dx*s} {-dy*5.5}" stroke="#5499ff" stroke-dasharray="3 2"/>')
 sections.setdefault(p['section'],[a,b]);sections[p['section']][1]=max(sections[p['section']][1],b)
for k,c in [('hazards','#ff615d'),('pads','#6bffc5'),('enemies','#ffe77c'),('turrets','#fb62c5')]:
 for z in r[k]:out.append(f'<circle cx="{x(z["x"])}" cy="{y(z["y"])-3}" r="3.5" fill="{c}"/>')
for i,(name,(a,b)) in enumerate(sections.items(),1):
 mid=x((a+b)/2);out.append(f'<path d="M{mid} 425v12" stroke="#485779"/><text x="{mid}" y="{458 if i%2 else 482}" text-anchor="middle" font-size="10">{i}. {html.escape(name)}</text>')
for name,pt in [('SPAWN',r['spawn']),('CHECKPOINT',r['checkpoint']),('GOAL',r['goal'])]:out.append(f'<circle cx="{x(pt[0])}" cy="{y(pt[1])}" r="5" fill="none" stroke="#6bffc5"/><text x="{x(pt[0])-12}" y="{y(pt[1])-13}" font-size="11">{name}</text>')
out.append('</svg>');Path('Reports/Level03/LevelMap.svg').write_text(''.join(out))
