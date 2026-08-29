# Gameplay Systems

## Farming

A plot moves through empty, tilled, planted, sprout, growing, mature, harvested, and empty. Planting checks the crop unlock and seed cost. Harvesting checks capacity before changing the plot. Growth uses saved timestamps so it continues after reload.

## Inventory

The barn stores crops, animal products, and crafted goods. Transactions are atomic: validate all quantities and capacity first, then make the change once. Failed transactions change nothing.

## Animals

Each animal has an unlock level, feed item and quantity, production duration, product, reward, and visual states. Its loop is hungry → eating → producing → ready → collect → hungry.

## Production

Each recipe defines ingredients, output, duration, unlock level, XP, price, and station. Queue jobs run in order. Collection checks barn capacity. Speed-up affects the selected job and adjusts later queued times correctly.

## Economy

Coins come from orders, selling, goals, and achievements. Coins pay for seeds, buildings, and expansions. Gems are rare and optional. Every transaction rejects negative values and records visible feedback.

## Orders

The board creates requests using content unlocked at the player's level. Delivery removes all required goods in one transaction, grants coins and XP, marks the order complete, and refreshes it after a delay.

## Progression

XP raises levels. Levels unlock crops, animals, recipes, plots, buildings, and districts. Level-up shows the exact rewards and new content.

## Goals

Tutorial goals teach the first loop in order. Daily goals reset on a new UTC day and rewards can be claimed once. Achievements use lifetime totals and never reset.

## World

The simple map has connected spaces rather than separate large worlds. Expansion gates reveal reusable districts. Day/night, rain, irrigation, and decoration add atmosphere after the loop works.
