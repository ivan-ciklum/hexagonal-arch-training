# 📬 Exercise 06: Add Messaging Output Port

Time to make your system talk!  
In this exercise, you’ll add a **messaging output port** to publish events to the outside world.

This is where your hexagonal architecture really starts to shine — and communicate.

---

## 🎯 Goal

Create a new output port for **event publishing**.  
Then implement it in a new adapter using **Apache Pulsar** (or any other message broker).

---

## 🏗 Project Structure

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
 │   ├─ Adapter.Redis/
 │   ├─ Adapter.gRPC/
 │   └─ Adapter.Pulsar/           # 🆕 Messaging Adapter
 │
 └─ Host.Api/
```

---

## ⚙️ Steps

### 1. Define the Output Port
Create `IMessagePublisher` in `Core/OutputPorts` with methods like:

```csharp
Task PublishAsync<T>(string topic, T message);
```

Keep it as generic as possible — the Core doesn’t know about Pulsar or Kafka, only about the *concept* of publishing messages.

---

### 2. Implement the Adapter
In `Adapter.Pulsar`, implement `IMessagePublisher` using the Pulsar client library.

Handle serialization, logging, and connection management *inside the adapter*, never in the Core.

---

### 3. Register the Adapter
Add a `DependencyInjection.cs` file in `Adapter.Pulsar`:

```csharp
services.AddSingleton<IMessagePublisher, PulsarMessagePublisher>();
```

Then call this from `Host.Api`:

```csharp
builder.Services.AddPulsarAdapter();
```

---

## 🧠 Key Takeaways

- The Core defines the **contract** for messaging.
- The Adapter implements the **details**.
- The Host wires them together.
- You can replace Pulsar with Kafka, RabbitMQ, or anything else — and the Core won’t care.

---

## 🎬 Mission

Your mission:  
- Create the output port for messaging.  
- Implement it with Pulsar.  
- Wire it all together in the Host.  

Then sit back and admire your work — your system now stores, caches, and communicates.  
A true distributed microservice citizen. 🌍
