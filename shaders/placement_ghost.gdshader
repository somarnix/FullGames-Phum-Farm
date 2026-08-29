shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_opaque;

uniform vec4 valid_color : source_color = vec4(0.35, 0.75, 0.3, 0.5);

void fragment() {
	ALBEDO = valid_color.rgb;
	ALPHA = valid_color.a;
}
