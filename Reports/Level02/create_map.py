import json, html
from pathlib import Path
r=json.loads(Path('Reports/Level02/Layout.json').read_text()); w=1580; h=390; scale=2.15
out=[f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{w}" height="{h}"><rect width="100%" height="100%" fill="#090e20"/><style>text{{font-family:Arial,sans-serif;fill:#e0e8ff}}.note{{font-size:11px}}</style><text x="30" y="32" font-size="24">VOLTIX DASH / 02 — OVERCHARGE SECTOR</text><text x="30" y="54" font-size="13">Layout map · cyan = upper route · magenta = optional lower route · red = hazards · yellow = PatrolBots</text>']
def x(v): return 35+v*scale
def y(v): return 305-v*6
sections={}
for p in r['platforms']:
 color={'upper':'#51ecfc','lower':'#e666cc','shared':'#9767ed'}[p['route']]
 out.append(f'<rect x="{x(p["minX"]):.2f}" y="{y(p["top"]):.2f}" width="{(p["maxX"]-p["minX"])*scale:.2f}" height="{max(2,p["thickness"]*6)}" fill="{color}"/>')
 sections.setdefault(p['section'],[p['minX'],p['maxX']]); sections[p['section']][1]=max(sections[p['section']][1],p['maxX'])
for z in r['hazards']:
 out.append(f'<circle cx="{x(z["x"]):.2f}" cy="{y(z["y"]):.2f}" r="3" fill="#ff5357"/>')
for z in r['enemies']:
 out.append(f'<rect x="{x(z["x"])-3:.2f}" y="{y(z["y"])-5:.2f}" width="6" height="6" fill="#ffe175"/>')
for i,(section,(a,b)) in enumerate(sections.items(),1):
 mid=x((a+b)/2);out.append(f'<path d="M {mid:.2f} 323 v 14" stroke="#556080"/><text x="{mid:.2f}" y="{354 if i%2 else 374}" text-anchor="middle" class="note">{i}. {html.escape(section)}</text>')
out.append(f'<circle cx="{x(4)}" cy="{y(1.1)}" r="5" fill="#51ecfc"/><text x="{x(4)}" y="{y(1.1)-12}" class="note">SPAWN</text><ellipse cx="{x(690.8)}" cy="{y(19.7)}" rx="5" ry="10" fill="none" stroke="#51ecfc" stroke-width="2"/><text x="{x(690.8)-25}" y="{y(19.7)-15}" class="note">GOAL</text></svg>')
Path('Reports/Level02/LevelMap.svg').write_text(''.join(out))
