extends Node

signal catalogs_loaded

var crops: Dictionary = {}
var products: Dictionary = {}
var animals: Dictionary = {}
var buildings: Dictionary = {}
var recipes: Dictionary = {}
var orders: Dictionary = {}
var quests: Dictionary = {}
var achievements: Dictionary = {}
var rewards: Dictionary = {}
var economy: Dictionary = {}
var levels: Array = []
var plot_unlocks: Array = []
var expansions: Array = []

func _ready() -> void:
	reload_catalogs()

func reload_catalogs() -> void:
	var combined := _read_json("res://data/game_data.json")
	crops = combined.get("crops", {})
	products = combined.get("products", {})
	levels = combined.get("levels", [])
	plot_unlocks = combined.get("plot_unlocks", [])
	expansions = combined.get("expansions", [])

	crops = _merge_catalogs(crops, _read_json("res://data/crops.json"))
	recipes = _read_json("res://data/recipes.json")
	products = _merge_catalogs(products, recipes)
	animals = _read_json("res://data/animals.json")
	buildings = _read_json("res://data/buildings.json")
	orders = _read_json("res://data/orders.json")
	quests = _read_json("res://data/quests.json")
	achievements = _read_json("res://data/achievements.json")
	rewards = _read_json("res://data/rewards.json")
	economy = _read_json("res://data/economy.json")
	var level_catalog := _read_json("res://data/levels.json")
	if level_catalog.has("xp_thresholds"):
		levels = level_catalog.xp_thresholds
	catalogs_loaded.emit()

func crop(id: String) -> Dictionary:
	return crops.get(id, {})

func product(id: String) -> Dictionary:
	return products.get(id, {})

func level_for_xp(value: int) -> int:
	var result := 1
	for index in range(levels.size()):
		if value >= int(levels[index]):
			result = index + 1
	return result

func xp_for_next_level(level: int) -> int:
	if level >= levels.size():
		return int(levels.back()) if not levels.is_empty() else 0
	return int(levels[level])

func _read_json(path: String) -> Dictionary:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("Could not load data catalog: " + path)
		return {}
	var parsed = JSON.parse_string(file.get_as_text())
	if parsed is not Dictionary:
		push_error("Invalid data catalog: " + path)
		return {}
	return parsed

func _merge_catalogs(base: Dictionary, overlay: Dictionary) -> Dictionary:
	var result := base.duplicate(true)
	for entry_id in overlay:
		if result.get(entry_id) is Dictionary and overlay[entry_id] is Dictionary:
			var merged: Dictionary = result[entry_id].duplicate(true)
			merged.merge(overlay[entry_id], true)
			result[entry_id] = merged
		else:
			result[entry_id] = overlay[entry_id]
	return result
