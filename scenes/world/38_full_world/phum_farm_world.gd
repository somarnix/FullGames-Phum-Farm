extends Node3D

signal prompt_changed(text: String)
signal world_message(text: String)
signal crop_tray_requested(plot_index: int)
signal action_sheet_requested(type: String, id: String, station: Node)

const PlayerScript = preload("res://scenes/characters/player/player.gd")
const PlotScript = preload("res://scenes/gameplay/crops/farm_plot.gd")
const StationScript = preload("res://scenes/gameplay/buildings/farm_station.gd")

var player: CharacterBody3D
var camera_pivot: Node3D
var camera: Camera3D
var sun: DirectionalLight3D
var environment: WorldEnvironment
var rain: GPUParticles3D
var pending_interaction: Node3D
var plots: Array[Node] = []
var expansion_roots: Array[Node3D] = []
var camera_yaw := 0.0
var camera_size := 30.0
var material_cache: Dictionary = {}

func _ready() -> void:
	_build_environment()
	_build_landscape()
	_build_starter_farm()
	_build_crop_district()
	_build_animals()
	_build_production_village()
	_build_market_and_village()
	_build_river_orchard()
	_build_expansion_gates()
	_build_player_and_camera()
	_connect_interaction_interfaces()
	GameState.changed.connect(_refresh_unlocks)
	_refresh_unlocks()

func _process(delta: float) -> void:
	GameState.advance_time(delta)
	_update_lighting()
	if camera_pivot != null and player != null:
		camera_pivot.global_position = camera_pivot.global_position.lerp(player.global_position, clampf(delta * 5.0, 0.0, 1.0))
	if is_instance_valid(pending_interaction) and player.global_position.distance_to(pending_interaction.global_position) < 3.4:
		if pending_interaction.has_method("interact"):
			pending_interaction.interact(player)
		pending_interaction = null
	if Input.is_action_just_pressed("camera_left"):
		rotate_camera(-45.0)
	if Input.is_action_just_pressed("camera_right"):
		rotate_camera(45.0)
	if Input.is_action_just_pressed("zoom_in"):
		zoom_camera(-2.0)
	if Input.is_action_just_pressed("zoom_out"):
		zoom_camera(2.0)

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed:
		if event.button_index == MOUSE_BUTTON_WHEEL_UP:
			zoom_camera(-2.0)
			return
		if event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			zoom_camera(2.0)
			return
		if event.button_index == MOUSE_BUTTON_LEFT:
			_handle_world_click(event.position)

func _handle_world_click(screen_position: Vector2) -> void:
	if camera == null:
		return
	var origin := camera.project_ray_origin(screen_position)
	var end := origin + camera.project_ray_normal(screen_position) * 200.0
	var query := PhysicsRayQueryParameters3D.create(origin, end)
	query.collide_with_areas = true
	var hit := get_world_3d().direct_space_state.intersect_ray(query)
	if hit.is_empty():
		return
	var collider = hit.get("collider")
	if collider is Node and (collider as Node).is_in_group("interactable"):
		var target := collider as Node3D
		if player.global_position.distance_to(target.global_position) < 3.5:
			target.interact(player)
		else:
			pending_interaction = target
			player.walk_to(target.global_position)
	else:
		pending_interaction = null
		player.walk_to(hit.get("position", player.global_position))

func rotate_camera(degrees: float) -> void:
	camera_yaw += degrees
	camera_pivot.rotation_degrees.y = camera_yaw

func zoom_camera(amount: float) -> void:
	camera_size = clampf(camera_size + amount, 18.0, 48.0)
	if camera != null:
		camera.size = camera_size

func set_mobile_input(value: Vector2) -> void:
	if player != null:
		player.mobile_input = value

func interact() -> void:
	if player != null:
		player.interact_nearest()

func focus_home() -> void:
	if player != null:
		player.global_position = Vector3(-20, 0.2, -13)

