extends Node3D

var sun: DirectionalLight3D
var world_environment: WorldEnvironment
var rain: GPUParticles3D

func _ready() -> void:
	_build_environment()

func update_for_state() -> void:
	if sun == null or world_environment == null or rain == null:
		return
	var hour := float(GameState.data.minutes) / 60.0
	var daylight := clampf(sin((hour - 5.0) / 14.0 * PI), 0.08, 1.0)
	sun.light_energy = 0.18 + daylight * 1.18
	sun.rotation_degrees.x = -12.0 - daylight * 58.0
	if hour < 6.0 or hour > 18.0:
		sun.light_color = Color("#9db8d8")
		world_environment.environment.ambient_light_color = Color("#52657f")
	else:
		sun.light_color = Color("#fff0c3")
		world_environment.environment.ambient_light_color = Color("#d8e0c2")
	rain.emitting = str(GameState.data.weather) == "rain"
	world_environment.environment.fog_density = 0.008 if rain.emitting else 0.002

func _build_environment() -> void:
	world_environment = WorldEnvironment.new()
	world_environment.name = "WorldEnvironment"
	var environment := Environment.new()
	environment.background_mode = Environment.BG_SKY
	var sky := Sky.new()
	var sky_material := ProceduralSkyMaterial.new()
	sky_material.sky_top_color = Color("#5aa8d2")
	sky_material.sky_horizon_color = Color("#d8eef0")
	sky_material.ground_bottom_color = Color("#6a7352")
	sky_material.ground_horizon_color = Color("#d8d2a1")
	sky_material.sun_angle_max = 12.0
	sky.sky_material = sky_material
	environment.sky = sky
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color("#d8e0c2")
	environment.ambient_light_energy = 0.65
	environment.tonemap_mode = Environment.TONE_MAPPER_ACES
	environment.adjustment_enabled = true
	environment.adjustment_saturation = 1.12
	environment.fog_enabled = true
	environment.fog_light_color = Color("#d6e4cb")
	environment.fog_density = 0.002
	world_environment.environment = environment
	add_child(world_environment)
	sun = DirectionalLight3D.new()
	sun.name = "Sun"
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
