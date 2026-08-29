extends Node

signal orders_changed

var active_orders: Array[Dictionary] = []

func set_orders(orders: Array[Dictionary]) -> void:
	active_orders = orders
	orders_changed.emit()
