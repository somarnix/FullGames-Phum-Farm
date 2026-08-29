extends Node3D

func _ready() -> void:
	var art := WorldArtBuilder.new()
	for z in range(-24, 29, 7):
		art.add_palm(self, Vector3(34, 0, z), 0.78)
	for z in range(-25, 30, 8):
		art.add_banana(self, Vector3(27, 0, z + 2))
	art.build_dock(self, Vector3(35.5, 0, 24))
	art.add_boat(self, Vector3(40, 0.12, 25))
	art.build_lotus_pond(self, Vector3(26, 0, 29), Vector2(7, 5))
