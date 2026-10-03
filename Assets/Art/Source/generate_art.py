"""Generate editable SVG geometry and antialiased PNG sprites. No Unity dependencies."""
from pathlib import Path
from PIL import Image, ImageDraw
from xml.sax.saxutils import escape
import math
ROOT=Path(__file__).resolve().parents[1]
N='#0b1223'; PANEL='#19283c'; EDGE='#34465e'; CYAN='#52e9f5'; WHITE='#d5fcff'; VIOLET='#9165ec'; ORANGE='#ff8539'; RED='#ee4e4c'
class Art:
 def __init__(self,name,w,h,out=None):
  self.name,self.w,self.h,self.out=name,w,h,out or (w,h);self.s=3
  self.im=Image.new('RGBA',(w*self.s,h*self.s));self.d=ImageDraw.Draw(self.im);self.svg=[]
 def poly(self,p,fill,stroke=None,width=1):
  self.d.polygon([(x*self.s,y*self.s) for x,y in p],fill=fill)
  if stroke:self.d.line([(x*self.s,y*self.s) for x,y in p+[p[0]]],fill=stroke,width=round(width*self.s),joint='curve')
  self.svg.append(f'<polygon points="'+ ' '.join(f'{x},{y}' for x,y in p)+f'" fill="{fill}"'+(f' stroke="{stroke}" stroke-width="{width}"' if stroke else '')+'/>')
 def rect(self,x,y,w,h,fill):self.poly([(x,y),(x+w,y),(x+w,y+h),(x,y+h)],fill)
 def line(self,p,color,width):
  self.d.line([(x*self.s,y*self.s) for x,y in p],fill=color,width=round(width*self.s),joint='curve')
  self.svg.append('<polyline points="'+' '.join(f'{x},{y}' for x,y in p)+f'" fill="none" stroke="{color}" stroke-width="{width}" stroke-linejoin="round"/>')
 def ellipse(self,x,y,w,h,fill,stroke=None,width=1):
  self.d.ellipse((x*self.s,y*self.s,(x+w)*self.s,(y+h)*self.s),fill=fill,outline=stroke,width=round(width*self.s))
  self.svg.append(f'<ellipse cx="{x+w/2}" cy="{y+h/2}" rx="{w/2}" ry="{h/2}" fill="{fill}"'+(f' stroke="{stroke}" stroke-width="{width}"' if stroke else '')+'/>')
 def save(self):
  self.im.resize(self.out,Image.Resampling.LANCZOS).save(ROOT/'Sprites'/f'{self.name}.png')
  (ROOT/'Source'/f'{self.name}.svg').write_text(f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {self.w} {self.h}">\n'+ '\n'.join(self.svg)+'\n</svg>\n')
# Runner: armored capsule silhouette, bright visor and power core, distinct boots.
a=Art('Player',256,512,(256,256))
a.poly([(80,5),(176,5),(207,40),(207,133),(177,162),(79,162),(49,133),(49,40)],N,CYAN,8)
a.poly([(69,49),(187,49),(187,99),(165,119),(69,119)],CYAN)
a.rect(82,60,91,12,WHITE);a.rect(152,88,23,12,N)
a.poly([(76,174),(180,174),(218,211),(196,340),(60,340),(38,211)],PANEL,EDGE,7)
a.line([(79,181),(98,209),(158,209),(177,181)],CYAN,8)
a.poly([(128,226),(153,261),(128,299),(103,261)],CYAN)
a.poly([(128,240),(139,260),(128,283),(117,260)],WHITE)
a.poly([(37,206),(61,221),(54,315),(24,306),(15,237)],N,CYAN,5)
a.poly([(219,206),(241,237),(232,306),(202,315),(195,221)],N,CYAN,5)
a.rect(63,323,130,16,VIOLET)
a.poly([(64,348),(120,348),(114,464),(96,496),(32,496),(32,474),(57,456)],PANEL,CYAN,5)
a.poly([(136,348),(192,348),(199,456),(224,474),(224,496),(160,496),(142,464)],PANEL,CYAN,5)
a.rect(34,478,64,19,N);a.rect(158,478,64,19,N);a.save()
# Bot: squat violet chassis, cyclops sensor, hazard bumper and wheel silhouette.
a=Art('PatrolBot',256,256)
a.poly([(39,39),(65,15),(191,15),(217,39),(238,171),(208,207),(48,207),(18,171)],N,VIOLET,8)
a.poly([(49,58),(73,37),(182,37),(207,58),(216,151),(40,151)],PANEL)
a.poly([(43,72),(213,72),(208,113),(48,113)],VIOLET)
a.rect(64,85,128,13,CYAN);a.rect(100,80,56,23,WHITE)
a.line([(74,135),(182,135)],EDGE,9)
for x in (65,106,147,188):a.line([(x,159),(x-15,181)],ORANGE,8)
a.rect(34,190,188,18,EDGE)
for x in (47,163):
 a.rect(x,207,46,38,N);a.line([(x+7,232),(x+39,232)],VIOLET,6)
a.save()
# Seamless platform tiles. Texture repetition is explicit in SpriteRenderer tiled mode.
a=Art('PlatformBody',1024,256)
a.rect(0,0,1024,256,N);a.rect(0,14,1024,224,PANEL);a.rect(0,0,1024,12,EDGE)
for x in (0,256,512,768):
 a.poly([(x+19,39),(x+231,39),(x+241,52),(x+241,174),(x+223,191),(x+20,191),(x+10,175),(x+10,52)],N,EDGE,3)
 a.line([(x+28,151),(x+80,99),(x+178,99),(x+225,53)],EDGE,6)
 a.rect(x+33,53,52,6,VIOLET);a.rect(x+199,154,18,8,VIOLET)
a.rect(0,229,1024,8,VIOLET);a.rect(0,242,1024,14,N);a.save()
a=Art('PlatformSurface',1024,64,(1024,41))
a.rect(0,0,1024,64,N);a.rect(0,1,1024,7,WHITE);a.rect(0,8,1024,13,CYAN);a.rect(0,23,1024,21,EDGE)
for x in range(0,1024,64):a.poly([(x,27),(x+17,27),(x+36,44),(x+19,44)],PANEL)
a.rect(0,49,1024,5,VIOLET);a.save()
# Spike profile exactly matches the existing one-unit triangular footprint.
a=Art('StaticSpike',256,256)
a.poly([(0,256),(128,0),(256,256)],N)
a.poly([(9,253),(128,11),(247,253)],ORANGE)
a.poly([(34,240),(128,46),(222,240)],PANEL)
a.poly([(89,235),(128,132),(166,235)],RED)
a.line([(125,39),(43,223)],'#ffc075',5);a.rect(9,243,238,13,RED);a.save()
# Barrier: danger rails frame a violet field; cyan remains secondary to orange warning.
a=Art('ElectricBarrier',128,512,(256,256))
a.rect(8,0,112,512,N);a.rect(21,26,86,460,'#272345')
a.rect(7,0,15,512,ORANGE);a.rect(106,0,15,512,ORANGE)
for y in range(48,480,64):a.line([(27,y),(101,y)],'#45406a',3)
a.poly([(0,0),(128,0),(113,39),(15,39)],PANEL,ORANGE,5)
a.poly([(15,473),(113,473),(128,512),(0,512)],PANEL,ORANGE,5)
a.line([(62,40),(42,112),(82,181),(46,260),(79,326),(46,407),(63,470)],CYAN,8)
a.line([(61,41),(49,111),(75,182),(52,260),(73,326),(53,406),(62,470)],WHITE,2);a.save()
# Moving hazard is drawn square before its existing 45-degree visual rotation.
a=Art('MovingHazard',256,256)
a.poly([(36,4),(220,4),(252,36),(252,220),(220,252),(36,252),(4,220),(4,36)],N,ORANGE,9)
a.poly([(55,30),(201,30),(226,55),(226,201),(201,226),(55,226),(30,201),(30,55)],PANEL,RED,4)
a.ellipse(61,61,134,134,N,ORANGE,8);a.ellipse(85,85,86,86,RED)
a.poly([(141,92),(115,130),(133,130),(115,165),(147,122),(128,122)],'#fff4d2')
for x,y in [(45,45),(193,45),(45,193),(193,193)]:a.rect(x,y,18,18,ORANGE)
a.save()
# Legacy test hazard receives a crate skin with an unmistakable warning face.
a=Art('DangerCrate',256,256)
a.poly([(24,2),(232,2),(254,24),(254,232),(232,254),(24,254),(2,232),(2,24)],N,RED,7)
a.rect(22,24,212,205,PANEL)
a.poly([(128,43),(220,208),(36,208)],ORANGE)
a.poly([(128,65),(199,193),(57,193)],N);a.rect(120,104,16,51,ORANGE);a.rect(120,169,16,16,ORANGE)
for x in (12,220):
 for y in (10,226):a.rect(x,y,22,16,EDGE)
a.save()
# Electric line texture: white core with translucent cyan edge, used by slash material.
a=Art('ElectricSlash',256,64)
for y in range(64):
 dist=abs((y+.5-32)/32);alpha=round(255*max(0,1-dist)**1.5)
 a.d.rectangle((0,y*a.s,256*a.s,(y+1)*a.s),fill=(225,255,255,alpha))
 a.svg.append(f'<rect x="0" y="{y}" width="256" height="1" fill="#e1ffff" opacity="{alpha/255:.4f}"/>')
a.save()
# Exit: large open frame, violet rails and cyan forward chevron. Decoration only.
a=Art('GoalPortal',384,640)
a.poly([(74,8),(310,8),(367,78),(367,570),(330,630),(54,630),(17,570),(17,78)],N,VIOLET,12)
a.poly([(80,50),(304,50),(326,88),(326,552),(303,592),(81,592),(58,552),(58,88)],'#13263a',CYAN,8)
a.line([(87,83),(87,546)],VIOLET,8);a.line([(298,83),(298,546)],VIOLET,8)
for y in range(125,540,64):a.line([(102,y),(282,y)],'#20374e',3)
a.poly([(139,237),(197,237),(267,320),(197,403),(139,403),(207,320)],CYAN)
a.line([(147,250),(190,250),(248,320),(190,390)],WHITE,5)
a.rect(68,601,248,23,PANEL);a.rect(101,605,180,5,CYAN);a.save()
# Subdued, seamless distant infrastructure. No foreground collision or motion.
a=Art('Background',1536,1024)
a.rect(0,0,1536,1024,'#0b1223')
for x in range(0,1536,128):a.line([(x,0),(x,1024)],'#101c30',2)
for y in range(0,1024,128):a.line([(0,y),(1536,y)],'#101c30',2)
for i,x in enumerate(range(55,1490,180)):
 h=[260,430,330,520][i%4];base=900
 a.poly([(x,base),(x,base-h+30),(x+25,base-h),(x+115,base-h),(x+145,base-h+35),(x+145,base)],'#142136')
 a.line([(x+21,base-30),(x+21,base-h+55),(x+111,base-h+55)],'#22304a',3)
 for y in range(base-h+80,base-30,60):a.rect(x+36,y,8,18,'#294058');a.rect(x+101,y,8,18,'#294058')
 a.line([(x+35,base-h+26),(x+95,base-h+26)],'#34305d',4)
for x,y in [(115,132),(390,76),(660,202),(930,115),(1200,252),(1420,83),(286,335),(805,389),(1092,440)]:a.rect(x,y,3,3,'#35536d')
a.line([(0,933),(1536,933)],'#1b2b41',4);a.line([(0,960),(1536,960)],'#242444',2);a.save()
print('Created 11 SVG sources and 11 raster sprites.')
