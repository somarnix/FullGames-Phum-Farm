extends RefCounted
class_name AssetSlots

const MODEL_ROOT := "res://assets/models/"

static func find_model(category: String, model_name: String) -> PackedScene:
	for extension in ["glb", "fbx", "gltf"]:
		var path := "%s%s/%s.%s" % [MODEL_ROOT, category, model_name, extension]
		if ResourceLoader.exists(path):
			var resource = load(path)
			if resource is PackedScene:
				return resource
	return null

static func instantiate_model(category: String, model_name: String, target_height: float = 0.0) -> Node3D:
	var scene := find_model(category, model_name)
	if scene == null:
		return null
	var instance := scene.instantiate() as Node3D
	if instance == null:
		return null
	instance.name = "Imported_%s" % model_name
	if target_height > 0.0:
		_normalize_height(instance, target_height)
	return instance

static func _normalize_height(node: Node3D, target_height: float) -> void:
	var bounds := AABB()
	var has_bounds := false
	for child in node.find_children("*", "MeshInstance3D", true, false):
		var mesh_node := child as MeshInstance3D
		if mesh_node.mesh == null:
			continue
		var local_bounds := mesh_node.transform * mesh_node.mesh.get_aabb()
		if has_bounds:
			bounds = bounds.merge(local_bounds)
		else:
			bounds = local_bounds
			has_bounds = true
	if has_bounds and bounds.size.y > 0.001:
		var factor := target_height / bounds.size.y
		node.scale = Vector3.ONE * factor
		node.position.y = -bounds.position.y * factor
