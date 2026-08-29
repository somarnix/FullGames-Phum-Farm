extends Node3D

const StationScene = preload("res://scenes/gameplay/buildings/farm_station.tscn")

func _ready() -> void:
	var art := WorldArtBuilder.new()
	var chicken_root := Node3D.new()
	chicken_root.name = "ChickenDistrict"
	add_child(chicken_root)
	art.build_khmer_building(chicken_root, "chicken_coop", "ChickenCoop", Vector3(-29, 0, 12), Vector3(4.5, 3.2, 3.5), Color("#855137"))
	art.build_fence_rect(chicken_root, Vector3(-24, 0, 11), Vector2(14, 10))
	for position in [Vector3(-23, 0, 9), Vector3(-20, 0, 12), Vector3(-25, 0, 14)]:
		art.add_animal(chicken_root, "chicken", position, Color("#f5eee0"), 0.65)
	_add_station(chicken_root, "chickens", "chickens", "Chicken Enclosure", Vector3(-23, 0, 11), 2)

	var pasture_root := Node3D.new()
	pasture_root.name = "CowBuffaloDistrict"
	add_child(pasture_root)
	art.build_khmer_building(pasture_root, "cow_shelter", "CowShelter", Vector3(-25, 0, 26), Vector3(6, 4, 4), Color("#654536"))
	art.build_fence_rect(pasture_root, Vector3(-23, 0, 25), Vector2(18, 10))
	art.add_animal(pasture_root, "cow", Vector3(-20, 0, 23), Color("#ead8b8"), 1.35)
	art.add_animal(pasture_root, "water_buffalo", Vector3(-25, 0, 27), Color("#4f5551"), 1.45)
	_add_station(pasture_root, "cows", "cows", "Cow & Buffalo Pasture", Vector3(-22, 0, 25), 4)

func _add_station(parent: Node3D, type: String, id: String, title: String, position: Vector3, level: int) -> void:
	var station = StationScene.instantiate()
	station.setup(type, id, title, level)
	station.position = position
	parent.add_child(station)