func _build_environment() -> void:
	environment = WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	var sky := Sky.new()
	var sky_material := ProceduralSkyMaterial.new()
	sky_material.sky_top_color = Color("#5aa8d2")
	sky_material.sky_horizon_color = Color("#d8eef0")
	sky_material.ground_bottom_color = Color("#6a7352")
	sky_material.ground_horizon_color = Color("#d8d2a1")
	sky_material.sun_angle_max = 12.0
	sky.sky_material = sky_material
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("#d8e0c2")
	env.ambient_light_energy = 0.65
	env.tonemap_mode = Environment.TONE_MAPPER_ACES
	env.adjustment_enabled = true
	env.adjustment_saturation = 1.12
	env.fog_enabled = true
	env.fog_light_color = Color("#d6e4cb")
	env.fog_density = 0.002
	environment.environment = env
	add_child(environment)
	sun = DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-52, -35, 0)
	sun.light_color = Color("#fff1cc")
	sun.light_energy = 1.35
	sun.shadow_enabled = true
	sun.directional_shadow_max_distance = 90.0
	add_child(sun)
	rain = GPUParticles3D.new()
	rain.name = "MonsoonRain"
	rain.amount = 900
	rain.lifetime = 1.5
	rain.visibility_aabb = AABB(Vector3(-45, -5, -40), Vector3(90, 45, 80))
	var rain_process := ParticleProcessMaterial.new()
	rain_process.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	rain_process.emission_box_extents = Vector3(38, 1, 32)
	rain_process.direction = Vector3(0.15, -1, 0.08)
	rain_process.initial_velocity_min = 17.0
	rain_process.initial_velocity_max = 22.0
	rain.process_material = rain_process
	var drop := QuadMesh.new()
	drop.size = Vector2(0.025, 0.6)
	var rain_material := StandardMaterial3D.new()
	rain_material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	rain_material.albedo_color = Color(0.75, 0.9, 1.0, 0.65)
	rain_material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	drop.material = rain_material
	rain.draw_pass_1 = drop
	rain.position.y = 22
	rain.emitting = false
	add_child(rain)

func _update_lighting() -> void:
	var hour := float(GameState.data.minutes) / 60.0
	var daylight := clampf(sin((hour - 5.0) / 14.0 * PI), 0.08, 1.0)
	sun.light_energy = 0.18 + daylight * 1.18
	sun.rotation_degrees.x = -12.0 - daylight * 58.0
	if hour < 6.0 or hour > 18.0:
		sun.light_color = Color("#9db8d8")
		environment.environment.ambient_light_color = Color("#52657f")
	else:
		sun.light_color = Color("#fff0c3")
		environment.environment.ambient_light_color = Color("#d8e0c2")
	rain.emitting = str(GameState.data.weather) == "rain"
	environment.environment.fog_density = 0.008 if rain.emitting else 0.002

func _build_landscape() -> void:
	_add_static_box("WorldGround", Vector3(92, 0.3, 72), Vector3(0, -0.15, 0), Color("#79a958"), self)
	# Main roads deliberately connect every gameplay district.
	_add_box("MainDirtRoad", Vector3(78, 0.05, 4.8), Vector3(-4, 0.04, -1), Color("#ba8857"), self)
	_add_box("FarmRoad", Vector3(4.8, 0.06, 57), Vector3(-7, 0.05, 1), Color("#ba8857"), self)
	_add_box("VillageRoad", Vector3(4.4, 0.06, 30), Vector3(22, 0.05, 9), Color("#b68051"), self)
	# Rice irrigation canal and river.
	_add_box("IrrigationCanal", Vector3(2.1, 0.12, 31), Vector3(6, -0.02, -13), Color("#4799a7"), self, _water_material())
	_add_box("River", Vector3(13, 0.16, 72), Vector3(39.5, -0.03, 0), Color("#378fa3"), self, _water_material())
	_build_bridge(Vector3(39.5, 0.2, -1), Vector3(15, 0.4, 3.4))
	# Outer vegetation gives the playable map a lush edge.
	for z in range(-31, 32, 7):
		_add_palm(Vector3(-43, 0, z), 0.8 + float(posmod(z, 3)) * 0.08)
		_add_tree(Vector3(45, 0, z + 2), 0.9)
	for x in range(-37, 38, 7):
		_add_tree(Vector3(x, 0, -34), 0.75)
		_add_palm(Vector3(x + 2, 0, 34), 0.72)

