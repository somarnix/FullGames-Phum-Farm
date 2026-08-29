# Phum Farm Knowledge

This is the design and build library for Phum Farm. It explains how to finish a small, complete farming game first and then expand it toward a much larger Hay Day style game over time.

The long roadmap is an order of growth. It is not a promise that one person must finish a Hay Day scale game in three years.

## Library map

1. [Master blueprint](01_MASTER_BLUEPRINT/Blueprint.md) explains the game identity, target player, and product boundaries.
2. [Simple game specification](02_PRODUCT_SPEC/Simple-Game-Specification.md) defines the first complete release.
3. [Gameplay systems](03_GAMEPLAY_SYSTEMS/Gameplay-Systems.md) defines farming, animals, production, economy, orders, and progression.
4. [UI and UX](04_UI_UX_DESIGN/UI-UX-Specification.md) defines screens, controls, feedback, and accessibility.
5. [Architecture](05_GAME_ARCHITECTURE/Architecture.md) defines Unity code, data, saves, scenes, and content ownership.
6. [Content catalog](06_CONTENT_SYSTEM/Starter-Content.md) lists the small release content and its expansion rules.
7. [Step by step build](07_BUILD_GUIDE/Step-by-Step-Build.md) is the main implementation checklist.
8. [Testing and release](08_TESTING_RELEASE/Testing-and-Release.md) defines proof required before release.
9. [Growth roadmap](09_ROADMAP/Growth-Roadmap.md) expands the game by stages without a fixed deadline.
10. [Current status](CURRENT_IMPLEMENTATION_STATUS.md) separates implemented work from plans.
11. [Study book](10_STUDY_BOOK/Study-Book.md) explains what to learn while building.
12. [Engineering rules](11_ENGINEERING_RULES/Rules.md) prevents the project from becoming difficult to maintain.
13. [Feature matrix](12_FEATURE_MATRIX/Feature-Matrix.md) compares the simple game with later expansion.
14. [Dictionary](13_DICTIONARY/Dictionary.md) explains important project terms.
15. [Documentation QA](DOCUMENTATION_QA_CHECKLIST.md) keeps plans accurate and navigable.

## Source of truth

The playable project is in `../UnityProject`. Markdown files in this library are the game plan.

## How to work

Read the simple game specification, choose the next incomplete build gate, implement one playable vertical slice, run its tests, update the implementation status, and commit it. Do not add large online systems until the simple offline game is stable.
