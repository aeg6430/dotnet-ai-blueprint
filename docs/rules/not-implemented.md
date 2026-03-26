---
inclusion: always
---

# Not Implemented Pattern

When logic cannot be implemented now — always do both steps:

**1. Comment block above the throw:**
```
// NOT IMPLEMENTED - {date}
//
// What:   Describe what this was supposed to do.
// Why:    Why it can't be done now.
// Needs:  - Bullet list of what's required before this can be finished
```

**2. Throw:**
```csharp
throw new NotImplementedException("Short reason — see comment above");
```

Never: delete the method, comment out the body, silent `// TODO`, or return default value without explanation.
