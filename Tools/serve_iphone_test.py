"""Serve only the generated Unity web test on the local network. Ctrl+C stops it."""
import argparse
import functools
import http.server
import socket
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument("--port", type=int, default=8080)
parser.add_argument("--bind", default="127.0.0.1")
parser.add_argument("--stop-file", type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[1] / "Android Puzzle" / "Builds" / "iPhoneWebTest"
if not (root / "index.html").is_file():
    raise SystemExit("Web build missing. In Unity: Puzzle > iPhone Test > Build Touch Test (Web).")

class Handler(http.server.SimpleHTTPRequestHandler):
    extensions_map = {**http.server.SimpleHTTPRequestHandler.extensions_map,
                      ".wasm": "application/wasm", ".data": "application/octet-stream",
                      ".js": "application/javascript"}

    def end_headers(self):
        self.send_header("Cache-Control", "no-store")
        super().end_headers()

    def list_directory(self, path):
        self.send_error(403, "Directory listing disabled")
        return None

class TestServer(http.server.ThreadingHTTPServer):
    allow_reuse_address = False

    def server_bind(self):
        if hasattr(socket, 'SO_EXCLUSIVEADDRUSE'):
            self.socket.setsockopt(socket.SOL_SOCKET, socket.SO_EXCLUSIVEADDRUSE, 1)
        super().server_bind()

handler = functools.partial(Handler, directory=str(root))
with TestServer((args.bind, args.port), handler) as server:
    print(f"Serving Unity web test on port {args.port}. Ctrl+C to stop.", flush=True)
    print("On iPhone Safari, open http://<PC IPv4 address>:" + str(args.port) + "/", flush=True)
    try:
        server.timeout = 0.5
        while args.stop_file is None or not args.stop_file.exists():
            server.handle_request()
    except KeyboardInterrupt:
        pass

