"""Original geometric SVG/PNG artwork for Reactor Heart. Reuses the established palette."""
from pathlib import Path
# Load only the existing geometry writer; never regenerate existing artwork.
source = Path(__file__).with_name('generate_art.py').read_text().split('# Runner:')[0]
exec(compile(source, str(Path(__file__).with_name('generate_art.py')), 'exec'))
a=Art('LaunchPad',256,96)
a.poly([(6,80),(24,48),(232,48),(250,80),(232,94),(24,94)],N,CYAN,5)
a.rect(34,54,188,12,VIOLET);a.line([(54,39),(85,10),(116,39)],CYAN,8);a.line([(140,39),(171,10),(202,39)],CYAN,8);a.line([(32,80),(224,80)],WHITE,4);a.save()
a=Art('Checkpoint',128,256)
a.poly([(16,248),(16,216),(42,204),(42,30),(64,8),(86,30),(86,204),(112,216),(112,248)],N,CYAN,5)
a.poly([(64,32),(94,78),(64,127),(34,78)],VIOLET,CYAN,4);a.poly([(64,52),(80,78),(64,106),(48,78)],WHITE)
a.line([(64,133),(64,196)],CYAN,6);a.line([(25,229),(103,229)],CYAN,6);a.save()
a=Art('SentryTurret',256,192)
a.poly([(36,184),(25,151),(50,123),(70,34),(100,16),(188,16),(212,43),(219,145),(235,159),(226,184)],N,VIOLET,6)
a.poly([(83,41),(174,41),(197,63),(195,130),(58,130)],PANEL)
a.poly([(108,61),(190,61),(208,86),(243,86),(243,116),(157,116),(111,102)],N,RED,5)
a.rect(125,79,66,9,RED);a.rect(211,91,28,17,WHITE);a.ellipse(57,54,40,40,RED,WHITE,3)
a.line([(47,152),(212,152)],VIOLET,5);a.rect(60,164,138,11,EDGE);a.save()
a=Art('SentryProjectile',128,64)
a.poly([(4,32),(38,12),(81,12),(124,32),(81,52),(38,52)],RED)
a.poly([(22,32),(48,22),(87,22),(107,32),(87,42),(48,42)],WHITE);a.line([(2,32),(34,32)],VIOLET,4);a.save()
