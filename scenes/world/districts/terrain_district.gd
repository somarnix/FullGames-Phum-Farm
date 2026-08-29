extends Node3D

func _ready() -> void:
	var art := WorldArtBuilder.new()
	art.add_static_box(self, "WorldGround", Vector3(92, 0.3, 72), Vector3(0, -0.15, 0), Color("#79a958"))
	art.add_box(self, "MainDirtRoad", Vector3(78, 0.05, 4.8), Vector3(-4, 0.04, -1), Color("#ba8857"))
	art.add_box(self, "FarmRoad", Vector3(4.8, 0.06, 57), Vector3(-7, 0.05, 1), Color("#ba8857"))
	art.add_box(self, "VillageRoad", Vector3(4.4, 0.06, 30), Vector3(22, 0.05, 9), Color("#b68051"))
	art.add_box(self, "IrrigationCanal", Vector3(2.1, 0.12, 31), Vector3(6, -0.02, -13), Color("#4799a7"), art.water_material())
	art.add_box(self, "River", Vector3(13, 0.16, 72), Vector3(39.5, -0.03, 0), Color("#378fa3"), art.water_material())
	art.build_bridge(self, Vector3(39.5, 0.2, -1), Vector3(15, 0.4, 3.4))
	for z in range(-31, 32, 7):
		art.add_palm(self, Vector3(-43, 0, z), 0.8 + float(posmod(z, 3)) * 0.08)
		art.add_tree(self, Vector3(45, 0, z + 2), 0.9)
	for x in range(-37, 38, 7):
		art.add_tree(self, Vector3(x, 0, -34), 0.75)
		art.add_palm(self, Vector3(x + 2, 0, 34), 0.72)
