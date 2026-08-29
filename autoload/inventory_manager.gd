extends Node

func amount(item_id: String) -> int:
	return int(GameState.data.inventory.get(item_id, 0))

func add(item_id: String, quantity: int) -> void:
	GameState.add_item(item_id, quantity)
