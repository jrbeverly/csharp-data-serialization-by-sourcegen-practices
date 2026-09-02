# Mock Design

## Status

Superseded by `HLD.md`.

## Purpose

This document is a high-level mock design for the repository based on:

- [PROBLEM.md](./PROBLEM.md)
- [TECHNICAL.md](./TECHNICAL.md)
- [VISION.md](./VISION.md)

Its purpose is to validate whether the proposed system is conceptually implementable, identify major architectural concerns, and surface unresolved decisions that should be addressed before detailed implementation begins.

This is intentionally not a low-level implementation spec. It avoids locking into concrete schemas, exact domain models, file formats, or detailed generated APIs unless needed to evaluate feasibility.

## Executive Assessment

The proposed system is conceptually viable, but only if it is framed as a layered serialization platform rather than a single universal generator.

The central idea is strong:

- treat domain types as the canonical source of truth
- synthesize serialization infrastructure from compile-time information
- generate format-specific behavior rather than hand-writing it repeatedly
- make validation and testing first-class generated capabilities

The main feasibility constraint is not code generation itself. C# source generation and AOT-friendly patterns are already proven for at least JSON-oriented scenarios. The harder problem is semantic translation across formats with incompatible capabilities.

That means the system is viable if it explicitly embraces all of the following:

- a canonical intermediate serialization model
- format capability analysis
- opt-in metadata for ambiguous semantics
- clear diagnostics for unsupported mappings
- generated testing that validates format-specific correctness, not fake uniformity

The system is not viable if it assumes every domain type can be projected cleanly into every format without restrictions, overrides, or loss-model decisions.

## Core Judgment

### Fundamentally implementable

Yes, with scoped expectations.

### Primary architectural truth

This is not one problem. It is three related systems:

1. A compile-time domain analysis and semantic modeling system.
2. A set of format-specific code generation backends.
3. A generated verification and regression harness.

If those are kept separate, the design is coherent. If they collapse into a monolithic "serialize anything everywhere" abstraction, the design will likely become brittle and difficult to reason about.

## Design Principles

The following principles appear necessary for success:

### 1. Domain-first, but not domain-only

The domain model should be the canonical source of truth, but it will not fully encode every serialization decision. Some intent must be supplied explicitly through metadata, conventions, or overrides.

### 2. Canonical semantics before format semantics

The generator should first derive a normalized semantic view of a type system, and only then map that model to JSON, XML, YAML, CSV, TOML, CBOR, or binary formats.

### 3. Unsupported must be explicit

Unsupported constructs should produce diagnostics, not silent fallback behavior.

### 4. Format-specific behavior is a feature, not a leak

Different formats genuinely differ. The system should unify developer workflow, not pretend all formats share one wire model.

### 5. Generated testing is part of the product

Testing should not be an afterthought. The proposed vision depends on generated validation, round-trip checks, regression fixtures, and fuzz-like exploration.

## Proposed High-Level Architecture

### 1. Domain Contracts Layer

This is where application developers define:

- types
- relationships
- nullability and optionality
- semantic constraints
- serialization intent
- explicit overrides where required

This layer should remain business-focused and should avoid format-specific implementation logic wherever possible.

### 2. Metadata and Analysis Layer

An incremental source generator and companion analyzers inspect the domain model and build a canonical representation of serializable semantics.

This layer should answer questions such as:

- what is the shape of each type?
- what members participate in serialization?
- what names, ordering, defaults, and constraints apply?
- what parts of intent are inferred versus explicitly declared?
- what constructs are incompatible with particular formats?

This layer is the conceptual heart of the system.

### 3. Canonical Serialization Model

The generator should project domain types into an intermediate representation that is independent of any one wire format.

That model would likely describe concepts such as:

- record-like objects
- scalar values
- collections
- maps
- discriminated or polymorphic shapes
- nullability
- required versus optional values
- ordering requirements
- identity or reference semantics where supported
- versioning and compatibility intent where declared

This model should be semantic rather than syntax-driven.

### 4. Format Capability Layer

Each format backend should declare what it can represent and what rules it requires.

Examples:

- CSV may only support flat projections or explicitly tabular object graphs.
- INI may only support shallow section-key models.
- XML may require element versus attribute policy and namespace treatment.
- YAML may support rich structure but introduce ambiguity in text representation.
- Binary formats may require stricter ordering, encoding guarantees, and forward compatibility rules.

