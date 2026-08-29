extends StaticBody3D

signal plot_changed
signal crop_selection_requested(plot_index: int)

var plot_index := 0
var soil_mesh: MeshInstance3D
var crop_root: Node3D
var lock_root: Node3D
var refresh_accumulator := 0.0

func setup(index: int) -> void:
	plot_index = index
	name = "FarmPlot_%02d" % (index + 1)
	add_to_group("interactable")
	_build_base()
	refresh()

func _process(delta: float) -> void:
	refresh_accumulator += delta
	if refresh_accumulator >= 1.0:
		refresh_accumulator = 0.0
		refresh()

func _build_base() -> void:
	soil_mesh = MeshInstance3D.new()
	var mesh := BoxMesh.new()
	mesh.size = Vector3(3.25, 0.28, 2.6)
	soil_mesh.mesh = mesh
	soil_mesh.position.y = 0.14
	add_child(soil_mesh)
	var collider := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(3.25, 0.35, 2.6)
	collider.shape = shape
	collider.position.y = 0.14
	add_child(collider)
	crop_root = Node3D.new()
	crop_root.name = "Crops"
	add_child(crop_root)
	lock_root = Node3D.new()
	lock_root.name = "LockedLand"
	add_child(lock_root)

func is_unlocked() -> bool:
	return UnlockSystem.is_plot_unlocked(plot_index)

func prompt_text() -> String:
	if not is_unlocked():
		return "Plot unlocks at level %d" % int(FarmData.plot_unlocks[plot_index])
	var plot: Dictionary = GameState.data.plots[plot_index]
	var state := str(plot.get("state", "empty"))
	if state == "empty":
		return "E  Till this plot"
	if state == "tilled":
		var crop := FarmData.crop(str(GameState.data.selected_crop))
		return "E  Plant %s (%d coins)" % [str(crop.get("name", "Seed")), int(crop.get("seed_cost", 0))]
	if _is_ready(plot):
		return "E  Harvest %s" % str(FarmData.crop(str(plot.crop)).get("name", "crop"))
	return "%s is growing - %d%%" % [str(FarmData.crop(str(plot.crop)).get("name", "Crop")), int(_growth_ratio(plot) * 100.0)]

func interact(_player: Node) -> void:
	if not is_unlocked():
		GameState.message_requested.emit(prompt_text())
		return
	var plot: Dictionary = GameState.data.plots[plot_index]
	var state := str(plot.get("state", "empty"))
	if state == "empty":
		plot.state = "tilled"
		GameState.data.tutorial_step = maxi(int(GameState.data.tutorial_step), 1)
		GameState.message_requested.emit("Soil tilled. Choose a seed and plant it.")
	elif state == "tilled":
		crop_selection_requested.emit(plot_index)
		return
	elif _is_ready(plot):
		var crop_id := str(plot.crop)
		var crop := FarmData.crop(crop_id)
		GameState.add_item(crop_id, 3)
		GameState.add_xp(int(crop.get("xp", 1)))
		GameState.data.total_harvested = int(GameState.data.total_harvested) + 3
		plot.state = "empty"
		plot.crop = ""
		plot.planted_at = 0.0
		GameState.data.tutorial_step = maxi(int(GameState.data.tutorial_step), 3)
		GameState.message_requested.emit("Harvested 3 %s" % str(crop.get("name", "crop")))
	else:
		GameState.message_requested.emit(prompt_text())
	GameState.changed.emit()
	plot_changed.emit()
	refresh()

func plant_crop(crop_id: String) -> bool:
	if not is_unlocked():
		return false
	var plot: Dictionary = GameState.data.plots[plot_index]
	if str(plot.get("state", "")) != "tilled":
		return false
	var crop := FarmData.crop(crop_id)
	if crop.is_empty() or not GameState.spend(int(crop.get("seed_cost", 0))):
		return false
	plot.state = "growing"
	plot.crop = crop_id
	plot.planted_at = GameState.game_time()
	GameState.data.selected_crop = crop_id
	GameState.data.tutorial_step = maxi(int(GameState.data.tutorial_step), 2)
	GameState.message_requested.emit("%s planted" % str(crop.get("name", "Seed")))
	GameState.changed.emit()
	plot_changed.emit()
	refresh()
	return true

