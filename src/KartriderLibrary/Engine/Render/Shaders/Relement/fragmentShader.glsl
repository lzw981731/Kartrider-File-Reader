#version 450 core

#define D3DCMP_NEVER 1
#define D3DCMP_LESS 2
#define D3DCMP_EQUAL 3
#define D3DCMP_LESSEQUAL 4
#define D3DCMP_GREATER 5
#define D3DCMP_NOTEQUAL 6
#define D3DCMP_GREATEREQUAL 7
#define D3DCMP_ALWAYS 8

layout(set = 0, binding = 1) uniform TexProperty{
	float _TexAlpha;
	float _TexOffsetX;
	float _TexOffsetY;
	int _TexMirrorY;
};

layout(set = 0, binding = 2) uniform AlphaProperty{
	int _AlphaTestEnabled;
	int _AlphaTestFunction;
	int _AlphaTestRef;
};

layout(set = 0, binding = 3) uniform texture2D SurfaceTexture;
layout(set = 0, binding = 4) uniform sampler SurfaceSampler;

layout(location = 0) in FragInfo{
	smooth vec2 _FragTexCoord;
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
	vec2 offsetTexCoord = _FragTexCoord - vec2(_TexOffsetX, _TexOffsetY);
	if(_TexMirrorY == 1){
		offsetTexCoord = vec2(offsetTexCoord.x, 1 - offsetTexCoord.y);
	}
	vec4 texColor = texture(sampler2D(SurfaceTexture, SurfaceSampler), offsetTexCoord, 0);
	texColor.a *= _TexAlpha;
	if (_AlphaTestEnabled > 0) {
		float src = (texColor.a * 255.0f);
		float floorSrc = floor(src);
		float preciousPart = src - floorSrc;
		float dst = _AlphaTestRef;
        int cmpFunc = _AlphaTestFunction;
		bool orgCmp = CompareValue(src, dst, cmpFunc);
		bool floorCmp = CompareValue(floorSrc, dst, cmpFunc);
        if(!orgCmp){
			texColor.a = 0;
            // discard;
        }
	}
	else{
	}
	fsout_color = texColor;
}