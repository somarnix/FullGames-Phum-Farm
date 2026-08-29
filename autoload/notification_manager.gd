extends Node

signal notification_requested(message: String)

func show_message(message: String) -> void:
	notification_requested.emit(message)
