# Host.ReverseProxy - API Gateway with YARP

## ?? Purpose

This project implements a **Reverse Proxy / API Gateway** using **YARP (Yet Another Reverse Proxy)** to provide a unified entry point for the Club Management system's Read and Write APIs.

## ??? Architecture

```
???????????????????????????????????????????????????
?          API Gateway (YARP Proxy)               ?
?              http://localhost:5000              ?
???????????????????????????????????????????????????
                  ?
        ?????????????????????
        ?                   ?
??????????????????  ????????????????
?  Host.Write    ?  ?  Host.Read   ?
?  (Commands)    ?  ?  (Queries)   ?
?  + gRPC        ?  ?              ?
??????????????????  ????????????????
```

## ?? Features

### 1. **Routing**
- `/api/write/**` ? Routes to Host.Write (Commands + gRPC)
- `/api/read/**` ? Routes to Host.Read (Queries)

### 2. **Load Balancing Policies**

The proxy supports multiple load balancing strategies:

| Environment | Policy | Description | Use Case |
|------------|---------|-------------|----------|
| **Production** | `PowerOfTwoChoices` | Randomly selects two destinations and picks the one with fewer active requests | Default - Best balance between performance and distribution |
| **Development** | `RoundRobin` | Sequential distribution across destinations | Testing and predictable behavior |
| **Canary** | `WeightedRoundRobin` | 90% stable + 10% canary | Safe deployments with minimal risk |

### 3. **Health Checks**
- **Active Health Checks**: Polls `/health` endpoint every 30 seconds
- **Passive Health Checks**: Monitors transport failures and automatically removes unhealthy instances
- **Auto-Reactivation**: Unhealthy instances rejoin after recovery period

### 4. **Path Transformation**
Removes the `/api/write` or `/api/read` prefix before forwarding:
- Request: `GET /api/read/members`
- Forwarded as: `GET /members`

## ?? Configuration

### Production (appsettings.json)
```json
{
  "ReverseProxy": {
    "Clusters": {
      "write-cluster": {
        "LoadBalancingPolicy": "PowerOfTwoChoices",
        "Destinations": {
          "write-instance-1": { "Address": "http://localhost:5001" },
          "write-instance-2": { "Address": "http://localhost:5002" }
        }
      }
    }
  }
}
```

### Development (appsettings.Development.json)
Uses `RoundRobin` for predictable testing:
```bash
Request 1 ? Instance 1
Request 2 ? Instance 2
Request 3 ? Instance 1
...
```

### Canary (appsettings.Canary.json)
Uses `WeightedRoundRobin` for gradual rollouts:
```bash
90% ? Stable version (port 5001/4095)
10% ? Canary version (port 5003/4097)
```

## ??? Usage

### Running the Gateway

```bash
# Default (Production settings)
cd src/Host.ReverseProxy
dotnet run

# Development mode with RoundRobin
dotnet run --environment Development

# Canary testing mode
dotnet run --environment Canary
```

### Testing Different Load Balancing Policies

#### 1. **PowerOfTwoChoices (Production)**
Start two write instances and observe smart distribution:
```bash
# Terminal 1: Instance 1
cd src/Host.Write
dotnet run --urls "http://localhost:5001"

# Terminal 2: Instance 2
cd src/Host.Write
dotnet run --urls "http://localhost:5002"

# Terminal 3: Gateway
cd src/Host.ReverseProxy
dotnet run

# Terminal 4: Load test
for ($i=1; $i -le 10; $i++) {
    Invoke-RestMethod -Uri "http://localhost:5000/api/write/members" -Method Get
}
```

#### 2. **RoundRobin (Development)**
Perfect for debugging - predictable, sequential distribution:
```bash
cd src/Host.ReverseProxy
dotnet run --environment Development

# Each request will alternate between instances
curl http://localhost:5000/api/write/members  # ? Instance 1
curl http://localhost:5000/api/write/members  # ? Instance 2
curl http://localhost:5000/api/write/members  # ? Instance 1
```

