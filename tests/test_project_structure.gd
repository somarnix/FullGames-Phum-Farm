extends SceneTree

var failures: Array[String] = []
var loaded_resources := 0

func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	for required in ["addons", "autoload", "systems", "scenes", "assets", "data", "localization", "references", "resources", "shaders", "scripts", "tests"]:
		_check(DirAccess.dir_exists_absolute(ProjectSettings.globalize_path("res://" + required)), "directory res://%s exists" % required)
	for index in range(1, 39):
		var prefix := "%02d_" % index
		var found := false
		var directory := DirAccess.open("res://scenes/world")
		if directory != null:
			for folder in directory.get_directories():
				if folder.begins_with(prefix):
					found = true
					break
		_check(found, "world module %02d exists" % index)
	_validate_directory("res://")
	_check(loaded_resources >= 100, "at least 100 production resources load")
	if failures.is_empty():
		print("STRUCTURE TEST PASSED: 38 world modules and ", loaded_resources, " loadable production resources")
		quit(0)
	else:
		for failure in failures:
			push_error(failure)
		quit(1)

func _validate_directory(path: String) -> void:
	var directory := DirAccess.open(path)
	if directory == null:
		failures.append("Cannot open " + path)
		return
	for folder in directory.get_directories():
		if folder in [".godot", ".git"]:
			continue
		_validate_directory(path.path_join(folder))
	for file_name in directory.get_files():
		var resource_path := path.path_join(file_name)
		var extension := file_name.get_extension().to_lower()
		if extension in ["gd", "tscn", "tres", "translation", "gdshader"]:
			var resource = load(resource_path)
			if resource == null:
				failures.append("Cannot load " + resource_path)
			else:
				loaded_resources += 1
		elif extension == "json":
			var file := FileAccess.open(resource_path, FileAccess.READ)
			if file == null or JSON.parse_string(file.get_as_text()) == null:
				failures.append("Invalid JSON " + resource_path)

func _check(condition: bool, description: String) -> void:
	if condition:
		print("PASS: ", description)
	else:
		failures.append("FAIL: " + description)
