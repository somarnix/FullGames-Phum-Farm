# Testing and Release

## Required automated tests

Test new farm defaults, inventory atomicity, capacity, economy validation, level unlocks, crop stages, animal states, production queue order, speed-up, order delivery, daily reset, one-time goal claims, save round trip, backup recovery, migration, localization, placement validity, and expansion purchase.

## Play Mode smoke path

Boot → New Farm → move → till → plant → water → harvest → open barn → complete order → level up → produce feed → feed chicken → collect egg → unlock land → save → return to menu → continue.

## Manual checks

- Verify controls with keyboard, mouse, game window resizing, touch, and phone safe areas.
- Check every screen in English, Khmer, and Chinese.
- Test full barn, insufficient coins, locked content, repeated clicks, background/resume, and corrupted save.
- Profile frame time, allocations, scene load, UI rebuilds, particles, shadows, and world object counts.

## Definition of done

A feature is done when the successful path works, failure paths preserve state, save/reload works, UI explains the result, content data is complete, and a meaningful test protects its key rule.

## Release blockers

Data loss, blocked tutorial, missing core screen, incorrect payment, duplicate reward, broken build, script error, severe performance problem, or unreadable primary UI blocks release.
