shader_type spatial;

#include "res://addons/ShaderFunction-Extras/Noise/noise.gdshaderinc"

uniform float noise_scale = 1.0;
uniform vec2 noise_period = vec2(0.0, 0.0);
uniform float height_scale = 2.5;
uniform vec4 albedo_color: source_color = vec4(0.1, 0.6, 0.2, 1.0);
uniform float curvature;
uniform float curvature_distance;

varying float n_height;

void vertex() {
    vec2 v = noise_scale * VERTEX.xz;
    vec2 g;
	float n = 0.5 + 0.5 * psrdnoise2_with_gradient(v, noise_period, 0.0, g);
	n = n * height_scale;
 	n_height = n;
	
	NORMAL = (MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz;
	float dist = length(INV_VIEW_MATRIX[3].xyz - VERTEX) / curvature_distance;
    VERTEX.y = n - pow(dist, curvature);
}

void fragment() {
	ALBEDO = albedo_color.rgb * vec3(n_height / 30.0);
}
