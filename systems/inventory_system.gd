extends Node

signal inventory_changed(item_id: String, quantity: int)
signal capacity_changed(used: int, capacity: int)

func amount(item_id: String) -> int:
	return int(GameState.data.inventory.get(item_id, 0))

func all_items() -> Dictionary:
	return GameState.data.inventory.duplicate(true)

func used_capacity() -> int:
	var used := 0
	for quantity in GameState.data.inventory.values():
		used += maxi(0, int(quantity))
	return used

func capacity() -> int:
	return maxi(1, int(FarmData.economy.get("barn_capacity", 75)))

func can_add(quantity: int) -> bool:
	return quantity >= 0 and used_capacity() + quantity <= capacity()

func add(item_id: String, quantity: int, enforce_capacity: bool = false) -> bool:
	if item_id.is_empty() or quantity <= 0:
		return false
	if enforce_capacity and not can_add(quantity):
		GameState.message_requested.emit("Barn storage is full")
		return false
	GameState.data.inventory[item_id] = amount(item_id) + quantity
	inventory_changed.emit(item_id, amount(item_id))
	capacity_changed.emit(used_capacity(), capacity())
	GameState.notify_changed()
	return true

func has(requirements: Dictionary) -> bool:
	for item_id in requirements:
		if amount(str(item_id)) < maxi(0, int(requirements[item_id])):
			return false
	return true

func remove(item_id: String, quantity: int) -> bool:
	if item_id.is_empty() or quantity <= 0 or amount(item_id) < quantity:
		return false
	GameState.data.inventory[item_id] = amount(item_id) - quantity
	inventory_changed.emit(item_id, amount(item_id))
	capacity_changed.emit(used_capacity(), capacity())
	GameState.notify_changed()
	return true

func remove_many(requirements: Dictionary) -> bool:
	if not has(requirements):
		return false
	for item_id in requirements:
		var id := str(item_id)
		GameState.data.inventory[id] = amount(id) - maxi(0, int(requirements[item_id]))
		inventory_changed.emit(id, amount(id))
	capacity_changed.emit(used_capacity(), capacity())
	GameState.notify_changed()
	return true
