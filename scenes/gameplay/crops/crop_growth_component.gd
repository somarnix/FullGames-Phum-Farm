extends Node

@export var duration := 30.0
var elapsed := 0.0

func ratio() -> float:
	return clampf(elapsed / maxf(duration, 0.01), 0.0, 1.0)
