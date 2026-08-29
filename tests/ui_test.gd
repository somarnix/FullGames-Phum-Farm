extends SceneTree

var failures: Array[String] = []

func _initialize() -> void:
	var scene: Node = load("res://scenes/menus/main_menu.tscn").instantiate()
	root.add_child(scene)
	await process_frame
	_check(scene.interface != null, "title screen builds")
	scene._start_game(false)
	await process_frame
	await process_frame
	_check(scene.world != null, "main game starts")
	_check(scene.hud_root != null, "HUD builds")
	_check(scene.coin_label != null and not scene.coin_label.text.is_empty(), "compact currency HUD updates")
	_check(scene.bottom_buttons.size() == 5, "five icon-first navigation controls build")
	_check(scene.world.camera != null and scene.world.camera.current, "game camera is current")
	_check(scene.world.player != null, "player is connected to main scene")

	var plot = scene.world.plots[0]
	plot.interact(scene.world.player)
	plot.interact(scene.world.player)
	await process_frame
	_check(scene.active_sheet != null, "tilled plot opens contextual crop tray")
	scene._close_sheet()

	for open_method in ["_open_inventory", "_open_shop", "_open_orders", "_open_map", "_open_quest_book"]:
		scene.call(open_method)
		await process_frame
		_check(scene.active_overlay != null, "%s opens" % open_method)
		scene._close_overlay()

	scene._open_settings()
	await process_frame
	_check(scene.active_overlay != null, "settings opens with dynamic language controls")
	scene._close_overlay()

	var feed_mill = scene.world.get_node_or_null("AnimalFeed")
	scene._open_production_sheet("animal_feed", feed_mill)
	await process_frame
	_check(scene.active_sheet != null, "production building opens bottom sheet")
	scene._close_sheet()

	scene._show_level_up(2)
	await process_frame
	_check(scene.active_overlay != null, "level-up celebration opens")
	scene._close_overlay()
	if failures.is_empty():
		print("UI TEST PASSED: menu, HUD, navigation, contextual sheets, screens, level-up, player, and camera")
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
