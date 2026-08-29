extends Control

signal navigation_requested(destination: String)

func navigate(destination: String) -> void:
	navigation_requested.emit(destination)
