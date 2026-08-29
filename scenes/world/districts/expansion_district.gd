extends Node3D

const StationScene = preload("res://scenes/gameplay/buildings/farm_station.tscn")

var expansion_roots: Array[Node3D] = []

func _ready() -> void:
	var art := WorldArtBuilder.new()
	var positions := [Vector3(-16, 0, 5), Vector3(1, 0, 27), Vector3(28, 0, -2)]
	for index in range(3):
		var root := Node3D.new()
		root.name = "ExpansionGate_%d" % index
		root.position = positions[index]
		add_child(root)
		expansion_roots.append(root)
		for x in [-1.5, 0.0, 1.5]:
			art.add_rock(root, Vector3(x, 0, 0), 0.55)
		var station = StationScene.instantiate()
		station.setup("expansion", str(index), str(FarmData.expansions[index].name), int(FarmData.expansions[index].level))
		station.position = positions[index]
		add_child(station)
	refresh_unlocks()

func refresh_unlocks() -> void:
	for index in range(expansion_roots.size()):
		expansion_roots[index].visible = not UnlockSystem.is_expansion_unlocked(index)
