extends Node

var settings := {"music": 0.72, "effects": 0.86, "vibration": true, "locale": "en"}

func set_value(key: String, value: Variant) -> void:
	settings[key] = value
