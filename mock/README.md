# mock - KONAMINET front-server mock

Status (2026-11): historical harness. Hosted play now runs WebGL through the WebFront gate to the self-hosted core + plugin (docs/28); this mock stays for offline protocol experiments and as the reference for the transport seam. The mechanics below describe the mock as built and still hold.

Stands in for the real AOG front server so the decompiled game can talk HTTP in
`-KONAMINET` mode without `proxy-jp100.konami` existing. It emulates the whole transport seam:
TCP connect to the proxy port, `CONNECT` tunnel, TLS with an in-memory self-signed certificate,
then plain HTTP/1.1 request/response with XML bodies.

- `mock/Program.cs` + `mock/MockServer.csproj`: .NET 8 console app, no external packages.
- `mock/specs/*.xml`: one response template per endpoint path (48 files, derived from the parsers).
- `mock/FINDINGS.md`: endpoint -> parser class -> required / modeled / skipped nodes table.
- `mock/mock.log.txt`: created at runtime (append mode), never committed content of interest.

## Run

```
dotnet build mock
dotnet run --project mock              # listens on 127.0.0.1:8080
dotnet run --project mock 18080        # alternative port (first positional arg)
```

Or the built binary: `mock\bin\Debug\net8.0\MockServer.exe [port]`.

Port note: on the machine this was built on, 8080 was already held by an unrelated
`llama-server` process, so the smoke tests below ran with `18080`. On a clean box the default
`8080` is what the game expects (`NetEnvironmentProxy` proxy port), no argument needed.

## Hosts entries the caller must provide

The mock does not touch the hosts file. For the game to reach it, these need to resolve:

```
127.0.0.1   proxy-jp100.konami        # required: the game connects to this proxy host
```

`aog03-stg.pvt.mo.konami.net` does not need an entry: it appears only as the CONNECT authority and
inside the `Host` header, and the mock answers every tunnel regardless of authority. The client
accepts any certificate (all `SslPolicyErrors` are bypassed in
`api/recovered/Assembly-CSharp/Konami.System.Net/HttpWebRequestor.cs:82-93`), and curl is run with
`-k` for the same reason.

## Behavior

- `TcpListener` on `127.0.0.1:<port>` (default 8080), one thread per connection.
- `CONNECT host:port HTTP/1.1` is answered with `HTTP/1.1 200 Connection established`, then the
  socket is wrapped in an `SslStream` as server. Certificate: RSA 2048, SHA-256 self-signed, 10
  year validity, SAN dNSName list starting at `aog03-stg.pvt.mo.konami.net` and extended with any
  hostname that shows up in a CONNECT authority or a `Host` header (rebuilt in memory when a new
  host appears). PFX round trip because Schannel rejects ephemeral key sets for server auth.
- Plain requests whose request line carries an absolute URI (`POST http://host/aog/login HTTP/1.1`)
  are served on the same code path without TLS. Origin-form request lines (`GET /aog/login`) also
  work, so a direct curl without `-x` is fine.
- Routing: the `/aog` prefix is stripped and the remaining endpoint name selects
  `mock/specs/<name>.xml`; longer paths are matched by segment and then by longest spec name, so
  `gget_stamp_info` is never stolen by `gget`, and `reconnect/...` style URLs resolve. Anything
  else gets `mock/specs/_default.xml` (`<res><serv_st><code>0</code></serv_st><code>0</code></res>`).
- Every reply is `200 OK`, `Content-Type: text/xml; charset=utf-8`, `Content-Length`,
  `Connection: close`, body UTF-8. Requests with `Content-Length` or chunked bodies are drained
  fully so logs are complete. Bodies are logged raw (percent-encoding not decoded).
- Two dynamic tokens are substituted outside XML comments:
  - `{{user_id}}`: value of the posted `user_id` field (used by `login.xml`, XML escaped).
  - `{{seq}}`: monotonic request counter (used by `gget`, `gpost`, `gchat` for fetch windows and
    stamp ids).
- Console and `mock/mock.log.txt` (append, lock guarded) log UTC timestamps, `START`, `LISTEN`,
  `CONNECT_OPEN`, `TLS_OK`/`TLS_ERR`, `REQUEST` with method, full target, body, spec served and
  byte count, `CONNECT_CLOSE`, `ACCEPT_ERR`, `CONN_ERR`.

## Response contract

Body is XML text; the root element name is arbitrary (`RequestApiBase.cs:113`). `code` is read out
of `serv_st` when `serv_st` exists, and the parse of an endpoint aborts when `serv_st` is missing
(`RequestApiBase.cs:160-179`), so every template carries `<serv_st><code>0</code></serv_st>` plus
payload as siblings of `serv_st`. A missing `code` is not treated as success: it falls through to
the 630 default and throws `NetworkResponseException`, so `code 0` is mandatory.

## Smoke test used

Server started as `MockServer.exe 18080` (port free on this box), then:

```
curl.exe -s -x http://127.0.0.1:18080 -k https://aog03-stg.pvt.mo.konami.net/aog/appli_info -d "matching_ver=1"
curl.exe -s -x http://127.0.0.1:18080 -k https://aog03-stg.pvt.mo.konami.net/aog/login -d "web_id=1&area_id=1&user_id=MOCKUSER42&bypass_auth=true&guest=1"
curl.exe -s -x http://127.0.0.1:18080 -k https://aog03-stg.pvt.mo.konami.net/aog/gget -d "pcuid=MOCKUSER42&gserv_id=1&must=1.0.0/MOCKUSER42/1/0/1000001/0"
curl.exe -s -x http://127.0.0.1:18080 -k https://aog03-stg.pvt.mo.konami.net/aog/no_such_thing -d "a=1"
curl.exe -s -x http://127.0.0.1:18080 http://aog03-stg.pvt.mo.konami.net/aog/keep_alive -d "web_id=1&pcuid=MOCKUSER42"
```

Returned XML in every case. Sample of the responses:

```
  <expire_seconds>3600</expire_seconds>
  <info_data kind="pro_stats">eyJub3dfcHJvX3N0YXRzIjpmYWxzZSwiUHJvU3RheURhdGFzIjpbXX0=</info_data>
```

```
    <session_id>mock_pcuid_MOCKUSER42</session_id>
    <user_id>MOCKUSER42</user_id>
```

`gget` shows the counter substituted (`<cell_sno start="5" count="0" />`, then `start="6"` on the
next poll), the unknown endpoint returns `_default.xml`, and the absolute-URI style request routes
to `keep_alive.xml`. Corresponding log line:

```
2026-10-24T03:27:29.379Z REQUEST seq=4 POST host=aog03-stg.pvt.mo.konami.net target=/aog/login spec=login.xml bytes=576 body=web_id=1&area_id=1&user_id=MOCKUSER42&bypass_auth=true&guest=1
```

## Limits

- Templates are static: no game state is simulated, so a match never advances past the first
  command fetch (`cell_sno count 0`), and `client_state_read` only carries the `version_game` and
  `player_game` states.
- Unknown `one_kind` values on `client_state_read` still return the same body.
- GET requests work (no body), but every real endpoint is POST in this build.
