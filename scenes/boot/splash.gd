extends Control

signal finished

func continue_to_menu() -> void:
	finished.emit()
