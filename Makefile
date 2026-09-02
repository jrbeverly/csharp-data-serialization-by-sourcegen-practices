SLN := Serialization.slnx

.DEFAULT_GOAL := help
.PHONY: help build test test-all clean

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "} {printf "  \033[36m%-12s\033[0m %s\n", $$1, $$2}'

build: ## Restore and build the whole solution
	dotnet build $(SLN)

test: build ## Build, then run the test suite (summary only)
	dotnet test $(SLN) --nologo --no-build

test-all: build ## Build, then run tests showing every individual test result
	dotnet test $(SLN) --nologo --no-build --logger "console;verbosity=detailed"

clean: ## Remove build outputs (bin/obj) across the solution
	dotnet clean $(SLN)
