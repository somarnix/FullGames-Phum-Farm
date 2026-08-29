extends Node

const FarmWorldScene = preload("res://scenes/world/farm_world.tscn")
const FarmIconScript = preload("res://scenes/ui/common/farm_icon.gd")

const CREAM := Color("#FFF2D2")
const CREAM_LIGHT := Color("#FFF9E8")
const DARK_WOOD := Color("#5B3526")
const MEDIUM_WOOD := Color("#815238")
const GREEN := Color("#5B9B3E")
const GREEN_HIGHLIGHT := Color("#79B84C")
const TERRACOTTA := Color("#C8663D")
const GOLD := Color("#E8B947")
const GEM := Color("#A96B9E")
const TEXT_DARK := Color("#493226")
const TEXT_SECONDARY := Color("#796250")

var world: Node3D
var interface: CanvasLayer
var hud_root: Control
var menu_mode := false
var active_overlay: Control
var active_sheet: Control
var active_station: Node
var selected_plot_index := -1

var coin_label: Label
var gem_label: Label
var level_label: Label
var xp_bar: ProgressBar
var clock_label: Label
var weather_icon: Control
var quest_label: Label
var prompt_panel: PanelContainer
var prompt_label: Label
var toast_panel: PanelContainer
var toast_label: Label
var toast_tween: Tween
var bottom_buttons: Dictionary = {}

func _ready() -> void:
	_build_theme()
	GameState.message_requested.connect(_show_toast)
	GameState.level_up.connect(_show_level_up)
	GameState.changed.connect(_refresh_hud)
	show_main_menu()

func _process(delta: float) -> void:
	if menu_mode and is_instance_valid(world) and is_instance_valid(world.camera_pivot):
		world.camera_pivot.rotation_degrees.y += delta * 1.35
	if not menu_mode and is_instance_valid(world):
		if clock_label != null:
			clock_label.text = "%s" % GameState.clock_text()
		if weather_icon != null:
			weather_icon.icon_name = "rain" if str(GameState.data.weather) == "rain" else "sun"

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("pause_game") and not menu_mode:
		if active_overlay != null:
			_close_overlay()
		elif active_sheet != null:
			_close_sheet()
		else:
			_open_pause()

func _build_theme() -> void:
	var theme := Theme.new()
	theme.default_font_size = 18
	theme.set_font_size("font_size", "Button", 17)
	theme.set_font_size("font_size", "Label", 17)
	theme.set_color("font_color", "Label", TEXT_DARK)
	theme.set_color("font_color", "Button", CREAM_LIGHT)
	theme.set_color("font_hover_color", "Button", Color.WHITE)
	theme.set_color("font_pressed_color", "Button", CREAM)
	theme.set_color("font_disabled_color", "Button", Color("#A89075"))
	theme.set_stylebox("normal", "Button", _style(GREEN, 16, 2, Color("#3F742E"), 5))
	theme.set_stylebox("hover", "Button", _style(GREEN_HIGHLIGHT, 16, 2, Color("#D7E99E"), 6))
	theme.set_stylebox("pressed", "Button", _style(Color("#4B8434"), 16, 2, Color("#335E26"), 2))
	theme.set_stylebox("disabled", "Button", _style(Color("#B49A78"), 16, 2, Color("#8B7359"), 2))
	theme.set_stylebox("focus", "Button", StyleBoxEmpty.new())
	theme.set_stylebox("background", "ProgressBar", _style(Color("#D5B987"), 8, 2, DARK_WOOD, 1))
	theme.set_stylebox("fill", "ProgressBar", _style(GOLD, 8, 1, Color("#FFE095"), 1))
	get_tree().root.theme = theme

func _style(color: Color, radius: int = 14, border: int = 0, border_color: Color = Color.TRANSPARENT, shadow: int = 0) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.corner_radius_top_left = radius
	style.corner_radius_top_right = radius
	style.corner_radius_bottom_left = radius
	style.corner_radius_bottom_right = radius
	style.content_margin_left = 14
	style.content_margin_right = 14
	style.content_margin_top = 10
	style.content_margin_bottom = 10
	if border > 0:
		style.border_width_left = border
		style.border_width_right = border
		style.border_width_top = border
		style.border_width_bottom = border
		style.border_color = border_color
	if shadow > 0:
		style.shadow_color = Color(0.15, 0.07, 0.025, 0.36)
		style.shadow_size = shadow
		style.shadow_offset = Vector2(0, shadow * 0.7)
	return style

func _clear_screen() -> void:
	get_tree().paused = false
	if is_instance_valid(world):
		world.queue_free()
	world = null
	if is_instance_valid(interface):
		interface.queue_free()
	interface = CanvasLayer.new()
	interface.name = "PhumFarmInterface"
	add_child(interface)
	hud_root = null
	active_overlay = null
	active_sheet = null
	bottom_buttons.clear()

func show_main_menu() -> void:
	_clear_screen()
	menu_mode = true
	world = FarmWorldScene.instantiate()
	world.name = "AnimatedMenuFarm"
	add_child(world)
	world.set_process_unhandled_input(false)
	world.player.set_physics_process(false)
	world.player.visible = false
	world.camera_size = 34.0
	world.camera.size = 34.0
	world.camera_pivot.position = Vector3(-12, 0, -8)

	var root := Control.new()
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	interface.add_child(root)
	var warm_tint := ColorRect.new()
	warm_tint.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	warm_tint.color = Color(0.13, 0.055, 0.025, 0.12)
	warm_tint.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.add_child(warm_tint)
	var top_vignette := ColorRect.new()
	top_vignette.position = Vector2(0, 0)
	top_vignette.size = Vector2(1280, 200)
	top_vignette.color = Color(0.12, 0.055, 0.025, 0.34)
	top_vignette.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.add_child(top_vignette)
	var bottom_vignette := ColorRect.new()
	bottom_vignette.position = Vector2(0, 470)
	bottom_vignette.size = Vector2(1280, 250)
	bottom_vignette.color = Color(0.09, 0.04, 0.02, 0.5)
	bottom_vignette.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.add_child(bottom_vignette)

	var logo := PanelContainer.new()
	logo.position = Vector2(415, 42)
	logo.size = Vector2(450, 158)
	logo.add_theme_stylebox_override("panel", _style(Color(0.32, 0.16, 0.09, 0.88), 30, 4, GOLD, 8))
	root.add_child(logo)
	var logo_box := VBoxContainer.new()
	logo_box.alignment = BoxContainer.ALIGNMENT_CENTER
	logo.add_child(logo_box)
	var title := Label.new()
	title.text = "PHUM FARM"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 57)
	title.add_theme_color_override("font_color", CREAM_LIGHT)
	title.add_theme_color_override("font_shadow_color", Color(0.12, 0.04, 0.01, 0.8))
	title.add_theme_constant_override("shadow_offset_x", 0)
	title.add_theme_constant_override("shadow_offset_y", 4)
	logo_box.add_child(title)
	var subtitle := Label.new()
	subtitle.text = "Grow your village. Share the harvest."
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.add_theme_font_size_override("font_size", 16)
	subtitle.add_theme_color_override("font_color", GOLD)
	logo_box.add_child(subtitle)

	var play_button := _make_icon_button("play", "PLAY", Vector2(300, 76), 31)
	play_button.position = Vector2(490, 535)
	play_button.pressed.connect(_play_from_menu)
	root.add_child(play_button)
	var small_row := HBoxContainer.new()
	small_row.position = Vector2(476, 625)
	small_row.size = Vector2(328, 62)
	small_row.alignment = BoxContainer.ALIGNMENT_CENTER
	small_row.add_theme_constant_override("separation", 18)
	root.add_child(small_row)
	for spec in [["profile", "PROFILE", _open_profile], ["language", "LANGUAGE", _open_language], ["settings", "SETTINGS", _open_settings]]:
		var button := _make_round_icon_button(str(spec[0]), str(spec[1]))
		button.pressed.connect(spec[2])
		small_row.add_child(button)
	var version := Label.new()
	version.text = "v1.0"
	version.position = Vector2(1214, 685)
	version.add_theme_font_size_override("font_size", 12)
	version.add_theme_color_override("font_color", Color(1, 0.95, 0.82, 0.7))
	root.add_child(version)

