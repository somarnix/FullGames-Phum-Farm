extends Node

signal transition_started(path: String)
signal transition_completed(path: String)
signal transition_failed(path: String, reason: String)

const BOOT_SCENE := "res://scenes/boot/boot.tscn"
const MAIN_MENU_SCENE := "res://scenes/menus/main_menu.tscn"
const CURRENT_FARM_SCENE := "res://scenes/world/farm_world.tscn"

var transition_in_progress := false
var current_scene_path := ""

func can_change_scene(path: String) -> bool:
	return not path.is_empty() and ResourceLoader.exists(path, "PackedScene")

func change_scene(path: String) -> bool:
	if transition_in_progress:
		transition_failed.emit(path, "A scene transition is already in progress")
		return false
	if not can_change_scene(path):
		transition_failed.emit(path, "Scene does not exist or is not a PackedScene")
		return false
	transition_in_progress = true
	transition_started.emit(path)
	var error := get_tree().change_scene_to_file(path)
	transition_in_progress = false
	if error != OK:
		transition_failed.emit(path, error_string(error))
		return false
	current_scene_path = path
	transition_completed.emit(path)
	return true

func open_main_menu() -> bool:
	return change_scene(MAIN_MENU_SCENE)

func open_current_farm_scene() -> bool:
	return change_scene(CURRENT_FARM_SCENE)
