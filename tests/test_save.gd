extends SceneTree

var failures: Array[String] = []

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var state := root.get_node("GameState")
	var save_manager := root.get_node("SaveManager")
	var original_path: String = state.save_path
	state.save_path = "user://phum_farm_phase2_save_test.json"
	save_manager.delete_current_save()
	state.new_game()
	state.data.coins = 777
	state.data.gems = 23
	state.data.inventory.rice = 19
	state.data.plots[0] = {"state": "growing", "crop": "rice", "planted_at": 123.0}
	state.data.animals.chickens_fed = true
	state.data.machines.animal_feed = {"started_at": 10.0, "ready_at": 28.0}
	state.data.building_placements.append({"id": "bakery", "position": [4, 0, 6], "rotation": 90})
	state.data.quests.active = {"first_harvest": 2}
	state.data.orders.active = [{"id": "village_breakfast"}]

	_check(state.save_game(), "versioned save writes successfully")
	var file := FileAccess.open(state.save_path, FileAccess.READ)
	var document = JSON.parse_string(file.get_as_text()) if file != null else null
	file = null
	_check(document is Dictionary, "saved document is valid JSON")
	if document is Dictionary:
		_check(int(document.get("save_version", 0)) == save_manager.SAVE_VERSION, "save version is current")
		for section in ["player", "economy", "inventory", "world", "buildings", "crops", "animals", "production", "quests", "orders"]:
			_check(document.has(section), "save contains %s section" % section)

	state.data.coins = 1
	state.data.inventory.rice = 0
	state.data.building_placements.clear()
	_check(state.load_game(), "versioned save loads successfully")
	_check(int(state.data.coins) == 777, "economy state restores")
	_check(int(state.data.inventory.rice) == 19, "inventory state restores")
	_check(str(state.data.plots[0].crop) == "rice", "crop state restores")
	_check(bool(state.data.animals.chickens_fed), "animal state restores")
	_check(state.data.machines.has("animal_feed"), "production state restores")
	_check(state.data.building_placements.size() == 1, "building placement state restores")
	_check(state.data.quests.active.has("first_harvest"), "quest state restores")
	_check(state.data.orders.active.size() == 1, "order state restores")

	state.data.coins = 888
	_check(state.save_game(), "second save keeps a recovery generation")
	var corrupt_file := FileAccess.open(state.save_path, FileAccess.WRITE)
	if corrupt_file != null:
		corrupt_file.store_string("{corrupted")
		corrupt_file = null
	_check(state.load_game(), "backup recovers a corrupted primary save")
	_check(int(state.data.coins) == 777, "backup restores the previous valid generation")

	save_manager.delete_current_save()
	var legacy_data: Dictionary = state.default_data()
	legacy_data["coins"] = 432
	(legacy_data["inventory"] as Dictionary)["rice"] = 12
	var legacy_file := FileAccess.open(state.save_path, FileAccess.WRITE)
	if legacy_file != null:
		legacy_file.store_string(JSON.stringify(legacy_data))
		legacy_file = null
	_check(state.load_game(), "legacy flat save migrates")
	_check(int(state.data.coins) == 432, "legacy economy value survives migration")
	_check(int(state.data.inventory.rice) == 12, "legacy inventory value survives migration")
	var upgraded_file := FileAccess.open(state.save_path, FileAccess.READ)
	var upgraded = JSON.parse_string(upgraded_file.get_as_text()) if upgraded_file != null else null
	upgraded_file = null
	_check(upgraded is Dictionary and int(upgraded.get("save_version", 0)) == save_manager.SAVE_VERSION, "legacy save is rewritten at the current version")

	save_manager.delete_current_save()
	state.save_path = original_path
	state.new_game()
	_finish("SAVE TEST PASSED: versioning, sections, persistence, and recovery state")

func _check(condition: bool, description: String) -> void:
	if condition:
		print("PASS: ", description)
	else:
		failures.append("FAIL: " + description)

func _finish(success_message: String) -> void:
	if failures.is_empty():
		print(success_message)
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		quit(1)
