# Vision

Build a comprehensive source-generator-driven serialization architecture for C# where serialization infrastructure is automatically synthesized from domain object models.

The system should make serialization feel like an inherent capability of a type system rather than a manually implemented application concern.

## Desired Developer Workflow

Developers should define their domain types normally within standard domain projects.

For example:

- `MyDomain`
- `MyDomain.Core`
- `MyDomain.Models`

Developers should then specify:

- Semantic constraints
- Serialization intent
- Configuration metadata
- Optional format-specific overrides

Everything else should be generated automatically.

## Generated Serialization Infrastructure

The source generators should synthesize:

- Serializers
- Deserializers
- Validation logic
- Type metadata
- Format mappings
- Compatibility layers
- Regression tooling
- Fuzz testing infrastructure

Across all supported serialization formats.

## Multi-Format Serialization Model

The architecture should support a broad range of serialization systems, including:

- JSON
- YAML
- XML
- CSV
- TOML
- CBOR
- Binary protocols
- Other structured data formats

The generated infrastructure should adapt to the semantics and limitations of each format while preserving a unified developer experience.

## Canonical Source of Truth

The domain object model should become the canonical source of truth.

From that model, the system should derive:

- Serialization behavior
- Deserialization behavior
- Validation rules
- Compatibility semantics
- Testing infrastructure

Developers should not repeatedly hand-author serialization infrastructure for every format.

## Automatic Testing Ecosystem

Testing should be treated as a first-class capability of the serialization architecture.

The system should automatically support:

- Fuzz testing
- Regression testing
- Round-trip testing
- Structural equivalence testing
- Boundary-case testing
- Compatibility validation

The generated testing system should systematically explore domain objects using:

- Randomized instances
- Recursive structures
- Nested collections
- Optional values
- Complex object graphs
- Boundary conditions

## Round-Trip Validation Model

The generated testing workflow should:

1. Generate randomized object graphs.
2. Serialize them into supported formats.
3. Deserialize them back into memory.
4. Re-serialize them.
5. Validate correctness and stability.

Validation semantics should vary by format.

Examples:

- Binary protocols may require byte-for-byte equality.
- JSON and YAML may only require structural equivalence.

## AOT and Predictable Runtime Behavior

The generated serialization infrastructure should support:

- Ahead-of-time compilation
- Predictable runtime behavior
- Reflection minimization or elimination
- Strong typing
- Explicit metadata generation

The resulting serialization behavior should be deterministic, testable, and analyzable.

## Long-Term Direction

The long-term goal is a generalized serialization architecture where:

“I have a type”

effectively becomes:

“This type can automatically participate in all supported data transmission formats.”

At that point:

- Serialization becomes synthesized infrastructure.
- Regression testing becomes automatic.
- Multi-format support becomes systematic.
- Serialization behavior becomes predictable and strongly typed.

Application developers primarily focus on domain modelling and semantic intent rather than repetitive serialization implementation details.