This layer should be responsible for deciding whether a canonical type shape is:

- directly supported
- supported with explicit overrides
- supported with restrictions
- unsupported

### 5. Format Generation Backends

Each backend should generate:

- serializers
- deserializers where meaningful
- format-specific mapping helpers
- validation code
- capability diagnostics
- equivalence comparers or normalization helpers for testing

The backends should share common analysis infrastructure but remain independently evolvable.

### 6. Runtime Support Layer

Some generated code will require a thin runtime support library for:

- shared abstractions
- helper primitives
- compatibility utilities
- common validation helpers
- possibly common test harness utilities

This runtime should stay minimal. The value of the system comes from generated code and compile-time diagnostics, not from a large reflective runtime.

### 7. Generated Verification Layer

The system should also generate or assemble testing infrastructure for:

- round-trip testing
- structural equivalence checks
- format-normalized comparisons
- regression fixtures
- randomized instance generation
- boundary-case exploration

This layer is essential because the architecture promises predictable, testable behavior across multiple formats.

## Conceptual Data Flow

The overall flow could look like this:

1. Developers define domain types and semantic metadata.
2. Build-time analysis extracts canonical serialization semantics.
3. Capability analysis evaluates those semantics against enabled formats.
4. Backends generate format-specific code and diagnostics.
5. Generated testing infrastructure validates behavior and compatibility.
6. Consumers use generated serializers through strongly typed APIs.

## Proposed Repository Shape

A conceptual repository layout could be:

- `docs/`
- `src/Serialization.Abstractions`
- `src/Serialization.Annotations`
- `src/Serialization.Generator`
- `src/Serialization.Diagnostics`
- `src/Serialization.Runtime`
- `src/Serialization.Testing`
- `src/Serialization.Format.Json`
- `src/Serialization.Format.Xml`
- `src/Serialization.Format.Yaml`
- `src/Serialization.Format.Toml`
- `src/Serialization.Format.Csv`
- `src/Serialization.Format.Cbor`
- `src/Serialization.Format.Binary`
- `samples/`
- `tests/`

This shape keeps concerns separate:

- analysis and generation stay centralized
- format backends stay modular
- testing remains a first-class subsystem
- the repository can scale without forcing all formats into one package

## Boundaries and Responsibilities

### Generator Core

Responsible for:

- type inspection
- semantic model construction
- common diagnostics
- backend orchestration

### Format Modules

Responsible for:

- format capability rules
- format-specific mapping interpretation
- generated read/write logic
- format-aware comparison semantics

### Testing Subsystem

Responsible for:

- object generation strategies
- regression harness behavior
- round-trip and structural equivalence workflows
- failure reporting suited to serialization debugging

### Consumer Domain Projects

Responsible for:

- defining domain types
- annotating or configuring intent
- choosing supported formats
- handling business-level constraints the generator cannot infer

## Viability by Concern Area

### Source generation

Viable.

This is the strongest part of the proposal. Compile-time extraction of type shape, metadata, and configuration maps naturally to incremental source generation and AOT-friendly code emission.

### AOT compatibility

Viable, with discipline.

The design aligns well with AOT goals if it avoids reflection-dependent fallback paths and keeps runtime behavior explicit.

### Multi-format support

Viable, but not uniformly.

Multi-format support is realistic if the system uses a capability-driven model and accepts that some formats require restricted projections of the domain.

### Universal automatic deserialization

Partially viable.

Deserialization is easier for some formats than others. Ambiguous formats and lossy mappings may require stricter constraints or explicit user configuration.

### Versioning and compatibility

Viable, but only with explicit policy.

Versioning intent cannot be safely inferred from types alone. This must be treated as declared metadata plus backend-specific compatibility strategy.

### Fuzz and randomized testing

Viable, but this is a significant subsystem, not a minor add-on.

It should likely be designed as a generated or assembled test harness rather than a simple utility.

## Major Risks

### 1. Over-generalization

The vision can drift into "all shapes, all formats, all semantics" too early. That would make the canonical model vague and backends inconsistent.

Mitigation:

- define a supported object-model subset for early phases
- make unsupported constructs explicit
- phase formats in gradually

### 2. False semantic uniformity

If the system pretends YAML, CSV, XML, and binary protocols are just cosmetic variations of one model, generated behavior will be surprising or lossy.

Mitigation:

- build a capability matrix into the architecture
- allow opt-in per-format overrides
- define equivalence rules per format

