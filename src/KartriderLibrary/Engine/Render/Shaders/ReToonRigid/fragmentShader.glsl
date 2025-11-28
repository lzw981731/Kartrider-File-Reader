#version 450 core

#define D3DCMP_NEVER 1
#define D3DCMP_LESS 2
#define D3DCMP_EQUAL 3
#define D3DCMP_LESSEQUAL 4
#define D3DCMP_GREATER 5
#define D3DCMP_NOTEQUAL 6
#define D3DCMP_GREATEREQUAL 7
#define D3DCMP_ALWAYS 8


layout(set = 0, binding = 1) uniform AlphaProperty{
	int _AlphaTestEnabled;
	int _AlphaTestFunction;
	int _AlphaTestRef;
};

layout(set = 0, binding = 2) uniform texture2D SurfaceTexture;
layout(set = 0, binding = 3) uniform sampler SurfaceSampler;
layout(set = 0, binding = 4) uniform texture2D ColorMaskingTexture;
layout(set = 0, binding = 5) uniform sampler ColorMaskingSampler;

layout(location = 0) in FragInfo{
    vec3 _NormalVec;
	vec3 _FragTexCoord;
};

layout(location = 0) out vec4 fsout_color;

bool CompareValue(float src, float dst, int cmpFunc) {
	switch (cmpFunc) {
	case D3DCMP_NEVER:
		return false;
	case D3DCMP_LESS:
		return src < dst;
	case D3DCMP_EQUAL:
		return src == dst;
	case D3DCMP_LESSEQUAL:
		return src <= dst;
	case D3DCMP_GREATER:
		return src > dst;
	case D3DCMP_NOTEQUAL:
		return src != dst;
	case D3DCMP_GREATEREQUAL:
		return src >= dst;
	case D3DCMP_ALWAYS:
		return true;
	default:
		return false;
	}
}

void main() {
	vec3 offsetTexCoord = _FragTexCoord;
	vec4 texColor = texture(sampler2D(SurfaceTexture, SurfaceSampler), offsetTexCoord.yz);
	vec4 maskingColor = texture(sampler2D(ColorMaskingTexture, ColorMaskingSampler), offsetTexCoord.yz);
    // maskingColor = vec4(maskingColor.rgb * vec3(1.0f, 0.0f, 0.0f), maskingColor.a);
	vec3 testColor = vec3(0.125f, 0.341f, 0.506f);
    texColor = vec4(texColor.xyz * texColor.a + testColor * (1 - texColor.a) + maskingColor.xyz * maskingColor.a * 0.75f, 1.0f);

	if (_AlphaTestEnabled > 0) {
		float src = floor(texColor.a * 255.0f);
		float dst = _AlphaTestRef;
        int cmpFunc = _AlphaTestFunction;
        if(CompareValue(src, dst, cmpFunc) != true){
            discard;
        }
	}
	fsout_color = texColor;
}