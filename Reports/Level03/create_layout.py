import json
from pathlib import Path
p=[]; pads=[]; hazards=[]; bots=[]; turrets=[]
def deck(name,a,b,y,section,**extra):
 d=dict(name=name,minX=a,maxX=b,top=y,section=section,thickness=extra.pop('thickness',1),**extra);p.append(d);return d
def pad(name,deck,x,force=25,horizontal=0):pads.append(dict(name=name,platform=deck['name'],x=x,y=deck['top']+.12,force=force,horizontal=horizontal))
def hazard(kind,deck,x): hazards.append(dict(name=kind+'_'+deck['name'],type=kind,platform=deck['name'],x=x,y=deck['top']+({'spikes':.5,'barrier':1.25,'moving':.85}[kind])))
def bot(name,deck,x):bots.append(dict(name=name,platform=deck['name'],x=x,y=deck['top']+.6,patrolDistance=3,speed=2))
def turret(name,deck,x,swivel=False):turrets.append(dict(name=name,platform=deck['name'],x=x,y=deck['top']+.8,swivel=swivel))
def moving(name,a,b,y,section,dx=0,dy=0,speed=2.8):return deck(name,a,b,y,section,thickness=.5,moving=True,pointB=[dx,dy],speed=speed,endpointPause=.55 if dy else .2)
for i,(a,b,y) in enumerate([(0,12,0),(14.2,26.2,.6),(28.4,42.4,.2),(44.6,58.6,1.2)],1): deck('Entry_%02d'%i,a,b,y,'FastEntry')
bot('PatrolBot_Entry',p[2],35.4);hazard('spikes',p[3],52)
for i,(a,b,y,x) in enumerate([(60.8,72.8,1.2,70.4),(75,87,7.2,84.4),(89.2,101.2,13.2,98.6)],1):
 d=deck('Launch_%02d'%i,a,b,y,'LaunchIntroduction');pad('LaunchPad_Intro_%d'%i,d,x)
