extends Node3D

const StationScene = preload("res://scenes/gameplay/buildings/farm_station.tscn")

func _ready() -> void:
	var art := WorldArtBuilder.new()
	for position in [Vector3(26, 0, -17), Vector3(32, 0, -17), Vector3(26, 0, -10), Vector3(32, 0, -10)]:
		art.build_market_stall(self, position)
	var market = StationScene.instantiate()
	market.setup("market", "market", "Village Market")
	market.position = Vector3(29, 0, -13)
	add_child(market)
	for position in [Vector3(26, 0, 11), Vector3(31, 0, 18), Vector3(27, 0, 26)]:
		art.build_khmer_building(self, "village_house", "VillageHouse", position, Vector3(5.5, 4.4, 4.5), Color("#6f4935"), true)
	art.build_pavilion(self, Vector3(33, 0, 6))
	art.add_bicycle(self, Vector3(25, 0, 15))
