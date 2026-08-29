extends Node3D

@export var cell_size := 1.0

func snap_position(value: Vector3) -> Vector3:
	return Vector3(round(value.x / cell_size) * cell_size, value.y, round(value.z / cell_size) * cell_size)
