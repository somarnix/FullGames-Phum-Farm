extends Control

signal category_selected(category: String)

func select_category(category: String) -> void:
	category_selected.emit(category)
