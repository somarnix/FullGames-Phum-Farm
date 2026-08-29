extends Node

signal changed
signal level_up(new_level: int)
signal message_requested(text: String)

const DEFAULT_SAVE_PATH := "user://phum_farm_save.json"
const LEGACY_SAVE_PATH := "user://khmer_harvest_save.json"

var data: Dictionary = {}
var save_path := DEFAULT_SAVE_PATH

func _ready() -> void:
	new_game()

func new_game() -> void:
	data = {
		"coins": 120,
		"gems": 10,
		"xp": 0,
		"level": 1,
		"day": 1,
		"minutes": 420.0,
		"selected_crop": "rice",
		"inventory": {"rice": 4, "corn": 0, "tomato": 0, "sugarcane": 0, "egg": 0, "milk": 0, "animal_feed": 2, "bread": 0, "butter": 0, "palm_sugar": 0},
		"plots": [],
		"machines": {},
		"animals": {"chickens_fed": false, "chicken_ready_at": 0.0, "cows_fed": false, "cow_ready_at": 0.0},
		"expansions": [false, false, false],
		"weather": "clear",
		"tutorial_step": 0,
		"total_harvested": 0,
		"total_earned": 0
	}
	for i in range(16):
		data.plots.append({"state": "empty", "crop": "", "planted_at": 0.0})
	changed.emit()

func has_save() -> bool:
	if FileAccess.file_exists(save_path):
		return true
	return save_path == DEFAULT_SAVE_PATH and FileAccess.file_exists(LEGACY_SAVE_PATH)

func save_game() -> bool:
	var file := FileAccess.open(save_path, FileAccess.WRITE)
	if file == null:
		return false
	file.store_string(JSON.stringify(data, "  "))
	message_requested.emit("Farm saved")
	return true

func load_game() -> bool:
	if not has_save():
		return false
	var path_to_load := save_path
	if not FileAccess.file_exists(path_to_load) and save_path == DEFAULT_SAVE_PATH:
		path_to_load = LEGACY_SAVE_PATH
	var file := FileAccess.open(path_to_load, FileAccess.READ)
	var parsed = JSON.parse_string(file.get_as_text())
	if parsed is not Dictionary:
		return false
	data = parsed
	_migrate()
	if path_to_load == LEGACY_SAVE_PATH:
		save_game()
	changed.emit()
	return true

func delete_save() -> void:
	if has_save():
		DirAccess.remove_absolute(ProjectSettings.globalize_path(save_path))
	new_game()

func _migrate() -> void:
	var defaults := {
		"coins": 120, "gems": 10, "xp": 0, "level": 1, "day": 1, "minutes": 420.0,
		"selected_crop": "rice", "inventory": {}, "plots": [], "machines": {},
		"animals": {}, "expansions": [false, false, false], "weather": "clear",
		"tutorial_step": 0, "total_harvested": 0, "total_earned": 0
	}
	for key in defaults:
		if not data.has(key):
			data[key] = defaults[key]
	while data.plots.size() < 16:
		data.plots.append({"state": "empty", "crop": "", "planted_at": 0.0})
	for item in ["rice", "corn", "tomato", "sugarcane", "egg", "milk", "animal_feed", "bread", "butter", "palm_sugar"]:
		if not data.inventory.has(item):
			data.inventory[item] = 0
	for key in ["chickens_fed", "cows_fed"]:
		if not data.animals.has(key):
			data.animals[key] = false
	for key in ["chicken_ready_at", "cow_ready_at"]:
		if not data.animals.has(key):
			data.animals[key] = 0.0

func add_item(id: String, amount: int) -> void:
	data.inventory[id] = int(data.inventory.get(id, 0)) + amount
	changed.emit()

func take_items(requirements: Dictionary) -> bool:
	for id in requirements:
		if int(data.inventory.get(id, 0)) < int(requirements[id]):
			return false
	for id in requirements:
		data.inventory[id] = int(data.inventory.get(id, 0)) - int(requirements[id])
	changed.emit()
	return true

func spend(amount: int) -> bool:
	if int(data.coins) < amount:
		message_requested.emit("Not enough coins")
		return false
	data.coins = int(data.coins) - amount
	changed.emit()
	return true

func earn(amount: int) -> void:
	data.coins = int(data.coins) + amount
	data.total_earned = int(data.total_earned) + amount
	changed.emit()

func add_xp(amount: int) -> void:
	data.xp = int(data.xp) + amount
	var previous := int(data.level)
	data.level = FarmData.level_for_xp(int(data.xp))
	if int(data.level) > previous:
		level_up.emit(int(data.level))
	changed.emit()

func game_time() -> float:
	return float(data.day) * 1440.0 + float(data.minutes)

func advance_time(real_delta: float) -> void:
	data.minutes = float(data.minutes) + real_delta * 3.0
	if float(data.minutes) >= 1440.0:
		data.minutes = fmod(float(data.minutes), 1440.0)
		data.day = int(data.day) + 1
		save_game()

func clock_text() -> String:
	var minutes := int(data.minutes)
	return "%02d:%02d" % [minutes / 60, minutes % 60]