func _build_starter_farm() -> void:
	_build_khmer_building("farmhouse", "Farmhouse", Vector3(-25, 0, -16), Vector3(7.5, 5.5, 6), Color("#70432f"), true)
	var home := StationScript.new()
	home.setup("home", "farmhouse", "Farmhouse")
	home.position = Vector3(-25, 0, -16)
	add_child(home)
	_build_khmer_building("barn", "Barn", Vector3(-24, 0, -5), Vector3(6, 4.5, 5), Color("#80523b"))
	_build_lotus_pond(Vector3(-34, 0, -14), Vector2(7, 5))
	_build_well(Vector3(-15, 0, -14))
	for x in range(-32, -17, 3):
		_add_fence_post(Vector3(x, 0, -23))
	for z in range(-22, -8, 3):
		_add_fence_post(Vector3(-34, 0, z))
	_add_cart(Vector3(-20, 0, -8))
	_add_hay(Vector3(-28, 0, -7))

func _build_crop_district() -> void:
	var start := Vector3(-1.5, 0, -22)
	for row in range(4):
		for column in range(4):
			var plot := PlotScript.new()
			plot.position = start + Vector3(column * 4.1, 0, row * 3.5)
			add_child(plot)
			plot.setup(row * 4 + column)
			plot.crop_selection_requested.connect(func(index: int): crop_tray_requested.emit(index))
			plots.append(plot)
	_build_bridge(Vector3(6, 0.18, -14.5), Vector3(3.0, 0.28, 2.7))
	for z in range(-26, -8, 3):
		_add_fence_post(Vector3(-4, 0, z))
	_add_scarecrow(Vector3(14, 0, -9.5))

func _build_animals() -> void:
	var chicken_root := Node3D.new()
	chicken_root.name = "ChickenDistrict"
	chicken_root.position = Vector3(-25, 0, 11)
	add_child(chicken_root)
	_build_khmer_building("chicken_coop", "ChickenCoop", Vector3(-29, 0, 12), Vector3(4.5, 3.2, 3.5), Color("#855137"))
	_build_fence_rect(Vector3(-24, 0, 11), Vector2(14, 10))
	for p in [Vector3(-23, 0, 9), Vector3(-20, 0, 12), Vector3(-25, 0, 14)]:
		_add_animal("chicken", p, Color("#f5eee0"), 0.65)
	var chickens := StationScript.new()
	chickens.setup("chickens", "chickens", "Chicken Enclosure", 2)
	chickens.position = Vector3(-23, 0, 11)
	add_child(chickens)
	var cow_root := Node3D.new()
	cow_root.name = "CowBuffaloDistrict"
	add_child(cow_root)
	_build_khmer_building("cow_shelter", "CowShelter", Vector3(-25, 0, 26), Vector3(6, 4, 4), Color("#654536"))
	_build_fence_rect(Vector3(-23, 0, 25), Vector2(18, 10))
	_add_animal("cow", Vector3(-20, 0, 23), Color("#ead8b8"), 1.35)
	_add_animal("water_buffalo", Vector3(-25, 0, 27), Color("#4f5551"), 1.45)
	var cows := StationScript.new()
	cows.setup("cows", "cows", "Cow & Buffalo Pasture", 4)
	cows.position = Vector3(-22, 0, 25)
	add_child(cows)