func _play_from_menu() -> void:
	_start_game(GameState.has_save())

func _start_game(load_existing: bool) -> void:
	if load_existing:
		GameState.load_game()
	else:
		GameState.new_game()
	_clear_screen()
	menu_mode = false
	world = FarmWorldScene.instantiate()
	world.name = "PhumFarmWorld"
	add_child(world)
	world.prompt_changed.connect(_set_prompt)
	world.world_message.connect(_show_toast)
	world.crop_tray_requested.connect(_open_crop_tray)
	world.action_sheet_requested.connect(_open_action_sheet)
	_build_hud()
	_refresh_hud()
	_show_toast("Welcome home! Tap a field to begin.")

func _build_hud() -> void:
	hud_root = Control.new()
	hud_root.name = "CommercialHUD"
	hud_root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	hud_root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	interface.add_child(hud_root)
	_build_player_status()
	_build_currency_hud()
	_build_time_hud()
	_build_quest_hud()
	_build_bottom_navigation()
	_build_context_prompt()
	_build_toast()

func _build_player_status() -> void:
	var card := PanelContainer.new()
	card.position = Vector2(16, 14)
	card.size = Vector2(275, 78)
	card.mouse_filter = Control.MOUSE_FILTER_STOP
	card.add_theme_stylebox_override("panel", _style(Color(0.32, 0.17, 0.11, 0.93), 24, 3, Color("#B7804A"), 5))
	hud_root.add_child(card)
	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 10)
	card.add_child(row)
	var portrait_frame := PanelContainer.new()
	portrait_frame.custom_minimum_size = Vector2(58, 58)
	portrait_frame.add_theme_stylebox_override("panel", _style(GREEN, 29, 3, GOLD, 2))
	row.add_child(portrait_frame)
	var portrait := FarmIconScript.new()
	portrait.icon_name = "profile"
	portrait.icon_color = CREAM
	portrait.accent_color = GOLD
	portrait_frame.add_child(portrait)
	var status_box := VBoxContainer.new()
	status_box.add_theme_constant_override("separation", 4)
	status_box.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(status_box)
	level_label = Label.new()
	level_label.add_theme_font_size_override("font_size", 18)
	level_label.add_theme_color_override("font_color", CREAM_LIGHT)
	status_box.add_child(level_label)
	xp_bar = ProgressBar.new()
	xp_bar.custom_minimum_size = Vector2(175, 18)
	xp_bar.show_percentage = false
	status_box.add_child(xp_bar)
	var xp_caption := Label.new()
	xp_caption.name = "XpCaption"
	xp_caption.add_theme_font_size_override("font_size", 12)
	xp_caption.add_theme_color_override("font_color", Color("#EAD2AC"))
	status_box.add_child(xp_caption)

func _build_currency_hud() -> void:
	var row := HBoxContainer.new()
	row.position = Vector2(844, 16)
	row.size = Vector2(400, 58)
	row.add_theme_constant_override("separation", 10)
	row.mouse_filter = Control.MOUSE_FILTER_STOP
	hud_root.add_child(row)
	coin_label = _currency_capsule(row, "coin", GOLD, true)
	gem_label = _currency_capsule(row, "gem", GEM, false)
	var gear := _make_icon_button("gear", "", Vector2(55, 55), 0)
	gear.tooltip_text = "Menu"
	gear.pressed.connect(_open_pause)
	row.add_child(gear)

func _currency_capsule(parent: Control, icon_name: String, accent: Color, add_plus: bool) -> Label:
	var capsule := PanelContainer.new()
	capsule.custom_minimum_size = Vector2(150 if add_plus else 112, 52)
	capsule.add_theme_stylebox_override("panel", _style(Color(0.31, 0.17, 0.11, 0.94), 25, 2, Color("#A97646"), 4))
	parent.add_child(capsule)
	var row := HBoxContainer.new()
	row.alignment = BoxContainer.ALIGNMENT_CENTER
	row.add_theme_constant_override("separation", 5)
	capsule.add_child(row)
	var icon := FarmIconScript.new()
	icon.icon_name = icon_name
	icon.icon_color = CREAM
	icon.accent_color = accent
	icon.custom_minimum_size = Vector2(34, 34)
	row.add_child(icon)
	var value := Label.new()
	value.custom_minimum_size.x = 42
	value.add_theme_font_size_override("font_size", 18)
	value.add_theme_color_override("font_color", CREAM_LIGHT)
	value.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	row.add_child(value)
	if add_plus:
		var plus := Button.new()
		plus.text = "+"
		plus.custom_minimum_size = Vector2(29, 29)
		plus.add_theme_font_size_override("font_size", 18)
		plus.add_theme_stylebox_override("normal", _style(GREEN, 15, 1, GREEN_HIGHLIGHT, 2))
		plus.add_theme_stylebox_override("hover", _style(GREEN_HIGHLIGHT, 15, 1, CREAM, 3))
		plus.pressed.connect(func(): _show_toast("Visit the market to earn more coins."))
		row.add_child(plus)
	return value

func _build_time_hud() -> void:
	var pill := PanelContainer.new()
	pill.position = Vector2(552, 17)
	pill.size = Vector2(176, 50)
	pill.mouse_filter = Control.MOUSE_FILTER_STOP
	pill.add_theme_stylebox_override("panel", _style(Color(0.31, 0.17, 0.11, 0.88), 24, 2, Color("#A97646"), 4))
	hud_root.add_child(pill)
	var row := HBoxContainer.new()
	row.alignment = BoxContainer.ALIGNMENT_CENTER
	row.add_theme_constant_override("separation", 10)
	pill.add_child(row)
	weather_icon = FarmIconScript.new()
	weather_icon.icon_name = "sun"
	weather_icon.icon_color = CREAM
	weather_icon.accent_color = GOLD
	weather_icon.custom_minimum_size = Vector2(32, 32)
	row.add_child(weather_icon)
	clock_label = Label.new()
	clock_label.add_theme_color_override("font_color", CREAM_LIGHT)
	clock_label.add_theme_font_size_override("font_size", 17)
	row.add_child(clock_label)
	var weather_button := Button.new()
	weather_button.flat = true
	weather_button.tooltip_text = "Change weather for testing"
	weather_button.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	weather_button.pressed.connect(_toggle_weather)
	pill.add_child(weather_button)

