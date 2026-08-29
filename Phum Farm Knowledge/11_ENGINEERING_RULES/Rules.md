# Engineering Rules

1. Keep one source of truth for every state.
2. Keep gameplay rules outside UI classes.
3. Add content through definitions and reusable prefabs.
4. Validate a transaction completely before changing state.
5. Version saves and write migrations before changing persisted data.
6. Finish one playable vertical slice before broad scaffolding.
7. Do not claim a feature is complete without runnable evidence.
8. Fix compiler errors before adding features.
9. Test failure paths that could lose currency, items, or progress.
10. Profile measured bottlenecks before optimizing.
11. Keep online and monetization work out of the first release.
12. Update `CURRENT_IMPLEMENTATION_STATUS.md` after every completed gate.
