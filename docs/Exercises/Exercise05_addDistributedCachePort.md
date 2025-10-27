# ⚡ Exercise 05: Add Distributed Cache Port

You’ve got your database and gRPC communication in place.  
Now, it’s time to bring speed to the party — by adding a distributed cache.

No long lectures here — you already know what caching is and why it matters.  
Let’s make it part of your hexagonal architecture.

---

## 🎯 Goal

Introduce a **Distributed Cache Port** that abstracts cache operations behind a clean Core interface, allowing different implementations (like Redis or in-memory) without touching business logic.

---

## 🧩 New Project Structure

```text
src/
 ├─ Core/
 │   ├─ InputPorts/
 │   ├─ OutputPorts/
 │   └─ UseCases/
 │
 ├─ Adapters/
 │   ├─ Adapter.Api/
 │   ├─ Adapter.PostgreSQL/
 │   ├─ Adapter.Redis/           # 🆕 New Cache Adapter
 │   └─ Adapter.gRPC/
 │
 └─ Host.Api/
```

---

## ⚙️ Steps

### 1. Create the Output Port Interface
In `Core/OutputPorts`, define `ICacheRepository` (or `IDistributedCachePort`) with methods like:

- `Task<T?> GetAsync<T>(string key)`
- `Task SetAsync<T>(string key, T value, TimeSpan? expiration)`
- `Task RemoveAsync(string key)`

Keep it simple and framework-agnostic.

---

### 2. Implement the Redis Adapter
Create a new project: `Adapter.Redis`.

Add a class implementing the cache interface using `IDistributedCache` from `Microsoft.Extensions.Caching.Distributed`.

**Pro tip:** handle serialization (JSON) inside the adapter — never in the Core.

---

### 3. Dependency Injection
In `Adapter.Redis/DependencyInjection.cs`:

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});
services.AddScoped<ICacheRepository, RedisCacheRepository>();
```

Then, register it in your `Host.Api` project along with Core and other adapters.

---

## 🧠 Key Takeaways

- The Core defines *what* to cache, not *how* to cache it.
- Adapters do the infrastructure work — in this case, Redis.
- The Host wires everything together.
- Adding caching doesn’t change your Core — it just gets faster. 🚀

---

When you finish, you’ll have one more layer of performance wrapped neatly inside your hexagon.  
One port closer to architectural perfection.
