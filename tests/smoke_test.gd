extends SceneTree

var failures: Array[String] = []
var state: Node

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	state = root.get_node("GameState")
	state.save_path = "user://phum_farm_smoke_test.json"
	if state.has_save():
		DirAccess.remove_absolute(ProjectSettings.globalize_path(state.save_path))
	state.new_game()
	var world = load("res://scenes/world/farm_world.tscn").instantiate()
	world.name = "SmokeTestWorld"
	root.add_child(world)
	await process_frame
	await process_frame
	_check(world.player != null, "player is created")
	_check(world.camera != null and world.camera.projection == Camera3D.PROJECTION_ORTHOGONAL, "orthographic camera is active")
	_check(world.plots.size() == 16, "sixteen farm plots are created")
	_check(world.get_tree().get_nodes_in_group("interactable").size() >= 25, "gameplay stations are registered")

	var plot = world.plots[0]
	plot.interact(world.player)
	_check(str(state.data.plots[0].state) == "tilled", "plot can be tilled")
	plot.plant_crop("rice")
	_check(str(state.data.plots[0].state) == "growing", "seed can be planted")
	state.data.plots[0].planted_at = state.game_time() - 1000.0
	plot.interact(world.player)
	_check(int(state.data.inventory.rice) >= 7, "mature crop can be harvested")

	var feed_mill = world.get_station("AnimalFeed")
	_check(feed_mill != null, "feed mill exists")
	if feed_mill != null:
		feed_mill.perform_primary_action()
		_check(state.data.machines.has("animal_feed"), "feed production can start")
		state.data.machines.animal_feed.ready_at = state.game_time() - 1.0
		feed_mill.perform_primary_action()
		_check(int(state.data.inventory.animal_feed) >= 3, "feed product can be collected")

	var chickens = world.get_station("Chickens")
	_check(chickens != null, "chicken enclosure exists")
	if chickens != null:
		state.data.level = 2
		chickens.perform_primary_action()
		state.data.animals.chicken_ready_at = state.game_time() - 1.0
		chickens.perform_primary_action()
		_check(int(state.data.inventory.egg) == 2, "eggs can be collected")

	var coins_before := int(state.data.coins)
	var market = world.get_station("Market")
	_check(market != null, "market exists")
	if market != null:
		market.interact(world.player)
		_check(int(state.data.coins) > coins_before, "market sale earns coins")

	_check(state.save_game(), "save file writes")
	var saved_coins := int(state.data.coins)
	state.data.coins = 1
	_check(state.load_game() and int(state.data.coins) == saved_coins, "save file loads")
	DirAccess.remove_absolute(ProjectSettings.globalize_path(state.save_path))

	if failures.is_empty():
		print("SMOKE TEST PASSED: world, crops, production, animals, market, and saves")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		quit(1)

func _check(condition: bool, description: String) -> void:
	if condition:
		print("PASS: ", description)
	else:
		failures.append("FAIL: " + description)
