extends RefCounted
class_name WorldArtBuilder

var material_cache: Dictionary = {}

func material(color: Color) -> StandardMaterial3D:
	var key := color.to_html()
	if material_cache.has(key):
		return material_cache[key]
	var value := StandardMaterial3D.new()
	value.albedo_color = color
	value.roughness = 0.9
	material_cache[key] = value
	return value

func glass_material() -> StandardMaterial3D:
	var value := StandardMaterial3D.new()
	value.albedo_color = Color(0.45, 0.78, 0.8, 0.72)
	value.metallic = 0.12
	value.roughness = 0.2
	value.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	return value

func water_material() -> StandardMaterial3D:
	var value := StandardMaterial3D.new()
	value.albedo_color = Color(0.20, 0.56, 0.64, 0.88)
	value.metallic = 0.18
	value.roughness = 0.16
	value.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	return value

func add_box(parent: Node3D, node_name: String, size: Vector3, position: Vector3, color: Color, override_material: Material = null) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = node_name
	var mesh := BoxMesh.new()
	mesh.size = size
	node.mesh = mesh
	node.position = position
	node.material_override = override_material if override_material != null else material(color)
	parent.add_child(node)
	return node

func add_static_box(parent: Node3D, node_name: String, size: Vector3, position: Vector3, color: Color, override_material: Material = null, visible := true) -> StaticBody3D:
	var body := StaticBody3D.new()
	body.name = node_name
	body.position = position
	parent.add_child(body)
	if visible:
		add_box(body, "Mesh", size, Vector3.ZERO, color, override_material)
	var collider := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = size
	collider.shape = shape
	body.add_child(collider)
	return body

func add_cylinder(parent: Node3D, node_name: String, position: Vector3, radius: float, height: float, color: Color, rotation := Vector3.ZERO) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = node_name
	var mesh := CylinderMesh.new()
	mesh.top_radius = radius
	mesh.bottom_radius = radius
	mesh.height = height
	node.mesh = mesh
	node.position = position
	node.rotation_degrees = rotation
	node.material_override = material(color)
	parent.add_child(node)
	return node

func add_sphere(parent: Node3D, node_name: String, position: Vector3, radius: float, color: Color) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = node_name
	var mesh := SphereMesh.new()
	mesh.radius = radius
	mesh.height = radius * 2.0
	node.mesh = mesh
	node.position = position
	node.material_override = material(color)
	parent.add_child(node)
	return node

func add_torus(parent: Node3D, node_name: String, position: Vector3, radius: float, color: Color) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = node_name
	var mesh := TorusMesh.new()
	mesh.inner_radius = radius * 0.72
	mesh.outer_radius = radius
	node.mesh = mesh
	node.position = position
	node.material_override = material(color)
	parent.add_child(node)
	return node

func build_khmer_building(parent: Node3D, model_id: String, title: String, position: Vector3, size: Vector3, wall_color: Color, elevated := false) -> Node3D:
	var root := Node3D.new()
	root.name = title.validate_node_name()
	root.position = position
	parent.add_child(root)
	var imported := AssetSlots.instantiate_model("buildings", model_id, size.y)
	if imported != null:
		root.add_child(imported)
		add_static_box(root, "ImportedCollision", size, Vector3(0, size.y * 0.5, 0), Color.TRANSPARENT, null, false)
		return root
	var floor_y := 1.4 if elevated else 0.15
	if elevated:
		for x in [-size.x * 0.35, size.x * 0.35]:
			for z in [-size.z * 0.35, size.z * 0.35]:
				add_box(root, "Stilt", Vector3(0.32, 1.5, 0.32), Vector3(x, 0.75, z), Color("#493127"))
	add_box(root, "BuildingBody", Vector3(size.x, size.y * 0.55, size.z), Vector3(0, floor_y + size.y * 0.275, 0), wall_color)
	add_box(root, "Veranda", Vector3(size.x * 0.9, 0.18, 1.2), Vector3(0, floor_y, size.z * 0.62), Color("#b87a49"))
	for angle in [-25.0, 25.0]:
		var roof := add_box(root, "ClayRoof", Vector3(size.x * 0.65, 0.35, size.z * 1.25), Vector3(0, floor_y + size.y * 0.64, 0), Color("#b64d32"))
		roof.rotation_degrees.z = angle
	add_box(root, "Door", Vector3(1.0, 1.8, 0.12), Vector3(0, floor_y + 0.9, size.z * 0.51), Color("#3f2b25"))
	for x in [-size.x * 0.28, size.x * 0.28]:
		add_box(root, "Window", Vector3(0.85, 0.85, 0.12), Vector3(x, floor_y + 1.5, size.z * 0.515), Color("#8fc5c0"), glass_material())
	return root