func _build_quest_hud() -> void:
	var card := Button.new()
	card.position = Vector2(18, 108)
	card.size = Vector2(238, 82)
	card.add_theme_stylebox_override("normal", _style(Color(1, 0.95, 0.82, 0.95), 18, 3, MEDIUM_WOOD, 5))
	card.add_theme_stylebox_override("hover", _style(CREAM_LIGHT, 18, 3, GOLD, 7))
	card.pressed.connect(_open_quest_book)
	hud_root.add_child(card)
	var icon := FarmIconScript.new()
	icon.icon_name = "quest"
	icon.icon_color = MEDIUM_WOOD
	icon.accent_color = TERRACOTTA
	icon.position = Vector2(12, 19)
	icon.size = Vector2(42, 42)
	card.add_child(icon)
	quest_label = Label.new()
	quest_label.position = Vector2(62, 10)
	quest_label.size = Vector2(150, 60)
	quest_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	quest_label.add_theme_font_size_override("font_size", 14)
	quest_label.add_theme_color_override("font_color", TEXT_DARK)
	card.add_child(quest_label)
	var arrow := Label.new()
	arrow.text = ">"
	arrow.position = Vector2(213, 27)
	arrow.add_theme_font_size_override("font_size", 20)
	arrow.add_theme_color_override("font_color", TERRACOTTA)
	card.add_child(arrow)

func _build_bottom_navigation() -> void:
	var tray := PanelContainer.new()
	tray.position = Vector2(318, 624)
	tray.size = Vector2(644, 82)
	tray.mouse_filter = Control.MOUSE_FILTER_STOP
	tray.add_theme_stylebox_override("panel", _style(Color(0.30, 0.15, 0.09, 0.94), 26, 3, Color("#B47B44"), 7))
	hud_root.add_child(tray)
	var row := HBoxContainer.new()
	row.alignment = BoxContainer.ALIGNMENT_CENTER
	row.add_theme_constant_override("separation", 10)
	tray.add_child(row)
	for spec in [["shop", "SHOP", _open_shop], ["build", "BUILD", _open_build], ["orders", "ORDERS", _open_orders], ["bag", "BAG", _open_inventory], ["map", "MAP", _open_map]]:
		var button := _make_nav_button(str(spec[0]), str(spec[1]))
		button.pressed.connect(spec[2])
		row.add_child(button)
		bottom_buttons[str(spec[0])] = button

func _build_context_prompt() -> void:
	prompt_panel = PanelContainer.new()
	prompt_panel.position = Vector2(448, 558)
	prompt_panel.size = Vector2(384, 49)
	prompt_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	prompt_panel.add_theme_stylebox_override("panel", _style(Color(0.30, 0.15, 0.09, 0.9), 18, 2, GOLD, 4))
	hud_root.add_child(prompt_panel)
	prompt_label = Label.new()
	prompt_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	prompt_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	prompt_label.add_theme_font_size_override("font_size", 15)
	prompt_label.add_theme_color_override("font_color", CREAM_LIGHT)
	prompt_panel.add_child(prompt_label)
	prompt_panel.visible = false

func _build_toast() -> void:
	toast_panel = PanelContainer.new()
	toast_panel.position = Vector2(440, 92)
	toast_panel.size = Vector2(400, 54)
	toast_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	toast_panel.add_theme_stylebox_override("panel", _style(Color(0.27, 0.15, 0.09, 0.96), 19, 2, GOLD, 5))
	hud_root.add_child(toast_panel)
	toast_label = Label.new()
	toast_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	toast_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	toast_label.add_theme_color_override("font_color", CREAM_LIGHT)
	toast_panel.add_child(toast_label)
	toast_panel.visible = false

func _make_icon_button(icon_name: String, label_text: String, button_size: Vector2, font_size: int = 17) -> Button:
	var button := Button.new()
	button.custom_minimum_size = button_size
	button.size = button_size
	button.text = "     " + label_text if not label_text.is_empty() else ""
	button.add_theme_stylebox_override("normal", _style(GREEN, 16, 2, Color("#3F742E"), 5))
	button.add_theme_stylebox_override("hover", _style(GREEN_HIGHLIGHT, 16, 2, Color("#D7E99E"), 6))
	button.add_theme_stylebox_override("pressed", _style(Color("#4B8434"), 16, 2, Color("#335E26"), 2))
	button.add_theme_stylebox_override("disabled", _style(Color("#B49A78"), 16, 2, Color("#8B7359"), 2))
	if font_size > 0:
		button.add_theme_font_size_override("font_size", font_size)
	var icon := FarmIconScript.new()
	icon.icon_name = icon_name
	icon.icon_color = CREAM_LIGHT
	icon.accent_color = GOLD
	icon.position = Vector2(18, (button_size.y - 34) * 0.5)
	icon.size = Vector2(34, 34)
	button.add_child(icon)
	_polish_button(button)
	return button

func _make_round_icon_button(icon_name: String, tooltip: String) -> Button:
	var button := Button.new()
	button.custom_minimum_size = Vector2(66, 58)
	button.tooltip_text = tooltip
	button.add_theme_stylebox_override("normal", _style(Color(0.32, 0.17, 0.1, 0.94), 28, 2, Color("#B7804A"), 4))
	button.add_theme_stylebox_override("hover", _style(MEDIUM_WOOD, 28, 2, GOLD, 6))
	var icon := FarmIconScript.new()
	icon.icon_name = icon_name
	icon.icon_color = CREAM
	icon.accent_color = GOLD
	icon.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT, Control.PRESET_MODE_MINSIZE, 12)
	button.add_child(icon)
	_polish_button(button)
	return button

func _make_nav_button(icon_name: String, label_text: String) -> Button:
	var button := Button.new()
	button.custom_minimum_size = Vector2(112, 64)
	button.add_theme_stylebox_override("normal", _style(MEDIUM_WOOD, 18, 2, Color("#A87548"), 3))
	button.add_theme_stylebox_override("hover", _style(TERRACOTTA, 18, 2, GOLD, 5))
	button.add_theme_stylebox_override("pressed", _style(Color("#9F4D31"), 18, 2, CREAM, 1))
	var icon := FarmIconScript.new()
	icon.icon_name = icon_name
	icon.icon_color = CREAM_LIGHT
	icon.accent_color = GOLD
	icon.position = Vector2(40, 5)
	icon.size = Vector2(32, 32)
	button.add_child(icon)
	var label := Label.new()
	label.text = label_text
	label.position = Vector2(4, 39)
	label.size = Vector2(104, 21)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.add_theme_font_size_override("font_size", 12)
	label.add_theme_color_override("font_color", CREAM_LIGHT)
	button.add_child(label)
	_polish_button(button)
	return button

func _polish_button(button: Button) -> void:
	button.pivot_offset = button.custom_minimum_size * 0.5
	button.button_down.connect(func():
		var tween := create_tween()
		tween.tween_property(button, "scale", Vector2(0.96, 0.96), 0.07)
		Input.vibrate_handheld(20)
	)
	button.button_up.connect(func():
		var tween := create_tween()
		tween.tween_property(button, "scale", Vector2.ONE, 0.1).set_trans(Tween.TRANS_BACK)
	)

