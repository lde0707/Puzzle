from http.server import SimpleHTTPRequestHandler,ThreadingHTTPServer
from pathlib import Path
from urllib.parse import urlsplit,unquote
ROOT=Path(__file__).resolve().parents[3]
class Handler(SimpleHTTPRequestHandler):
 def translate_path(self,path):
  p=unquote(urlsplit(path).path)
  for prefix,base in [('/source/',ROOT/'Sites/mallang-orbit/public'),('/legacy/',ROOT/'Releases/itchio/2026-11-02-v1.1.0/web'),('/candidate/',ROOT/'Releases/itchio/2026-11-04-v1.2.1/web'),('/qa/',ROOT/'Tools/Itch/qa')]:
   if p.startswith(prefix):
    target=(base/p[len(prefix):]).resolve()
    if target.is_relative_to(base.resolve()):return str(target)
  return str(ROOT/'Tools/Itch/qa/not-found')
ThreadingHTTPServer(('127.0.0.1',8093),Handler).serve_forever()
