extends Node3D

signal crop_selection_requested(plot_index: int)

const FarmPlotScene = preload("res://scenes/gameplay/crops/farm_plot.tscn")

var plots: Array[Node] = []

func _ready() -> void:
	var art := WorldArtBuilder.new()
	var start := Vector3(-1.5, 0, -22)
	for row in range(4):
		for column in range(4):
			var plot = FarmPlotScene.instantiate()
			plot.position = start + Vector3(column * 4.1, 0, row * 3.5)
			add_child(plot)
			plot.setup(row * 4 + column)
			plot.crop_selection_requested.connect(_forward_crop_selection)
			plots.append(plot)
	art.build_bridge(self, Vector3(6, 0.18, -14.5), Vector3(3.0, 0.28, 2.7))
	for z in range(-26, -8, 3):
		art.add_fence_post(self, Vector3(-4, 0, z))
	art.add_scarecrow(self, Vector3(14, 0, -9.5))

func refresh_unlocks() -> void:
	for plot in plots:
		plot.refresh()

func plant_crop(plot_index: int, crop_id: String) -> bool:
	if plot_index < 0 or plot_index >= plots.size():
		return false
	return plots[plot_index].plant_crop(crop_id)

func highlight_plot(plot_index: int) -> void:
	for index in range(plots.size()):
		plots[index].set_selected(index == plot_index)

func _forward_crop_selection(plot_index: int) -> void:
	crop_selection_requested.emit(plot_index)
