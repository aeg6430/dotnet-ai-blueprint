.DEFAULT_GOAL := help

.PHONY: help init build test verify

DOTNET ?= dotnet
SKELETON_SOLUTION ?= skeleton/StarterPack.Skeleton.sln

help:
	@echo Available targets:
	@echo   make init TARGET_PROJECT_NAME=Acme.Ordering
	@echo   make build
	@echo   make test
	@echo   make verify
	@echo Notes:
	@echo   - This Makefile is an optional convenience layer for users who already have GNU Make.
	@echo   - Windows-native users can run the documented dotnet commands directly.
	@echo   - build/test validate the bundled net8.0 skeleton solution in this repository.
	@echo   - The skeleton is intended to build under either the .NET 8 or .NET 9 SDK.

init:
ifeq ($(strip $(TARGET_PROJECT_NAME)),)
	$(error Usage: make init TARGET_PROJECT_NAME=Acme.Ordering)
else
	@echo TargetProjectName=$(TARGET_PROJECT_NAME)
	@echo 1. Read docs/START_HERE.md
	@echo 2. Read docs/starter-pack/project-setup-protocol.md
	@echo 3. Rename namespaces, solution names, directory paths, and setup references to $(TARGET_PROJECT_NAME)
	@echo 4. Keep architecture, rule intent, and folder responsibilities intact
	@echo 5. After setup, move the retained content into your target repository
endif

build:
	$(DOTNET) build "$(SKELETON_SOLUTION)" -c Release

test:
	$(DOTNET) test "$(SKELETON_SOLUTION)" -c Release

verify: build test