func _build_production_village() -> void:
	var specs := [
		["feed_mill", "animal_feed", "Feed Mill", Vector3(5, 0, 7), 1, Color("#77513b")],
		["bakery", "bread", "Rice Bakery", Vector3(14, 0, 7), 3, Color("#a55d3d")],
		["dairy", "butter", "Dairy Workshop", Vector3(5, 0, 17), 5, Color("#6e604e")],
		["sugar_mill", "palm_sugar", "Sugar Mill", Vector3(14, 0, 17), 7, Color("#87613b")]
	]
	for spec in specs:
		_build_khmer_building(str(spec[0]), str(spec[2]), spec[3], Vector3(5.8, 4.4, 4.8), spec[5])
		var station := StationScript.new()
		station.setup("machine", str(spec[1]), str(spec[2]), int(spec[4]))
		station.position = spec[3]
		add_child(station)
	for p in [Vector3(1, 0, 2), Vector3(10, 0, 2), Vector3(18, 0, 2), Vector3(1, 0, 12), Vector3(18, 0, 12), Vector3(1, 0, 22), Vector3(18, 0, 22)]:
		_add_lantern(p)
	_add_crates(Vector3(9, 0, 13))

func _build_market_and_village() -> void:
	var market_root := Node3D.new()
	market_root.name = "VillageMarket"
	add_child(market_root)
	for p in [Vector3(26, 0, -17), Vector3(32, 0, -17), Vector3(26, 0, -10), Vector3(32, 0, -10)]:
		_build_market_stall(p)
	var market := StationScript.new()
	market.setup("market", "market", "Village Market")
	market.position = Vector3(29, 0, -13)
	add_child(market)
	for p in [Vector3(26, 0, 11), Vector3(31, 0, 18), Vector3(27, 0, 26)]:
		_build_khmer_building("village_house", "VillageHouse", p, Vector3(5.5, 4.4, 4.5), Color("#6f4935"), true)
	_build_pavilion(Vector3(33, 0, 6))
	_add_bicycle(Vector3(25, 0, 15))

func _build_river_orchard() -> void:
	for z in range(-24, 29, 7):
		_add_palm(Vector3(34, 0, z), 0.78)
	for z in range(-25, 30, 8):
		_add_banana(Vector3(27, 0, z + 2))
	_build_dock(Vector3(35.5, 0, 24))
	_add_boat(Vector3(40, 0.12, 25))
	_build_lotus_pond(Vector3(26, 0, 29), Vector2(7, 5))

func _build_expansion_gates() -> void:
	var positions := [Vector3(-16, 0, 5), Vector3(1, 0, 27), Vector3(28, 0, -2)]
	for i in range(3):
		var root := Node3D.new()
		root.name = "ExpansionGate_%d" % i
		root.position = positions[i]
		add_child(root)
		expansion_roots.append(root)
		for x in [-1.5, 0.0, 1.5]:
			_add_rock(root, Vector3(x, 0, 0), 0.55)
		var station := StationScript.new()
		station.setup("expansion", str(i), str(FarmData.expansions[i].name), int(FarmData.expansions[i].level))
		station.position = positions[i]
		add_child(station)

func _refresh_unlocks() -> void:
	for plot in plots:
		plot.refresh()
	for i in range(expansion_roots.size()):
		expansion_roots[i].visible = not bool(GameState.data.expansions[i])

func _connect_interaction_interfaces() -> void:
	for node in get_tree().get_nodes_in_group("interactable"):
		if node.has_signal("interface_requested") and not node.interface_requested.is_connected(_forward_action_sheet):
			node.interface_requested.connect(_forward_action_sheet)

func _forward_action_sheet(type: String, id: String, station: Node) -> void:
	action_sheet_requested.emit(type, id, station)

func plant_selected_plot(plot_index: int, crop_id: String) -> bool:
	if plot_index < 0 or plot_index >= plots.size():
		return false
	return plots[plot_index].plant_crop(crop_id)

func highlight_plot(plot_index: int) -> void:
	for i in range(plots.size()):
		plots[i].set_selected(i == plot_index)

