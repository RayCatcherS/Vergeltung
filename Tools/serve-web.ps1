<#
Server HTTP locale per testare una build Unity WebGL (Vergeltung), senza dipendenze.
Equivalente PowerShell di serve-web.py (utile se Python non e' installato).

Uso (da C:\Github\Vergeltung):
    powershell -ExecutionPolicy Bypass -File Tools\serve-web.ps1
    powershell -ExecutionPolicy Bypass -File Tools\serve-web.ps1 -Root Builds\Web -Port 8080

Poi apri:  http://localhost:8000   (Ctrl+C per fermare)

Cosa fa di "giusto":
- invia Content-Encoding gzip/br per i file .gz/.br (il loader Unity altrimenti fallisce);
- imposta il Content-Type del file DECOMPRESSO (.wasm -> application/wasm, ...);
- aggiunge COOP/COEP (servono se la build usa i thread; innocui altrimenti);
- streama i file (il Web.data.gz da ~600 MB non viene caricato in memoria).
#>
param(
    [string]$Root = "Builds\Web",
    [int]$Port = 8000
)

$ErrorActionPreference = "Stop"

# Risolvi root assoluto rispetto alla cartella corrente
$Root = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $Root))
if (-not (Test-Path $Root)) {
    Write-Host "[ERRORE] Cartella non trovata: $Root" -ForegroundColor Red
    Write-Host "Compila prima la build Web (output atteso in Builds\Web)."
    exit 1
}

# Content-Type del file "vero" (sotto eventuale compressione)
function Get-BaseContentType([string]$path) {
    $p = $path
    foreach ($c in @(".br", ".gz")) {
        if ($p.EndsWith($c)) { $p = $p.Substring(0, $p.Length - $c.Length); break }
    }
    switch -Regex ($p) {
        "\.wasm$"  { return "application/wasm" }
        "\.js$"    { return "application/javascript" }
        "\.json$"  { return "application/json" }
        "\.data$"  { return "application/octet-stream" }
        "\.html?$" { return "text/html; charset=utf-8" }
        "\.css$"   { return "text/css; charset=utf-8" }
        "\.png$"   { return "image/png" }
        "\.ico$"   { return "image/x-icon" }
        default    { return "application/octet-stream" }
    }
}

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$Port/")
try {
    $listener.Start()
} catch {
    Write-Host "[ERRORE] Impossibile aprire la porta ${Port}: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Prova un'altra porta:  ... -Port 8080"
    exit 1
}

Write-Host "Servo '$Root' su  http://localhost:$Port" -ForegroundColor Green
Write-Host "Ctrl+C per fermare."

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $req = $context.Request
        $res = $context.Response

        try {
            $rel = [System.Uri]::UnescapeDataString($req.Url.AbsolutePath.TrimStart("/"))
            if ([string]::IsNullOrEmpty($rel)) { $rel = "index.html" }
            $full = [System.IO.Path]::GetFullPath((Join-Path $Root $rel))

            # Evita path traversal fuori da Root
            if (-not $full.StartsWith($Root, [System.StringComparison]::OrdinalIgnoreCase) -or -not (Test-Path $full -PathType Leaf)) {
                $res.StatusCode = 404
                $res.Close()
                continue
            }

            # Header
            $res.ContentType = Get-BaseContentType $full
            if ($full.EndsWith(".br")) { $res.Headers["Content-Encoding"] = "br" }
            elseif ($full.EndsWith(".gz")) { $res.Headers["Content-Encoding"] = "gzip" }
            $res.Headers["Cross-Origin-Opener-Policy"] = "same-origin"
            $res.Headers["Cross-Origin-Embedder-Policy"] = "require-corp"
            $res.Headers["Cache-Control"] = "no-store"

            # Stream del file (no caricamento in RAM)
            $fs = [System.IO.File]::OpenRead($full)
            $len = $fs.Length
            try {
                $res.ContentLength64 = $len
                $fs.CopyTo($res.OutputStream)
            } finally {
                $fs.Close()
            }
            Write-Host ("  {0}  ({1:N0} bytes)" -f $rel, $len)
        } catch {
            Write-Host "  [warn] $($_.Exception.Message)" -ForegroundColor Yellow
        } finally {
            try { $res.Close() } catch {}
        }
    }
} finally {
    $listener.Stop()
    Write-Host "`nServer fermato."
}
