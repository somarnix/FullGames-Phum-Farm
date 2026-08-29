extends SceneTree

var failures: Array[String] = []

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var state := root.get_node("GameState")
	var progression := root.get_node("ProgressionSystem")
	state.new_game()
	_check(progression.current_level() == 1, "new farm starts at level one")
	_check(progression.add_xp(44), "XP can be awarded")
	_check(progression.current_level() == 1, "level remains locked below threshold")
	_check(progression.add_xp(1), "threshold XP can be awarded")
	_check(progression.current_level() == 2, "level increases at threshold")
	_check(progression.next_level_xp() == 115, "next threshold is data driven")
	_check(progression.progress_to_next_level() == 0.0, "new level progress starts at zero")
	progression.set_xp(210)
	_check(progression.current_level() == 4, "setting XP recalculates level")
	_finish()

func _check(condition: bool, description: String) -> void:
	if condition:
		print("PASS: ", description)
	else:
		failures.append("FAIL: " + description)

func _finish() -> void:
	if failures.is_empty():
		print("PROGRESSION TEST PASSED: XP thresholds and level calculation")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		quit(1)