func _build_player_and_camera() -> void:
	player = PlayerScript.new()
	player.name = "Farmer"
	player.position = Vector3(-18, 0.25, -13)
	add_child(player)
	player.prompt_changed.connect(func(text: String): prompt_changed.emit(text))
	camera_pivot = Node3D.new()
	camera_pivot.name = "IsometricCameraRig"
	camera_pivot.position = player.position
	add_child(camera_pivot)
	camera = Camera3D.new()
	camera.name = "IsometricCamera"
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.size = camera_size
	camera.position = Vector3(21, 25, 21)
	camera_pivot.add_child(camera)
	camera.look_at(camera_pivot.global_position, Vector3.UP)
	camera.current = true

func _build_khmer_building(model_id: String, title: String, position: Vector3, size: Vector3, wall_color: Color, elevated: bool = false) -> Node3D:
	var root := Node3D.new()
	root.name = title.validate_node_name()
	root.position = position
	add_child(root)
	var imported := AssetSlots.instantiate_model("buildings", model_id, size.y)
	if imported != null:
		root.add_child(imported)
		_add_static_box("ImportedCollision", Vector3(size.x, size.y, size.z), position + Vector3(0, size.y * 0.5, 0), Color.TRANSPARENT, self, null, false)
		return root
	var floor_y := 1.4 if elevated else 0.15
	if elevated:
		for x in [-size.x * 0.35, size.x * 0.35]:
			for z in [-size.z * 0.35, size.z * 0.35]:
				_add_box("Stilt", Vector3(0.32, 1.5, 0.32), Vector3(x, 0.75, z), Color("#493127"), root)
	_add_box("BuildingBody", Vector3(size.x, size.y * 0.55, size.z), Vector3(0, floor_y + size.y * 0.275, 0), wall_color, root)
	_add_box("Veranda", Vector3(size.x * 0.9, 0.18, 1.2), Vector3(0, floor_y, size.z * 0.62), Color("#b87a49"), root)
	# Chunky two-slope clay roof silhouette.
	for angle in [-25.0, 25.0]:
		var roof := _add_box("ClayRoof", Vector3(size.x * 0.65, 0.35, size.z * 1.25), Vector3(0, floor_y + size.y * 0.64, 0), Color("#b64d32"), root)
		roof.rotation_degrees.z = angle
	_add_box("Door", Vector3(1.0, 1.8, 0.12), Vector3(0, floor_y + 0.9, size.z * 0.51), Color("#3f2b25"), root)
	for x in [-size.x * 0.28, size.x * 0.28]:
		_add_box("Window", Vector3(0.85, 0.85, 0.12), Vector3(x, floor_y + 1.5, size.z * 0.515), Color("#8fc5c0"), root, _glass_material())
	return root

func _build_well(position: Vector3) -> void:
	var root := Node3D.new()
	root.name = "IrrigationWell"
	root.position = position
	add_child(root)
	_add_cylinder("Well", Vector3(0, 0.45, 0), 0.9, 0.8, Color("#9a8264"), root)
	_add_cylinder("Water", Vector3(0, 0.86, 0), 0.68, 0.04, Color("#4f9ca9"), root)
	for x in [-0.75, 0.75]:
		_add_box("Post", Vector3(0.16, 2.2, 0.16), Vector3(x, 1.2, 0), Color("#5d402c"), root)
	var station := StationScript.new()
	station.setup("well", "well", "Irrigation Well")
	station.position = position
	add_child(station)

func _build_lotus_pond(position: Vector3, size: Vector2) -> void:
	_add_box("LotusPond", Vector3(size.x, 0.13, size.y), position + Vector3(0, -0.02, 0), Color("#4a9ca6"), self, _water_material())
	for offset in [Vector3(-2, 0.08, -1), Vector3(0, 0.08, 1), Vector3(2, 0.08, -0.5), Vector3(1, 0.08, 1.4)]:
		_add_cylinder("LilyPad", position + offset, 0.34, 0.035, Color("#4f8b4d"), self)
		if int(offset.x) % 2 == 0:
			_add_sphere("Lotus", position + offset + Vector3(0, 0.18, 0), 0.13, Color("#f19ab4"), self)

