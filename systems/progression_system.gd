extends Node

signal xp_changed(total_xp: int, delta: int)
signal level_changed(previous_level: int, new_level: int)

func level_for_xp(xp: int) -> int:
	return FarmData.level_for_xp(maxi(0, xp))

func current_level() -> int:
	return int(GameState.data.get("level", 1))

func current_xp() -> int:
	return int(GameState.data.get("xp", 0))

func next_level_xp() -> int:
	return FarmData.xp_for_next_level(current_level())

func add_xp(amount: int) -> bool:
	if amount <= 0:
		return false
	var previous_level := current_level()
	GameState.data.xp = current_xp() + amount
	GameState.data.level = level_for_xp(current_xp())
	xp_changed.emit(current_xp(), amount)
	if current_level() > previous_level:
		level_changed.emit(previous_level, current_level())
		GameState.level_up.emit(current_level())
	GameState.notify_changed()
	return true

func set_xp(value: int) -> void:
	var previous_level := current_level()
	GameState.data.xp = maxi(0, value)
	GameState.data.level = level_for_xp(current_xp())
	xp_changed.emit(current_xp(), 0)
	if current_level() != previous_level:
		level_changed.emit(previous_level, current_level())
		if current_level() > previous_level:
			GameState.level_up.emit(current_level())
	GameState.notify_changed()

func progress_to_next_level() -> float:
	var level := current_level()
	var previous_threshold := int(FarmData.levels[level - 1]) if level - 1 < FarmData.levels.size() else current_xp()
	var next_threshold := FarmData.xp_for_next_level(level)
	if next_threshold <= previous_threshold:
		return 1.0
	return clampf(float(current_xp() - previous_threshold) / float(next_threshold - previous_threshold), 0.0, 1.0)
