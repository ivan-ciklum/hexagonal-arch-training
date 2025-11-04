# 📡 Exercise 09: Adding OpenTelemetry (OTel for Friends)

Observability is not optional anymore — it’s your flashlight in the dark tunnels of distributed systems.  
Welcome to **OpenTelemetry**, or **OTel** for friends. 🪄  

Like the One Ring from *The Lord of the Rings*:  
> "One ring to rule them all, one ring to find them, one ring to bring them all and in the darkness bind them..."  
OTel centralizes your logs, traces, and metrics across every adapter.

---

## 🎯 Goal

Instrument your solution with **OpenTelemetry** to collect and export telemetry data (traces, logs, metrics) from every adapter and layer.

---

## 🧱 What You’ll Add

- **Tracing**: see how requests flow through adapters and Core.  
- **Metrics**: monitor performance and usage.  
- **Logging**: enrich logs with context (correlation IDs, spans, etc.).

---

## ⚙️ Steps

### 1. Install Dependencies
Add the following NuGet packages to each project that needs instrumentation:

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Exporter.Otlp
```

For EF Core, Redis, and PostgreSQL add their specific instrumentations as well:
```bash
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
dotnet add package OpenTelemetry.Instrumentation.StackExchangeRedis
dotnet add package OpenTelemetry.Instrumentation.Npgsql
```

---

### 2. Configure OpenTelemetry

In `Host.Api/Program.cs`:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t =>
    {
        t.AddAspNetCoreInstrumentation()
         .AddHttpClientInstrumentation()
         .AddEntityFrameworkCoreInstrumentation()
         .AddRedisInstrumentation()
         .AddNpgsql()
         .AddOtlpExporter();
    });
```

---

### 3. Apply to Each Layer

| Layer | Example Instrumentation |
|-------|--------------------------|
| **Adapter.Api** | Trace incoming HTTP requests |
| **Core** | Create spans in use cases |
| **Adapter.PostgreSQL** | EF Core instrumentation |
| **Adapter.Redis** | Redis instrumentation |
| **Adapter.Pulsar** | Custom spans for published messages |

---

### 4. View OTel in a Colletor
Add Jaeger in the **docker compose**, and add the environment variable in the read/write hosts to send telemetry to Jaeger.

### 5. Sit back and enjoy the show!
Observe the full trace of your `RegisterMember` use case in Jaeger.
Simulate a canary test with `WeightedRoundRobin`.
Now...... You can [move like Jagger 🕺](https://www.youtube.com/watch?v=suRsxpoAc5w)

### 6. Replace Jaeger with Standalone Aspire Dashboard (Docker)

- **Stop and Remove Jaeger:** Ensure your Jaeger container is stopped and your old Jaeger exporter configuration is removed from all services (the `OpenTelemetry.Exporter.Jaeger` NuGet package).

- **Add Aspire Dashboard to docker-compose.yml** Open your docker-compose.yml file and add the aspire-dashboard service. This service exposes the UI port (18888) and the OTLP port (18889), which is where your applications will send telemetry.
  ```yaml

  aspire-dashboard:
    image: mcr.microsoft.com/dotnet/aspire-dashboard:latest
    container_name: aspire-dashboard
    # Map UI port (18888) and OTLP port (18889)
    ports:
      - "18888:18888" 
      - "18889:18889" # OTLP (gRPC) Port to receive data
    restart: unless-stopped
  ```

- **Configure Your Services (OTLP Exporter):** In all your .NET services, use the `OpenTelemetry.Exporter.OpenTelemetryProtocol` package and remove the Jaeger package.

- **Environment Variable:** Set this in your service's configuration or Dockerfile:
```yaml
        OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:18889"
```
**If services are in the same Docker Compose network, use the container name: `http://aspire-dashboard:18889`)**.


-----



## 🧠 Key Takeaways

- OTel gives you a unified observability story across your hexagon.  
- Each adapter adds its own **instrumentation** — API, DB, Redis, Messaging.  
- Logs, metrics, and traces flow together to a single backend (Jaeger, Grafana, etc.).  
- Finally, debugging becomes storytelling. ✨  

> You now wield one telemetry to rule them all — use it wisely, architect.
