shader_type spatial;
render_mode diffuse_toon, specular_toon;

uniform sampler2D albedo;

varying vec2 tex_position;

void vertex() {
  VERTEX.y = texture(albedo, UV).x * 5.0;
}

void fragment(){
	float fresnel = sqrt(1.0 - dot(NORMAL, VIEW));
	//RIM = 0.2;
	//METALLIC = 0.0;
	//ROUGHNESS = 0.01 * (1.0 - fresnel);
	ALBEDO = texture(albedo, UV).xyz + vec3(0.1, 0.3, 0.5) + (0.1 * fresnel);
	//ALBEDO = vec3(0.1, 0.3, 0.5) + (0.1 * fresnel);
	NORMAL = texture(albedo, UV).xyz;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
