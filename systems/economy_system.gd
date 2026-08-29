extends Node

signal coins_changed(balance: int, delta: int)
signal gems_changed(balance: int, delta: int)

func coins() -> int:
	return int(GameState.data.get("coins", 0))

func gems() -> int:
	return int(GameState.data.get("gems", 0))

func can_afford_coins(amount: int) -> bool:
	return amount >= 0 and coins() >= amount

func can_afford_gems(amount: int) -> bool:
	return amount >= 0 and gems() >= amount

func spend_coins(amount: int) -> bool:
	if amount < 0:
		return false
	if not can_afford_coins(amount):
		GameState.message_requested.emit("Not enough coins")
		return false
	GameState.data.coins = coins() - amount
	coins_changed.emit(coins(), -amount)
	GameState.notify_changed()
	return true

func add_coins(amount: int) -> bool:
	if amount <= 0:
		return false
	GameState.data.coins = coins() + amount
	GameState.data.total_earned = int(GameState.data.get("total_earned", 0)) + amount
	coins_changed.emit(coins(), amount)
	GameState.notify_changed()
	return true

func spend_gems(amount: int) -> bool:
	if amount < 0 or not can_afford_gems(amount):
		return false
	GameState.data.gems = gems() - amount
	gems_changed.emit(gems(), -amount)
	GameState.notify_changed()
	return true

func add_gems(amount: int) -> bool:
	if amount <= 0:
		return false
	GameState.data.gems = gems() + amount
	gems_changed.emit(gems(), amount)
	GameState.notify_changed()
	return true
