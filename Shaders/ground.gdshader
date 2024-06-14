shader_type spatial;
render_mode diffuse_toon;

uniform sampler2D height_map;
uniform sampler2D albedo;
uniform float height_multiplier;

varying vec2 tex_position;

void vertex() {
  VERTEX.y = texture(height_map, UV).x * height_multiplier * 5.0;
}

void fragment(){
	float height = (texture(height_map, UV).x * height_multiplier) * 0.5 - 0.60;
	float fresnel = sqrt(1.0 - dot(NORMAL, VIEW));
	//RIM = 0.2;
	//METALLIC = 0.0;
	//ROUGHNESS = 0.01 * (1.0 - fresnel);
	//float color_x = min(height + 0.22f, 0.95);
	ALBEDO = vec3((height_multiplier + 1.0)/2.0);//texture(albedo, vec2(height, 0.5f)).rgb;// + vec3(0.1, 0.3, 0.5) + (0.1 * fresnel);
	//ALBEDO = texture(height_map, UV).rgb;//vec3(0.1, 0.3, 0.5) + (0.1 * fresnel);
	NORMAL = texture(height_map, UV).xyz;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
