# PromptSpark.Chat .NET 10.0 Upgrade Tasks

## Overview

This document tracks the execution of PromptSpark.Chat upgrade from .NET 9.0 to .NET 10.0 (LTS). All project files, package references, and code modifications will be performed simultaneously in a single atomic operation, followed by testing and validation.

**Progress**: 0/3 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

---

## Tasks

### [▶] TASK-001: Verify prerequisites
**References**: Plan §Phase 0

- [▶] (1) Verify .NET 10 SDK is installed on development machine per Plan §Prerequisites
- [ ] (2) .NET 10 SDK meets minimum requirements (**Verify**)

---

### [ ] TASK-002: Atomic framework and package upgrade with compilation fixes
**References**: Plan §Phase 1, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [ ] (1) Update TargetFramework to net10.0 in PromptSpark.Chat\PromptSpark.Chat.csproj
- [ ] (2) Project file TargetFramework updated to net10.0 (**Verify**)
- [ ] (3) Update package references per Plan §Package Update Reference (Microsoft.AspNetCore.OpenApi 9.0.4→10.0.1, Microsoft.Extensions.Options.ConfigurationExtensions 9.0.4→10.0.1, System.Text.Json 9.0.4→10.0.1)
- [ ] (4) Remove Microsoft.AspNetCore.SignalR v1.2.0 and add Microsoft.AspNetCore.SignalR.Client v10.0.1
- [ ] (5) All package references updated per Plan §Package Update Reference (**Verify**)
- [ ] (6) Restore all dependencies with `dotnet restore`
- [ ] (7) All dependencies restored successfully (**Verify**)
- [ ] (8) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog sections 1-2 (focus: ConfigurationBinder.GetValue<T> signature changes in Program.cs lines 33,73,74 and ConfigurationDiagnostics.cs lines 21,22; OptionsConfigurationServiceCollectionExtensions.Configure<T> in Program.cs line 94)
- [ ] (9) Solution builds with 0 errors (**Verify**)
- [ ] (10) Commit changes with message: "TASK-002: Atomic .NET 10.0 upgrade - framework, packages, and compilation fixes"

---

### [ ] TASK-003: Run tests and validate behavioral changes
**References**: Plan §Phase 2, Plan §Breaking Changes Catalog sections 3-7

- [ ] (1) Run all tests in solution with `dotnet test PromptSpark.Chat.sln`
- [ ] (2) Fix any test failures found caused by behavioral changes (reference Plan §Breaking Changes Catalog sections 3-7 for Uri constructor changes in Program.cs lines 55,66; HttpContent.ReadAsStringAsync in HomeController.cs lines 26,40; AddHttpClient in Program.cs line 31)
- [ ] (3) Re-run tests after fixes to verify all issues resolved
- [ ] (4) All tests pass with 0 failures (**Verify**)
- [ ] (5) Commit test fixes with message: "TASK-003: Validate .NET 10.0 behavioral changes and fix test failures"

---
