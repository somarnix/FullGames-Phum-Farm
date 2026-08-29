extends SceneTree

var failures: Array[String] = []

func _initialize() -> void:
	var scene_manager := root.get_node("SceneManager")
	_check(scene_manager.can_change_scene(scene_manager.MAIN_MENU_SCENE), "main menu scene is valid")
	_check(scene_manager.can_change_scene(scene_manager.CURRENT_FARM_SCENE), "current farm scene is valid")
	_check(not scene_manager.can_change_scene("res://missing_scene.tscn"), "missing scene is rejected")
	_check(not scene_manager.transition_in_progress, "scene manager starts idle")
	if failures.is_empty():
		print("SCENE MANAGER TEST PASSED: scene validation and transition state")
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
