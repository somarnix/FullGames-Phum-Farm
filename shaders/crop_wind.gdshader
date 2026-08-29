shader_type spatial;

uniform float strength = 0.06;
uniform float speed = 1.8;

void vertex() {
	VERTEX.x += sin(TIME * speed + VERTEX.y * 3.0 + VERTEX.z) * strength * max(VERTEX.y, 0.0);
}
