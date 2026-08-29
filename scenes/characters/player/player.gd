extends CharacterBody3D

signal prompt_changed(text: String)

const SPEED := 6.0
const ARRIVE_DISTANCE := 0.25

var move_target := Vector3.ZERO
var has_move_target := false
var mobile_input := Vector2.ZERO
var visual: Node3D
var animation_player: AnimationPlayer
var current_animation := ""

func _ready() -> void:
	_build_collision()
	_build_visual()

func _build_collision() -> void:
	var collider := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.38
	shape.height = 1.75
	collider.shape = shape
	collider.position.y = 0.9
	add_child(collider)

func _build_visual() -> void:
	visual = AssetSlots.instantiate_model("characters", "farmer", 1.8)
	if visual != null:
		add_child(visual)
		animation_player = _find_animation_player(visual)
		return
	visual = Node3D.new()
	visual.name = "PlaceholderFarmer"
	add_child(visual)
	_add_capsule(Vector3(0, 1.05, 0), 0.36, 0.8, Color("#4d8f67"))
	_add_sphere(Vector3(0, 1.75, 0), 0.32, Color("#b97850"))
	_add_cylinder(Vector3(0, 2.02, 0), 0.46, 0.12, Color("#d7a53f"))
	_add_cylinder(Vector3(0, 2.10, 0), 0.22, 0.20, Color("#d7a53f"))
	_add_box(Vector3(0.15, 0.28, 0), Vector3(0.18, 0.55, 0.22), Color("#384f69"))
	_add_box(Vector3(-0.15, 0.28, 0), Vector3(0.18, 0.55, 0.22), Color("#384f69"))

func _physics_process(_delta: float) -> void:
	var input := Input.get_vector("move_left", "move_right", "move_up", "move_down")
	if mobile_input.length() > input.length():
		input = mobile_input
	var direction := Vector3(input.x, 0, input.y)
	if direction.length() > 0.1:
		has_move_target = false
	else:
		direction = _target_direction()
	velocity.x = direction.x * SPEED
	velocity.z = direction.z * SPEED
	if not is_on_floor():
		velocity.y -= 24.0 * get_physics_process_delta_time()
	else:
		velocity.y = -0.2
	move_and_slide()
	if direction.length() > 0.1:
		rotation.y = lerp_angle(rotation.y, atan2(direction.x, direction.z), 0.18)
		_set_animation("walk")
	else:
		_set_animation("idle")
	_update_prompt()
	if Input.is_action_just_pressed("interact"):
		interact_nearest()

func _target_direction() -> Vector3:
	if not has_move_target:
		return Vector3.ZERO
	var delta := move_target - global_position
	delta.y = 0.0
	if delta.length() <= ARRIVE_DISTANCE:
		has_move_target = false
		return Vector3.ZERO
	return delta.normalized()

func walk_to(point: Vector3) -> void:
	move_target = point
	move_target.y = global_position.y
	has_move_target = true

func interact_nearest() -> void:
	var target := nearest_interactable()
	if target != null and target.has_method("interact"):
		target.interact(self)

func nearest_interactable(max_distance: float = 3.5) -> Node3D:
	var best: Node3D
	var best_distance := max_distance
	for candidate in get_tree().get_nodes_in_group("interactable"):
		if candidate is not Node3D or not candidate.is_visible_in_tree():
			continue
		var distance := global_position.distance_to((candidate as Node3D).global_position)
		if distance < best_distance:
			best_distance = distance
			best = candidate
	return best

func _update_prompt() -> void:
	var target := nearest_interactable()
	var text := ""
	if target != null and target.has_method("prompt_text"):
		text = target.prompt_text()
	prompt_changed.emit(text)

func _set_animation(kind: String) -> void:
	if animation_player == null or current_animation == kind:
		return
	var options := ["Walk", "walk", "Walking"] if kind == "walk" else ["Idle", "idle"]
	for clip in options:
		if animation_player.has_animation(clip):
			animation_player.play(clip)
			current_animation = kind
			return

func _find_animation_player(node: Node) -> AnimationPlayer:
	if node is AnimationPlayer:
		return node
	for child in node.get_children():
		var result := _find_animation_player(child)
		if result != null:
			return result
	return null

func _material(color: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.88
	return material

func _add_box(position: Vector3, size: Vector3, color: Color) -> void:
	var mesh := MeshInstance3D.new()
	var shape := BoxMesh.new()
	shape.size = size
	mesh.mesh = shape
	mesh.position = position
	mesh.material_override = _material(color)
	visual.add_child(mesh)

func _add_sphere(position: Vector3, radius: float, color: Color) -> void:
	var mesh := MeshInstance3D.new()
	var shape := SphereMesh.new()
	shape.radius = radius
	shape.height = radius * 2.0
	mesh.mesh = shape
	mesh.position = position
	mesh.material_override = _material(color)
	visual.add_child(mesh)

func _add_capsule(position: Vector3, radius: float, height: float, color: Color) -> void:
	var mesh := MeshInstance3D.new()
	var shape := CapsuleMesh.new()
	shape.radius = radius
	shape.height = height
	mesh.mesh = shape
	mesh.position = position
	mesh.material_override = _material(color)
	visual.add_child(mesh)

func _add_cylinder(position: Vector3, radius: float, height: float, color: Color) -> void:
	var mesh := MeshInstance3D.new()
	var shape := CylinderMesh.new()
	shape.top_radius = radius
	shape.bottom_radius = radius
	shape.height = height
	mesh.mesh = shape
	mesh.position = position
	mesh.material_override = _material(color)
	visual.add_child(mesh)
