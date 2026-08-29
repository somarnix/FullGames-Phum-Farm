extends SceneTree

var failures: Array[String] = []

func _initialize() -> void:
	var state := root.get_node("GameState")
	var economy := root.get_node("EconomySystem")
	state.new_game()
	var starting_coins: int = economy.coins()
	var starting_gems: int = economy.gems()
	_check(economy.can_afford_coins(50), "coin affordability is calculated")
	_check(economy.spend_coins(50), "coin purchase succeeds")
	_check(economy.coins() == starting_coins - 50, "coin balance decreases")
	_check(not economy.spend_coins(99999), "unaffordable purchase is rejected")
	_check(economy.coins() == starting_coins - 50, "rejected purchase keeps balance")
	_check(economy.add_coins(80), "coin reward succeeds")
	_check(int(state.data.total_earned) == 80, "earned coin total is tracked")
	_check(economy.spend_gems(3), "gem spending succeeds")
	_check(economy.add_gems(2), "gem reward succeeds")
	_check(economy.gems() == starting_gems - 1, "gem balance is correct")
	_finish()

func _check(condition: bool, description: String) -> void:
	if condition:
		print("PASS: ", description)
	else:
		failures.append("FAIL: " + description)

func _finish() -> void:
	if failures.is_empty():
		print("ECONOMY TEST PASSED: affordability, spending, rewards, and premium currency")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		quit(1)
