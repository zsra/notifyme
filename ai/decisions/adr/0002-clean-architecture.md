# ADR-0002: Clean Architecture (Domain / Application / Infrastructure / Api)

- **Status**: Accepted
- **Date**: 2026-09-22

## Context
The brief is intentionally vague and will evolve (more channels, more event types, an admin
view, eventually a frontend). The chosen architecture needs to make that evolution cheap while
staying easy to review and reason about, since the process/design thinking is what's being
evaluated as much as the code.

## Decision
Use Clean Architecture with four projects: `Domain` (entities, value objects, interfaces, no
outward dependencies), `Application` (use cases/services, depends only on `Domain`),
`Infrastructure` (implements interfaces from `Domain`/`Application`: persistence, event sources,
notification channels), and `Api` (composition root: HTTP endpoints, DI wiring, hosted
services).

## Alternatives considered
- Vertical Slice Architecture - less cross-layer ceremony, feature-folder based; rejected
  because the brief's core ask (pluggable event sources and pluggable channels) maps more
  naturally onto a small number of well-defined abstraction boundaries than onto independent
  feature slices.
- Simple layered N-tier (Controllers/Services/Repositories) - faster to start, but weaker at
  enforcing the "swap the event source / add a channel without touching core logic"
  extensibility the brief explicitly asks for.

## Consequences
More upfront project/interface ceremony than a simple layered approach. In exchange, the
event-ingestion and notification-channel extension points (see ADR-0004, ADR-0005) become
structural rather than incidental, and are easy for a reviewer (or another AI agent) to locate.
