# Not Implemented Pattern

> This rule applies to both developers and the AI assistant.
> The matching code template is in `templates/NotImplementedPattern.cs`.

---

## Rule

If a piece of logic cannot be implemented right now — whether by the AI or a developer — **never**:
- Delete the method or class
- Comment out the body
- Leave a silent `// TODO: implement this`
- Return a default value like `return null` or `return 0` without explanation

---

## The Correct Pattern

Two steps, always together:

**1. Leave a detailed comment block directly above the throw:**

```
// NOT IMPLEMENTED - {date}
//
// What:
//   Describe exactly what cannot be implemented and what it was supposed to do.
//
// Why:
//   Explain why it cannot be done right now — missing spec, missing data,
//   pending business decision, dependency not yet built, etc.
//
// What is needed to implement:
//   - Bullet list of concrete things required before this can be finished
```

**2. Throw so it fails loudly at runtime:**

```csharp
throw new NotImplementedException("Short reason — see comment above");
```

---

## Why This Matters

| Approach | Problem |
|---|---|
| Delete the code | Bug is still there, witness is gone |
| `// TODO: implement this` | Silent — nothing breaks, problem is invisible |
| Comment out the code | Logic quietly skipped, no runtime signal |
| `throw new NotImplementedException()` with no comment | Loud but no context — next developer has no idea why |
| ✅ Detailed comment + `throw new NotImplementedException()` | Honest, loud, fully documented — nothing hidden, nothing lost |

> *Deleting code to make a problem disappear is the equivalent of executing the person who raised the bug — the bug is still there, you just got rid of the witness.*
