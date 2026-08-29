extends Node

signal production_started(recipe_id: String)
signal product_ready(product_id: String)

var active_recipe := ""

func start(recipe_id: String) -> void:
	active_recipe = recipe_id
	production_started.emit(recipe_id)
