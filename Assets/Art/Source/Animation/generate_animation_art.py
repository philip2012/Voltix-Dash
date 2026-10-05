"""Pose existing editable Voltix geometry into compact sprite sheets; no external art.
Run with Python + Pillow. SVG sheets remain independently editable vector artwork.
"""
from pathlib import Path
import xml.etree.ElementTree as ET
import math, json, copy
from PIL import Image, ImageDraw
ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT/'Source'/'Animation'; OUTPUT = ROOT/'Sprites'/'Animation'
N='#0b1223'; CYAN='#52e9f5'; WHITE='#d5fcff'; VIOLET='#9165ec'
def original(name):
    return list(ET.parse(ROOT/'Source'/f'{name}.svg').getroot())
def points(e):
    return [tuple(map(float,s.split(','))) for s in e.attrib['points'].split()]
def shape(e):
    tag=e.tag.split('}')[-1]; a=e.attrib
    if tag in ('polygon','polyline'): return points(e),tag=='polygon'
    if tag=='rect':
        x,y,w,h=[float(a.get(k,0)) for k in ('x','y','width','height')]
        return [(x,y),(x+w,y),(x+w,y+h),(x,y+h)],True
    if tag=='ellipse':
        x,y,rx,ry=[float(a[k]) for k in ('cx','cy','rx','ry')]
        return [(x+rx*math.cos(i*math.tau/64),y+ry*math.sin(i*math.tau/64)) for i in range(64)],True
    raise ValueError(tag)
def transformed(e, angle=0, pivot=(128,340), dx=0, dy=0, sx=1, sy=1, opacity=1):
    p,closed=shape(e); r=math.radians(angle); co,si=math.cos(r),math.sin(r)
    result=[]
    for x,y in p:
        x=(x-pivot[0])*sx; y=(y-pivot[1])*sy
        result.append((pivot[0]+x*co-y*si+dx,pivot[1]+x*si+y*co+dy))
    return {'p':result,'closed':closed,'fill':e.attrib.get('fill','none'),'stroke':e.attrib.get('stroke'), 'width':float(e.attrib.get('stroke-width',1)), 'opacity':opacity}
def line(p, color, width=3, opacity=1):
    return {'p':p,'closed':False,'fill':'none','stroke':color,'width':width,'opacity':opacity}
def polygon(p,color,opacity=1):
    return {'p':p,'closed':True,'fill':color,'stroke':None,'width':0,'opacity':opacity}
def player_pose(state,i,n):
    phase=i*math.tau/n; swing=0; lean=0; bob=0; arm_r=0; arm_l=0; leg_r=0; leg_l=0; squat=1
    if state=='Idle': bob=-2*math.sin(phase); arm_r=2*math.sin(phase); arm_l=-arm_r
    elif state=='Run':
        swing=math.sin(phase); lean=6; bob=4*abs(math.cos(phase)); arm_r=-28*swing; arm_l=28*swing; leg_r=25*swing; leg_l=-25*swing
    elif state=='JumpStart':
        squat=[.96,.9,1.01][i]; bob=[5,14,-3][i]; arm_r=[-8,-24,-42][i]; arm_l=-arm_r;leg_r=[5,8,-10][i];leg_l=-leg_r
    elif state=='Rise': lean=3; arm_r=-36+i*4;arm_l=24;leg_r=-12;leg_l=14;bob=-2
    elif state=='Fall': lean=-3;arm_r=-58+i*4;arm_l=50-i*4;leg_r=-6;leg_l=6
    elif state=='Land': squat=[.91,.95,1][i];bob=[13,7,0][i];arm_r=[-26,-12,0][i];arm_l=-arm_r
    elif state=='Attack':
        lean=[-7,-2,7,6,0][i];arm_r=[28,-25,-83,-72,-20][i];arm_l=[-20,-16,-12,-10,0][i];leg_r=[4,4,-4,-4,0][i];leg_l=-leg_r
    elif state=='Hurt': lean=[-12,-7,-1][i];arm_r=[-25,-15,0][i];arm_l=[-30,-12,0][i];bob=[1,0,0][i]
    elements=original('Player'); result=[]
    # Keep the original helmet, chest diamond, violet belt and boot silhouettes.
    for j,e in enumerate(elements):
        angle=0;pivot=(128,340)
        if j==8:angle=arm_l;pivot=(49,213)
        elif j==9:angle=arm_r;pivot=(208,213)
        elif j in (11,13):angle=leg_l;pivot=(92,348)
        elif j in (12,14):angle=leg_r;pivot=(165,348)
        part=transformed(e,angle,pivot)
        # Lean upper armor while leaving leg contact near the physical feet.
        proxy=ET.Element('polygon' if part['closed'] else 'polyline', {'points':' '.join(f'{x},{y}' for x,y in part['p']), 'fill':part['fill']})
        if part['stroke']:proxy.set('stroke',part['stroke']);proxy.set('stroke-width',str(part['width']))
        part=transformed(proxy,lean if j<11 else 0,(128,340),128,bob,sy=squat)
        if state=='Hurt' and i==0 and part['fill'] in (CYAN,VIOLET):part['fill']=WHITE
        result.append(part)
    # Small charge motes follow the forward arm. The existing slash/hit query is untouched.
    if state=='Attack' and i in (1,2,3):
        x=[0,388,450,428,0][i];y=[0,241,226,236,0][i]
        result.append(line([(x-18,y+12),(x-3,y-3),(x+5,y+8),(x+24,y-15)],CYAN,6,.9))
        result.append(polygon([(x+19,y-18),(x+25,y-24),(x+31,y-18),(x+25,y-12)],WHITE,.8))
    return result

