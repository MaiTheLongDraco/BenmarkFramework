---
description: General agent rules and guidelines for PerfFramework
---

# PerfFramework Rules

These rules apply to all agent interactions within the PerfFramework repository. They are heavily influenced by the Andrej Karpathy guidelines.

## 1. Core Principles (Karpathy Guidelines)

*   **Think Before Coding:**
    *   Never assume. Always state your assumptions explicitly.
    *   If the requirement is ambiguous (e.g., "how should we handle this edge case?"), ask the user. DO NOT pick an implementation silently.
    *   If a simpler approach exists, propose it first.

*   **Simplicity First:**
    *   Write the minimum amount of code to solve the immediate problem.
    *   **NO speculative abstractions:** Do not add DI containers, event buses, or complex patterns unless explicitly required by the current task.
    *   **NO features beyond what was asked:** Do not add "nice-to-have" features (e.g., a web dashboard) if the task is about core tracing.
    *   Keep the core engine small and allocation-free on the hot path.

*   **Surgical Changes:**
    *   Touch ONLY what you must.
    *   Do not "clean up" adjacent code, reformat files, or fix warnings in areas you are not explicitly working on.
    *   If you create unused imports/variables because of your change, remove them. Do not remove pre-existing dead code.
    *   Match the existing coding style exactly.

*   **Goal-Driven Execution:**
    *   Every task must have verifiable success criteria.
    *   Before writing code, state the plan and how you will verify it.
    *   Never say "make it work"; say "make test X pass".

## 2. Project-Specific Architectural Rules

*   **Hot Path Performance is King:**
    *   The `Perf.Measure()` code path must have near-zero allocations when disabled, and minimal overhead when enabled.
    *   **NO string allocations** for operation identities on the hot path. Use `OperationId` (uint/ulong).
    *   **NO Reflection** or dynamic code generation on the hot path. Rely on Source Generators (compile-time) for instrumentation.

*   **Async Correctness is Mandatory:**
    *   Trace context must survive `await`, `Task.Run`, and thread pool switching.
    *   Use `AsyncLocal<T>`, NEVER `ThreadLocal<T>`.

*   **Separation of Concerns (Adapters):**
    *   `Perf.Core` must NOT depend on ASP.NET, Unity, Redis, SQL, etc.
    *   All external integrations must be implemented as separate adapter packages.

*   **Unity Compatibility:**
    *   Code must be compatible with Unity IL2CPP (AOT compilation). Avoid features that break under AOT (e.g., `System.Reflection.Emit`).

*   **Security:**
    *   Never capture sensitive data (passwords, tokens, SQL parameters, request bodies) by default. Implementing redaction is a core requirement.

## 3. Workflow Rules

*   **Plan Before Execution:**
    *   Before starting any implementation (e.g., a new Phase or feature), you MUST present a detailed implementation plan to the user.
    *   Do not write code until the user approves the plan or unless explicitly told to proceed.

*   **Checklist Tracking:**
    *   Whenever you complete a module or a task, you MUST check off the corresponding item in the `CHECKLIST.md` file located at the root of the project.
    *   Use the appropriate tools (like `replace_file_content`) to change the `- [ ]` to `- [x]` for the completed task.