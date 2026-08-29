shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back;

uniform vec4 shallow_color : source_color = vec4(0.18, 0.58, 0.65, 0.82);

void fragment() {
	ALBEDO = shallow_color.rgb;
	ALPHA = shallow_color.a;
	ROUGHNESS = 0.18;
	METALLIC = 0.12;
}
