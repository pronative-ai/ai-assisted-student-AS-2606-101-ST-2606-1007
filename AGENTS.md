# AGENTS.md — amazon-fbm-fulfillment-agent

## Commands

| Task | Command | Notes |
|---|---|---|
| Dev server | `npm run dev` | Uses `ts-node`, no build step needed |
| Build | `npm run build` | Outputs to `dist/` |
| Start (prod) | `npm start` | Runs `dist/index.js` |
| Prisma generate | `npm run db:generate` | Required after any schema change |
| Push schema | `npm run db:push` | Creates/updates SQLite `dev.db` |
| Prisma Studio | `npm run db:studio` | GUI for SQLite data |

No tests, linter, or formatter are configured. Add them if needed.

## Architecture

- **Framework**: Express 5 + TypeScript, CommonJS output
- **ORM**: Prisma 5 with SQLite (`DATABASE_URL=file:./dev.db`)
- **API prefix**: All routes under `/api/v1`
- **Entrypoint**: `src/index.ts` — loads dotenv, mounts router, starts on `PORT` (default 3000)
- **Services layer**: `src/services/` — `SpApiClient`, `OrderSyncService`, `BatchSplitService`
- Each service file creates its own `PrismaClient` instance

## Amazon SP-API Auth

Uses LWA (Login with Amazon) OAuth2 — refresh token exchanged for an access token. The client auto-refreshes tokens in the Axios request interceptor. Requires 6 env vars:

```
AMAZON_SP_API_CLIENT_ID
AMAZON_SP_API_CLIENT_SECRET
AMAZON_REFRESH_TOKEN
AMAZON_AWS_ACCESS_KEY_ID
AMAZON_AWS_SECRET_ACCESS_KEY
AMAZON_AWS_ROLE_ARN
```

`getSpApiConfig()` in `src/config/index.ts` throws on missing vars at startup.

## Express 5 Quirk

`req.params` and `req.query` are typed as `string | string[]` in Express 5. Always cast with `as string` when assigning to a typed variable.

## Database

- SQLite file `dev.db` at project root
- After editing `prisma/schema.prisma`, always run `db:generate` then `db:push`
- Models: `AmazonOrder`, `OrderItem`, `OrderBatch`, `OrderBatchItem`, `SyncLog`
- Composite unique on `OrderItem`: `(amazonOrderId, orderItemId)`

## Logging

Winston writes to `logs/combined.log` (all levels, 10MB rotated) and `logs/error.log` (errors only, 5MB rotated). Console output always active. Set `LOG_LEVEL` env for verbosity.

## Workflows

| Type | Location | What it does |
|---|---|---|
| GitHub Actions CI | `.github/workflows/ci.yml` | Runs on push/PR to `main`: `npm ci` → `prisma generate` → `tsc --noEmit` |
| Pre-commit hook | `hooks/pre-commit` | Runs `tsc --noEmit` before each commit |

Install the pre-commit hook: `powershell -ExecutionPolicy Bypass -File scripts\setup-hooks.ps1`

## Missing (watch for)

- No test framework
- No CI/CD config (historical — CI workflow added)
