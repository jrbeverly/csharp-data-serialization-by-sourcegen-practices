# High-Level Design

## Status

This document is the authoritative build document for this experiment.

It supersedes `MOCK-DESIGN.md`.

## Objective

Prove that a Roslyn incremental source generator can derive format-specific serializers from a constrained domain object model, using a canonical intermediate representation and a format capability matrix, for two formats that are semantically different enough to validate the model.

## Locked Formats

Two formats are in scope for v1.

**JSON** is the baseline hierarchical format. It natively supports nested objects, scalars of all kinds, and nullability. It is the easiest first target and establishes the canonical model.

**CSV** is the flat tabular format. It has no hierarchical semantics, no native null representation, and no type information. It is semantically different enough from JSON that a second backend working from the same canonical model proves the capability matrix is real.

No other formats are in scope. The two chosen formats are sufficient to validate the architecture.

## Supported Object-Model Subset

The generator handles records and POCOs whose members are drawn from this set.

Supported scalar types:

- `bool`
- `int`
- `long`
- `double`
- `decimal`
- `string`
- Nullable value-type variants of all of the above (`bool?`, `int?`, `long?`, `double?`, `decimal?`)

Supported structural extension for JSON only:

- One level of nested record whose members are themselves drawn from the scalar set above.

Nothing else is in scope. Cyclic graphs, collections, inheritance, polymorphism, generic types, and deeper nesting are all out of scope.

## Metadata Model

Developers opt types and members into generation with attributes. There is no convention-based discovery, no fluent configuration API, and no inference from type shape beyond what the attribute explicitly declares.

The attribute surface is minimal. A type-level attribute marks a record or POCO as a serialization target. Member-level attributes supply an explicit name and an explicit ordinal for ordering. Nullability is read from the type itself, not from an attribute.

Format-specific overrides are not part of v1. The same canonical view is used by both backends.

## Canonical Serialization Model

Between domain type inspection and format code generation, the generator builds a per-type intermediate model.

Each type's model is a flat ordered list of member descriptors. Each descriptor captures:

- **name**: the wire name, taken from the attribute value
- **kind**: either `scalar` or `nested` (nested is the scalar-record extension)
- **nullability**: whether the member is nullable
- **order**: the explicit integer ordinal from the attribute

The canonical model has no format-specific content. Both JSON and CSV backends consume this representation.

## Capability Matrix

| Construct | JSON | CSV |
|---|---|---|
| Scalar members | Supported | Supported |
| Nullable scalars | Supported | Supported |
| One-level nested record | Supported | Not supported |

Any type whose canonical model contains a nested-record member is incompatible with the CSV backend.

## Failure Model

Incompatible constructs are build errors, not silent fallbacks or warnings.

When the CSV backend encounters a nested-record member during analysis, it emits a build error and stops generation for that type. The developer must either remove the nested member or exclude the type from CSV generation.

Diagnostic IDs use the `SER0xx` range.

- `SER001`: type contains a nested-record member incompatible with CSV
- `SER002`: duplicate explicit ordinal within a type
- `SER003`: required attribute missing on an opted-in member

Errors are reported at the attribute site that is closest to the violation.

## Reflection-Free Generation

All generated code is AOT-compatible.

For JSON: generated serializers use `Utf8JsonWriter` directly. Generated deserializers use `Utf8JsonReader` directly. No `JsonSerializer.Serialize` / `Deserialize` calls, no reflection, no `dynamic`.

For CSV: generated serializers write directly to a `StringBuilder` or `TextWriter`, converting each scalar to its string representation and joining with commas. Generated deserializers split on comma and parse each field with the appropriate `Parse` or `TryParse` overload. No external CSV library.

The runtime support layer, if any, contains only small stateless helpers. The value of the system is in the generated code, not in a runtime framework.

## System Boundaries

Three concerns: generator, backends, sample consumer.

**Generator core**: inspects the opted-in type, validates it against the supported object-model subset, builds the canonical member list, emits `SER0xx` diagnostics for violations, and dispatches to each backend.

**Format backends**: JSON and CSV are separate emission paths inside the generator. Each backend receives the canonical member list and emits format-specific read and write methods. Backends share no logic between them.

**Sample consumer**: a minimal project containing one or two opted-in types, used to validate end-to-end compilation and behavior. Not a test framework.

## Explicit Deferrals

The following are not part of v1 and should not be added during initial implementation:

- Any format other than JSON and CSV
- Collections (arrays, lists, dictionaries) of any kind
- Polymorphism and discriminated unions
- Cyclic or reference-preserving graphs
- Nesting beyond one level
- Generic types
- Versioning and schema compatibility
- CLI tooling for generation or inspection
- Property-based or fuzz testing infrastructure
- Custom adapter hooks for unsupported types
- Convention-based discovery without explicit attributes
- Format-specific attribute overrides

## Implementation Sequence

1. Define the opt-in attribute surface.
2. Build the canonical member model and validator (type inspection + `SER0xx` diagnostics).
3. Build the JSON backend (writer and reader).
4. Build the CSV backend (writer and reader, with nested-record error).
5. Wire both backends into the incremental generator.
6. Build the sample consumer and validate end-to-end compilation.
7. Write a minimal script or test that exercises round-trip for both formats.