func build_lotus_pond(parent: Node3D, position: Vector3, size: Vector2) -> void:
	add_box(parent, "LotusPond", Vector3(size.x, 0.13, size.y), position + Vector3(0, -0.02, 0), Color("#4a9ca6"), water_material())
	for offset in [Vector3(-2, 0.08, -1), Vector3(0, 0.08, 1), Vector3(2, 0.08, -0.5), Vector3(1, 0.08, 1.4)]:
		add_cylinder(parent, "LilyPad", position + offset, 0.34, 0.035, Color("#4f8b4d"))
		if int(offset.x) % 2 == 0:
			add_sphere(parent, "Lotus", position + offset + Vector3(0, 0.18, 0), 0.13, Color("#f19ab4"))

func build_bridge(parent: Node3D, position: Vector3, size: Vector3) -> void:
	var root := Node3D.new()
	root.name = "WoodenBridge"
	root.position = position
	parent.add_child(root)
	for x in range(-int(size.x * 0.5), int(size.x * 0.5) + 1):
		add_static_box(root, "BridgePlank", Vector3(0.82, size.y, size.z), Vector3(float(x), size.y * 0.5, 0), Color("#8a5b38"))
	for x in [-size.x * 0.48, size.x * 0.48]:
		for z in [-size.z * 0.55, size.z * 0.55]:
			add_box(root, "RailPost", Vector3(0.14, 1.2, 0.14), Vector3(x, 0.7, z), Color("#513a2c"))

func add_fence_post(parent: Node3D, position: Vector3) -> void:
	add_box(parent, "BambooFence", Vector3(0.12, 1.05, 0.12), position + Vector3(0, 0.52, 0), Color("#a47c42"))

func build_fence_rect(parent: Node3D, center: Vector3, size: Vector2) -> void:
	for x in range(int(center.x - size.x * 0.5), int(center.x + size.x * 0.5) + 1, 2):
		add_fence_post(parent, Vector3(x, 0, center.z - size.y * 0.5))
		add_fence_post(parent, Vector3(x, 0, center.z + size.y * 0.5))
	for z in range(int(center.z - size.y * 0.5), int(center.z + size.y * 0.5) + 1, 2):
		add_fence_post(parent, Vector3(center.x - size.x * 0.5, 0, z))
		add_fence_post(parent, Vector3(center.x + size.x * 0.5, 0, z))

func add_animal(parent: Node3D, model_id: String, position: Vector3, color: Color, scale_value: float) -> void:
	var imported := AssetSlots.instantiate_model("animals", model_id, scale_value)
	if imported != null:
		imported.position = position
		parent.add_child(imported)
		return
	var root := Node3D.new()
	root.name = model_id.to_pascal_case()
	root.position = position
	parent.add_child(root)
	add_sphere(root, "Body", Vector3(0, scale_value * 0.55, 0), scale_value * 0.43, color)
	add_sphere(root, "Head", Vector3(0, scale_value * 0.72, scale_value * 0.42), scale_value * 0.27, color.lightened(0.08))
	for x in [-0.24, 0.24]:
		for z in [-0.22, 0.22]:
			add_cylinder(root, "Leg", Vector3(x * scale_value, scale_value * 0.22, z * scale_value), scale_value * 0.06, scale_value * 0.42, Color("#6b4936"))

func build_market_stall(parent: Node3D, position: Vector3) -> void:
	var root := Node3D.new()
	root.name = "MarketStall"
	root.position = position
	parent.add_child(root)
	for x in [-1.4, 1.4]:
		for z in [-1.0, 1.0]:
			add_box(root, "Post", Vector3(0.14, 2.7, 0.14), Vector3(x, 1.35, z), Color("#60402c"))
	add_box(root, "Counter", Vector3(3.2, 0.7, 1.4), Vector3(0, 0.65, 0.4), Color("#885639"))
	var roof := add_box(root, "Roof", Vector3(3.8, 0.28, 2.7), Vector3(0, 2.75, 0), Color("#c85c3d"))
	roof.rotation_degrees.z = 4.0
	for x in [-0.8, 0.0, 0.8]:
		add_sphere(root, "Produce", Vector3(x, 1.12, 0.45), 0.22, Color("#e0a43c") if x < 0.0 else Color("#67a84b"))

func build_pavilion(parent: Node3D, position: Vector3) -> void:
	var root := Node3D.new()
	root.name = "FestivalPavilion"
	root.position = position
	parent.add_child(root)
	add_cylinder(root, "Platform", Vector3(0, 0.25, 0), 3.4, 0.4, Color("#a6784c"))
	for x in [-2.2, 2.2]:
		for z in [-1.5, 1.5]:
			add_box(root, "Post", Vector3(0.2, 3.2, 0.2), Vector3(x, 1.8, z), Color("#5b3b2b"))
	var roof := add_cylinder(root, "Roof", Vector3(0, 3.5, 0), 4.1, 1.0, Color("#b84e35"))
	(roof.mesh as CylinderMesh).top_radius = 0.7

