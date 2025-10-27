# 🚀 Exercise 04: Add gRPC Adapter

You’ve already built a clean API adapter.  
Now it’s time to add another entry point — this time using **gRPC**.  

Because in a real-world system, you may expose multiple communication protocols, and Hexagonal Architecture makes this effortless.

---

## 🎯 Goal

Add a new **Adapter.gRPC** project that communicates with the Core through Input Ports, just like your HTTP API does.

This exercise focuses on:
- Understanding how to add a new protocol (gRPC) without changing your Core.
- Seeing the power of “ports and adapters” in practice.
- Setting the stage for future exercises where we’ll add caching and messaging.

---

## 🏗 Example Project Structure

```text
src/
 ├─ Core/ 
 ├─ Adapters/
 │   ├─ Adapter.Api/           # REST API
 │   ├─ Adapter.gRPC/          # gRPC Service
 │   ├─ Adapter.PostgreSQL/    # Persistence
 │   └─ Adapter.Redis/         # Cache (later)
 └─ Host.Api/                  # Composition Root
```

---

## ⚙️ Implementation Overview

1. **Define the .proto file** for your service.
   - Place it inside the `Adapter.gRPC` project under `Protos/`.
   - Keep it small — just one message and one service method to call your use case.

2. **Generate the gRPC code**.
   - Update your `.csproj` to include the proto definition and generate C# classes automatically.

3. **Implement the Service**.
   - Inherit from the generated base class and inject the corresponding Core Input Port.
   - Map incoming gRPC messages to Core commands.
   - Call the use case just like the HTTP adapter does.

4. **Dependency Injection**
   - Register the gRPC service in the `Host.Api` project.
   - Don’t forget to configure endpoints and ports for gRPC in `Program.cs`.

---

## 💡 Why This Matters

Adding gRPC demonstrates how the Core remains **untouched**, even as we introduce a new communication protocol.  
That’s the magic of Hexagonal Architecture: your domain logic doesn’t care *how* it’s called — REST, gRPC, GraphQL, Morse code — it just works.

---

## 🧠 Key Takeaways

- **Ports and Adapters = Freedom** — you can add new protocols without refactoring your domain.  
- **Core stays pure** — the same use case now powers both HTTP and gRPC.  
- **Host orchestrates everything** — it’s the glue, not the logic.  

---

Please, follow the instructions to [create a gRPC adapter](Docs/Exercises/Exercise04_addgRPC.md).

Then continue your journey with the next two core training modules:
- ⚡ [Add a Distributed Cache Port](Docs/Exercises/Exercise05_addDistributedCachePort.md)
- 📬 [Add a Messaging Output Port](Docs/Exercises/Exercise06_addPublisherPort.md)

By the end of these exercises, you’ll have conquered the **holy trinity of microservice infrastructure**:  
database, distributed cache, and messaging.  
**Three adapters to rule them all.** 🔥  

A wise observation, young architect — but your journey is far from over.  
Darker patterns await: **CQRS**, **reverse proxies**, **telemetry**…  
So don’t celebrate just yet — the road to *hexagonal mastery* is long and full of abstractions. ⚔️
