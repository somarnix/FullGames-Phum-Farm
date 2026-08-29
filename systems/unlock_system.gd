extends Node

signal unlocks_changed

const FALLBACK_CROP_LEVELS := {"rice": 1, "corn": 2, "tomato": 4, "sugarcane": 6}
const DISTRICT_LEVELS := {
	"starter_farm": 1,
	"crop_district": 1,
	"animal_district": 3,
	"production_district": 5,
	"market_district": 1,
	"river_district": 8,
	"orchard_district": 8
}

func _ready() -> void:
	ProgressionSystem.level_changed.connect(_on_level_changed)
	GameState.game_loaded.connect(refresh)
	GameState.game_reset.connect(refresh)

func player_level() -> int:
	return int(GameState.data.get("level", 1))

func is_level_unlocked(required_level: int) -> bool:
	return player_level() >= maxi(1, required_level)

func crop_required_level(crop_id: String) -> int:
	var crop_data: Dictionary = FarmData.crop(crop_id)
	return int(crop_data.get("unlock_level", FALLBACK_CROP_LEVELS.get(crop_id, 1)))

func is_crop_unlocked(crop_id: String) -> bool:
	return FarmData.crops.has(crop_id) and is_level_unlocked(crop_required_level(crop_id))

func animal_required_level(animal_id: String) -> int:
	return int(FarmData.animals.get(animal_id, {}).get("unlock_level", 1))

func is_animal_unlocked(animal_id: String) -> bool:
	return FarmData.animals.has(animal_id) and is_level_unlocked(animal_required_level(animal_id))

func building_required_level(building_id: String) -> int:
	return int(FarmData.buildings.get(building_id, {}).get("unlock_level", 1))

func is_building_unlocked(building_id: String) -> bool:
	return FarmData.buildings.has(building_id) and is_level_unlocked(building_required_level(building_id))

func is_plot_unlocked(plot_index: int) -> bool:
	if plot_index < 0 or plot_index >= FarmData.plot_unlocks.size():
		return false
	return is_level_unlocked(int(FarmData.plot_unlocks[plot_index]))

func is_expansion_unlocked(expansion_index: int) -> bool:
	return expansion_index >= 0 and expansion_index < GameState.data.expansions.size() and bool(GameState.data.expansions[expansion_index])

func can_purchase_expansion(expansion_index: int) -> bool:
	if expansion_index < 0 or expansion_index >= FarmData.expansions.size() or is_expansion_unlocked(expansion_index):
		return false
	var expansion: Dictionary = FarmData.expansions[expansion_index]
	return is_level_unlocked(int(expansion.get("level", 1))) and EconomySystem.can_afford_coins(int(expansion.get("cost", 0)))

func is_district_unlocked(district_id: String) -> bool:
	return is_level_unlocked(int(DISTRICT_LEVELS.get(district_id, 1)))

func refresh() -> void:
	unlocks_changed.emit()

func _on_level_changed(_previous_level: int, _new_level: int) -> void:
	refresh()
