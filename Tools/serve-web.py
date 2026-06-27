#!/usr/bin/env python3
"""
Server HTTP locale per testare una build Unity WebGL (Vergeltung).

Perché non basta `python -m http.server`:
- le build Unity 6 sono compresse (Brotli `.br` o Gzip `.gz`): il server deve
  inviare l'header `Content-Encoding` corretto, altrimenti il loader fallisce;
- i file .wasm/.data/.js vogliono il giusto Content-Type;
- gli header COOP/COEP abilitano SharedArrayBuffer (multithreading) se la build lo usa.

Uso:
    python Tools/serve-web.py                 # serve ./Builds/Web sulla porta 8000
    python Tools/serve-web.py Builds/Web 8080 # cartella e porta custom

Poi apri:  http://localhost:8000
(Ctrl+C per fermare)
"""
import http.server
import os
import sys

ROOT = sys.argv[1] if len(sys.argv) > 1 else "Builds/Web"
PORT = int(sys.argv[2]) if len(sys.argv) > 2 else 8000

# Content-Type di base dei file "veri" sotto la compressione
BASE_TYPES = {
    ".wasm": "application/wasm",
    ".js": "application/javascript",
    ".json": "application/json",
    ".data": "application/octet-stream",
    ".symbols.json": "application/octet-stream",
}


class UnityWebGLHandler(http.server.SimpleHTTPRequestHandler):

    def end_headers(self):
        # Cross-origin isolation: necessario per i thread (SharedArrayBuffer).
        # Innocuo se la build non li usa. Rimuovili se carichi risorse cross-origin.
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        super().end_headers()

    def guess_type(self, path):
        # Per i file compressi (xxx.wasm.br / xxx.data.gz ...) il Content-Type
        # deve descrivere il file DECOMPRESSO, non l'archivio.
        stripped = path
        for comp in (".br", ".gz"):
            if stripped.endswith(comp):
                stripped = stripped[: -len(comp)]
                break
        for ext, ctype in BASE_TYPES.items():
            if stripped.endswith(ext):
                return ctype
        return super().guess_type(path)

    def send_head(self):
        # Aggiunge Content-Encoding per i file compressi.
        path = self.translate_path(self.path)
        if os.path.isfile(path):
            if path.endswith(".br"):
                self._content_encoding = "br"
            elif path.endswith(".gz"):
                self._content_encoding = "gzip"
            else:
                self._content_encoding = None
        return super().send_head()

    def send_header(self, keyword, value):
        super().send_header(keyword, value)
        # Inietta Content-Encoding subito dopo il Content-Type
        if keyword == "Content-type" and getattr(self, "_content_encoding", None):
            super().send_header("Content-Encoding", self._content_encoding)
            self._content_encoding = None


def main():
    if not os.path.isdir(ROOT):
        print(f"[ERRORE] Cartella non trovata: {ROOT}")
        print("Compila prima la build Web (output atteso in Builds/Web).")
        sys.exit(1)

    os.chdir(ROOT)
    handler = UnityWebGLHandler
    with http.server.ThreadingHTTPServer(("127.0.0.1", PORT), handler) as httpd:
        print(f"Servo '{ROOT}' su  http://localhost:{PORT}")
        print("Ctrl+C per fermare.")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nServer fermato.")


if __name__ == "__main__":
    main()
