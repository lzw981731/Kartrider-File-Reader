#version 450 core

layout (constant_id = 0) const bool UseTexture = false;
layout (constant_id = 1) const bool UseColorMixing = true;

layout(location = 0) in FragInfo{
	vec2 _FragTexCoord;
	vec4 _FragColor;
};

layout(location = 0) out vec4 fsout_color;

layout(set = 0, binding = 1) uniform texture2D SurfaceTexture;
layout(set = 0, binding = 2) uniform sampler SurfaceSampler;


void main() {
	vec4 color = vec4(1.0f, 1.0f, 1.0f, 1.0f);
	if(UseTexture){
		color = texture(sampler2D(SurfaceTexture, SurfaceSampler), _FragTexCoord);
	}
	if(UseColorMixing){
		color = color * _FragColor;
	}
	// color = vec4(1.0f, 0.0f, 0.0f, 1.0f);
	fsout_color = color;
}