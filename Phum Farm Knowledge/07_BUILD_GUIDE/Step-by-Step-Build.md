# Step by Step Build Guide

This is the main order for finishing the simple game. Complete and test one gate before starting the next.

## Gate 1 Project opens

Open `UnityProject` in Unity 6000.5.10f1. Confirm packages resolve, scripts compile, Boot is first in Build Settings, and Boot opens Main Menu. Remove all missing components and console errors.

## Gate 2 New farm starts

New Farm creates default data and loads Farm World. Confirm player movement, click movement, camera pan, zoom, rotation, bounds, pause, and return to menu.

## Gate 3 Rice can be harvested

Select a plot, till, plant rice, water, show growth stages, reach mature, harvest, add items, grant XP, and reset the plot. Test locked plots, missing coins, early harvest, and full barn.

## Gate 4 Save is trustworthy

Save after planting, exit, reopen, load, and finish the crop. Test main file, backup recovery, invalid JSON, old save migration, offline time, and application pause/quit autosave.

## Gate 5 Economy and progression work

Add direct selling, coins, XP, level thresholds, plot unlocks, crop unlocks, level-up screen, and exact reward feedback. Test overspending and negative values.

## Gate 6 Production works

Place or unlock the feed mill, enqueue animal feed, remove ingredients once, process jobs in queue order, collect output, and handle full storage. Then add bread, butter, and palm sugar.

## Gate 7 Animals work

Unlock the chicken, consume feed, show hungry/eating/producing/ready states, collect egg, and persist the timer. Reuse the same system for cow and water buffalo.

## Gate 8 Orders work

Generate three level-appropriate orders, display requirements, block incomplete delivery, remove goods atomically, grant reward, save completion, and refresh after the timer.

## Gate 9 Goals teach and retain

Complete the tutorial from camera movement to level-up. Add daily harvest, production, and order goals with UTC reset. Add lifetime achievements and prevent double claims.

## Gate 10 World progression works

Buy Animal Meadow, Production Village, and River Orchard using level and coin requirements. Persist unlocked state. Add clear locked visuals and requirements.

## Gate 11 Presentation is consistent

Replace the most visible placeholders, add crop and animal animation, sound, particles, lighting, rain, water, readable icons, responsive panels, and localization QA.

## Gate 12 Release candidate exists

Run Edit Mode and Play Mode suites, complete the smoke path, build Windows, test a fresh install and upgraded save, fix all blockers, then create Android input and performance qualification.
