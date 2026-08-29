extends Node

signal order_completed(order_id: String)

func complete(order_id: String) -> void:
	order_completed.emit(order_id)