func _create_modal(title_text: String, panel_size: Vector2, pauses_game: bool = false) -> VBoxContainer:
	_close_sheet()
	_close_overlay()
	var shade := ColorRect.new()
	shade.name = "%sOverlay" % title_text.replace(" ", "")
	shade.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	shade.color = Color(0.10, 0.045, 0.02, 0.56)
	shade.mouse_filter = Control.MOUSE_FILTER_STOP
	shade.process_mode = Node.PROCESS_MODE_ALWAYS
	shade.set_meta("pauses_game", pauses_game)
	interface.add_child(shade)
	active_overlay = shade
	var panel := PanelContainer.new()
	panel.position = Vector2((1280.0 - panel_size.x) * 0.5, (720.0 - panel_size.y) * 0.5)
	panel.size = panel_size
	panel.pivot_offset = panel_size * 0.5
	panel.scale = Vector2(0.88, 0.88)
	panel.modulate.a = 0.0
	panel.add_theme_stylebox_override("panel", _style(CREAM, 28, 7, DARK_WOOD, 12))
	shade.add_child(panel)
	var outer := VBoxContainer.new()
	outer.add_theme_constant_override("separation", 10)
	panel.add_child(outer)
	var header := HBoxContainer.new()
	header.custom_minimum_size.y = 52
	outer.add_child(header)
	var motif_left := FarmIconScript.new()
	motif_left.icon_name = "leaf"
	motif_left.icon_color = GREEN
	motif_left.accent_color = GOLD
	motif_left.custom_minimum_size = Vector2(38, 38)
	header.add_child(motif_left)
	var title := Label.new()
	title.text = title_text.to_upper()
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 30)
	title.add_theme_color_override("font_color", DARK_WOOD)
	header.add_child(title)
	var close := Button.new()
	close.text = "×"
	close.custom_minimum_size = Vector2(46, 42)
	close.add_theme_font_size_override("font_size", 26)
	close.add_theme_stylebox_override("normal", _style(TERRACOTTA, 20, 2, Color("#98452D"), 2))
	close.pressed.connect(_close_overlay)
	header.add_child(close)
	outer.add_child(HSeparator.new())
	var content := VBoxContainer.new()
	content.size_flags_vertical = Control.SIZE_EXPAND_FILL
	content.add_theme_constant_override("separation", 12)
	outer.add_child(content)
	var tween := create_tween().set_parallel(true)
	tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	tween.tween_property(panel, "scale", Vector2.ONE, 0.22).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.tween_property(panel, "modulate:a", 1.0, 0.15)
	if pauses_game:
		get_tree().paused = true
	return content

func _close_overlay() -> void:
	if active_overlay == null:
		return
	var paused_by_overlay := bool(active_overlay.get_meta("pauses_game", false))
	active_overlay.queue_free()
	active_overlay = null
	if paused_by_overlay:
		get_tree().paused = false

func _create_sheet(title_text: String, height: float = 260.0) -> VBoxContainer:
	_close_overlay()
	_close_sheet()
	var holder := Control.new()
	holder.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	holder.mouse_filter = Control.MOUSE_FILTER_IGNORE
	interface.add_child(holder)
	active_sheet = holder
	var panel := PanelContainer.new()
	panel.name = "%sSheet" % title_text.replace(" ", "")
	panel.position = Vector2(120, 730)
	panel.size = Vector2(1040, height)
	panel.mouse_filter = Control.MOUSE_FILTER_STOP
	panel.add_theme_stylebox_override("panel", _style(CREAM, 28, 7, DARK_WOOD, 12))
	holder.add_child(panel)
	var outer := VBoxContainer.new()
	outer.add_theme_constant_override("separation", 8)
	panel.add_child(outer)
	var header := HBoxContainer.new()
	header.custom_minimum_size.y = 42
	outer.add_child(header)
	var title := Label.new()
	title.text = title_text.to_upper()
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	title.add_theme_font_size_override("font_size", 25)
	title.add_theme_color_override("font_color", DARK_WOOD)
	header.add_child(title)
	var close := Button.new()
	close.text = "×"
	close.custom_minimum_size = Vector2(44, 38)
	close.add_theme_stylebox_override("normal", _style(TERRACOTTA, 18, 1, Color("#94432C"), 2))
	close.pressed.connect(_close_sheet)
	header.add_child(close)
	var content := VBoxContainer.new()
	content.size_flags_vertical = Control.SIZE_EXPAND_FILL
	content.add_theme_constant_override("separation", 8)
	outer.add_child(content)
	var tween := create_tween()
	tween.tween_property(panel, "position:y", 720.0 - height - 10.0, 0.26).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	return content

func _close_sheet() -> void:
	if active_sheet != null:
		active_sheet.queue_free()
		active_sheet = null
	active_station = null
	if selected_plot_index >= 0 and is_instance_valid(world):
		world.highlight_plot(-1)
	selected_plot_index = -1

func _open_crop_tray(plot_index: int) -> void:
	selected_plot_index = plot_index
	world.highlight_plot(plot_index)
	var content := _create_sheet("Choose a Crop", 244)
	# _create_sheet clears the selection bookkeeping, restore it afterward.
	selected_plot_index = plot_index
	world.highlight_plot(plot_index)
	var hint := Label.new()
	hint.text = "Select a seed for this field"
	hint.add_theme_color_override("font_color", TEXT_SECONDARY)
	content.add_child(hint)
	var row := HBoxContainer.new()
	row.alignment = BoxContainer.ALIGNMENT_CENTER
	row.add_theme_constant_override("separation", 14)
	content.add_child(row)
	for crop_id in ["rice", "corn", "tomato", "sugarcane"]:
		var crop := FarmData.crop(crop_id)
		var required_level := UnlockSystem.crop_required_level(crop_id)
		var card := Button.new()
		card.custom_minimum_size = Vector2(220, 126)
		card.add_theme_stylebox_override("normal", _style(CREAM_LIGHT, 18, 3, MEDIUM_WOOD, 4))
		card.add_theme_stylebox_override("hover", _style(Color("#FFF5C9"), 18, 4, GOLD, 7))
		card.add_theme_color_override("font_color", TEXT_DARK)
		card.disabled = not UnlockSystem.is_crop_unlocked(crop_id)
		row.add_child(card)
		var icon := FarmIconScript.new()
		icon.icon_name = crop_id
		icon.icon_color = GREEN
		icon.accent_color = GOLD
		icon.position = Vector2(16, 25)
		icon.size = Vector2(58, 58)
		card.add_child(icon)
		var name_label := Label.new()
		name_label.text = str(crop.get("name", crop_id)).to_upper()
		name_label.position = Vector2(84, 13)
		name_label.size = Vector2(122, 28)
		name_label.add_theme_font_size_override("font_size", 18)
		name_label.add_theme_color_override("font_color", TEXT_DARK)
		card.add_child(name_label)
		var detail := Label.new()
		if card.disabled:
			detail.text = "Locked • Level %d" % required_level
		else:
			detail.text = "%d coins\n%s growth" % [int(crop.get("seed_cost", 0)), _format_duration(float(crop.get("grow_seconds", 0)))]
		detail.position = Vector2(84, 43)
		detail.size = Vector2(125, 56)
		detail.add_theme_font_size_override("font_size", 14)
		detail.add_theme_color_override("font_color", TEXT_SECONDARY)
		card.add_child(detail)
		card.pressed.connect(func(): _plant_crop(crop_id))
		_polish_button(card)

func _plant_crop(crop_id: String) -> void:
	if selected_plot_index < 0:
		return
	if world.plant_selected_plot(selected_plot_index, crop_id):
		_close_sheet()

func _open_action_sheet(type: String, id: String, station: Node) -> void:
	active_station = station
	match type:
		"production": _open_production_sheet(id, station)
		"animal": _open_animal_sheet(id, station)
		"expansion": _open_expansion_popup(id, station)

