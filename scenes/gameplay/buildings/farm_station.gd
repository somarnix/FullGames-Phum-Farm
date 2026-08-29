extends StaticBody3D

signal interface_requested(type: String, id: String, station: Node)

var station_type := ""
var station_id := ""
var display_name := ""
var unlock_level := 1

func setup(type: String, id: String, title: String, level: int = 1) -> void:
	station_type = type
	station_id = id
	display_name = title
	unlock_level = level
	name = id.to_pascal_case()
	add_to_group("interactable")
	var collider := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(3.0, 2.5, 3.0)
	collider.shape = shape
	collider.position.y = 1.25
	add_child(collider)

func prompt_text() -> String:
	if not UnlockSystem.is_level_unlocked(unlock_level):
		return "%s unlocks at level %d" % [display_name, unlock_level]
	match station_type:
		"market":
			return "E  Sell all farm goods"
		"machine":
			return _machine_prompt()
		"chickens":
			return _animal_prompt("chicken")
		"cows":
			return _animal_prompt("cow")
		"expansion":
			return _expansion_prompt()
		"home":
			return "E  Save and rest until morning"
		"well":
			return "E  Bless the fields (+small growth boost)"
	return "E  Use %s" % display_name

func interact(_player: Node) -> void:
	if not UnlockSystem.is_level_unlocked(unlock_level):
		GameState.message_requested.emit(prompt_text())
		return
	match station_type:
		"market": _sell_goods()
		"machine":
			var machine: Dictionary = GameState.data.machines.get(station_id, {})
			if not machine.is_empty() and GameState.game_time() >= float(machine.get("ready_at", 0.0)):
				_use_machine()
			else:
				interface_requested.emit("production", station_id, self)
		"chickens", "cows": interface_requested.emit("animal", station_type, self)
		"expansion": interface_requested.emit("expansion", station_id, self)
		"home": _rest()
		"well": _boost_fields()

func _machine_prompt() -> String:
	var product := FarmData.product(station_id)
	var machine: Dictionary = GameState.data.machines.get(station_id, {})
	if not machine.is_empty():
		if GameState.game_time() >= float(machine.get("ready_at", 0.0)):
			return "E  Collect %s" % str(product.get("name", display_name))
		return "%s working - %d%%" % [display_name, int(_machine_ratio(machine, product) * 100.0)]
	var ingredients: Array[String] = []
	for id in product.get("inputs", {}):
		ingredients.append("%d %s" % [int(product.inputs[id]), str(id).capitalize()])
	return "E  Make %s (%s)" % [str(product.get("name", display_name)), ", ".join(ingredients)]

func _use_machine() -> void:
	var product := FarmData.product(station_id)
	var machine: Dictionary = GameState.data.machines.get(station_id, {})
	if not machine.is_empty():
		if GameState.game_time() < float(machine.get("ready_at", 0.0)):
			GameState.message_requested.emit(_machine_prompt())
			return
		GameState.data.machines.erase(station_id)
		GameState.add_item(station_id, 1)
		GameState.add_xp(int(product.get("xp", 1)))
		GameState.message_requested.emit("Collected %s" % str(product.get("name", display_name)))
		return
	var inputs: Dictionary = product.get("inputs", {})
	if not GameState.take_items(inputs):
		GameState.message_requested.emit("Missing ingredients for %s" % str(product.get("name", display_name)))
		return
	GameState.data.machines[station_id] = {"started_at": GameState.game_time(), "ready_at": GameState.game_time() + float(product.get("seconds", 20.0))}
	GameState.message_requested.emit("Production started")
	GameState.changed.emit()

func perform_primary_action() -> void:
	match station_type:
		"machine": _use_machine()
		"chickens": _use_animal("chicken", "egg", 2)
		"cows": _use_animal("cow", "milk", 2)
		"expansion": _buy_expansion()

func _machine_ratio(machine: Dictionary, product: Dictionary) -> float:
	var duration := maxf(float(product.get("seconds", 20.0)), 1.0)
	return clampf((GameState.game_time() - float(machine.get("started_at", 0.0))) / duration, 0.0, 1.0)

