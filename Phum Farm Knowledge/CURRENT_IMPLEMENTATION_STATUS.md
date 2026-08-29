# Current Implementation Status

Updated 2026-09-09.

## Implemented foundation

The Unity source currently contains Boot, Main Menu and Farm World scenes; service lifetime; camera and player controls; game state; versioned local save; inventory; economy; XP progression; unlocks; crops; animals; production; orders; tutorial; placement; districts; localization; audio; HUD and modal screens; content definitions; editor bootstrap; and Edit Mode and Play Mode tests.

Daily goals and achievements now have persistent state, UTC-day reset behavior, progress recording, claim rewards, and duplicate-claim protection. Inventory requirement checks aggregate duplicate item requirements. Production validates unlock levels and preserves queue timing.

## Still requires proof or polish

- Run the full Unity Edit Mode and Play Mode test suites after the latest source changes.
- Complete a Windows release build and the full smoke path.
- Inspect all UI at target aspect ratios and verify Khmer and Chinese rendering.
- Replace procedural placeholder art with final or production-quality assets.
- Complete Android device controls, performance testing, and packaging.
- Balance progression using play-session evidence.

## Current recommended gate

Finish Gate 1 and Gate 2 verification, then run through Gate 3 rice harvesting in the Unity editor. Do not begin cloud or social systems yet.