func _open_production_sheet(product_id: String, station: Node) -> void:
	var product := FarmData.product(product_id)
	var content := _create_sheet(str(product.get("name", "Workshop")), 270)
	active_station = station
	var body := HBoxContainer.new()
	body.add_theme_constant_override("separation", 18)
	content.add_child(body)
	var recipe_card := PanelContainer.new()
	recipe_card.custom_minimum_size = Vector2(600, 155)
	recipe_card.add_theme_stylebox_override("panel", _style(CREAM_LIGHT, 18, 3, MEDIUM_WOOD, 3))
	body.add_child(recipe_card)
	var recipe_row := HBoxContainer.new()
	recipe_row.add_theme_constant_override("separation", 18)
	recipe_card.add_child(recipe_row)
	var icon := FarmIconScript.new()
	icon.icon_name = "orders"
	icon.icon_color = TERRACOTTA
	icon.accent_color = GOLD
	icon.custom_minimum_size = Vector2(86, 86)
	recipe_row.add_child(icon)
	var info := VBoxContainer.new()
	info.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	recipe_row.add_child(info)
	var recipe_name := Label.new()
	recipe_name.text = str(product.get("name", product_id))
	recipe_name.add_theme_font_size_override("font_size", 25)
	recipe_name.add_theme_color_override("font_color", DARK_WOOD)
	info.add_child(recipe_name)
	var requirements: Array[String] = []
	for item_id in product.get("inputs", {}):
		requirements.append("%d %s" % [int(product.inputs[item_id]), str(item_id).capitalize()])
	var detail := Label.new()
	detail.text = "Ingredients: %s\nTime: %s   •   Value: %d coins" % [", ".join(requirements), _format_duration(float(product.get("seconds", 0))), int(product.get("sell", 0))]
	detail.add_theme_color_override("font_color", TEXT_SECONDARY)
	info.add_child(detail)
	var queue := PanelContainer.new()
	queue.custom_minimum_size = Vector2(360, 155)
	queue.add_theme_stylebox_override("panel", _style(Color("#EAD2A7"), 18, 3, DARK_WOOD, 3))
	body.add_child(queue)
	var queue_box := VBoxContainer.new()
	queue_box.alignment = BoxContainer.ALIGNMENT_CENTER
	queue.add_child(queue_box)
	var machine: Dictionary = GameState.data.machines.get(product_id, {})
	var queue_label := Label.new()
	queue_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	if machine.is_empty():
		queue_label.text = "QUEUE SLOT\nReady to craft"
	else:
		queue_label.text = "IN PRODUCTION\n%s" % station.prompt_text()
	queue_label.add_theme_color_override("font_color", DARK_WOOD)
	queue_box.add_child(queue_label)
	var action := _make_icon_button("play", "START", Vector2(210, 52), 18)
	action.disabled = not machine.is_empty()
	action.pressed.connect(func():
		if is_instance_valid(active_station):
			active_station.perform_primary_action()
		_close_sheet()
	)
	queue_box.add_child(action)

func _open_animal_sheet(id: String, station: Node) -> void:
	var title := "Chicken Coop" if id == "chickens" else "Cow & Buffalo Pasture"
	var content := _create_sheet(title, 232)
	active_station = station
	var row := HBoxContainer.new()
	row.alignment = BoxContainer.ALIGNMENT_CENTER
	row.add_theme_constant_override("separation", 30)
	content.add_child(row)
	var portrait := PanelContainer.new()
	portrait.custom_minimum_size = Vector2(160, 120)
	portrait.add_theme_stylebox_override("panel", _style(Color("#EAD2A7"), 22, 3, MEDIUM_WOOD, 4))
	row.add_child(portrait)
	var animal_icon := FarmIconScript.new()
	animal_icon.icon_name = "leaf"
	animal_icon.icon_color = GREEN
	animal_icon.accent_color = GOLD
	animal_icon.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT, Control.PRESET_MODE_MINSIZE, 28)
	portrait.add_child(animal_icon)
	var status := Label.new()
	status.text = station.prompt_text().replace("E  ", "")
	status.custom_minimum_size = Vector2(380, 70)
	status.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	status.add_theme_font_size_override("font_size", 21)
	status.add_theme_color_override("font_color", DARK_WOOD)
	row.add_child(status)
	var action_text := "FEED / COLLECT"
	var action := _make_icon_button("bag", action_text, Vector2(260, 62), 17)
	action.pressed.connect(func():
		if is_instance_valid(active_station):
			active_station.perform_primary_action()
		_close_sheet()
	)
	row.add_child(action)

func _open_expansion_popup(id: String, station: Node) -> void:
	var index := id.to_int()
	var info: Dictionary = FarmData.expansions[index]
	var content := _create_modal("Expand the Farm", Vector2(590, 400))
	active_station = station
	var icon := FarmIconScript.new()
	icon.icon_name = "map"
	icon.icon_color = GREEN
	icon.accent_color = GOLD
	icon.custom_minimum_size = Vector2(110, 110)
	content.add_child(icon)
	var name_label := Label.new()
	name_label.text = str(info.name)
	name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_label.add_theme_font_size_override("font_size", 25)
	content.add_child(name_label)
	var requirements := Label.new()
	requirements.text = "Unlock a new place for your growing village.\nRequired: Level %d   •   %d coins" % [int(info.level), int(info.cost)]
	requirements.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	requirements.add_theme_color_override("font_color", TEXT_SECONDARY)
	content.add_child(requirements)
	var buy := _make_icon_button("coin", "UNLOCK", Vector2(260, 60), 19)
	buy.disabled = not UnlockSystem.can_purchase_expansion(index)
	buy.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	buy.pressed.connect(func():
		if is_instance_valid(active_station):
			active_station.perform_primary_action()
		_close_overlay()
	)
	content.add_child(buy)

func _open_inventory() -> void:
	var content := _create_modal("Barn Inventory", Vector2(940, 590))
	var tabs := HBoxContainer.new()
	tabs.alignment = BoxContainer.ALIGNMENT_CENTER
	tabs.add_theme_constant_override("separation", 8)
	content.add_child(tabs)
	for spec in [["rice", "Crops"], ["bag", "Animal"], ["orders", "Food"], ["build", "Materials"], ["gem", "Special"]]:
		var tab := _make_icon_button(str(spec[0]), str(spec[1]), Vector2(150, 44), 13)
		tabs.add_child(tab)
	var scroll := ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	content.add_child(scroll)
	var grid := GridContainer.new()
	grid.columns = 5
	grid.add_theme_constant_override("h_separation", 14)
	grid.add_theme_constant_override("v_separation", 14)
	scroll.add_child(grid)
	for item_id in ["rice", "corn", "tomato", "sugarcane", "egg", "milk", "animal_feed", "bread", "butter", "palm_sugar"]:
		grid.add_child(_inventory_card(item_id))
	var capacity_row := HBoxContainer.new()
	content.add_child(capacity_row)
	var capacity_label := Label.new()
	var used := 0
	for value in GameState.data.inventory.values():
		used += int(value)
	capacity_label.text = "BARN STORAGE   %d / 75" % used
	capacity_label.custom_minimum_size.x = 210
	capacity_row.add_child(capacity_label)
	var capacity := ProgressBar.new()
	capacity.max_value = 75
	capacity.value = used
	capacity.show_percentage = false
	capacity.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	capacity_row.add_child(capacity)

func _inventory_card(item_id: String) -> PanelContainer:
	var card := PanelContainer.new()
	card.custom_minimum_size = Vector2(160, 150)
	card.add_theme_stylebox_override("panel", _style(CREAM_LIGHT, 18, 3, Color("#B88857"), 4))
	var box := VBoxContainer.new()
	box.alignment = BoxContainer.ALIGNMENT_CENTER
	card.add_child(box)
	var icon := FarmIconScript.new()
	icon.icon_name = item_id if item_id in ["rice", "corn", "tomato", "sugarcane"] else "bag"
	icon.icon_color = GREEN
	icon.accent_color = GOLD
	icon.custom_minimum_size = Vector2(62, 62)
	box.add_child(icon)
	var label := Label.new()
	label.text = item_id.capitalize()
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.add_theme_color_override("font_color", TEXT_DARK)
	box.add_child(label)
	var badge := Label.new()
	badge.text = "× %d" % int(GameState.data.inventory.get(item_id, 0))
	badge.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	badge.add_theme_font_size_override("font_size", 16)
	badge.add_theme_color_override("font_color", TERRACOTTA)
	box.add_child(badge)
	return card