func add_palm(parent: Node3D, position: Vector3, size: float) -> void:
	var imported := AssetSlots.instantiate_model("props", "sugar_palm", 5.5 * size)
	if imported != null:
		imported.position = position
		parent.add_child(imported)
		return
	var root := Node3D.new()
	root.name = "SugarPalm"
	root.position = position
	parent.add_child(root)
	add_cylinder(root, "Trunk", Vector3(0, 2.4 * size, 0), 0.2 * size, 4.8 * size, Color("#6d5135"))
	for angle in range(0, 360, 45):
		var leaf := add_sphere(root, "PalmLeaf", Vector3(cos(deg_to_rad(angle)) * 1.2 * size, 5.0 * size, sin(deg_to_rad(angle)) * 1.2 * size), 0.62 * size, Color("#397447"))
		leaf.scale = Vector3(1.7, 0.28, 0.65)

func add_tree(parent: Node3D, position: Vector3, size: float) -> void:
	var root := Node3D.new()
	root.name = "MangoTree"
	root.position = position
	parent.add_child(root)
	add_cylinder(root, "Trunk", Vector3(0, 1.5 * size, 0), 0.28 * size, 3.0 * size, Color("#654833"))
	for point in [Vector3(0, 3.5, 0), Vector3(-0.8, 3.2, 0.2), Vector3(0.7, 3.25, 0.3), Vector3(0, 3.2, -0.8)]:
		add_sphere(root, "Canopy", point * size, 1.15 * size, Color("#4f8645"))

func add_banana(parent: Node3D, position: Vector3) -> void:
	var root := Node3D.new()
	root.name = "BananaPlant"
	root.position = position
	parent.add_child(root)
	add_cylinder(root, "Stem", Vector3(0, 1.2, 0), 0.16, 2.4, Color("#72944a"))
	for angle in range(0, 360, 60):
		var leaf := add_sphere(root, "Leaf", Vector3(cos(deg_to_rad(angle)) * 0.8, 2.5, sin(deg_to_rad(angle)) * 0.8), 0.58, Color("#579447"))
		leaf.scale = Vector3(1.5, 0.22, 0.65)

func add_scarecrow(parent: Node3D, position: Vector3) -> void:
	add_box(parent, "Scarecrow", Vector3(0.18, 2.6, 0.18), position + Vector3(0, 1.3, 0), Color("#6e4b31"))
	add_box(parent, "ScarecrowArms", Vector3(2.0, 0.16, 0.16), position + Vector3(0, 1.9, 0), Color("#6e4b31"))
	add_sphere(parent, "ScarecrowHead", position + Vector3(0, 2.45, 0), 0.35, Color("#c69a5d"))

func add_hay(parent: Node3D, position: Vector3) -> void:
	add_cylinder(parent, "HayBale", position + Vector3(0, 0.55, 0), 0.72, 1.1, Color("#d4a33e"))

func add_crates(parent: Node3D, position: Vector3) -> void:
	for offset in [Vector3.ZERO, Vector3(1.0, 0, 0.2), Vector3(0.5, 0.8, 0.1)]:
		add_box(parent, "Crate", Vector3(0.85, 0.7, 0.85), position + offset + Vector3(0, 0.35, 0), Color("#94623c"))

func add_cart(parent: Node3D, position: Vector3) -> void:
	add_box(parent, "WoodCart", Vector3(2.2, 0.7, 1.2), position + Vector3(0, 0.8, 0), Color("#815435"))
	for x in [-0.8, 0.8]:
		add_cylinder(parent, "CartWheel", position + Vector3(x, 0.45, 0.68), 0.45, 0.14, Color("#493329"), Vector3(90, 0, 0))

func add_bicycle(parent: Node3D, position: Vector3) -> void:
	for x in [-0.7, 0.7]:
		var wheel := add_torus(parent, "BicycleWheel", position + Vector3(x, 0.55, 0), 0.45, Color("#2e3432"))
		wheel.rotation_degrees.x = 90
	add_box(parent, "BicycleFrame", Vector3(1.3, 0.12, 0.12), position + Vector3(0, 0.8, 0), Color("#bd4f3f"))

func add_lantern(parent: Node3D, position: Vector3) -> void:
	add_box(parent, "LanternPost", Vector3(0.13, 2.6, 0.13), position + Vector3(0, 1.3, 0), Color("#41342b"))
	var light_mesh := add_sphere(parent, "Lantern", position + Vector3(0, 2.4, 0), 0.25, Color("#ffc45b"))
	light_mesh.material_override.emission_enabled = true
	light_mesh.material_override.emission = Color("#ffb43b")
	light_mesh.material_override.emission_energy_multiplier = 1.2

func build_dock(parent: Node3D, position: Vector3) -> void:
	for z in range(0, 7):
		add_static_box(parent, "DockPlank", Vector3(3, 0.22, 0.78), position + Vector3(0, 0.2, float(z)), Color("#80583b"))

func add_boat(parent: Node3D, position: Vector3) -> void:
	var hull := add_cylinder(parent, "Boat", position + Vector3(0, 0.15, 0), 1.2, 0.35, Color("#70422f"), Vector3(0, 0, 90))
	hull.scale.z = 0.45

func add_rock(parent: Node3D, position: Vector3, size: float) -> void:
	var rock := add_sphere(parent, "RemovableRock", position + Vector3(0, size * 0.45, 0), size, Color("#7d8178"))
	rock.scale = Vector3(1.0, 0.72, 0.85)