func _build_bridge(position: Vector3, size: Vector3) -> void:
	var root := Node3D.new()
	root.name = "WoodenBridge"
	root.position = position
	add_child(root)
	for x in range(-int(size.x * 0.5), int(size.x * 0.5) + 1):
		_add_static_box("BridgePlank", Vector3(0.82, size.y, size.z), Vector3(float(x), size.y * 0.5, 0), Color("#8a5b38"), root)
	for x in [-size.x * 0.48, size.x * 0.48]:
		for z in [-size.z * 0.55, size.z * 0.55]:
			_add_box("RailPost", Vector3(0.14, 1.2, 0.14), Vector3(x, 0.7, z), Color("#513a2c"), root)

func _build_fence_rect(center: Vector3, size: Vector2) -> void:
	for x in range(int(center.x - size.x * 0.5), int(center.x + size.x * 0.5) + 1, 2):
		_add_fence_post(Vector3(x, 0, center.z - size.y * 0.5))
		_add_fence_post(Vector3(x, 0, center.z + size.y * 0.5))
	for z in range(int(center.z - size.y * 0.5), int(center.z + size.y * 0.5) + 1, 2):
		_add_fence_post(Vector3(center.x - size.x * 0.5, 0, z))
		_add_fence_post(Vector3(center.x + size.x * 0.5, 0, z))

func _add_fence_post(position: Vector3) -> void:
	_add_box("BambooFence", Vector3(0.12, 1.05, 0.12), position + Vector3(0, 0.52, 0), Color("#a47c42"), self)

func _add_animal(model_id: String, position: Vector3, color: Color, scale_value: float) -> void:
	var imported := AssetSlots.instantiate_model("animals", model_id, scale_value)
	if imported != null:
		imported.position = position
		add_child(imported)
		return
	var root := Node3D.new()
	root.name = model_id.to_pascal_case()
	root.position = position
	add_child(root)
	_add_sphere("Body", Vector3(0, scale_value * 0.55, 0), scale_value * 0.43, color, root)
	_add_sphere("Head", Vector3(0, scale_value * 0.72, scale_value * 0.42), scale_value * 0.27, color.lightened(0.08), root)
	for x in [-0.24, 0.24]:
		for z in [-0.22, 0.22]:
			_add_cylinder("Leg", Vector3(x * scale_value, scale_value * 0.22, z * scale_value), scale_value * 0.06, scale_value * 0.42, Color("#6b4936"), root)

func _build_market_stall(position: Vector3) -> void:
	var root := Node3D.new()
	root.name = "MarketStall"
	root.position = position
	add_child(root)
	for x in [-1.4, 1.4]:
		for z in [-1.0, 1.0]:
			_add_box("Post", Vector3(0.14, 2.7, 0.14), Vector3(x, 1.35, z), Color("#60402c"), root)
	_add_box("Counter", Vector3(3.2, 0.7, 1.4), Vector3(0, 0.65, 0.4), Color("#885639"), root)
	var roof := _add_box("Roof", Vector3(3.8, 0.28, 2.7), Vector3(0, 2.75, 0), Color("#c85c3d"), root)
	roof.rotation_degrees.z = 4.0
	for x in [-0.8, 0.0, 0.8]:
		_add_sphere("Produce", Vector3(x, 1.12, 0.45), 0.22, Color("#e0a43c") if x < 0.0 else Color("#67a84b"), root)

func _build_pavilion(position: Vector3) -> void:
	var root := Node3D.new()
	root.name = "FestivalPavilion"
	root.position = position
	add_child(root)
	_add_cylinder("Platform", Vector3(0, 0.25, 0), 3.4, 0.4, Color("#a6784c"), root)
	for x in [-2.2, 2.2]:
		for z in [-1.5, 1.5]:
			_add_box("Post", Vector3(0.2, 3.2, 0.2), Vector3(x, 1.8, z), Color("#5b3b2b"), root)
	var roof := _add_cylinder("Roof", Vector3(0, 3.5, 0), 4.1, 1.0, Color("#b84e35"), root)
	(roof.mesh as CylinderMesh).top_radius = 0.7