func _open_shop() -> void:
	var content := _create_modal("Village Shop", Vector2(960, 590))
	var tabs := HBoxContainer.new()
	tabs.alignment = BoxContainer.ALIGNMENT_CENTER
	tabs.add_theme_constant_override("separation", 8)
	content.add_child(tabs)
	for spec in [["home", "Buildings"], ["leaf", "Animals"], ["rice", "Fields"], ["gem", "Decorations"], ["map", "Roads"]]:
		tabs.add_child(_make_icon_button(str(spec[0]), str(spec[1]), Vector2(160, 46), 13))
	var scroll := ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	content.add_child(scroll)
	var grid := GridContainer.new()
	grid.columns = 4
	grid.add_theme_constant_override("h_separation", 16)
	grid.add_theme_constant_override("v_separation", 16)
	scroll.add_child(grid)
	var offers := [
		["Chicken Coop", "home", 180, 2], ["Rice Field", "rice", 80, 1],
		["Banana Grove", "leaf", 260, 3], ["Lotus Lantern", "gem", 95, 2],
		["Cow Shelter", "home", 520, 4], ["Stone Path", "map", 45, 2],
		["Bakery", "orders", 760, 5], ["Palm Garden", "leaf", 310, 3]
	]
	for offer in offers:
		grid.add_child(_shop_card(str(offer[0]), str(offer[1]), int(offer[2]), int(offer[3])))

func _shop_card(title: String, icon_name: String, price: int, required_level: int) -> Button:
	var card := Button.new()
	card.custom_minimum_size = Vector2(205, 190)
	card.add_theme_stylebox_override("normal", _style(CREAM_LIGHT, 20, 3, MEDIUM_WOOD, 5))
	card.add_theme_stylebox_override("hover", _style(Color("#FFF5C9"), 20, 4, GOLD, 8))
	card.add_theme_color_override("font_color", TEXT_DARK)
	card.disabled = int(GameState.data.level) < required_level
	var icon := FarmIconScript.new()
	icon.icon_name = icon_name
	icon.icon_color = GREEN
	icon.accent_color = GOLD
	icon.position = Vector2(65, 16)
	icon.size = Vector2(74, 74)
	card.add_child(icon)
	var name_label := Label.new()
	name_label.text = title
	name_label.position = Vector2(10, 94)
	name_label.size = Vector2(185, 29)
	name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_label.add_theme_font_size_override("font_size", 18)
	name_label.add_theme_color_override("font_color", TEXT_DARK)
	card.add_child(name_label)
	var price_label := Label.new()
	price_label.text = "%d coins" % price if not card.disabled else "Unlocks at level %d" % required_level
	price_label.position = Vector2(10, 128)
	price_label.size = Vector2(185, 28)
	price_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	price_label.add_theme_color_override("font_color", TERRACOTTA if not card.disabled else TEXT_SECONDARY)
	card.add_child(price_label)
	card.pressed.connect(func():
		_close_overlay()
		_show_toast("%s selected. Tap open land to place it." % title)
		_open_build()
	)
	_polish_button(card)
	return card

func _open_orders() -> void:
	var content := _create_modal("Delivery Orders", Vector2(900, 560))
	var board := PanelContainer.new()
	board.size_flags_vertical = Control.SIZE_EXPAND_FILL
	board.add_theme_stylebox_override("panel", _style(Color("#9D6A42"), 18, 4, DARK_WOOD, 5))
	content.add_child(board)
	var cards := HBoxContainer.new()
	cards.alignment = BoxContainer.ALIGNMENT_CENTER
	cards.add_theme_constant_override("separation", 18)
	board.add_child(cards)
	var orders := [
		["Village Breakfast", {"rice": 3, "egg": 1}, 64, 18],
		["Market Basket", {"corn": 2, "tomato": 2}, 92, 24],
		["Bakery Delivery", {"bread": 1, "milk": 1}, 115, 30]
	]
	for order in orders:
		cards.add_child(_order_card(str(order[0]), order[1], int(order[2]), int(order[3])))
	var sell_row := HBoxContainer.new()
	sell_row.alignment = BoxContainer.ALIGNMENT_CENTER
	content.add_child(sell_row)
	var market_button := _make_icon_button("coin", "SELL AVAILABLE GOODS", Vector2(310, 58), 16)
	market_button.pressed.connect(_sell_at_market)
	sell_row.add_child(market_button)

func _order_card(title: String, requirements: Dictionary, coins: int, xp: int) -> PanelContainer:
	var ready := true
	for item_id in requirements:
		if int(GameState.data.inventory.get(item_id, 0)) < int(requirements[item_id]):
			ready = false
	var card := PanelContainer.new()
	card.custom_minimum_size = Vector2(245, 310)
	card.rotation_degrees = randf_range(-1.2, 1.2)
	card.add_theme_stylebox_override("panel", _style(Color("#FFF8E5"), 12, 2, GREEN if ready else Color("#B69A77"), 5))
	var box := VBoxContainer.new()
	box.add_theme_constant_override("separation", 10)
	card.add_child(box)
	var pin := FarmIconScript.new()
	pin.icon_name = "quest"
	pin.icon_color = TERRACOTTA
	pin.accent_color = GOLD
	pin.custom_minimum_size = Vector2(42, 42)
	box.add_child(pin)
	var name_label := Label.new()
	name_label.text = title
	name_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	name_label.add_theme_font_size_override("font_size", 20)
	box.add_child(name_label)
	for item_id in requirements:
		var have := int(GameState.data.inventory.get(item_id, 0))
		var needed := int(requirements[item_id])
		var line := Label.new()
		line.text = "%s     %d / %d" % [str(item_id).capitalize(), have, needed]
		line.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		line.add_theme_color_override("font_color", GREEN if have >= needed else TEXT_SECONDARY)
		box.add_child(line)
	box.add_child(HSeparator.new())
	var reward := Label.new()
	reward.text = "%d coins   •   %d XP" % [coins, xp]
	reward.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	reward.add_theme_color_override("font_color", TERRACOTTA)
	box.add_child(reward)
	var action := Button.new()
	action.text = "DELIVER" if ready else "COLLECT ITEMS"
	action.disabled = not ready
	action.pressed.connect(func(): _complete_order(requirements, coins, xp, title))
	box.add_child(action)
	return card

func _complete_order(requirements: Dictionary, coins: int, xp: int, title: String) -> void:
	if GameState.take_items(requirements):
		GameState.earn(coins)
		GameState.add_xp(xp)
		_close_overlay()
		_show_toast("%s delivered! +%d coins" % [title, coins])

func _sell_at_market() -> void:
	if not is_instance_valid(world):
		return
	var market = world.get_station("Market")
	if market != null:
		market.interact(world.player)
	_close_overlay()

