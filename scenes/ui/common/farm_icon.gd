extends Control
class_name FarmIcon

@export var icon_name := "leaf":
	set(value):
		icon_name = value
		queue_redraw()
@export var icon_color := Color("#FFF2D2"):
	set(value):
		icon_color = value
		queue_redraw()
@export var accent_color := Color("#E8B947"):
	set(value):
		accent_color = value
		queue_redraw()

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	custom_minimum_size = Vector2(32, 32)

func _draw() -> void:
	var center := size * 0.5
	var unit := minf(size.x, size.y) / 32.0
	match icon_name:
		"profile": _profile(center, unit)
		"coin": _coin(center, unit)
		"gem": _gem(center, unit)
		"gear", "settings": _gear(center, unit)
		"quest": _quest(center, unit)
		"shop": _shop(center, unit)
		"build": _build_icon(center, unit)
		"orders": _orders(center, unit)
		"bag": _bag(center, unit)
		"map": _map_icon(center, unit)
		"sun": _sun(center, unit)
		"rain": _rain(center, unit)
		"rice": _crop(center, unit, Color("#E8B947"))
		"corn": _crop(center, unit, Color("#F1A93A"))
		"tomato": _tomato(center, unit)
		"sugarcane": _crop(center, unit, Color("#79B84C"))
		"play": _play(center, unit)
		"language": _language(center, unit)
		"home": _home(center, unit)
		_: _leaf(center, unit)

func _profile(c: Vector2, u: float) -> void:
	draw_circle(c + Vector2(0, -5) * u, 5.0 * u, accent_color)
	draw_arc(c + Vector2(0, 9) * u, 9.0 * u, PI, TAU, 24, icon_color, 4.0 * u, true)
	draw_arc(c, 13.0 * u, 0, TAU, 32, icon_color, 2.0 * u, true)

func _coin(c: Vector2, u: float) -> void:
	draw_circle(c, 12.0 * u, Color("#C98B27"))
	draw_circle(c + Vector2(0, -1) * u, 9.0 * u, accent_color)
	draw_arc(c, 5.0 * u, 0, TAU, 24, Color("#FFF2B5"), 2.0 * u, true)
	draw_line(c + Vector2(-3, 0) * u, c + Vector2(3, 0) * u, Color("#FFF2B5"), 2.0 * u, true)

func _gem(c: Vector2, u: float) -> void:
	var points := PackedVector2Array([c + Vector2(0, -13) * u, c + Vector2(11, -4) * u, c + Vector2(7, 10) * u, c + Vector2(0, 14) * u, c + Vector2(-7, 10) * u, c + Vector2(-11, -4) * u])
	draw_colored_polygon(points, Color("#A96B9E"))
	draw_polyline(PackedVector2Array(points + PackedVector2Array([points[0]])), Color("#F0B7DC"), 2.0 * u, true)
	draw_line(points[0], points[3], Color(1, 0.8, 0.95, 0.55), 1.5 * u, true)

func _gear(c: Vector2, u: float) -> void:
	for angle in range(0, 360, 45):
		var direction := Vector2(cos(deg_to_rad(angle)), sin(deg_to_rad(angle)))
		draw_line(c + direction * 8.0 * u, c + direction * 13.0 * u, icon_color, 4.0 * u, true)
	draw_circle(c, 9.0 * u, icon_color)
	draw_circle(c, 4.0 * u, Color("#815238"))

func _quest(c: Vector2, u: float) -> void:
	draw_rect(Rect2(c + Vector2(-10, -12) * u, Vector2(20, 24) * u), Color("#FFF2D2"), true)
	draw_rect(Rect2(c + Vector2(-10, -12) * u, Vector2(20, 24) * u), icon_color, false, 2.0 * u)
	for y in [-5.0, 1.0, 7.0]:
		draw_line(c + Vector2(-5, y) * u, c + Vector2(6, y) * u, Color("#815238"), 2.0 * u, true)
	draw_circle(c + Vector2(-9, -7) * u, 3.0 * u, accent_color)

func _shop(c: Vector2, u: float) -> void:
	draw_rect(Rect2(c + Vector2(-11, -4) * u, Vector2(22, 16) * u), icon_color, true)
	var awning := PackedVector2Array([c + Vector2(-14, -5) * u, c + Vector2(-10, -12) * u, c + Vector2(10, -12) * u, c + Vector2(14, -5) * u])
	draw_colored_polygon(awning, accent_color)
	draw_rect(Rect2(c + Vector2(-4, 3) * u, Vector2(8, 9) * u), Color("#815238"), true)

func _build_icon(c: Vector2, u: float) -> void:
	draw_line(c + Vector2(-10, 10) * u, c + Vector2(8, -8) * u, accent_color, 6.0 * u, true)
	draw_circle(c + Vector2(-11, 11) * u, 4.0 * u, icon_color)
	draw_colored_polygon(PackedVector2Array([c + Vector2(4, -12) * u, c + Vector2(13, -10) * u, c + Vector2(11, -1) * u]), icon_color)