### 3. Metadata insufficiency

Type shape alone is not enough for naming, ordering, versioning, attribute-vs-element policy, tabular flattening, or graph identity.

Mitigation:

- treat metadata design as a first-order architecture decision
- support conventions plus explicit overrides
- avoid pretending inference is always enough

### 4. Generator complexity and build performance

A large multi-format generator can become difficult to maintain and expensive to run.

Mitigation:

- use incremental generation
- keep canonical analysis centralized
- isolate format backends
- generate only for enabled formats

### 5. Testing scope explosion

Generated testing across many formats can become more expensive than serializer generation itself.

Mitigation:

- separate smoke, regression, and deeper randomized modes
- allow configurable depth and breadth
- normalize expectations by format

### 6. Binary format ambiguity

"Binary formats" is too broad to treat as one target. Some binary systems are schema-rich, some are positional, some care about exact wire compatibility, and some do not.

Mitigation:

- do not treat "binary" as one backend
- define which binary families are in scope
- require stricter metadata for strict-wire formats

## Key Ambiguities and Unresolved Decisions

The current documents are directionally aligned, but several important design decisions remain open.

### 1. Scope of the supported domain model

Unclear:

- Are cyclic graphs in scope?
- Are reference-preserving graphs required?
- Is polymorphism central or optional?
- Are immutable types and constructor binding first-class?
- Are custom collection types expected?

This affects almost every backend.

### 2. Metadata model

Unclear:

- Will the system rely primarily on attributes, fluent configuration, conventions, or a mix?
- How are format-specific overrides represented without polluting domain types?
- How are compatibility and versioning rules expressed?

### 3. Backend strategy

Unclear:

- Should format backends emit raw wire logic directly?
- Should they generate adapters over existing libraries where practical?
- Is backend behavior expected to match existing serializer ecosystems or define its own semantics?

### 4. Failure model

Unclear:

- What should happen when a type is only partially representable in a format?
- Should generation fail, warn, or require an explicit projection?
- Can users provide custom adapters for unsupported cases?

### 5. Testing product surface

Unclear:

- Is the generated testing harness a library, CLI, or both?
- Are golden files part of the intended workflow?
- How configurable should randomized generation be?

### 6. Versioning ambitions

Unclear:

- Is backward and forward compatibility a v1 requirement?
- Is schema evolution format-independent or backend-specific?
- Are compatibility guarantees advisory or enforceable?

## Clarifying Questions

These are the questions that seem most important to answer before detailed implementation:

1. Is the near-term goal a research prototype for a constrained subset of types, or a reusable general-purpose framework from the start?
2. Should format implementations primarily wrap mature libraries where possible, or is direct wire-level generation the intended default?
3. Does "multi-format support" mean "a domain can opt into several compatible formats" or "all supported types should work across nearly all formats unless explicitly excluded"?
4. Which object-model features are mandatory in the first milestone: immutable types, polymorphism, cyclic references, constructor binding, custom collections?
5. Is compatibility and version-evolution logic part of the first milestone, or should initial work focus on correctness of current-state serialization only?
6. When a format cannot faithfully represent a type, should the preferred response be diagnostic failure, explicit projection, or best-effort lossy mapping?

## Recommended Initial Direction

A pragmatic first milestone would be:

- define the canonical serialization model
- define the metadata strategy
- build generator diagnostics before broad backend coverage
- prove the design on a constrained object-model subset
- support one baseline format and one semantically different second format
- generate round-trip and structural-equivalence tests early

A sensible progression could be:

1. Establish the core analysis model and diagnostics.
2. Prove code generation and AOT-friendliness with a baseline format.
3. Add a second format with visibly different semantics to validate the capability model.
4. Introduce generated testing infrastructure.
5. Expand into harder formats such as CSV, INI, strict binary, and richer compatibility rules.

The most important early success metric is not the number of formats. It is whether the canonical model, capability analysis, and diagnostics remain coherent once a second genuinely different format is introduced.

## Overall Conclusion

The repository describes a coherent and worthwhile architectural direction.

The proposal is fundamentally implementable if it is treated as:

- a domain-to-canonical-model compiler
- a set of modular format backends
- a generated testing and diagnostics ecosystem

The largest risk is not technical impossibility. It is trying to promise too much semantic universality too early.

If the project is scoped around explicit capability modeling, format-aware diagnostics, and phased backend expansion, it has a credible path forward.