func _add_palm(position: Vector3, size: float) -> void:
	var imported := AssetSlots.instantiate_model("props", "sugar_palm", 5.5 * size)
	if imported != null:
		imported.position = position
		add_child(imported)
		return
	var root := Node3D.new()
	root.name = "SugarPalm"
	root.position = position
	add_child(root)
	_add_cylinder("Trunk", Vector3(0, 2.4 * size, 0), 0.2 * size, 4.8 * size, Color("#6d5135"), root)
	for angle in range(0, 360, 45):
		var leaf := _add_sphere("PalmLeaf", Vector3(cos(deg_to_rad(angle)) * 1.2 * size, 5.0 * size, sin(deg_to_rad(angle)) * 1.2 * size), 0.62 * size, Color("#397447"), root)
		leaf.scale = Vector3(1.7, 0.28, 0.65)

func _add_tree(position: Vector3, size: float) -> void:
	var root := Node3D.new()
	root.name = "MangoTree"
	root.position = position
	add_child(root)
	_add_cylinder("Trunk", Vector3(0, 1.5 * size, 0), 0.28 * size, 3.0 * size, Color("#654833"), root)
	for p in [Vector3(0, 3.5, 0), Vector3(-0.8, 3.2, 0.2), Vector3(0.7, 3.25, 0.3), Vector3(0, 3.2, -0.8)]:
		_add_sphere("Canopy", p * size, 1.15 * size, Color("#4f8645"), root)

func _add_banana(position: Vector3) -> void:
	var root := Node3D.new()
	root.name = "BananaPlant"
	root.position = position
	add_child(root)
	_add_cylinder("Stem", Vector3(0, 1.2, 0), 0.16, 2.4, Color("#72944a"), root)
	for angle in range(0, 360, 60):
		var leaf := _add_sphere("Leaf", Vector3(cos(deg_to_rad(angle)) * 0.8, 2.5, sin(deg_to_rad(angle)) * 0.8), 0.58, Color("#579447"), root)
		leaf.scale = Vector3(1.5, 0.22, 0.65)

func _add_scarecrow(position: Vector3) -> void:
	_add_box("Scarecrow", Vector3(0.18, 2.6, 0.18), position + Vector3(0, 1.3, 0), Color("#6e4b31"), self)
	_add_box("ScarecrowArms", Vector3(2.0, 0.16, 0.16), position + Vector3(0, 1.9, 0), Color("#6e4b31"), self)
	_add_sphere("ScarecrowHead", position + Vector3(0, 2.45, 0), 0.35, Color("#c69a5d"), self)

func _add_hay(position: Vector3) -> void:
	_add_cylinder("HayBale", position + Vector3(0, 0.55, 0), 0.72, 1.1, Color("#d4a33e"), self)

func _add_crates(position: Vector3) -> void:
	for offset in [Vector3.ZERO, Vector3(1.0, 0, 0.2), Vector3(0.5, 0.8, 0.1)]:
		_add_box("Crate", Vector3(0.85, 0.7, 0.85), position + offset + Vector3(0, 0.35, 0), Color("#94623c"), self)

func _add_cart(position: Vector3) -> void:
	_add_box("WoodCart", Vector3(2.2, 0.7, 1.2), position + Vector3(0, 0.8, 0), Color("#815435"), self)
	for x in [-0.8, 0.8]:
		_add_cylinder("CartWheel", position + Vector3(x, 0.45, 0.68), 0.45, 0.14, Color("#493329"), self, Vector3(90, 0, 0))

func _add_bicycle(position: Vector3) -> void:
	for x in [-0.7, 0.7]:
		var wheel := _add_torus("BicycleWheel", position + Vector3(x, 0.55, 0), 0.45, Color("#2e3432"), self)
		wheel.rotation_degrees.x = 90
	_add_box("BicycleFrame", Vector3(1.3, 0.12, 0.12), position + Vector3(0, 0.8, 0), Color("#bd4f3f"), self)

