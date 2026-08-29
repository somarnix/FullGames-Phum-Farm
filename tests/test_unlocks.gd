extends SceneTree

var failures: Array[String] = []

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var state := root.get_node("GameState")
	var unlocks := root.get_node("UnlockSystem")
	state.new_game()
	_check(unlocks.is_crop_unlocked("rice"), "rice starts unlocked")
	_check(not unlocks.is_crop_unlocked("corn"), "corn starts locked")
	_check(unlocks.is_plot_unlocked(0), "starter plot is unlocked")
	_check(not unlocks.is_plot_unlocked(4), "later plot starts locked")
	_check(unlocks.is_building_unlocked("feed_mill"), "starter building is unlocked")
	_check(not unlocks.is_building_unlocked("bakery"), "later building starts locked")
	_check(not unlocks.is_animal_unlocked("chicken"), "chicken follows its level rule")
	state.data.level = 5
	_check(unlocks.is_crop_unlocked("corn"), "crop unlock responds to level")
	_check(unlocks.is_animal_unlocked("chicken"), "animal unlock responds to level")
	_check(unlocks.is_building_unlocked("bakery"), "building unlock responds to level")
	_check(unlocks.is_district_unlocked("production_district"), "district unlock responds to level")
	_finish()

func _check(condition: bool, description: String) -> void:
	if condition:
		print("PASS: ", description)
	else:
		failures.append("FAIL: " + description)

func _finish() -> void:
	if failures.is_empty():
		print("UNLOCK TEST PASSED: crops, plots, animals, buildings, and districts")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		quit(1)
