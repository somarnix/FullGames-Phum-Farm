extends Node3D

signal prompt_changed(text: String)
signal world_message(text: String)
signal crop_tray_requested(plot_index: int)
signal action_sheet_requested(type: String, id: String, station: Node)

@onready var environment_controller: Node3D = $Environment
@onready var districts: Node3D = $Districts
@onready var crop_district: Node3D = $Districts/CropDistrict
@onready var expansion_district: Node3D = $Districts/ExpansionDistricts
@onready var player: CharacterBody3D = $Characters/Player
@onready var camera_pivot: Node3D = $CameraRig
@onready var camera: Camera3D = $CameraRig/FarmCamera

var plots: Array[Node] = []
var pending_interaction: Node3D
var camera_yaw := 0.0
var camera_size := 30.0

func _ready() -> void:
	plots = crop_district.plots
	crop_district.crop_selection_requested.connect(_forward_crop_selection)
	player.prompt_changed.connect(_forward_prompt)
	camera.size = camera_size
	camera.look_at(camera_pivot.global_position, Vector3.UP)
	camera.current = true
	_connect_interaction_interfaces()
	GameState.changed.connect(_refresh_unlocks)
	_refresh_unlocks()

func _process(delta: float) -> void:
	GameState.advance_time(delta)
	environment_controller.update_for_state()
	if player != null:
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

func rotate_camera(degrees: float) -> void:
	camera_yaw += degrees
	camera_pivot.rotation_degrees.y = camera_yaw

func zoom_camera(amount: float) -> void:
	camera_size = clampf(camera_size + amount, 18.0, 48.0)
	camera.size = camera_size

func set_mobile_input(value: Vector2) -> void:
	player.mobile_input = value

func interact() -> void:
	player.interact_nearest()

func focus_home() -> void:
	player.global_position = Vector3(-18, 0.25, -13)

func plant_selected_plot(plot_index: int, crop_id: String) -> bool:
	return crop_district.plant_crop(plot_index, crop_id)

func highlight_plot(plot_index: int) -> void:
	crop_district.highlight_plot(plot_index)

func get_station(station_name: String) -> Node:
	for node in get_tree().get_nodes_in_group("interactable"):
		if is_ancestor_of(node) and node.name == station_name:
			return node
	return null

func district(district_name: String) -> Node3D:
	return districts.get_node_or_null(district_name) as Node3D

func _handle_world_click(screen_position: Vector2) -> void:
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

func _connect_interaction_interfaces() -> void:
	for node in get_tree().get_nodes_in_group("interactable"):
		if not is_ancestor_of(node):
			continue
		if node.has_signal("interface_requested") and not node.interface_requested.is_connected(_forward_action_sheet):
			node.interface_requested.connect(_forward_action_sheet)

func _refresh_unlocks() -> void:
	crop_district.refresh_unlocks()
	expansion_district.refresh_unlocks()

func _forward_crop_selection(plot_index: int) -> void:
	crop_tray_requested.emit(plot_index)

func _forward_prompt(text: String) -> void:
	prompt_changed.emit(text)

func _forward_action_sheet(type: String, id: String, station: Node) -> void:
	action_sheet_requested.emit(type, id, station)
