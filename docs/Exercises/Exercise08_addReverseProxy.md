# 🔀 Exercise 08: Add a Reverse Proxy with YARP

Your system is growing — APIs, gRPC, adapters everywhere! It’s time to give them a single entry point.  
Enter the **Reverse Proxy** aka __api gateway__, your app’s friendly traffic cop.

---

## 🎯 Goal

Add a **Reverse Proxy** to your architecture using **YARP (Yet Another Reverse Proxy)**.  
You’ll centralize routing, simplify scaling, and open the door to modern patterns like API gateways.

Play with load balancing. 

---

## 🧭 Why YARP?

YARP is lightweight, extensible, and built on ASP.NET Core middleware.  
It allows you to:  
- Route traffic dynamically.  
- Load balance between multiple instances.  
- Add authentication, logging, or rate limiting globally.  

All without touching your Core or adapters.

YARP offers several built-in load balancing policies:

| Policy Name | Description | Primary Use Case |
| :--- | :--- | :--- |
| **`PowerOfTwoChoices`** | Randomly selects two destinations and chooses the one with the fewest active requests. | **Recommended Default.** Provides an excellent balance between performance and even distribution. |
| **`RoundRobin`** | Distributes requests sequentially and cyclically (if you have two node, destination 1, then 2, then 1, then2 , etc.). | Simple and predictable, best for destinations with identical capacity. |
| **`LeastRequests`** | Forwards the request to the destination that currently has the lowest number of active requests. | Ideal if destinations have varied processing capabilities or different latencies. |
| **`Random`** | Selects a destination purely at random for each request. | Useful for basic testing or when uniform balance is not critical. |
| **`FirstAlphabetical`** | Simply chooses the first destination in the alphabetically ordered list. | Primarily used for simple *failover* scenarios or testing (not true load balancing). |
| **`CookieStickySessions`** | Routes requests based on cookies to maintain session affinity. | Best for stateful applications where user sessions need to be preserved. |
| **`WeightedRoundRobin`** | Similar to Round Robin but allows assigning weights to destinations for uneven load distribution. | Useful when some destinations have higher capacity than others |


---

## 🏗 Updated Structure

```text
src/
 ├─ Core/
 ├─ Adapters/
 │   ├─ Adapter.Api/
 │   ├─ Adapter.gRPC/
 │   ├─ Adapter.PostgreSQL/
 │   └─ Adapter.Redis/
 ├─ Host.Api/
 └─ Host.ReverseProxy/    # 🆕 New Host project for YARP
```

---

## ⚙️ Steps

### 1. Create the Project
Add a new ASP.NET Core project: `Host.ReverseProxy`.

### 2. Configure YARP
Install the NuGet package:
```bash
dotnet add package Yarp.ReverseProxy
```

Add configuration in `appsettings.json`:
```json
"ReverseProxy": {
  "Routes": {
    "api-route": {
      "ClusterId": "api-cluster",
      "Match": { "Path": "/api/{**catch-all}" },
      "Transforms": [ { "PathRemovePrefix": "/api" } ]
    }
  },
  "Clusters": {
    "api-cluster": {
      "Destinations": {
        "api": { "Address": "http://localhost:5001" }
      }
    }
  }
}
```

### 3. Enable the Proxy
In `Program.cs`:

```csharp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();
app.Run();
```
### 4 . Play with Load balancing

Test with `RoundRobin` and `FirstAlphabetical`

### 5 . Simulate deployment with canary test

A "canary test" is a software testing technique that deploys a new version of a product to a small subset of real users, similar to how canaries were used in mines to detect toxic gases. The goal is to identify and resolve issues in a controlled environment with minimal impact, before the update is fully released to the rest of the user base.


---

## 💡 Why This Matters

The Reverse Proxy becomes your **front door**.  
You can now:  
- Deploy multiple API instances and load balance them.  
- Route gRPC, REST, and static content through one place.  
- Add cross-cutting middleware without touching adapters.

---

## 🧠 Key Takeaways

- YARP simplifies routing and scalability.  
- The Core remains untouched.  
- You’re one step closer to microservice nirvana. 🌐  

> Remember: With great proxies comes great responsibility. 😉
