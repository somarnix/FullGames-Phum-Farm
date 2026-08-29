extends Node3D

const StationScene = preload("res://scenes/gameplay/buildings/farm_station.tscn")

func _ready() -> void:
	var art := WorldArtBuilder.new()
	art.build_khmer_building(self, "farmhouse", "FarmhouseVisual", Vector3(-25, 0, -16), Vector3(7.5, 5.5, 6), Color("#70432f"), true)
	_add_station("home", "farmhouse", "Farmhouse", Vector3(-25, 0, -16))
	art.build_khmer_building(self, "barn", "Barn", Vector3(-24, 0, -5), Vector3(6, 4.5, 5), Color("#80523b"))
	art.build_lotus_pond(self, Vector3(-34, 0, -14), Vector2(7, 5))
	_build_well(art, Vector3(-15, 0, -14))
	for x in range(-32, -17, 3):
		art.add_fence_post(self, Vector3(x, 0, -23))
	for z in range(-22, -8, 3):
		art.add_fence_post(self, Vector3(-34, 0, z))
	art.add_cart(self, Vector3(-20, 0, -8))
	art.add_hay(self, Vector3(-28, 0, -7))

func _build_well(art: WorldArtBuilder, position: Vector3) -> void:
	var root := Node3D.new()
	root.name = "IrrigationWellVisual"
	root.position = position
	add_child(root)
	art.add_cylinder(root, "Well", Vector3(0, 0.45, 0), 0.9, 0.8, Color("#9a8264"))
	art.add_cylinder(root, "Water", Vector3(0, 0.86, 0), 0.68, 0.04, Color("#4f9ca9"))
	for x in [-0.75, 0.75]:
		art.add_box(root, "Post", Vector3(0.16, 2.2, 0.16), Vector3(x, 1.2, 0), Color("#5d402c"))
	_add_station("well", "well", "Irrigation Well", position)

func _add_station(type: String, id: String, title: String, position: Vector3, level := 1) -> Node:
	var station = StationScene.instantiate()
	station.setup(type, id, title, level)
	station.position = position
	add_child(station)
	return station
