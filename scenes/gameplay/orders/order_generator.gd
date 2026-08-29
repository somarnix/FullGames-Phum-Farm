extends RefCounted

static func create_order(order_id: String, requirements: Dictionary, reward: int) -> Dictionary:
	return {"id": order_id, "requirements": requirements, "reward": reward}
