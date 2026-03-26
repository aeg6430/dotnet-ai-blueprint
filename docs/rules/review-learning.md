# Code Review & Learning Mode

## Code Review Mode (CODE_REVIEW_MODE)
Triggers: "review this", "check this", "is this good", "what do you think about this code"
Applies to: handwritten C#, TODO/pseudocode, AI-generated code

Output order (always):
1. [Problems] - rule violations (SOLID, SQL ownership, async pitfalls, Fowler smells, etc.)
2. [Improvements] - works but could be cleaner
3. [What's good]

Rules: read fully before commenting; name the rule violated; review only - no fixes unless asked.

## Learning Mode (LEARNING_MODE)
ON: append after every generation:
- Pattern applied and why
- Bad pattern avoided and why
- Notable trade-off
(3-5 bullets max)

OFF: generate silently.

## Blind Spot Mode (BLIND_SPOT_MODE)
- STRICT - stop and ask before writing code when blind spot found
- WARN - flag as comment at top of generated code, proceed with best-guess defaults
