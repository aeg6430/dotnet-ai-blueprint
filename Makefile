.DEFAULT_GOAL := help

.PHONY: help init setup-scan setup-rewrite-placeholders setup-rewrite-content setup-rename-solution-projects setup-rename-paths setup-clean setup-verify build test test-edge test-placeholder-guard test-exception-leak test-dependency-graph verify commit-msg-check

DOTNET ?= dotnet
SKELETON_SOLUTION ?= skeleton/StarterPack.Skeleton.sln
TEST_PROJECT ?= skeleton/tests/Acme.Tests/Acme.Tests.csproj
SETUP_TOOL_PROJECT ?= tools/starter-pack-setup/StarterPack.Setup.Tool.csproj
SETUP_ROOT ?= .
SETUP_DRY_RUN ?=
ALLOWED_COMMIT_TYPES := build chore ci docs feat fix perf refactor revert style test
COMMIT_HEADER = $(firstword $(subst :, ,$(MSG)))
COMMIT_TYPE = $(firstword $(subst (, ,$(COMMIT_HEADER)))
COMMIT_SUBJECT = $(strip $(wordlist 2,999,$(subst :, ,$(MSG))))
SOLUTION_NAME ?= $(TARGET_PROJECT_NAME)
CORE_NAMESPACE ?= $(TARGET_PROJECT_NAME).Core
INFRASTRUCTURE_NAMESPACE ?= $(TARGET_PROJECT_NAME).Infrastructure
API_NAMESPACE ?= $(TARGET_PROJECT_NAME).Api
TESTS_NAMESPACE ?= $(TARGET_PROJECT_NAME).Tests
SETUP_TOOL = $(DOTNET) run --project "$(SETUP_TOOL_PROJECT)" --
SETUP_DRY_RUN_ARG = $(if $(filter 1 true yes,$(SETUP_DRY_RUN)),--dry-run,)

help:
	@echo Available targets:
	@echo   make init TARGET_PROJECT_NAME=Acme.Ordering
	@echo   make setup-scan SETUP_ROOT=.
	@echo   make setup-rewrite-placeholders TARGET_PROJECT_NAME=Acme.Ordering SETUP_ROOT=.
	@echo   make setup-rewrite-content TARGET_PROJECT_NAME=Acme.Ordering SETUP_ROOT=.
	@echo   make setup-rename-solution-projects TARGET_PROJECT_NAME=Acme.Ordering SETUP_ROOT=.
	@echo   make setup-rename-paths TARGET_PROJECT_NAME=Acme.Ordering SETUP_ROOT=.
	@echo   make setup-clean SETUP_ROOT=.
	@echo   make setup-verify SETUP_ROOT=.
	@echo   make build
	@echo   make test
	@echo   make test-edge
	@echo   make test-placeholder-guard
	@echo   make test-exception-leak
	@echo   make test-dependency-graph
	@echo   make verify
	@echo   make commit-msg-check MSG="docs(ai): clarify setup flow"
	@echo Notes:
	@echo   - This Makefile is the preferred command surface for repeatable setup and verification.
	@echo   - The documented setup flow and dotnet commands remain valid without GNU Make.
	@echo   - setup-* targets split placeholder rewrite, content rewrite, solution/project rename, path rename, cleanup, and verification.
	@echo   - Set SETUP_DRY_RUN=1 to preview file and path changes before applying them.
	@echo   - test-edge runs the highest-signal guardrail checks first, then you can run the full suite.
	@echo   - commit-msg-check validates the mechanical part of Conventional Commits.
	@echo   - build/test validate the bundled net8.0 skeleton solution in this repository.
	@echo   - The skeleton is intended to build under either the .NET 8 or .NET 9 SDK.

init:
ifeq ($(strip $(TARGET_PROJECT_NAME)),)
	$(error Usage: make init TARGET_PROJECT_NAME=Acme.Ordering)
else
	@echo TargetProjectName=$(TARGET_PROJECT_NAME)
	@echo SolutionName=$(SOLUTION_NAME)
	@echo CoreNamespace=$(CORE_NAMESPACE)
	@echo InfrastructureNamespace=$(INFRASTRUCTURE_NAMESPACE)
	@echo ApiNamespace=$(API_NAMESPACE)
	@echo TestsNamespace=$(TESTS_NAMESPACE)
	@echo SetupRoot=$(SETUP_ROOT)
	@echo 1. Read docs/START_HERE.md
	@echo 2. Read docs/starter-pack/project-setup-protocol.md
	@echo 3. make setup-scan SETUP_ROOT=$(SETUP_ROOT)
	@echo 4. make setup-rewrite-placeholders TARGET_PROJECT_NAME=$(TARGET_PROJECT_NAME) SETUP_ROOT=$(SETUP_ROOT)
	@echo 5. make setup-rewrite-content TARGET_PROJECT_NAME=$(TARGET_PROJECT_NAME) SETUP_ROOT=$(SETUP_ROOT)
	@echo 6. make setup-rename-solution-projects TARGET_PROJECT_NAME=$(TARGET_PROJECT_NAME) SETUP_ROOT=$(SETUP_ROOT)
	@echo 7. make setup-rename-paths TARGET_PROJECT_NAME=$(TARGET_PROJECT_NAME) SETUP_ROOT=$(SETUP_ROOT)
	@echo 8. make setup-clean SETUP_ROOT=$(SETUP_ROOT)
	@echo 9. make setup-verify SETUP_ROOT=$(SETUP_ROOT)
endif

setup-scan:
	$(SETUP_TOOL) scan --root "$(SETUP_ROOT)"

setup-rewrite-placeholders:
ifeq ($(strip $(TARGET_PROJECT_NAME)),)
	$(error Usage: make setup-rewrite-placeholders TARGET_PROJECT_NAME=Acme.Ordering [SETUP_ROOT=.])
else
	$(SETUP_TOOL) rewrite-placeholders --root "$(SETUP_ROOT)" --target-project-name "$(TARGET_PROJECT_NAME)" --solution-name "$(SOLUTION_NAME)" --core-namespace "$(CORE_NAMESPACE)" --infrastructure-namespace "$(INFRASTRUCTURE_NAMESPACE)" --api-namespace "$(API_NAMESPACE)" --tests-namespace "$(TESTS_NAMESPACE)" $(SETUP_DRY_RUN_ARG)
endif

setup-rewrite-content:
ifeq ($(strip $(TARGET_PROJECT_NAME)),)
	$(error Usage: make setup-rewrite-content TARGET_PROJECT_NAME=Acme.Ordering [SETUP_ROOT=.])
else
	$(SETUP_TOOL) rewrite-content --root "$(SETUP_ROOT)" --target-project-name "$(TARGET_PROJECT_NAME)" --solution-name "$(SOLUTION_NAME)" --core-namespace "$(CORE_NAMESPACE)" --infrastructure-namespace "$(INFRASTRUCTURE_NAMESPACE)" --api-namespace "$(API_NAMESPACE)" --tests-namespace "$(TESTS_NAMESPACE)" $(SETUP_DRY_RUN_ARG)
endif

setup-rename-solution-projects:
ifeq ($(strip $(TARGET_PROJECT_NAME)),)
	$(error Usage: make setup-rename-solution-projects TARGET_PROJECT_NAME=Acme.Ordering [SETUP_ROOT=.])
else
	$(SETUP_TOOL) rename-solution-projects --root "$(SETUP_ROOT)" --target-project-name "$(TARGET_PROJECT_NAME)" --solution-name "$(SOLUTION_NAME)" --core-namespace "$(CORE_NAMESPACE)" --infrastructure-namespace "$(INFRASTRUCTURE_NAMESPACE)" --api-namespace "$(API_NAMESPACE)" --tests-namespace "$(TESTS_NAMESPACE)" $(SETUP_DRY_RUN_ARG)
endif

setup-rename-paths:
ifeq ($(strip $(TARGET_PROJECT_NAME)),)
	$(error Usage: make setup-rename-paths TARGET_PROJECT_NAME=Acme.Ordering [SETUP_ROOT=.])
else
	$(SETUP_TOOL) rename-paths --root "$(SETUP_ROOT)" --target-project-name "$(TARGET_PROJECT_NAME)" --solution-name "$(SOLUTION_NAME)" --core-namespace "$(CORE_NAMESPACE)" --infrastructure-namespace "$(INFRASTRUCTURE_NAMESPACE)" --api-namespace "$(API_NAMESPACE)" --tests-namespace "$(TESTS_NAMESPACE)" $(SETUP_DRY_RUN_ARG)
endif

setup-clean:
	$(SETUP_TOOL) cleanup --root "$(SETUP_ROOT)" $(SETUP_DRY_RUN_ARG)

setup-verify:
	$(SETUP_TOOL) verify --root "$(SETUP_ROOT)"

build:
	$(DOTNET) build "$(SKELETON_SOLUTION)" -c Release

test:
	$(DOTNET) test "$(SKELETON_SOLUTION)" -c Release

test-edge: test-placeholder-guard test-exception-leak

test-placeholder-guard:
	$(DOTNET) test "$(TEST_PROJECT)" -c Release --filter "FullyQualifiedName~PlaceholderGuardTests"

test-exception-leak:
	$(DOTNET) test "$(TEST_PROJECT)" -c Release --filter "FullyQualifiedName~ExceptionLeakTests"

test-dependency-graph:
	$(DOTNET) test "$(TEST_PROJECT)" -c Release --filter "FullyQualifiedName~DependencyGraphTests"

verify: build test-edge test

commit-msg-check:
	$(if $(strip $(MSG)),,$(error Usage: make commit-msg-check MSG="feat(scope): summary"))
	$(if $(findstring : ,$(MSG)),,$(error Commit message must contain ': ' after the type or type(scope)))
	$(if $(filter $(COMMIT_TYPE),$(ALLOWED_COMMIT_TYPES)),,$(error Unsupported commit type '$(COMMIT_TYPE)'. Allowed: $(ALLOWED_COMMIT_TYPES)))
	$(if $(COMMIT_SUBJECT),,$(error Commit message must include a non-empty summary after ': '))
	@echo Commit message looks valid: $(MSG)
