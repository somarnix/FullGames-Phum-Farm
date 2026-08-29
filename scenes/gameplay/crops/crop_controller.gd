extends Node

signal crop_planted(plot_index: int, crop_id: String)

func plant(plot_index: int, crop_id: String) -> void:
	crop_planted.emit(plot_index, crop_id)
