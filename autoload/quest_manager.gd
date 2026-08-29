extends Node

signal quest_progressed(quest_id: String, progress: int)

var progress: Dictionary = {}

func advance(quest_id: String, amount: int = 1) -> void:
	progress[quest_id] = int(progress.get(quest_id, 0)) + amount
	quest_progressed.emit(quest_id, int(progress[quest_id]))
