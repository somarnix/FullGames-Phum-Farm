shader_type spatial;

uniform float progress : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	float pattern = fract(sin(dot(UV, vec2(12.9898, 78.233))) * 43758.5453);
	if (pattern < progress) discard;
}
