#version 450 core

layout(location = 0) in vec3 Position;
layout(location = 1) in vec2 TexCoord;


layout(location = 0) out FragInfo{
	vec2 _FragTexCoord;
};

void main() {
	vec4 transformedPos = vec4(Position, 1.0f);
	_FragTexCoord = TexCoord;
	gl_Position = transformedPos;
}