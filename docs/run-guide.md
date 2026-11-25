# Run guide

How to get the claims platform running on a fresh machine, try it, and shut it down. Everything runs locally in
Docker; nothing is hosted (the brief forbids public hosting).

## 1. What you need

| Tool | Why | Check |
| --- | --- | --- |
| Git | Clone the repository | `git --version` |
| Docker Desktop (or Docker Engine with Compose v2) | Runs PostgreSQL, Kafka, the API and the SPA | `docker compose version` |

That is all for running the app. The .NET 10 SDK and Node.js 24 are only needed to develop or run the tests outside
Docker (see [section 7](#7-optional-run-the-tests)).

Give Docker at least **4 GB of memory** (Docker Desktop → Settings → Resources). Kafka and the two image builds need it.

These ports must be free: **4200** (SPA), **5080** (API), **5432** (PostgreSQL), **9092** (Kafka), **8081** (Kafka UI).
To use others, see [Ports](#ports-already-in-use).

## 2. Get the code

The GitHub repository is private. Sign in to GitHub with an account that has access, then:

```bash
git clone https://github.com/Luckden/chubb_assignment.git
cd chubb_assignment
```

No access to GitHub? Copy the folder from the original machine, without `node_modules`, `bin`, `obj` and `dist`:

```bash
# on the original machine, from the repository root
git archive --format=zip -o claims-platform.zip main
```

## 3. Start everything

```bash
docker compose up --build -d
```

The first run downloads images and builds the API and SPA: allow **5–10 minutes**. Later runs take seconds.

On startup the API creates the database schema, seeds four demo users and creates its Kafka topics. Check that it is
ready:

```bash
docker compose ps                         # api, web, postgres, kafka, kafka-ui: running (postgres, kafka: healthy)
curl http://localhost:5080/health/ready   # Healthy
```

On Windows PowerShell, use `curl.exe` or open the URL in a browser.

| Open | What |
| --- | --- |
| http://localhost:4200 | The app (claimants and claims staff) |
| http://localhost:5080/openapi/v1.json | API contract (OpenAPI) |
| http://localhost:8081 | Kafka UI: topic `claims.claim-events` shows every claim event |

## 4. Try it

There is no sign-in. Use **Acting as** in the toolbar to switch between the four demo users.

| User | Role |
| --- | --- |
| Alice Tan, Bob Lim | Claimants |
| Carol Wong, Dave Ng | Claims officers (same team) |

1. As **Alice Tan**, choose **Report an incident**, fill in the form and submit. The claim opens with its history.
2. Switch to **Carol Wong**. Open **Work queue → Unassigned**, open the claim and choose **Pick up**.
3. Under **Ask the claimant**, send a question. The claim moves to **Waiting on claimant**.
4. Switch to **Alice**. The bell shows a notification; open it and answer the question.
5. Switch to **Carol**. Under **Decision**, settle or reject (a rejection needs a reason). Alice gets a notification.
6. As Carol, open **Team**: claimed exposure on open claims, and each officer's workload and recent decisions.

Changes made as one user reach the other within 10 seconds, without a reload. To try the API directly, see
[Try the API](../README.md#try-the-api) in the README.

## 5. Stop it

```bash
docker compose down        # stop and remove the containers; your claims are kept
docker compose down -v     # also delete the database and Kafka data (start from empty next time)
```

`docker compose up -d` starts it again without rebuilding. After pulling new code, use `docker compose up --build -d`.

## 6. Troubleshooting

### Ports already in use

Copy `.env.example` to `.env` and change the port, for example `WEB_PORT=4300`, then `docker compose up -d`. Find
what holds a port with `lsof -i :4200` (macOS/Linux) or `netstat -ano | findstr :4200` (Windows).

### The build seems stuck

Build with plain output to see which step it is on, then start without building:

```bash
docker compose build --progress plain api web
docker compose up -d
```

If a step makes no progress for several minutes, restart Docker Desktop and run the build again.

### The API keeps restarting

Look at its log: `docker compose logs api --tail 50`. The usual causes are PostgreSQL or Kafka not healthy yet
(`docker compose ps`; the API waits for both, so give Kafka a minute on the first start) or too little memory for
Docker.

### The app says "API: unreachable"

The SPA reaches the API through its own server (`/api` on port 4200). Check `docker compose ps` shows `api` running,
then `docker compose logs web --tail 20`.

### The app shows an old version after pulling

Rebuild: `docker compose up --build -d`, then hard-refresh the browser (Ctrl+Shift+R / Cmd+Shift+R).

### Start from empty

`docker compose down -v`, then `docker compose up --build -d`.

## 7. Optional: run the tests

Backend tests start their own PostgreSQL and Kafka containers (Testcontainers), so Docker must be running. They need
the .NET 10 SDK; the frontend tests need Node.js 24.

```bash
dotnet test backend                     # 111 tests
cd frontend
npm ci
npx playwright install chromium         # first time only
npm run lint && npm run format:check
npm test                                # unit tests
npm run e2e                             # end-to-end, API mocked
```

`npm run e2e` reuses whatever is already on port 4200. If the Docker stack is running, either stop it
(`docker compose down`) so the tests start their own dev server, or test the stack itself with
`E2E_BASE_URL=http://localhost:4200 npm run e2e` (PowerShell: `$env:E2E_BASE_URL='http://localhost:4200'; npm run e2e`).

Windows with Smart App Control may block the test DLL ("An Application Control policy has blocked this file"). Run the
backend tests in a container instead, from Git Bash: `./scripts/test-backend-in-docker.sh`.

## 8. Develop without rebuilding images

Run the infrastructure in Docker and the app from source, so changes reload instantly:

```bash
docker compose up -d postgres kafka kafka-ui
dotnet run --project backend/src/Claims.Api     # API on http://localhost:5080
cd frontend && npm ci && npm start              # SPA on http://localhost:4200, proxies /api to the API
```

Stop the `api` and `web` containers first if they are running (`docker compose stop api web`), since they use the same
ports.