def bot_pose(state,i,n):
    result=[]; elements=original('PatrolBot'); phase=i*math.tau/n
    for j,e in enumerate(elements):
        if state=='Patrol':
            part=transformed(e,dy=-2*math.sin(phase) if j<11 else 0,dx=(10+3*math.sin(phase)) if j==4 else 0)
        else:
            # Break armor into visible shards before fading; no gameplay object survives.
            progress=i/(n-1); ox=(-1 if j%2 else 1)*progress*26;oy=(j%3-1)*progress*26
            part=transformed(e,(j%2*2-1)*progress*14,(128,128),ox,oy,opacity=(1-progress)**.75)
        result.append(part)
    if state=='Patrol':
        for x in (70,186):
            a=phase*2;result.append(line([(x-13*math.cos(a),226-10*math.sin(a)),(x+13*math.cos(a),226+10*math.sin(a))],VIOLET,4))
    else:
        p=i/(n-1)
        for k in range(6):
            a=k*math.tau/6; x=128+math.cos(a)*(15+65*p);y=120+math.sin(a)*(15+65*p)
            result.append(polygon([(x-5,y),(x,y-8),(x+5,y),(x,y+8)],CYAN if k%2 else VIOLET,1-p))
    return result

def environment_pose(name,i,n):
    base=original(name); phase=i*math.tau/n; result=[]
    for j,e in enumerate(base):
        part=transformed(e,0,(0,0))
        if name=='ElectricBarrier' and j>=len(base)-2:
            part=transformed(e,dx=6*math.sin(phase),sx=1+.05*math.cos(phase),pivot=(64,256))
            if j==len(base)-2:part['stroke']=CYAN if i%2 else WHITE
        elif name=='MovingHazard' and j in (3,4):
            part=transformed(e,i*60,(128,128))
            if j==3:part['fill']=['#ee4e4c','#f16a49','#ff8539','#f16a49','#ee4e4c','#df494e'][i]
        elif name=='GoalPortal' and j in (11,12):
            part=transformed(e,dx=5*math.sin(phase))
        result.append(part)
    if name=='GoalPortal':
        y=130+(i/n)*380
        result.append(line([(106,y),(278,y)],CYAN,3,.45))
    return result

def color(hexcolor,opacity):
    h=hexcolor.lstrip('#');return tuple(int(h[x:x+2],16) for x in (0,2,4))+(round(255*opacity),)
def sheet(name,frames,view,out,ppu,fps,loop):
    w,h=view;pw,ph=out;scale=3
    image=Image.new('RGBA',(pw*len(frames),ph));svg=[]
    for i,items in enumerate(frames):
        im=Image.new('RGBA',(w*scale,h*scale));draw=ImageDraw.Draw(im)
        svg.append(f'<g id="frame-{i:02}" transform="translate({w*i},0)">')
        for q in items:
            coords=[(x*scale,y*scale) for x,y in q['p']];opacity=q['opacity']
            if opacity<=0:continue
            if q['fill']!='none':draw.polygon(coords,fill=color(q['fill'],opacity))
            if q['stroke']:
                draw.line(coords+([coords[0]] if q['closed'] else []),fill=color(q['stroke'],opacity),width=max(1,round(q['width']*scale)),joint='curve')
            attrs=f'points="'+ ' '.join(f'{x:.2f},{y:.2f}' for x,y in q['p'])+f'" fill="{q["fill"]}" opacity="{opacity:.3f}"'
            if q['stroke']:attrs+=f' stroke="{q["stroke"]}" stroke-width="{q["width"]}" stroke-linejoin="round"'
            svg.append(f'<{"polygon" if q["closed"] else "polyline"} {attrs}/>')
        svg.append('</g>');image.paste(im.resize((pw,ph),Image.Resampling.LANCZOS),(pw*i,0))
    image.save(OUTPUT/f'{name}.png')
    (SOURCE/f'{name}.svg').write_text(f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w*len(frames)} {h}">\n'+ '\n'.join(svg)+'\n</svg>\n')
    return dict(name=name,frames=len(frames),width=pw,height=ph,ppu=ppu,fps=fps,loop=loop)

specs=[]
for state,n,fps,loop in [('Idle',4,6,True),('Run',8,14,True),('JumpStart',3,30,False),('Rise',2,8,True),('Fall',2,8,True),('Land',3,30,False),('Attack',5,20,False),('Hurt',3,18,False)]:
    specs.append(sheet('Player'+state,[player_pose(state,i,n) for i in range(n)],(512,512),(256,128),128,fps,loop))
for state,n,fps,loop in [('Patrol',6,10,True),('Death',6,18,False)]:
    specs.append(sheet('PatrolBot'+state,[bot_pose(state,i,n) for i in range(n)],(256,256),(128,128),128,fps,loop))
for name,n,fps in [('ElectricBarrier',6,12),('MovingHazard',6,10),('GoalPortal',6,8)]:
    view,out,ppu=((384,640),(192,320),80) if name=='GoalPortal' else (((128,512),(128,128),128) if name=='ElectricBarrier' else ((256,256),(128,128),128))
    specs.append(sheet(name+'Loop',[environment_pose(name,i,n) for i in range(n)],view,out,ppu,fps,True))
(SOURCE/'manifest.json').write_text(json.dumps(specs,indent=2)+'\n')
print(f'Created {sum(s["frames"] for s in specs)} frames in {len(specs)} SVG/PNG sheets.')
