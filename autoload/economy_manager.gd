extends Node

func can_afford(amount: int) -> bool:
	return int(GameState.data.coins) >= amount

func purchase(amount: int) -> bool:
	return GameState.spend(amount)