func _animal_prompt(kind: String) -> String:
	var plural := "chickens" if kind == "chicken" else "cows"
	var ready_key := "%s_ready_at" % kind
	var fed_key := "%s_fed" % plural
	if bool(GameState.data.animals.get(fed_key, false)):
		if GameState.game_time() >= float(GameState.data.animals.get(ready_key, 0.0)):
			return "E  Collect from %s" % plural.capitalize()
		return "%s are producing" % plural.capitalize()
	return "E  Feed %s (1 Animal Feed)" % plural

func _use_animal(kind: String, reward: String, amount: int) -> void:
	var plural := "chickens" if kind == "chicken" else "cows"
	var fed_key := "%s_fed" % plural
	var ready_key := "%s_ready_at" % kind
	if bool(GameState.data.animals.get(fed_key, false)):
		if GameState.game_time() < float(GameState.data.animals.get(ready_key, 0.0)):
			GameState.message_requested.emit(_animal_prompt(kind))
			return
		GameState.data.animals[fed_key] = false
		GameState.add_item(reward, amount)
		GameState.add_xp(10 if kind == "chicken" else 16)
		GameState.message_requested.emit("Collected %d %s" % [amount, reward.capitalize()])
	elif GameState.take_items({"animal_feed": 1}):
		GameState.data.animals[fed_key] = true
		GameState.data.animals[ready_key] = GameState.game_time() + (40.0 if kind == "chicken" else 65.0)
		GameState.message_requested.emit("%s fed" % plural.capitalize())
	else:
		GameState.message_requested.emit("Make Animal Feed at the feed mill first")
	GameState.changed.emit()

func _sell_goods() -> void:
	var total := 0
	for id in GameState.data.inventory.keys():
		var count := int(GameState.data.inventory[id])
		if count <= 0:
			continue
		var price := 0
		if FarmData.crops.has(id):
			price = int(FarmData.crop(id).get("sell", 0))
		elif FarmData.products.has(id):
			price = int(FarmData.product(id).get("sell", 0))
		elif id == "egg":
			price = 13
		elif id == "milk":
			price = 18
		if price > 0:
			total += price * count
			GameState.data.inventory[id] = 0
	if total <= 0:
		GameState.message_requested.emit("No goods to sell yet")
		return
	GameState.earn(total)
	GameState.add_xp(maxi(1, total / 12))
	GameState.data.tutorial_step = maxi(int(GameState.data.tutorial_step), 4)
	GameState.message_requested.emit("Market sale: %d coins" % total)
	GameState.save_game()

func _expansion_prompt() -> String:
	var index := station_id.to_int()
	if index >= FarmData.expansions.size():
		return "Expansion land"
	var info: Dictionary = FarmData.expansions[index]
	if bool(GameState.data.expansions[index]):
		return "%s is unlocked" % str(info.name)
	return "E  Unlock %s (%d coins, level %d)" % [str(info.name), int(info.cost), int(info.level)]

func _buy_expansion() -> void:
	var index := station_id.to_int()
	if index >= FarmData.expansions.size() or bool(GameState.data.expansions[index]):
		GameState.message_requested.emit(_expansion_prompt())
		return
	var info: Dictionary = FarmData.expansions[index]
	if not UnlockSystem.is_level_unlocked(int(info.level)):
		GameState.message_requested.emit("Reach level %d first" % int(info.level))
		return
	if GameState.spend(int(info.cost)):
		GameState.data.expansions[index] = true
		GameState.add_xp(35 + index * 20)
		GameState.message_requested.emit("%s unlocked" % str(info.name))
		GameState.save_game()

func _rest() -> void:
	GameState.data.day = int(GameState.data.day) + 1
	GameState.data.minutes = 420.0
	GameState.save_game()
	GameState.message_requested.emit("A new morning begins")

func _boost_fields() -> void:
	for plot in GameState.data.plots:
		if str(plot.get("state", "")) == "growing":
			plot.planted_at = float(plot.planted_at) - 8.0
	GameState.changed.emit()
	GameState.message_requested.emit("Fresh water helped every crop grow")
