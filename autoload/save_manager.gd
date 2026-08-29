extends Node

signal save_started(path: String)
signal save_completed(success: bool)
signal load_completed(success: bool)
signal save_deleted

const SAVE_VERSION := 2
const TEMP_SUFFIX := ".tmp"
const BACKUP_SUFFIX := ".bak"

var last_error := ""

func has_save() -> bool:
	for path in _candidate_paths():
		if FileAccess.file_exists(path):
			return true
	return false

func save_current_game() -> bool:
	last_error = ""
	var path := GameState.save_path
	var temporary_path := path + TEMP_SUFFIX
	save_started.emit(path)
	var document := _encode_state(GameState.data)
	var serialized := JSON.stringify(document, "  ")
	if not _write_text(temporary_path, serialized):
		last_error = "Could not write temporary save file"
		save_completed.emit(false)
		return false
	if _parse_document(temporary_path).is_empty():
		last_error = "Temporary save verification failed"
		_remove_if_present(temporary_path)
		save_completed.emit(false)
		return false
	if not _commit_temporary_file(path, temporary_path):
		save_completed.emit(false)
		return false
	GameState.message_requested.emit("Farm saved")
	save_completed.emit(true)
	return true

func load_current_game() -> bool:
	last_error = ""
	for path in _candidate_paths():
		if not FileAccess.file_exists(path):
			continue
		var document := _parse_document(path)
		if document.is_empty():
			continue
		var runtime_data := _decode_document(document)
		if runtime_data.is_empty():
			continue
		GameState.replace_data(runtime_data)
		var needs_upgrade := int(document.get("save_version", 0)) != SAVE_VERSION
		var recovered_from_alternate := path != GameState.save_path
		if needs_upgrade or recovered_from_alternate:
			save_current_game()
		load_completed.emit(true)
		return true
	last_error = "No valid save file could be loaded"
	load_completed.emit(false)
	return false

func delete_current_save() -> void:
	var paths := [GameState.save_path, GameState.save_path + TEMP_SUFFIX, GameState.save_path + BACKUP_SUFFIX]
	if GameState.save_path == GameState.DEFAULT_SAVE_PATH:
		paths.append(GameState.LEGACY_SAVE_PATH)
	for path in paths:
		_remove_if_present(path)
	save_deleted.emit()

func _candidate_paths() -> Array[String]:
	var paths: Array[String] = [GameState.save_path, GameState.save_path + BACKUP_SUFFIX]
	if GameState.save_path == GameState.DEFAULT_SAVE_PATH:
		paths.append(GameState.LEGACY_SAVE_PATH)
	return paths

func _encode_state(runtime: Dictionary) -> Dictionary:
	return {
		"save_version": SAVE_VERSION,
		"saved_at_unix": int(Time.get_unix_time_from_system()),
		"player": {
			"xp": int(runtime.get("xp", 0)),
			"level": int(runtime.get("level", 1)),
			"tutorial_step": int(runtime.get("tutorial_step", 0)),
			"total_harvested": int(runtime.get("total_harvested", 0))
		},
		"economy": {
			"coins": int(runtime.get("coins", 0)),
			"gems": int(runtime.get("gems", 0)),
			"total_earned": int(runtime.get("total_earned", 0))
		},
		"inventory": runtime.get("inventory", {}).duplicate(true),
		"world": {
			"day": int(runtime.get("day", 1)),
			"minutes": float(runtime.get("minutes", 420.0)),
			"weather": str(runtime.get("weather", "clear")),
			"selected_crop": str(runtime.get("selected_crop", "rice")),
			"expansions": runtime.get("expansions", []).duplicate(true)
		},
		"buildings": {"placements": runtime.get("building_placements", []).duplicate(true)},
		"crops": {"plots": runtime.get("plots", []).duplicate(true)},
		"animals": runtime.get("animals", {}).duplicate(true),
		"production": {"machines": runtime.get("machines", {}).duplicate(true)},
		"quests": runtime.get("quests", {}).duplicate(true),
		"orders": runtime.get("orders", {}).duplicate(true)
	}

func _decode_document(document: Dictionary) -> Dictionary:
	if document.has("save_version") and document.has("player"):
		var runtime := GameState.default_data()
		var player: Dictionary = document.get("player", {})
		var economy: Dictionary = document.get("economy", {})
		var world: Dictionary = document.get("world", {})
		var buildings: Dictionary = document.get("buildings", {})
		var crops: Dictionary = document.get("crops", {})
		var production: Dictionary = document.get("production", {})
		runtime.xp = int(player.get("xp", runtime.xp))
		runtime.level = int(player.get("level", runtime.level))
		runtime.tutorial_step = int(player.get("tutorial_step", runtime.tutorial_step))
		runtime.total_harvested = int(player.get("total_harvested", runtime.total_harvested))
		runtime.coins = int(economy.get("coins", runtime.coins))
		runtime.gems = int(economy.get("gems", runtime.gems))
		runtime.total_earned = int(economy.get("total_earned", runtime.total_earned))
		runtime.inventory = document.get("inventory", runtime.inventory)
		runtime.day = int(world.get("day", runtime.day))
		runtime.minutes = float(world.get("minutes", runtime.minutes))
		runtime.weather = str(world.get("weather", runtime.weather))
		runtime.selected_crop = str(world.get("selected_crop", runtime.selected_crop))
		runtime.expansions = world.get("expansions", runtime.expansions)
		runtime.building_placements = buildings.get("placements", runtime.building_placements)
		runtime.plots = crops.get("plots", runtime.plots)
		runtime.animals = document.get("animals", runtime.animals)
		runtime.machines = production.get("machines", runtime.machines)
		runtime.quests = document.get("quests", runtime.quests)
		runtime.orders = document.get("orders", runtime.orders)
		return runtime
	# Version 0/1 saves used the runtime dictionary as the entire document.
	if document.has("coins") and document.has("inventory"):
		return document.duplicate(true)
	return {}

func _write_text(path: String, text: String) -> bool:
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		return false
	file.store_string(text)
	file.flush()
	file = null
	return true

func _read_text(path: String) -> String:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return ""
	return file.get_as_text()

func _parse_document(path: String) -> Dictionary:
	var parser := JSON.new()
	if parser.parse(_read_text(path)) != OK or parser.data is not Dictionary:
		return {}
	return parser.data

func _commit_temporary_file(path: String, temporary_path: String) -> bool:
	var backup_path := path + BACKUP_SUFFIX
	_remove_if_present(backup_path)
	if FileAccess.file_exists(path):
		var backup_error := DirAccess.rename_absolute(ProjectSettings.globalize_path(path), ProjectSettings.globalize_path(backup_path))
		if backup_error != OK:
			last_error = "Could not create save backup"
			_remove_if_present(temporary_path)
			return false
	var commit_error := DirAccess.rename_absolute(ProjectSettings.globalize_path(temporary_path), ProjectSettings.globalize_path(path))
	if commit_error != OK:
		last_error = "Could not commit temporary save"
		if FileAccess.file_exists(backup_path):
			DirAccess.rename_absolute(ProjectSettings.globalize_path(backup_path), ProjectSettings.globalize_path(path))
		return false
	return true

func _remove_if_present(path: String) -> void:
	if FileAccess.file_exists(path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(path))
