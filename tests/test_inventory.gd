extends SceneTree

var failures: Array[String] = []

func _initialize() -> void:
	var state := root.get_node("GameState")
	var inventory := root.get_node("InventorySystem")
	state.new_game()
	var starting_rice: int = inventory.amount("rice")
	_check(inventory.add("rice", 5), "items can be added")
	_check(inventory.amount("rice") == starting_rice + 5, "added quantity is recorded")
	_check(inventory.has({"rice": 3, "animal_feed": 1}), "multi-item requirements are detected")
	_check(inventory.remove_many({"rice": 3, "animal_feed": 1}), "multi-item removal succeeds atomically")
	var rice_after_removal: int = inventory.amount("rice")
	_check(not inventory.remove_many({"rice": 999}), "insufficient removal is rejected")
	_check(inventory.amount("rice") == rice_after_removal, "failed removal does not mutate inventory")
	_check(not inventory.add("rice", inventory.capacity() + 1, true), "capacity enforcement rejects overflow")
	_check(inventory.used_capacity() <= inventory.capacity(), "inventory remains within capacity")
	_finish()

func _check(condition: bool, description: String) -> void:
	if condition:
		print("PASS: ", description)
	else:
		failures.append("FAIL: " + description)

func _finish() -> void:
	if failures.is_empty():
		print("INVENTORY TEST PASSED: add, requirements, atomic removal, and capacity")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		quit(1)