func _open_build() -> void:
	var content := _create_sheet("Build & Edit", 230)
	var hint := Label.new()
	hint.text = "Tap a building or an open area to edit your farm. Controls only appear while editing."
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	hint.add_theme_color_override("font_color", TEXT_SECONDARY)
	content.add_child(hint)
	var row := HBoxContainer.new()
	row.alignment = BoxContainer.ALIGNMENT_CENTER
	row.add_theme_constant_override("separation", 14)
	content.add_child(row)
	for spec in [["build", "MOVE"], ["settings", "ROTATE"], ["bag", "STORE"], ["play", "CONFIRM"]]:
		var button := _make_icon_button(str(spec[0]), str(spec[1]), Vector2(190, 58), 14)
		button.pressed.connect(func(): _show_toast("Select an object in the farm first."))
		row.add_child(button)
	var cancel := Button.new()
	cancel.text = "CANCEL"
	cancel.custom_minimum_size = Vector2(150, 58)
	cancel.add_theme_stylebox_override("normal", _style(TERRACOTTA, 16, 2, Color("#97452E"), 4))
	cancel.pressed.connect(_close_sheet)
	row.add_child(cancel)

func _open_map() -> void:
	var content := _create_modal("Farm Map", Vector2(820, 550))
	var map_surface := PanelContainer.new()
	map_surface.size_flags_vertical = Control.SIZE_EXPAND_FILL
	map_surface.add_theme_stylebox_override("panel", _style(Color("#E5C98C"), 20, 3, MEDIUM_WOOD, 3))
	content.add_child(map_surface)
	var grid := GridContainer.new()
	grid.columns = 3
	grid.add_theme_constant_override("h_separation", 16)
	grid.add_theme_constant_override("v_separation", 16)
	map_surface.add_child(grid)
	for district in [["home", "Starter Farm", "Home, barn and fields"], ["leaf", "Animal Meadow", "Coops and pasture"], ["orders", "Production Village", "Mills and workshops"], ["shop", "Village Market", "Orders and trade"], ["map", "River Orchard", "Fruit and fishing"], ["quest", "Festival Grounds", "Seasonal events"]]:
		var card := PanelContainer.new()
		card.custom_minimum_size = Vector2(230, 150)
		card.add_theme_stylebox_override("panel", _style(CREAM_LIGHT, 18, 2, Color("#B68755"), 3))
		grid.add_child(card)
		var box := VBoxContainer.new()
		box.alignment = BoxContainer.ALIGNMENT_CENTER
		card.add_child(box)
		var icon := FarmIconScript.new()
		icon.icon_name = str(district[0])
		icon.icon_color = GREEN
		icon.accent_color = GOLD
		icon.custom_minimum_size = Vector2(55, 55)
		box.add_child(icon)
		var title := Label.new()
		title.text = str(district[1])
		title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		title.add_theme_font_size_override("font_size", 18)
		box.add_child(title)
		var detail := Label.new()
		detail.text = str(district[2])
		detail.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		detail.add_theme_font_size_override("font_size", 13)
		detail.add_theme_color_override("font_color", TEXT_SECONDARY)
		box.add_child(detail)

func _open_quest_book() -> void:
	var content := _create_modal("Quest Book", Vector2(780, 520))
	var quest := PanelContainer.new()
	quest.add_theme_stylebox_override("panel", _style(CREAM_LIGHT, 18, 3, MEDIUM_WOOD, 4))
	content.add_child(quest)
	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 20)
	quest.add_child(row)
	var icon := FarmIconScript.new()
	icon.icon_name = "quest"
	icon.icon_color = TERRACOTTA
	icon.accent_color = GOLD
	icon.custom_minimum_size = Vector2(80, 80)
	row.add_child(icon)
	var text_box := VBoxContainer.new()
	text_box.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(text_box)
	var title := Label.new()
	title.text = _quest_title()
	title.add_theme_font_size_override("font_size", 24)
	text_box.add_child(title)
	var body := Label.new()
	body.text = _quest_detail()
	body.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	body.add_theme_color_override("font_color", TEXT_SECONDARY)
	text_box.add_child(body)
	var progress := ProgressBar.new()
	progress.max_value = 4
	progress.value = mini(4, int(GameState.data.tutorial_step))
	progress.show_percentage = false
	text_box.add_child(progress)
	var reward := Label.new()
	reward.text = "REWARD   50 coins   •   25 XP"
	reward.add_theme_color_override("font_color", TERRACOTTA)
	text_box.add_child(reward)
	content.add_child(HSeparator.new())
	var daily := Label.new()
	daily.text = "DAILY GOALS\nHarvest 10 crops     0 / 10\nComplete 2 orders    0 / 2\nFeed every animal    0 / 2"
	daily.add_theme_font_size_override("font_size", 17)
	daily.add_theme_color_override("font_color", TEXT_DARK)
	content.add_child(daily)

func _open_settings() -> void:
	var content := _create_modal("Settings", Vector2(760, 570), not menu_mode)
	var tabs := HBoxContainer.new()
	tabs.alignment = BoxContainer.ALIGNMENT_CENTER
	for name in ["AUDIO", "GAMEPLAY", "GRAPHICS", "LANGUAGE", "ACCOUNT"]:
		var tab := Button.new()
		tab.text = name
		tab.custom_minimum_size = Vector2(132, 42)
		tabs.add_child(tab)
	content.add_child(tabs)
	content.add_child(_settings_slider("Music", 72))
	content.add_child(_settings_slider("Sound Effects", 86))
	content.add_child(_settings_toggle("Vibration", true))
	content.add_child(_settings_toggle("Camera Animation", true))
	content.add_child(_settings_toggle("Soft Shadows", true))
	var language_row := HBoxContainer.new()
	var language_label := Label.new()
	language_label.text = "Language"
	language_label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	language_row.add_child(language_label)
	var language := OptionButton.new()
	for option in ["English", "ភាសាខ្មែរ", "中文"]:
		language.add_item(option)
	language.custom_minimum_size = Vector2(220, 46)
	language_row.add_child(language)
	content.add_child(language_row)

func _settings_slider(label_text: String, value: float) -> HBoxContainer:
	var row := HBoxContainer.new()
	row.custom_minimum_size.y = 48
	var label := Label.new()
	label.text = label_text
	label.custom_minimum_size.x = 210
	row.add_child(label)
	var slider := HSlider.new()
	slider.value = value
	slider.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(slider)
	return row

func _settings_toggle(label_text: String, enabled: bool) -> HBoxContainer:
	var row := HBoxContainer.new()
	row.custom_minimum_size.y = 48
	var label := Label.new()
	label.text = label_text
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(label)
	var toggle := CheckButton.new()
	toggle.button_pressed = enabled
	toggle.add_theme_color_override("font_color", GREEN)
	row.add_child(toggle)
	return row

func _open_language() -> void:
	var content := _create_modal("Language", Vector2(520, 390))
	var detail := Label.new()
	detail.text = "Choose the language used by menus, quests and dialogue.\nWorld assets never contain baked text."
	detail.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	detail.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	detail.add_theme_color_override("font_color", TEXT_SECONDARY)
	content.add_child(detail)
	for option in ["English", "ភាសាខ្មែរ", "中文"]:
		var button := Button.new()
		button.text = option
		button.custom_minimum_size = Vector2(300, 52)
		button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		button.pressed.connect(func():
			_show_toast("Language selected: %s" % option)
			_close_overlay()
		)
		content.add_child(button)

