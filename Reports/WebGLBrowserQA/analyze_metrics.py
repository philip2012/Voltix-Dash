import json,glob,statistics
from pathlib import Path
p=Path(__file__).parent

def stats(v):
 v=sorted(v)
 return {} if not v else dict(count=len(v),mean=statistics.mean(v),p50=v[len(v)//2],p95=v[int(len(v)*.95)],p99=v[int(len(v)*.99)],max=v[-1])
def analyze(file,start,end):
 d=json.loads((p/file).read_text());s=d['summary'];frames=[r['dt'] for r in d['intervals'] if start<=r['t']<=end];calls=[r for r in d['callbacks'] if start<=r['t']<=end and r.get('draws',0)>0];return dict(source=file,windowMs=[start,end],fps=1000/statistics.mean(frames),intervalMs=stats(frames),renderCallbackMs=stats([r['cpu'] for r in calls]),drawCallsPerRenderedCallback=stats([r['draws'] for r in calls]),heapBytes=s.get('wasmHeapBytes'))
run=analyze('BrowserMetrics-1791286555.json',8606.3,70056.6)
dprfiles=[]
for file in p.glob('BrowserMetrics*.json'):
 d=json.loads(file.read_text());s=d['summary']
 if s.get('canvas',{}).get('width')==1920:dprfiles.append((file,d))
file,d=dprfiles[-1];reset=[x['t'] for x in d['summary']['timeline'] if x.get('kind')=='reset'][-1];dpr=analyze(file.name,reset+1000,max(r['t'] for r in d['intervals'])-500)
flow=json.loads((p/'BrowserMetrics-1791286555.json').read_text());s=flow['summary'];landings=[a for a in s['audioSources'] if .089<(a.get('duration') or 0)<.091 and (a.get('ended') or a['created'])-a['created']>50];clicks=[x for x in s['timeline'] if x['kind']=='pointer' and x['target']=='unity-canvas'];loads=[]
for x in clicks:
 nextLanding=next((a for a in landings if x['t']<=a['created']<x['t']+1000),None)
 if nextLanding: loads.append(dict(click=x,firstLandingCueDelayMs=nextLanding['created']-x['t'],largestCallbackMs=max((r['cpu'] for r in flow['callbacks'] if x['t']<=r['t']<=x['t']+500),default=0)))
result=dict(fullTraversal=run,doubledRenderResolution=dpr,sceneLoads=loads,note='Browser rAF intervals and instrumented Unity callback wall time, not Unity Profiler main-thread or GPU timers. Observer overhead is present. Load delay is an upper bound to first grounded-player landing cue, not exact scene deserialization time.')
(p/'PerformanceAnalysis.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