#### 3. **Canary Deployment**
Simulate a new version rollout with minimal risk:
```bash
# Terminal 1: Stable version (90% traffic)
cd src/Host.Write
dotnet run --urls "http://localhost:5001"

# Terminal 2: Canary version (10% traffic)
cd src/Host.Write
# Apply your new changes here
dotnet run --urls "http://localhost:5003"

# Terminal 3: Gateway in Canary mode
cd src/Host.ReverseProxy
dotnet run --environment Canary

# Terminal 4: Generate traffic
for ($i=1; $i -le 100; $i++) {
    Invoke-RestMethod -Uri "http://localhost:5000/api/write/members"
    # ~90 requests go to stable, ~10 to canary
}
```

## ?? Monitoring

### Gateway Info Endpoint (Development only)
```bash
GET http://localhost:5000/
```

Response shows current configuration:
```json
{
  "message": "Club Management API Gateway",
  "routes": {
    "write": "/api/write/**",
    "read": "/api/read/**"
  },
  "loadBalancing": {
    "policy": "RoundRobin"
  }
}
```

### Health Check
```bash
GET http://localhost:5000/health
```

## ?? Load Balancing Comparison

### Scenario: 1000 concurrent requests

| Policy | Distribution | Use Case |
|--------|-------------|----------|
| **PowerOfTwoChoices** | Instance1: ~500, Instance2: ~500 | Production - Best performance |
| **RoundRobin** | Instance1: 500, Instance2: 500 | Development - Predictable |
| **WeightedRoundRobin** | Stable: 900, Canary: 100 | Canary deployments |
| **FirstAlphabetical** | Instance1: 1000, Instance2: 0 | Failover testing |
| **LeastRequests** | Dynamic based on load | Mixed capacity instances |

## ?? Key Concepts

### 1. **Why YARP?**
- Built on ASP.NET Core middleware
- Configuration-based (no code for basic routing)
- Production-ready with Microsoft support
- Extensible with custom middleware

### 2. **PowerOfTwoChoices Algorithm**
- Randomly picks 2 destinations
- Sends request to the one with fewer active requests
- Near-optimal load distribution with minimal overhead
- Default recommendation for production

### 3. **Health Checks**
- **Active**: Proactive polling of health endpoints
- **Passive**: Monitoring actual traffic failures
- **Graceful Degradation**: Automatically removes failing instances
- **Auto-Recovery**: Re-introduces healthy instances

### 4. **Path Transformation**
Essential for clean APIs:
```
Client ? Gateway                  Gateway ? Backend
/api/write/members/123    ?      /members/123
/api/read/members?limit=10 ?     /members?limit=10
```

## ?? CQRS Integration

The proxy maintains **CQRS separation**:

```
Commands (Write):
POST   /api/write/members      ? Host.Write
PUT    /api/write/members/123  ? Host.Write
DELETE /api/write/members/123  ? Host.Write

Queries (Read):
GET    /api/read/members        ? Host.Read
GET    /api/read/members/123    ? Host.Read
```

## ?? Traffic Flow Examples

### Example 1: Register Member
```
Client ? POST /api/write/members
         ?
    Gateway (YARP)
         ?
    PowerOfTwoChoices selects Instance 1
         ?
    Host.Write:5001 ? POST /members
         ?
    PostgreSQL + Pulsar
```

### Example 2: Query Members
```
Client ? GET /api/read/members
         ?
    Gateway (YARP)
         ?
    RoundRobin ? Instance 2
         ?
    Host.Read:4096 ? GET /members
         ?
    Redis Cache ? PostgreSQL
```

## ?? Security Considerations

While this exercise focuses on routing and load balancing, in production you should add:

1. **Authentication/Authorization**: JWT validation at gateway level
2. **Rate Limiting**: Prevent abuse
3. **CORS**: Configure cross-origin policies
4. **HTTPS**: TLS termination
5. **Request Logging**: Track all incoming traffic

## ?? Further Reading

- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Load Balancing Policies](https://microsoft.github.io/reverse-proxy/articles/load-balancing.html)
- [Health Checks](https://microsoft.github.io/reverse-proxy/articles/dests-health-checks.html)
- [Transforms](https://microsoft.github.io/reverse-proxy/articles/transforms.html)

## ?? Exercise Goals Met

? Single entry point for all APIs  
? Load balancing across multiple instances  
? Health monitoring and auto-recovery  
? CQRS separation maintained  
? Zero changes to Core domain  
? Canary deployment simulation  

---

**Next Steps**: Consider adding authentication middleware, rate limiting, or distributed tracing to your gateway!
