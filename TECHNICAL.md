# Technical

## Language and Platform

The implementation should be built in C#.

The architecture should heavily leverage C# source generators.

The generated serialization infrastructure should support ahead-of-time (AOT) compilation scenarios.

## Supported Serialization Formats

The system should eventually support serializers and deserializers for formats including:

- JSON
- YAML
- CSV
- TOML
- INI
- XML
- CBOR
- Binary serialization formats
- Other structured transmission formats

The architecture must account for the fact that not all formats support identical semantics.

## Project Structure

The intended project organization separates domain models from generated serialization infrastructure.

Example structure:

- `MyDomain`
- `MyDomain.Core`
- `MyDomain.Models`

Generated format-specific serialization projects:

- `MyDomain.Serialization.Json`
- `MyDomain.Serialization.Yaml`
- `MyDomain.Serialization.Xml`
- `MyDomain.Serialization.Cbor`
- `MyDomain.Serialization.Toml`

Potential convention:

- `MyDomain.Serialization.<Format>`

## Source Generator Responsibilities

The source generators should analyze the domain object model and generate:

- Serializers
- Deserializers
- Validation logic
- Type metadata
- Format-specific mappings
- Compatibility layers
- Serialization helpers

Generation inputs should include:

- Type definitions
- Generic annotations
- Serialization configuration attributes
- Semantic constraints
- Explicit serialization semantics

## Serialization Metadata

Annotations and configuration should support concepts including:

- Naming behavior
- Optionality
- Nullability
- Ordering
- Versioning
- Format-specific overrides
- Compatibility constraints

## Format-Specific Semantics

The architecture must acknowledge that serialization formats have incompatible capabilities.

Examples:

### CSV

- Flat/tabular semantics
- Weak hierarchical support
- Limited graph representation

### YAML

- Formatting ambiguity
- Flexible structure representation
- Potential parser inconsistencies

### XML

- Namespace semantics
- Attribute vs element mapping
- Ordering considerations

### Binary Formats / CBOR

- Strict ordering
- Encoding constraints
- Potential byte-for-byte compatibility requirements

### Object Graph Constraints

Some formats may not naturally support:

- Recursive graphs
- Arbitrary references
- Cyclic structures
- Complex polymorphism

The system must support:

- Explicit restrictions
- Mandatory attributes
- Format-specific overrides
- Unsupported construct detection

## Generated Testing Infrastructure

The system should include extensive regression and fuzz testing support.

Potential capabilities include:

- Automatic CLI generation
- Massive randomized object generation
- Recursive structure generation
- Nested collection generation
- Optional value generation
- Boundary-case generation
- Complex graph exploration

## Round-Trip Testing

The testing infrastructure should support automated round-trip verification:

1. Generate randomized domain instances.
2. Serialize into supported formats.
3. Deserialize back into memory.
4. Re-serialize.
5. Compare outputs and structures.

Comparison rules vary by format:

### Binary / CBOR

- Byte-for-byte equality may be required.

### JSON / YAML

- Structural equivalence is sufficient.
- Formatting differences are acceptable.

## Validation and Regression Testing

The generated system should support:

- Regression testing
- Fuzz testing
- Round-trip validation
- Structural equivalence testing
- Encoding validation
- Compatibility verification

The testing infrastructure should operate as a serialization regression harness.

## Architectural Direction

Serialization should be treated as synthesized infrastructure derived from the domain model and semantic metadata.

The architecture should prioritize:

- Strong typing
- Predictability
- AOT compatibility
- Automatic infrastructure generation
- Consistent semantics
- Generated validation
- Generated testing
- Multi-format extensibility

Rather than manually implemented serialization logic within applications.