func _open_profile() -> void:
	var content := _create_modal("Farm Profile", Vector2(590, 460))
	var icon := FarmIconScript.new()
	icon.icon_name = "profile"
	icon.icon_color = GREEN
	icon.accent_color = GOLD
	icon.custom_minimum_size = Vector2(100, 100)
	content.add_child(icon)
	var summary := Label.new()
	summary.text = "PHUM FARM\nLevel %d   •   Day %d\nTotal earned: %d coins" % [int(GameState.data.level), int(GameState.data.day), int(GameState.data.total_earned)]
	summary.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	summary.add_theme_font_size_override("font_size", 19)
	content.add_child(summary)
	var save_action := Button.new()
	save_action.text = "SAVE MANAGEMENT"
	save_action.custom_minimum_size.y = 52
	content.add_child(save_action)
	var new_game := Button.new()
	new_game.text = "START A NEW FARM"
	new_game.custom_minimum_size.y = 52
	new_game.add_theme_stylebox_override("normal", _style(TERRACOTTA, 16, 2, Color("#94432C"), 4))
	new_game.pressed.connect(func():
		_close_overlay()
		_start_game(false)
	)
	content.add_child(new_game)

func _open_pause() -> void:
	var content := _create_modal("Farm Menu", Vector2(520, 540), true)
	for spec in [["play", "RESUME", _close_overlay], ["settings", "SETTINGS", _open_settings], ["quest", "HELP", _open_help], ["profile", "SAVE & ACCOUNT", _save_and_profile], ["home", "SAVE AND MAIN MENU", _save_and_leave]]:
		var button := _make_icon_button(str(spec[0]), str(spec[1]), Vector2(350, 58), 17)
		button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
		button.pressed.connect(spec[2])
		content.add_child(button)

func _open_help() -> void:
	var content := _create_modal("How to Play", Vector2(680, 500), true)
	var guide := Label.new()
	guide.text = "Tap the ground to walk. Tap a field to till, plant or harvest.\n\nTap workshops and animals for contextual actions. Use the bottom navigation for Shop, Build, Orders, Bag and Map.\n\nKeyboard: WASD or arrows to move, E/Space to interact, Q/R to rotate, mouse wheel to zoom."
	guide.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	guide.add_theme_font_size_override("font_size", 19)
	guide.add_theme_color_override("font_color", TEXT_DARK)
	content.add_child(guide)

func _save_and_profile() -> void:
	GameState.save_game()
	_close_overlay()
	_open_profile()

func _save_and_leave() -> void:
	GameState.save_game()
	get_tree().paused = false
	show_main_menu()

func _toggle_weather() -> void:
	GameState.data.weather = "clear" if str(GameState.data.weather) == "rain" else "rain"
	GameState.changed.emit()
	_show_toast("Monsoon rain" if str(GameState.data.weather) == "rain" else "Clear skies")

func _refresh_hud() -> void:
	if coin_label == null:
		return
	coin_label.text = str(int(GameState.data.coins))
	gem_label.text = str(int(GameState.data.get("gems", 0)))
	level_label.text = "LEVEL %d" % int(GameState.data.level)
	var level := int(GameState.data.level)
	var previous_xp := int(FarmData.levels[level - 1]) if level - 1 < FarmData.levels.size() else 0
	var next_xp := FarmData.xp_for_next_level(level)
	xp_bar.min_value = previous_xp
	xp_bar.max_value = max(previous_xp + 1, next_xp)
	xp_bar.value = int(GameState.data.xp)
	var caption = xp_bar.get_parent().get_node_or_null("XpCaption")
	if caption != null:
		caption.text = "%d / %d XP" % [int(GameState.data.xp), next_xp]
	if quest_label != null:
		quest_label.text = "%s\n%s" % [_quest_title(), _quest_progress()]

func _quest_title() -> String:
	return "First Harvest" if int(GameState.data.tutorial_step) < 3 else ("Market Day" if int(GameState.data.tutorial_step) == 3 else "Grow the Farm")

func _quest_progress() -> String:
	var step := int(GameState.data.tutorial_step)
	if step == 0: return "Till a farm plot  •  0/1"
	if step == 1: return "Plant rice  •  0/1"
	if step == 2: return "Harvest rice  •  0/1"
	if step == 3: return "Sell your goods  •  0/1"
	return "Unlock new land  •  0/3"

func _quest_detail() -> String:
	var step := int(GameState.data.tutorial_step)
	if step == 0: return "Prepare the soil on one of the four starter fields."
	if step == 1: return "Tap the tilled field and choose Rice from the crop tray."
	if step == 2: return "Let the rice grow, then tap the field to harvest it."
	if step == 3: return "Open Orders or visit the village market to sell the harvest."
	return "Produce goods, care for animals, and expand Phum Farm."

func _set_prompt(text: String) -> void:
	if prompt_panel == null or active_sheet != null or active_overlay != null:
		return
	prompt_label.text = text.replace("E  ", "")
	prompt_panel.visible = not text.is_empty()

func _show_toast(text: String) -> void:
	if toast_panel == null:
		return
	toast_label.text = text
	toast_panel.visible = true
	toast_panel.modulate.a = 1.0
	toast_panel.scale = Vector2(0.88, 0.88)
	toast_panel.pivot_offset = toast_panel.size * 0.5
	if toast_tween != null and toast_tween.is_valid():
		toast_tween.kill()
	toast_tween = create_tween()
	toast_tween.tween_property(toast_panel, "scale", Vector2.ONE, 0.18).set_trans(Tween.TRANS_BACK)
	toast_tween.tween_interval(2.2)
	toast_tween.tween_property(toast_panel, "modulate:a", 0.0, 0.35)
	toast_tween.tween_callback(func(): toast_panel.visible = false)

func _show_level_up(new_level: int) -> void:
	if menu_mode or interface == null:
		return
	var content := _create_modal("Level Up!", Vector2(720, 520), true)
	var medallion := PanelContainer.new()
	medallion.custom_minimum_size = Vector2(130, 130)
	medallion.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	medallion.add_theme_stylebox_override("panel", _style(GOLD, 65, 6, CREAM_LIGHT, 10))
	content.add_child(medallion)
	var number := Label.new()
	number.text = str(new_level)
	number.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	number.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	number.add_theme_font_size_override("font_size", 61)
	number.add_theme_color_override("font_color", DARK_WOOD)
	medallion.add_child(number)
	var congratulations := Label.new()
	congratulations.text = "Your village is growing!"
	congratulations.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	congratulations.add_theme_font_size_override("font_size", 24)
	content.add_child(congratulations)
	var unlock_row := HBoxContainer.new()
	unlock_row.alignment = BoxContainer.ALIGNMENT_CENTER
	unlock_row.add_theme_constant_override("separation", 12)
	content.add_child(unlock_row)
	for unlock in [["rice", "New crop"], ["home", "New building"], ["leaf", "New animal"], ["coin", "50 coins"]]:
		var card := PanelContainer.new()
		card.custom_minimum_size = Vector2(145, 105)
		card.add_theme_stylebox_override("panel", _style(CREAM_LIGHT, 16, 2, MEDIUM_WOOD, 3))
		unlock_row.add_child(card)
		var box := VBoxContainer.new()
		box.alignment = BoxContainer.ALIGNMENT_CENTER
		card.add_child(box)
		var icon := FarmIconScript.new()
		icon.icon_name = str(unlock[0])
		icon.icon_color = GREEN
		icon.accent_color = GOLD
		icon.custom_minimum_size = Vector2(48, 48)
		box.add_child(icon)
		var label := Label.new()
		label.text = str(unlock[1])
		label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		label.add_theme_font_size_override("font_size", 13)
		box.add_child(label)
	var continue_button := _make_icon_button("play", "CONTINUE", Vector2(280, 58), 19)
	continue_button.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	continue_button.pressed.connect(_close_overlay)
	content.add_child(continue_button)

func _format_duration(value: float) -> String:
	var seconds := int(value)
	return "%d:%02d" % [seconds / 60, seconds % 60]
