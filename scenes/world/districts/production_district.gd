extends Node3D

const StationScene = preload("res://scenes/gameplay/buildings/farm_station.tscn")

func _ready() -> void:
	var art := WorldArtBuilder.new()
	var specs := [
		["feed_mill", "animal_feed", "Feed Mill", Vector3(5, 0, 7), 1, Color("#77513b")],
		["bakery", "bread", "Rice Bakery", Vector3(14, 0, 7), 3, Color("#a55d3d")],
		["dairy", "butter", "Dairy Workshop", Vector3(5, 0, 17), 5, Color("#6e604e")],
		["sugar_mill", "palm_sugar", "Sugar Mill", Vector3(14, 0, 17), 7, Color("#87613b")]
	]
	for spec in specs:
		art.build_khmer_building(self, str(spec[0]), str(spec[2]), spec[3], Vector3(5.8, 4.4, 4.8), spec[5])
		var station = StationScene.instantiate()
		station.setup("machine", str(spec[1]), str(spec[2]), int(spec[4]))
		station.position = spec[3]
		add_child(station)
	for position in [Vector3(1, 0, 2), Vector3(10, 0, 2), Vector3(18, 0, 2), Vector3(1, 0, 12), Vector3(18, 0, 12), Vector3(1, 0, 22), Vector3(18, 0, 22)]:
		art.add_lantern(self, position)
	art.add_crates(self, Vector3(9, 0, 13))