deck('LaunchExit',103.4,121,19.2,'LaunchIntroduction')
moving('Lift_Intro_Horizontal',123.2,131.2,19.2,'MovingIntroduction',dx=6)
deck('MovingLanding',139.4,153.4,19.6,'MovingIntroduction')
moving('Lift_Intro_Vertical',155.6,163.6,19.6,'MovingIntroduction',dy=2)
deck('MovingExit',165.8,181.8,22.8,'MovingIntroduction')
d=deck('IntroRecovery',123.2,139.4,12.2,'MovingIntroduction',optional=True);pad('LaunchPad_Recovery',d,134.5,26,7)
d=deck('FirstSentryDeck',184,206,22.8,'FirstSentry');turret('Sentry_Intro',d,201)
deck('FirstSentryCover',189,192,24,'FirstSentry',optional=True,thickness=1.2)
d=deck('Ascent_01',208.2,222.2,23.9,'ReactorAscent');pad('LaunchPad_Ascent_A',d,220.2)
d=deck('Ascent_02',224.4,238.4,29.9,'ReactorAscent',thickness=.5);pad('LaunchPad_Ascent_B',d,227.5)
d=deck('Ascent_03',209.5,222.2,35.9,'ReactorAscent',thickness=.5);bot('PatrolBot_Ascent',d,215.7)
moving('Lift_Ascent',199.3,207.3,35.9,'ReactorAscent',dy=3.4,speed=2.8)
d=deck('AscentSentryDeck',209.5,222.2,40.5,'ReactorAscent');turret('Sentry_Ascent',d,216.7)
d=deck('AscentBarrierDeck',224.4,238.4,41.4,'ReactorAscent');hazard('barrier',d,232.4)
d=deck('AscentChargeDeck',240.6,250.6,42.6,'ReactorAscent');pad('LaunchPad_Ascent_C',d,248.2,26)
deck('AscentSummit',252.8,266.8,49,'ReactorAscent')
deck('AscentExit',269,281,49,'ReactorAscent')
deck('AscentExit_Second',283.2,297.2,50,'ReactorAscent')
deck('AscentExit_Third',299.4,311.4,49,'ReactorAscent')
deck('CheckpointDeck',313.6,333.6,49,'Checkpoint')
deck('CoreApproach',335.8,347.8,49,'CoreCrossing')
moving('Lift_Core_A',350,358,49,'CoreCrossing',dx=8,speed=3.5)
d=deck('CoreIsland',368.2,380.2,49.9,'CoreCrossing');turret('Sentry_Core',d,372.8)
moving('Lift_Core_B',382.4,390.4,49.9,'CoreCrossing',dx=8,speed=3.5)
deck('CoreSentryDeck',400.6,418.6,49.9,'CoreCrossing')
deck('CoreCover',369.8,371.3,51.1,'CoreCrossing',optional=True,thickness=1.2)
for i,(a,b,y) in enumerate([(420.8,432.8,48.1),(435,447,46.3),(449.2,461.2,44.5),(463.4,477.4,42.7),(479.6,493.6,40.9),(495.8,509.8,39.1),(512,528,40.1),(530.2,544.2,38.3),(546.4,560.4,36.5)],1):deck('Descent_%02d'%i,a,b,y,'OverloadDescent')
hazard('moving',p[-7],455.2);hazard('spikes',p[-6],470.4);hazard('barrier',p[-5],487);pad('LaunchPad_Redirect',p[-4],507.6,20,7)
d=deck('CombatDeck',562.6,598.6,36.5,'CombatChamber');bot('PatrolBot_Chamber_A',d,572);bot('PatrolBot_Chamber_B',d,585);turret('Sentry_ChamberOverwatch',d,594.3,True)
# The arena overwatch swivels and also covers the opening of the final run.
deck('Final_01',600.8,618.8,36.5,'FinalRun')
moving('Lift_Final_A',621,629,37.3,'FinalRun',dx=5,speed=3)
d=deck('Final_02',636.2,648.2,38.3,'FinalRun');hazard('spikes',d,642.2)
d=deck('Final_03',650.4,662.4,39.4,'FinalRun');pad('LaunchPad_Final',d,660,24)
deck('Final_04',664.6,680.6,44.8,'FinalRun')
moving('Lift_Final_B',682.8,690.8,44.8,'FinalRun',dy=2,speed=3)
d=deck('Final_05',693,709,47.8,'FinalRun');hazard('barrier',d,701)
d=deck('Final_06',711.2,731.2,47,'FinalRun');bot('PatrolBot_Final',d,721.2)
d=deck('Final_07',733.4,753.4,48.2,'FinalRun');hazard('moving',d,743.4)
d=deck('Final_08',755.6,765.6,49.4,'FinalRun')
d=deck('Final_09',767.8,783.8,48.4,'FinalRun');hazard('spikes',d,775.8)
deck('Final_10',786,796,49.6,'FinalRun')
deck('ExitApproach',798.2,810.2,49.6,'Goal')
deck('ExitDeck',812.4,832.4,49.6,'Goal')
# Varied recovery shelves in the ascent and final approach; normal controller jumps.
# Add five short terraces to increase variety without long empty running.
# Split selected generous decks into two reachable shelves with a mild height change.
for name in ['MovingExit','CoreApproach','Descent_08','Final_01']:
 idx=next(i for i,d in enumerate(p) if d['name']==name);d=p[idx];a,b,y=d['minX'],d['maxX'],d['top'];mid=(a+b)/2
 d['maxX']=mid-.8
 q=dict(d,name=name+'_Step',minX=mid+1.4,maxX=b,top=y+.8)
 p.insert(idx+1,q)
# Raised approaches let barrier jumps clear comfortably instead of using the apex limit.
for name,end in [('AscentBarrierDeck',229.8),('Descent_05',485.2),('Final_05',698)]:
 idx=next(i for i,d in enumerate(p) if d['name']==name);d=p[idx]
 approach=dict(d,name=name+'_Approach',maxX=end,top=d['top']+.8)
 d['minX']=end+1.0;p.insert(idx,approach)
route=[d['name'] for d in p if not d.get('optional')]
r=dict(concept='Level03 — Reactor Heart',platforms=p,hazards=hazards,enemies=bots,turrets=turrets,pads=pads,route=route,spawn=[4,1.1],goal=[825.4,51.6],checkpoint=[323.2,50.1],cameraBounds=dict(minX=0,maxX=832.4,minY=-5,maxY=60))
Path('Reports/Level03/Layout.json').write_text(json.dumps(r,indent=2)+'\n');print(len(p),'platforms',len(bots),'bots',len(turrets),'turrets',len(pads),'pads',sum(d.get('moving',False) for d in p),'moving')