func _add_lantern(position: Vector3) -> void:
	_add_box("LanternPost", Vector3(0.13, 2.6, 0.13), position + Vector3(0, 1.3, 0), Color("#41342b"), self)
	var light_mesh := _add_sphere("Lantern", position + Vector3(0, 2.4, 0), 0.25, Color("#ffc45b"), self)
	light_mesh.material_override.emission_enabled = true
	light_mesh.material_override.emission = Color("#ffb43b")
	light_mesh.material_override.emission_energy_multiplier = 1.2

func _build_dock(position: Vector3) -> void:
	for z in range(0, 7):
		_add_static_box("DockPlank", Vector3(3, 0.22, 0.78), position + Vector3(0, 0.2, float(z)), Color("#80583b"), self)

func _add_boat(position: Vector3) -> void:
	var hull := _add_cylinder("Boat", position + Vector3(0, 0.15, 0), 1.2, 0.35, Color("#70422f"), self, Vector3(0, 0, 90))
	hull.scale.z = 0.45

func _add_rock(parent: Node3D, position: Vector3, size: float) -> void:
	var rock := _add_sphere("RemovableRock", position + Vector3(0, size * 0.45, 0), size, Color("#7d8178"), parent)
	rock.scale = Vector3(1.0, 0.72, 0.85)

func _material(color: Color) -> StandardMaterial3D:
	var key := color.to_html()
	if material_cache.has(key):
		return material_cache[key]
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.9
	material_cache[key] = material
	return material

func _glass_material() -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = Color(0.45, 0.78, 0.8, 0.72)
	material.metallic = 0.12
	material.roughness = 0.2
	material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	return material

func _water_material() -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = Color(0.20, 0.56, 0.64, 0.88)
	material.metallic = 0.18
	material.roughness = 0.16
	material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	return material

func _add_box(node_name: String, size: Vector3, position: Vector3, color: Color, parent: Node3D, override_material: Material = null) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = node_name
	var mesh := BoxMesh.new()
	mesh.size = size
	node.mesh = mesh
	node.position = position
	node.material_override = override_material if override_material != null else _material(color)
	parent.add_child(node)
	return node

func _add_static_box(node_name: String, size: Vector3, position: Vector3, color: Color, parent: Node3D, override_material: Material = null, visible: bool = true) -> StaticBody3D:
	var body := StaticBody3D.new()
	body.name = node_name
	body.position = position
	parent.add_child(body)
	if visible:
		_add_box("Mesh", size, Vector3.ZERO, color, body, override_material)
	var collider := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = size
	collider.shape = shape
	body.add_child(collider)
	return body

func _add_cylinder(node_name: String, position: Vector3, radius: float, height: float, color: Color, parent: Node3D, rotation_degrees_value: Vector3 = Vector3.ZERO) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = node_name
	var mesh := CylinderMesh.new()
	mesh.top_radius = radius
	mesh.bottom_radius = radius
	mesh.height = height
	node.mesh = mesh
	node.position = position
	node.rotation_degrees = rotation_degrees_value
	node.material_override = _material(color)
	parent.add_child(node)
	return node

func _add_sphere(node_name: String, position: Vector3, radius: float, color: Color, parent: Node3D) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = node_name
	var mesh := SphereMesh.new()
	mesh.radius = radius
	mesh.height = radius * 2.0
	node.mesh = mesh
	node.position = position
	node.material_override = _material(color)
	parent.add_child(node)
	return node

func _add_torus(node_name: String, position: Vector3, radius: float, color: Color, parent: Node3D) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = node_name
	var mesh := TorusMesh.new()
	mesh.inner_radius = radius * 0.72
	mesh.outer_radius = radius
	node.mesh = mesh
	node.position = position
	node.material_override = _material(color)
	parent.add_child(node)
	return node
