"""Three original deterministic synthesized reactor cues; no samples or external assets."""
from pathlib import Path
import runpy,math
synth=runpy.run_path(str(Path(__file__).with_name('generate_placeholder_sfx.py')))['synth']
synth('Launch',.23,lambda t,u:280+1000*u,brightness=.2,decay=2.3)
synth('Checkpoint',.35,lambda t,u:[659.25,830.61,987.77][min(2,int(u*3))],brightness=.15,decay=1.8)
synth('TurretFire',.13,lambda t,u:1050-750*u,brightness=.25,noise=.25,decay=4)
