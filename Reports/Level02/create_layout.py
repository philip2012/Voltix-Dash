import json
from pathlib import Path
p=[]
def platform(name,a,b,top,section,route='shared',thickness=1):
 p.append(dict(name=name,minX=a,maxX=b,top=top,section=section,route=route,thickness=thickness))
for i,(a,b,y) in enumerate([(0,12,0),(14.2,24.2,.65),(26.4,38.4,1.35),(40.6,54.6,.8),(56.8,68.8,1.8)],1):platform(f'Fast_{i:02}',a,b,y,'FastOpening')
shaft_y=2.55
shaft_rises=[1.65,1.85,1.6,1.9,1.7,1.8,1.65,1.85,1.65,1.9,1.7]
for i in range(12):
 a=[70.8,69.8,71.8,70.2,71.2,70.8][i//2] if i%2==0 else 80.6
 b=78.8 if i%2==0 else [88.6,89.6,87.6,89,88.2,88.6][i//2]
 platform(f'Shaft_{i:02}',a,b,round(shaft_y,2),'VerticalShaft',thickness=.15)
 if i<11:shaft_y+=shaft_rises[i]
platform('ShaftExit',90.6,104.6,21.8,'VerticalShaft')
for i,(a,b,y) in enumerate([(106.8,126.8,21),(129,139,22.4),(141.2,161.2,21.8),(163.4,177.4,23)],1):platform(f'Timing_{i:02}',a,b,y,'MovingTiming')
for i,(a,b,y) in enumerate([(179.6,195.6,23),(197.8,207.8,24.2),(210,230,23.3),(232.2,242.2,24.5),(244.4,260.4,23.6)],1):platform(f'Corridor_{i:02}',a,b,y,'ElectricCorridor')
platform('RouteFork',262.6,276.6,24.8,'RouteChoice')
for i,(a,b,y) in enumerate([(278.8,286.8,26.55),(289,299,28.4),(301.2,313.2,29.3),(315.4,329.4,28.4),(331.6,341.6,29.3),(343.8,359.8,29.3)],1):platform(f'Upper_{i:02}',a,b,y,'RouteChoice','upper',.4)
platform('Upper_07',362,370,27.3,'RouteChoice','upper',.4)
for i,(a,b,y) in enumerate([(278.8,296.8,23.4),(299,321,22.6),(323.2,339.2,23.3),(341.4,353.8,24.2)],1):platform(f'Lower_{i:02}',a,b,y,'RouteChoice','lower')
platform('RouteRejoin',356,382.2,24.8,'RouteChoice')
platform('CombatDeck',370.2,404.2,24.8,'CombatPlatform')
for i,(a,b,y) in enumerate([(406.4,416.4,23),(418.6,432.6,21.2),(434.8,442.8,19.4),(445,459,17.6),(461.2,471.2,16),(473.4,485.4,14.4)],1):platform(f'Descent_{i:02}',a,b,y,'Descent')
platform('Breather',487.6,501.6,14.4,'Breather')
for i,(a,b,y) in enumerate([(503.8,519.8,15.6),(522,532,16.8),(534.2,550.2,15.8),(552.4,572.4,17),(574.6,594.6,16.2),(596.8,606.8,17.5),(609,631,17.5),(633.2,649.2,16.8)],1):platform(f'Gauntlet_{i:02}',a,b,y,'FinalGauntlet')
platform('ExitApproach',651.4,661.4,17.7,'Goal')
platform('ExitDeck',663.6,683.6,17.7,'Goal')
shift=False
for row in p:
 if row['name']=='CombatDeck':shift=True
 if shift:row['minX']=round(row['minX']+14.2,2);row['maxX']=round(row['maxX']+14.2,2)
by={x['name']:x for x in p}
h=[]
def hazard(name,kind,floor,x):h.append(dict(name=name,type=kind,platform=floor,x=round(x+(14.2 if by[floor]['section'] in ['CombatPlatform','Descent','FinalGauntlet'] else 0),2),y=by[floor]['top']+{'spikes':.5,'barrier':1,'moving':1.15}[kind]))
hazard('Spikes_Shaft','spikes','Shaft_03',86.8)
hazard('Mover_Timing_A','moving','Timing_01',115.8);hazard('Mover_Timing_B','moving','Timing_03',151.2)
for i,x in [(1,187.6),(3,218),(5,251.9)]:hazard(f'Barrier_Corridor_{i}','barrier',f'Corridor_{i:02}',x)
hazard('Barrier_LowerRoute','barrier','Lower_01',292.1);hazard('Mover_LowerRoute','moving','Lower_03',331.2)
hazard('Spikes_CombatExit','spikes','CombatDeck',401)
hazard('Mover_Descent','moving','Descent_02',426)
hazard('Spikes_FinalEntry','spikes','Gauntlet_01',511.5);hazard('Barrier_Final','barrier','Gauntlet_03',542)
hazard('Mover_Final','moving','Gauntlet_05',584.6);hazard('Spikes_FinalExit','spikes','Gauntlet_08',640.2)
bots=[]
for name,floor,x,d in [('PatrolBot_Shaft','Shaft_06',74.8,2),('PatrolBot_Optional','Lower_02',310,3),('PatrolBot_Arena_A','CombatDeck',380.5,3),('PatrolBot_Arena_B','CombatDeck',394.5,3),('PatrolBot_Final_A','Gauntlet_04',562.4,3),('PatrolBot_Final_B','Gauntlet_07',620,3)]:bots.append(dict(name=name,platform=floor,x=round(x+(14.2 if by[floor]['section'] in ['CombatPlatform','Descent','FinalGauntlet'] else 0),2),y=by[floor]['top']+.6,patrolDistance=d,speed=2,optional=name=='PatrolBot_Optional'))
shared=[x['name'] for x in p if x['route']=='shared'];idx=shared.index('RouteFork')+1
routes={r:shared[:idx]+[x['name'] for x in p if x['route']==r]+shared[idx:] for r in ['upper','lower']}
data=dict(concept='Level02 — Overcharge Sector',platforms=p,hazards=h,enemies=bots,routes=routes,spawn=[4,1.1],goal=[690.8,19.7],cameraBounds=dict(minX=0,maxX=697.8,minY=-5,maxY=36))
Path('Reports/Level02/Layout.json').write_text(json.dumps(data,indent=2))
print(len(p),'platforms',len(h),'hazards',len(bots),'bots; route distances:')
for r,names in routes.items():
 print(r,sum(abs((by[a]['minX']+by[a]['maxX'])/2-(by[b]['minX']+by[b]['maxX'])/2) for a,b in zip(names,names[1:])))