func set_selected(value: bool) -> void:
	if soil_mesh == null:
		return
	if value:
		var selected_material := _material(Color("#9A6A3F"))
		selected_material.emission_enabled = true
		selected_material.emission = Color("#E8B947")
		selected_material.emission_energy_multiplier = 0.24
		soil_mesh.material_override = selected_material
	else:
		refresh()

func _growth_ratio(plot: Dictionary) -> float:
	if str(plot.get("state", "")) != "growing":
		return 0.0
	var crop := FarmData.crop(str(plot.get("crop", "rice")))
	var duration := maxf(float(crop.get("grow_seconds", 30.0)), 1.0)
	return clampf((GameState.game_time() - float(plot.get("planted_at", 0.0))) / duration, 0.0, 1.0)

func _is_ready(plot: Dictionary) -> bool:
	return str(plot.get("state", "")) == "growing" and _growth_ratio(plot) >= 1.0

func refresh() -> void:
	if soil_mesh == null:
		return
	_clear_children(crop_root)
	_clear_children(lock_root)
	if not is_unlocked():
		soil_mesh.material_override = _material(Color("#66795b"))
		_build_locked_visual()
		return
	var plot: Dictionary = GameState.data.plots[plot_index]
	var state := str(plot.get("state", "empty"))
	soil_mesh.material_override = _material(Color("#75543b") if state != "empty" else Color("#a36b45"))
	if state == "tilled":
		for row in [-0.75, -0.25, 0.25, 0.75]:
			_add_box(crop_root, Vector3(2.9, 0.07, 0.12), Vector3(0, 0.33, row), Color("#513728"))
	elif state == "growing":
		_build_crops(str(plot.crop), _growth_ratio(plot))

func _build_crops(crop_id: String, ratio: float) -> void:
	var imported := AssetSlots.instantiate_model("crops", "%s_stage_%d" % [crop_id, mini(4, 1 + int(ratio * 4.0))], 0.45 + ratio * 0.9)
	if imported != null:
		for x in [-1.05, -0.35, 0.35, 1.05]:
			for z in [-0.72, 0.0, 0.72]:
				var plant := imported.duplicate()
				plant.position = Vector3(x, 0.3, z)
				crop_root.add_child(plant)
		return
	var color := Color(str(FarmData.crop(crop_id).get("color", "#73ad4b")))
	var height := 0.22 + ratio * 0.95
	for x in [-1.05, -0.35, 0.35, 1.05]:
		for z in [-0.72, 0.0, 0.72]:
			_add_cylinder(crop_root, Vector3(x, 0.3 + height * 0.5, z), 0.055 + ratio * 0.035, height, Color("#5f963d"))
			if ratio > 0.55:
				_add_sphere(crop_root, Vector3(x, 0.32 + height, z), 0.11 + ratio * 0.08, color)

func _build_locked_visual() -> void:
	for p in [Vector3(-1.0, 0.45, -0.65), Vector3(0.8, 0.38, 0.55), Vector3(0.15, 0.32, -0.15)]:
		_add_sphere(lock_root, p, 0.25, Color("#47714c"))
	_add_box(lock_root, Vector3(2.8, 0.1, 0.1), Vector3(0, 0.45, -1.05), Color("#7a5434"))

func _clear_children(node: Node) -> void:
	for child in node.get_children():
		child.queue_free()

func _material(color: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.95
	return material

func _add_box(parent: Node3D, size: Vector3, position: Vector3, color: Color) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	var mesh := BoxMesh.new()
	mesh.size = size
	node.mesh = mesh
	node.position = position
	node.material_override = _material(color)
	parent.add_child(node)
	return node

func _add_cylinder(parent: Node3D, position: Vector3, radius: float, height: float, color: Color) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	var mesh := CylinderMesh.new()
	mesh.top_radius = radius
	mesh.bottom_radius = radius
	mesh.height = height
	node.mesh = mesh
	node.position = position
	node.material_override = _material(color)
	parent.add_child(node)
	return node

func _add_sphere(parent: Node3D, position: Vector3, radius: float, color: Color) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	var mesh := SphereMesh.new()
	mesh.radius = radius
	mesh.height = radius * 2.0
	node.mesh = mesh
	node.position = position
	node.material_override = _material(color)
	parent.add_child(node)
	return node