func _orders(c: Vector2, u: float) -> void:
	draw_rect(Rect2(c + Vector2(-12, -9) * u, Vector2(24, 20) * u), Color("#FFF2D2"), true)
	draw_line(c + Vector2(-12, -9) * u, c + Vector2(0, 1) * u, accent_color, 2.0 * u, true)
	draw_line(c + Vector2(12, -9) * u, c + Vector2(0, 1) * u, accent_color, 2.0 * u, true)
	draw_arc(c, 14 * u, 0, TAU, 24, icon_color, 2.0 * u, true)

func _bag(c: Vector2, u: float) -> void:
	var points := PackedVector2Array([c + Vector2(-10, -5) * u, c + Vector2(10, -5) * u, c + Vector2(12, 12) * u, c + Vector2(-12, 12) * u])
	draw_colored_polygon(points, icon_color)
	draw_arc(c + Vector2(0, -5) * u, 7.0 * u, PI, TAU, 18, accent_color, 3.0 * u, true)
	_leaf(c + Vector2(0, 4) * u, u * 0.55)

func _map_icon(c: Vector2, u: float) -> void:
	var points := PackedVector2Array([c + Vector2(-13, -10) * u, c + Vector2(-4, -13) * u, c + Vector2(5, -9) * u, c + Vector2(13, -12) * u, c + Vector2(13, 10) * u, c + Vector2(4, 13) * u, c + Vector2(-5, 9) * u, c + Vector2(-13, 12) * u])
	draw_colored_polygon(points, icon_color)
	draw_line(c + Vector2(-4, -12) * u, c + Vector2(-5, 9) * u, accent_color, 2.0 * u)
	draw_line(c + Vector2(5, -9) * u, c + Vector2(4, 13) * u, accent_color, 2.0 * u)

func _sun(c: Vector2, u: float) -> void:
	draw_circle(c, 8.0 * u, accent_color)
	for angle in range(0, 360, 45):
		var d := Vector2(cos(deg_to_rad(angle)), sin(deg_to_rad(angle)))
		draw_line(c + d * 11.0 * u, c + d * 14.0 * u, icon_color, 2.0 * u, true)

func _rain(c: Vector2, u: float) -> void:
	draw_circle(c + Vector2(-5, -5) * u, 7.0 * u, icon_color)
	draw_circle(c + Vector2(4, -7) * u, 9.0 * u, icon_color)
	draw_rect(Rect2(c + Vector2(-11, -6) * u, Vector2(22, 9) * u), icon_color, true)
	for x in [-7.0, 0.0, 7.0]:
		draw_line(c + Vector2(x, 7) * u, c + Vector2(x - 2, 13) * u, accent_color, 2.0 * u, true)

func _crop(c: Vector2, u: float, grain: Color) -> void:
	draw_line(c + Vector2(0, 13) * u, c + Vector2(0, -11) * u, icon_color, 3.0 * u, true)
	for y in [-8.0, -3.0, 2.0, 7.0]:
		draw_circle(c + Vector2(-4, y) * u, 3.2 * u, grain)
		draw_circle(c + Vector2(4, y + 2) * u, 3.2 * u, grain)

func _tomato(c: Vector2, u: float) -> void:
	draw_circle(c + Vector2(0, 3) * u, 11.0 * u, Color("#D9573F"))
	for angle in range(0, 360, 72):
		var d := Vector2(cos(deg_to_rad(angle)), sin(deg_to_rad(angle)))
		draw_line(c + Vector2(0, -7) * u, c + Vector2(0, -7) * u + d * 6.0 * u, icon_color, 2.0 * u, true)

func _play(c: Vector2, u: float) -> void:
	draw_colored_polygon(PackedVector2Array([c + Vector2(-7, -12) * u, c + Vector2(13, 0) * u, c + Vector2(-7, 12) * u]), icon_color)

func _language(c: Vector2, u: float) -> void:
	draw_arc(c, 13.0 * u, 0, TAU, 32, icon_color, 2.0 * u, true)
	draw_arc(c, 7.0 * u, -PI * 0.5, PI * 0.5, 18, icon_color, 2.0 * u, true)
	draw_arc(c, 7.0 * u, PI * 0.5, PI * 1.5, 18, icon_color, 2.0 * u, true)
	draw_line(c + Vector2(-12, 0) * u, c + Vector2(12, 0) * u, icon_color, 2.0 * u)

func _home(c: Vector2, u: float) -> void:
	draw_colored_polygon(PackedVector2Array([c + Vector2(-14, 0) * u, c + Vector2(0, -13) * u, c + Vector2(14, 0) * u]), accent_color)
	draw_rect(Rect2(c + Vector2(-10, 0) * u, Vector2(20, 13) * u), icon_color, true)
	draw_rect(Rect2(c + Vector2(-3, 5) * u, Vector2(6, 8) * u), Color("#815238"), true)

func _leaf(c: Vector2, u: float) -> void:
	var points := PackedVector2Array([c + Vector2(-11, 7) * u, c + Vector2(-5, -9) * u, c + Vector2(11, -11) * u, c + Vector2(9, 5) * u])
	draw_colored_polygon(points, icon_color)
	draw_line(c + Vector2(-8, 9) * u, c + Vector2(8, -8) * u, accent_color, 2.0 * u, true)
