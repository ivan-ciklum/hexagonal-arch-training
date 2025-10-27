# ⚖️ Exercise 07: Introducing CQRS

You’ve come a long way, architect. Your Core is clean, your adapters are behaving, and your system talks like a true citizen of the distributed world.  
Now it’s time to split responsibilities and achieve true separation of concerns — welcome to **CQRS**: Command Query Responsibility Segregation.

---

## 🎯 Goal

Implement the CQRS pattern in your solution by separating **commands** (actions that change state) from **queries** (operations that read state).  
The idea is simple: one layer writes, the other reads — and they never get confused again.

---

## 🧱 Why CQRS?

CQRS helps you:  
- Simplify complex logic by separating reads and writes.  
- Scale each side independently.  
- Evolve your system with more flexibility — event sourcing, projections, or specialized read models come naturally later.

---

## 🏗 Project Structure Example

```text
src/
 ├─ Core/
 │   ├─ Commands/
 │   │   ├─ Handlers/
 │   │   └─ Models/
 │   ├─ Queries/
 │   │   ├─ Handlers/
 │   │   └─ Models/
 │   ├─ Domain/
 │   └─ UseCases/
 │
 ├─ Adapters/
 │   ├─ Adapter.Api/
 │   ├─ Adapter.PostgreSQL/
 │   ├─ Adapter.Redis/
 │   └─ Adapter.Pulsar/
 │
 ├─ Host.Read/             # Query-only host
 └─ Host.Write/            # Command-only host
```

---

## ⚙️ Implementation Steps

### 1. Create two distinct **Host projects**:
  - `Host.Write` – Only exposes **commands** (write operations) like Register Member. You can rename the existing host to this. 
  - `Host.Read` – Only exposes **queries** (read operations) like Get Members Expiring.  
  - Both hosts will:
    - Register **Core dependencies**.  
    - Register the adapters they need (PostgreSQL, Redis, API).  
    - Expose only the endpoints relevant to their responsibility. 

### 2. Update Docker
Add in the docker compose file the two hosts to run side by side, the write model with one instance and the read model with two instances.


---

### ⚙️ Benefits

1. **Separation of concerns** – Reads and writes can scale independently.  
2. **Simpler hosts** – Each host only knows about what it needs.  
3. **Shared Core** – Business logic stays in one place, no duplication.  
4. **Swappable adapters** – You can add another read store, caching layer, or even gRPC/HTTP for each host independently.

---

## 🧠 Key Takeaways

- Commands **change** data, Queries **read** data.  
- Handlers must be simple — one responsibility per handler.  
- CQRS is not about complexity; it’s about clarity.  
- This separation prepares your system for event sourcing or micro-optimizations later.

---

> Congratulations — you’ve achieved balance. ⚖️  
One side writes, the other reads. Harmony in your hexagon has been restored.

### Call the sheriff! 🤠
When you finish, shout "Code Review!" to your friendly architecture sheriff for a thorough review of your implementation to ensure all Hexagonal principles are followed.
