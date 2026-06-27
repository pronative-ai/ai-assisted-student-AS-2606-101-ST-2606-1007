# OpenCode Telemetry Contract

## Intent

- **Intent ID**: `intent-2606-101-1007-0002`
- **Type**: SubIntent — Telemetry Ingestion and Per-Student Aggregation API
- **Repository**: `pronative-ai/ai-assisted-student-AS-2606-101-ST-2606-1007`

---

## 1. Supported Signals

### 1.1 Metrics

| Signal | Type | Semantic | Unit |
|---|---|---|---|
| `opencode.token.usage` | Counter | cumulative | tokens |

**Supported token type values:**

- `input`
- `output`
- `reasoning`
- `cacheRead`
- `cacheCreation`

Only these five type values are accepted. Payloads with unsupported type values are silently skipped (not persisted).

### 1.2 Log Events

| Event Name | Description |
|---|---|
| `api_request` | Captured OpenCode API request event |
| `api_error` | Captured OpenCode API error event |

Log records with event names other than `api_request` or `api_error` are silently skipped (not persisted).

---

## 2. Storage Contract

### 2.1 Token Usage Metric Record (`token_usage_metric`)

Stored in Cosmos DB container `token-usage-metrics`, partitioned by `student_key`.

```json
{
  "id": "guid-or-deterministic-id",
  "record_type": "token_usage_metric",
  "student_key": "deployment-scoped-student",
  "metric_name": "opencode.token.usage",
  "token_type": "input",
  "cumulative_value": 1200,
  "unit": "tokens",
  "sample_timestamp_utc": "2026-01-01T10:00:00Z",
  "ingested_at_utc": "2026-01-01T10:00:05Z",
  "source_transport": "otlp_http_json",
  "raw_attributes": {
    "type": "input"
  }
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | string | yes | Unique identifier (auto-generated GUID) |
| `record_type` | string | yes | Fixed value: `"token_usage_metric"` |
| `student_key` | string | yes | Stable student attribution from deployment config |
| `metric_name` | string | yes | Exact OTLP metric name: `"opencode.token.usage"` |
| `token_type` | string | yes | One of `input`, `output`, `reasoning`, `cacheRead`, `cacheCreation` |
| `cumulative_value` | long | yes | Raw cumulative counter value from the OTLP data point |
| `unit` | string | yes | Measurement unit: `"tokens"` |
| `sample_timestamp_utc` | DateTime (UTC) | yes | Timestamp from the OTLP data point, normalized to UTC |
| `ingested_at_utc` | DateTime (UTC) | yes | Server timestamp when the record was persisted |
| `source_transport` | string | yes | Transport identifier: `"otlp_http_json"` |
| `raw_attributes` | Dictionary | no | Original OTLP attributes preserved for diagnostics |

### 2.2 OpenCode Log Record (`opencode_log`)

Stored in Cosmos DB container `opencode-logs`, partitioned by `student_key`.

```json
{
  "id": "guid-or-deterministic-id",
  "record_type": "opencode_log",
  "student_key": "deployment-scoped-student",
  "event_name": "api_error",
  "event_timestamp_utc": "2026-01-01T10:00:00Z",
  "ingested_at_utc": "2026-01-01T10:00:03Z",
  "severity_text": "Error",
  "trace_id": "optional-trace-id",
  "span_id": "optional-span-id",
  "body": "captured log body",
  "attributes": {
    "provider": "example"
  }
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | string | yes | Unique identifier (auto-generated GUID) |
| `record_type` | string | yes | Fixed value: `"opencode_log"` |
| `student_key` | string | yes | Stable student attribution from deployment config |
| `event_name` | string | yes | One of `"api_request"`, `"api_error"` |
| `event_timestamp_utc` | DateTime (UTC) | yes | Log event timestamp, normalized to UTC |
| `ingested_at_utc` | DateTime (UTC) | yes | Server timestamp when the record was persisted |
| `severity_text` | string | no | Log severity (e.g., `"Error"`, `"Info"`) |
| `trace_id` | string | no | OTLP trace correlation identifier |
| `span_id` | string | no | OTLP span correlation identifier |
| `body` | string | no | Log event body text |
| `attributes` | Dictionary | no | OTLP log attributes preserved for analysis |

---

## 3. API Contract

### 3.1 OTLP Ingestion Endpoints

#### `POST /otlp/v1/metrics`

Accepts OpenCode `opencode.token.usage` metric payloads.

- **Content-Type**: `application/json` (primary), `application/x-protobuf` (accepted with 202)
- **Request body**: OTLP HTTP JSON metrics payload (`OtlpMetricRequest`)
- **Response (200)**: `{ "accepted": true, "records_stored": <int>, "unsupported_skipped": 0 }`
- **Response (202)**: Protobuf accepted but not processed (informational)
- **Response (400)**: Invalid or unreadable JSON payload
- **Response (415)**: Unsupported content type

Only metric name `opencode.token.usage` with supported token type values is persisted.

#### `POST /otlp/v1/logs`

Accepts OpenCode `api_request` and `api_error` log payloads.

- **Content-Type**: `application/json` (primary), `application/x-protobuf` (accepted with 202)
- **Request body**: OTLP HTTP JSON logs payload (`OtlpLogRequest`)
- **Response (200)**: `{ "accepted": true, "records_stored": <int>, "unsupported_skipped": 0 }`
- **Response (202)**: Protobuf accepted but not processed (informational)
- **Response (400)**: Invalid or unreadable JSON payload
- **Response (415)**: Unsupported content type

Only log events named `api_request` or `api_error` are persisted.

### 3.2 Token Usage Aggregation Endpoint

#### `GET /api/opencode/token-usage`

Returns per-student token usage aggregated over a caller-specified time window. Computes deltas from cumulative counter samples.

**Query Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `start_time` | string (ISO 8601 UTC) | yes | Start of the aggregation window |
| `end_time` | string (ISO 8601 UTC) | yes | End of the aggregation window |

**Validation Rules:**

- Both `start_time` and `end_time` are required
- Values must be valid ISO 8601 timestamps (non-UTC timestamps are interpreted as UTC)
- `start_time` must not be after `end_time`
- Invalid or missing parameters return HTTP 400 with a descriptive error message

**Response (200):**

```json
{
  "start_time": "2026-01-01T10:00:00Z",
  "end_time": "2026-01-01T11:00:00Z",
  "calculation_mode": "cumulative_delta",
  "baseline_policy": "use_latest_at_or_before_start_else_earliest_available_within_or_immediately_before_window",
  "totals": {
    "input": 45,
    "output": 20,
    "reasoning": 0,
    "cacheRead": 10,
    "cacheCreation": 0
  },
  "warnings": []
}
```

| Field | Type | Description |
|---|---|---|
| `start_time` | DateTime (UTC) | Requested window start |
| `end_time` | DateTime (UTC) | Requested window end |
| `calculation_mode` | string | Fixed: `"cumulative_delta"` |
| `baseline_policy` | string | Describes the baseline selection algorithm |
| `totals.input` | long | Delta token count for type `input` |
| `totals.output` | long | Delta token count for type `output` |
| `totals.reasoning` | long | Delta token count for type `reasoning` |
| `totals.cacheRead` | long | Delta token count for type `cacheRead` |
| `totals.cacheCreation` | long | Delta token count for type `cacheCreation` |
| `warnings` | string[] | Non-empty when fallback baselines or counter decreases are detected |

**Response (400):**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Invalid request",
  "status": 400,
  "detail": "Missing required parameter: start_time."
}
```

---

## 4. Calculation Behavior

### 4.1 Delta Computation

Because `opencode.token.usage` is a cumulative Counter, all reported usage is computed as:

```
window_total = closing_sample_value - opening_baseline_value
```

Per token type, the API:

1. Finds the **opening baseline** — the latest cumulative sample at or before `start_time`
2. Finds the **closing sample** — the latest cumulative sample at or before `end_time`
3. Returns `closing - opening` as the window total for that type

### 4.2 Baseline Fallback Rule

If no sample exists at or before `start_time` for a given token type:

1. Use the **earliest available sample within the window** (`start_time` to `end_time`) as the baseline
2. A warning is added to the response indicating fallback behavior was used

If no sample exists anywhere (neither before the window nor within it), both baseline and closing are null, and the type total is returned as `0`.

### 4.3 Counter Decrease / Reset Detection

If the closing sample value is less than the baseline value (indicating a counter reset or data anomaly):

- The type total is returned as `0` (not a negative value)
- A warning is added to the response: `"Counter decrease detected for type '<type>': <baseline> -> <closing>. Returning 0 for this type."`
- Diagnostic telemetry is emitted to Application Insights via `ILogger`

### 4.4 Empty / Sparse Data

- If no records exist for a given token type within the window, that type's total is `0`
- If no records exist for any token type, all five totals are `0` with no warnings
- The response always contains all five token type fields; types without data are returned as `0`

### 4.5 Examples

| Scenario | Baseline | Closing | Result |
|---|---|---|---|
| 10:00=100, 11:00=145, window 10:00-11:00 | 100 | 145 | 45 |
| 09:55=80, 10:30=110, window 10:00-10:30 | 80 (before start) | 110 | 30 |
| Only in-window sample at 10:15=50, window 10:00-11:00 | 50 (fallback) | 50 | 0 |
| No data for type, window 10:00-11:00 | null | null | 0 |
| 10:00=200, 11:00=150 (decrease), window 10:00-11:00 | 200 | 150 | 0 (with warning) |

---

## 5. Operational Endpoints

### `GET /health`

Returns service health.

```json
{ "status": "healthy" }
```

### `GET /ready`

Returns readiness, including Cosmos DB connectivity check.

```json
{ "status": "ready" }
```

Returns HTTP 503 if Cosmos DB is unreachable.

---

## 6. Observability

- Application Insights is configured via `APPLICATIONINSIGHTS_CONNECTION_STRING` or `ApplicationInsights:ConnectionString`
- The following events emit diagnostic telemetry:
  - Ingestion request success/failure (via ASP.NET Core middleware + `IngestionExceptionFilter`)
  - Aggregation request success/failure (via ASP.NET Core middleware)
  - Counter decrease / reset detection (via `ILogger` warning in `TokenUsageAggregationService`)
  - Unsupported content type or malformed payloads (via endpoint problem responses)
  - Health endpoint status (via `/health` and `/ready`)

---

## 7. Security

- All secrets and sensitive configuration are sourced from Azure Key Vault via `DefaultAzureCredential`
- No secrets, connection strings, or keys are hard-coded in source code
- Cosmos DB connection string and student key are resolved from Key Vault at startup
- The API surface is read-only with respect to OpenCode telemetry; no create, update, delete, replay, or write-back endpoints exist
