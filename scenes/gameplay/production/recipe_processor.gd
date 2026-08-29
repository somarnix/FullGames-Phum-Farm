extends RefCounted

static func can_craft(recipe: Dictionary, inventory: Dictionary) -> bool:
	for item_id in recipe.get("inputs", {}):
		if int(inventory.get(item_id, 0)) < int(recipe.inputs[item_id]):
			return false
	return true
