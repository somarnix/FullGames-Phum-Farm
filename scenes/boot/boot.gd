extends Node

func _ready() -> void:
	call_deferred("_open_main_menu")

func _open_main_menu() -> void:
	SceneManager.open_main_menu()
