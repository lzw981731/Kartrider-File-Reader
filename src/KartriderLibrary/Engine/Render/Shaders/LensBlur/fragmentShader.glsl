#version 450 core

layout(location = 0) in FragInfo{
	vec2 _FragTexCoord;
};

layout(location = 0) out vec4 fsout_color;

layout(set = 0, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 0, binding = 1) uniform sampler SurfaceSampler;
layout(set = 0, binding = 2) uniform sampler2DArrayShadow DepthTexture;


void main() {
	vec4 texColor = texture(sampler2D(SurfaceTexture, SurfaceSampler), _FragTexCoord);
	fsout_color = texColor;
}