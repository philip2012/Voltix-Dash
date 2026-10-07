#!/usr/bin/env python3
"""Local Unity WebGL server: serves existing Brotli bytes with correct headers."""
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
import argparse, json, time

parser=argparse.ArgumentParser()
parser.add_argument('--root',default='Builds/WebGL')
parser.add_argument('--port',type=int,default=8765)
args=parser.parse_args()
root=Path(args.root).resolve()
log_path=Path(__file__).with_name('HTTPRequests.jsonl')

class Handler(SimpleHTTPRequestHandler):
    def __init__(self,*a,**kw):super().__init__(*a,directory=str(root),**kw)
    def translate_path(self,path):
        if path.startswith('/qa/'):
            clean=path.split('?',1)[0][4:]
            if '..' in clean:return str(root/'invalid')
            return str(Path(__file__).parent.resolve()/clean)
        return super().translate_path(path)
    def do_POST(self):
        if self.path!='/qa/results':self.send_error(404);return
        import base64
        obj=json.loads(self.rfile.read(int(self.headers['Content-Length'])))
        folder=Path(__file__).parent
        if obj['kind']=='screenshot':
            name=Path(obj['name']).name
            (folder/name).write_bytes(base64.b64decode(obj['data'].split(',',1)[1]))
        else:
            (folder/('BrowserMetrics-'+str(int(time.time()))+'.json')).write_text(json.dumps(obj,indent=2))
        self.send_response(200);self.end_headers();self.wfile.write(b'ok')
    def guess_type(self,path):
        uncompressed=path[:-3] if path.endswith('.br') else path
        if uncompressed.endswith('.wasm'):return 'application/wasm'
        if uncompressed.endswith('.data'):return 'application/octet-stream'
        if uncompressed.endswith('.js'):return 'application/javascript'
        if uncompressed.endswith('.json'):return 'application/json'
        return super().guess_type(uncompressed)
    def end_headers(self):
        if self.path.split('?',1)[0].endswith('.br'):
            self.send_header('Content-Encoding','br')
            self.send_header('Vary','Accept-Encoding')
        self.send_header('Cache-Control','no-store')
        self.send_header('X-Content-Type-Options','nosniff')
        super().end_headers()
    def log_message(self,fmt,*a):
        event={'utc':time.time(),'method':self.command,'path':self.path,'message':fmt%a}
        with log_path.open('a') as f:f.write(json.dumps(event)+'\n')
        print(json.dumps(event),flush=True)

print(f'Serving {root} at http://127.0.0.1:{args.port}',flush=True)
ThreadingHTTPServer(('127.0.0.1',args.port),Handler).serve_forever()
