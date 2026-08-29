extends Node

var music_volume := 0.72
var effects_volume := 0.86

func set_music_volume(value: float) -> void:
	music_volume = clampf(value, 0.0, 1.0)

func set_effects_volume(value: float) -> void:
	effects_volume = clampf(value, 0.0, 1.0)
