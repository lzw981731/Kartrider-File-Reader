#version 450 core

layout(location = 0) in FragInfo{
	vec2 _FragTexCoord;
};

layout(location = 0) out vec4 fsout_color;

layout(set = 0, binding = 0) uniform texture2DMS SurfaceTexture;
layout(set = 0, binding = 1) uniform sampler SurfaceSampler;
layout(set = 0, binding = 2) uniform texture2D DepthTexture;
layout(set = 0, binding = 3) uniform samplerShadow DepthSampler;
layout(set = 0, binding = 4) uniform FramebufferInfo{
	int _SampleCount;
};

void main() {
	ivec2 texSize = textureSize(sampler2DMS(SurfaceTexture, SurfaceSampler));
	ivec2 absTexCoord = ivec2(int(texSize.x * _FragTexCoord.x), int(texSize.y * _FragTexCoord.y));
	vec4 outColor = vec4(0, 0, 0, 0);
	for(int i = 0; i < _SampleCount; i++){
		outColor += texelFetch(sampler2DMS(SurfaceTexture, SurfaceSampler), absTexCoord, i);
	}
	outColor /= _SampleCount;
	outColor = vec4(outColor.rgb * outColor.a, 1.0f);
	float depth = texture(sampler2DShadow(DepthTexture, DepthSampler), vec3(_FragTexCoord, 1.0f));
	depth = pow(depth, 100);
	
	fsout_color = vec4(outColor);
}