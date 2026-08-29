extends Node

signal changed
signal level_up(new_level: int)
signal message_requested(text: String)
signal game_reset
signal game_loaded

const DEFAULT_SAVE_PATH := "user://phum_farm_save.json"
const LEGACY_SAVE_PATH := "user://khmer_harvest_save.json"
const PLOT_COUNT := 16

var data: Dictionary = {}
var save_path := DEFAULT_SAVE_PATH

func _ready() -> void:
	new_game()

func default_data() -> Dictionary:
	var plots: Array[Dictionary] = []
	for index in range(PLOT_COUNT):
		plots.append({"state": "empty", "crop": "", "planted_at": 0.0})
	return {
		"coins": 120,
		"gems": 10,
		"xp": 0,
		"level": 1,
		"day": 1,
		"minutes": 420.0,
		"selected_crop": "rice",
		"inventory": {
			"rice": 4, "corn": 0, "tomato": 0, "sugarcane": 0,
			"egg": 0, "milk": 0, "animal_feed": 2,
			"bread": 0, "butter": 0, "palm_sugar": 0
		},
		"plots": plots,
		"machines": {},
		"animals": {
			"chickens_fed": false, "chicken_ready_at": 0.0,
			"cows_fed": false, "cow_ready_at": 0.0
		},
		"expansions": [false, false, false],
		"building_placements": [],
		"quests": {"active": {}, "completed": []},
		"orders": {"active": [], "completed": []},
		"weather": "clear",
		"tutorial_step": 0,
		"total_harvested": 0,
		"total_earned": 0
	}

func new_game() -> void:
	data = default_data()
	game_reset.emit()
	changed.emit()

func replace_data(loaded_data: Dictionary) -> void:
	data = loaded_data.duplicate(true)
	normalize_runtime_data()
	game_loaded.emit()
	changed.emit()

func normalize_runtime_data() -> void:
	var defaults := default_data()
	for key in defaults:
		if not data.has(key):
			data[key] = defaults[key]
		elif defaults[key] is Dictionary and data[key] is not Dictionary:
			data[key] = defaults[key]
		elif defaults[key] is Array and data[key] is not Array:
			data[key] = defaults[key]

	for item_id in defaults.inventory:
		if not data.inventory.has(item_id):
			data.inventory[item_id] = defaults.inventory[item_id]
		else:
			data.inventory[item_id] = maxi(0, int(data.inventory[item_id]))

	while data.plots.size() < PLOT_COUNT:
		data.plots.append({"state": "empty", "crop": "", "planted_at": 0.0})
	for index in range(data.plots.size()):
		if data.plots[index] is not Dictionary:
			data.plots[index] = {"state": "empty", "crop": "", "planted_at": 0.0}
		else:
			var plot: Dictionary = data.plots[index]
			plot["state"] = str(plot.get("state", "empty"))
			plot["crop"] = str(plot.get("crop", ""))
			plot["planted_at"] = float(plot.get("planted_at", 0.0))

	for animal_key in defaults.animals:
		if not data.animals.has(animal_key):
			data.animals[animal_key] = defaults.animals[animal_key]
	while data.expansions.size() < defaults.expansions.size():
		data.expansions.append(false)

	data.coins = maxi(0, int(data.coins))
	data.gems = maxi(0, int(data.gems))
	data.xp = maxi(0, int(data.xp))
	data.level = maxi(1, int(data.level))
	data.day = maxi(1, int(data.day))
	data.minutes = clampf(float(data.minutes), 0.0, 1439.999)
	data.tutorial_step = maxi(0, int(data.tutorial_step))
	data.total_harvested = maxi(0, int(data.total_harvested))
	data.total_earned = maxi(0, int(data.total_earned))

func has_save() -> bool:
	return SaveManager.has_save()

func save_game() -> bool:
	return SaveManager.save_current_game()

func load_game() -> bool:
	return SaveManager.load_current_game()

func delete_save() -> void:
	SaveManager.delete_current_save()
	new_game()

func _migrate() -> void:
	# Compatibility entry point for older callers. File-version migration belongs to SaveManager.
	normalize_runtime_data()

func add_item(item_id: String, amount: int) -> void:
	InventorySystem.add(item_id, amount)

func take_items(requirements: Dictionary) -> bool:
	return InventorySystem.remove_many(requirements)

func spend(amount: int) -> bool:
	return EconomySystem.spend_coins(amount)

func earn(amount: int) -> void:
	EconomySystem.add_coins(amount)

func add_xp(amount: int) -> void:
	ProgressionSystem.add_xp(amount)

func notify_changed() -> void:
	changed.emit()

func game_time() -> float:
	return float(data.day) * 1440.0 + float(data.minutes)

func advance_time(real_delta: float) -> void:
	data.minutes = float(data.minutes) + maxf(real_delta, 0.0) * 3.0
	if float(data.minutes) >= 1440.0:
		data.minutes = fmod(float(data.minutes), 1440.0)
		data.day = int(data.day) + 1
		save_game()

func clock_text() -> String:
	var current_minutes := int(data.minutes)
	return "%02d:%02d" % [current_minutes / 60, current_minutes % 60]
