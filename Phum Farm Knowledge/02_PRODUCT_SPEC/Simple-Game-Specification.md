# Simple Complete Game Specification

## New player journey

1. Start a new farm.
2. Learn movement and camera controls.
3. Select and till an empty field.
4. Plant rice and water it.
5. Harvest the mature crop.
6. Open the barn and see the rice.
7. Complete a village order for coins and XP.
8. Level up and unlock new content.
9. Produce animal feed.
10. Feed a chicken and collect eggs.
11. Buy a land expansion.
12. Save, quit, reopen, and continue.

## First release content

| Area | Required content |
|---|---|
| Crops | Rice, corn, tomato, sugarcane |
| Animals | Chicken, cow, water buffalo |
| Products | Animal feed, bread, butter, palm sugar |
| Farm | 16 plots, farmhouse, barn, well, market |
| Districts | Starter farm, animal meadow, production village, river orchard |
| Progression | 10 levels, XP, coins, gems, unlocks |
| Goals | Tutorial, village orders, daily goals, achievements |
| Languages | English first; Khmer and Chinese after text QA |

## Core loop

Plant → wait and manage → harvest → store → produce or sell → earn coins and XP → unlock → improve the farm → repeat.

## Economy rules

Seeds cost less than harvested crops sell for. Produced goods are worth more than their ingredients. Orders pay a small bonus over direct selling. Expansion prices create medium goals. Premium currency is optional in the simple game and must never be required to continue.

## Save rules

Every important state persists: inventory, coins, gems, level, XP, plots, crop timers, animals, production jobs, buildings, expansions, tutorial, orders, goals, settings, game day, and weather. Offline progress may finish timers but must never remove goods.

## Completion test

The simple game is complete when the entire new player journey works in a release build with no manual editor setup and reload preserves the result.
