Write-Host "Compiling General shaders..."
glslangValidator -S vert -V ./General/vertexShader.glsl -o ./General/vertexShader.spv
glslangValidator -S frag -V ./General/fragmentShader.glsl   -o ./General/fragmentShader.spv

Write-Host "Compiling Relement shaders..."
glslangValidator -S vert -V ./Relement/vertexShader.glsl -o ./Relement/vertexShader.spv
glslangValidator -S frag -V ./Relement/fragmentShader.glsl   -o ./Relement/fragmentShader.spv

Write-Host "Compiling ReToonRigid shaders..."
glslangValidator -S vert -V ./ReToonRigid/vertexShader.glsl -o ./ReToonRigid/vertexShader.spv
glslangValidator -S frag -V ./ReToonRigid/fragmentShader.glsl   -o ./ReToonRigid/fragmentShader.spv

Write-Host "Compiling Framebuffer shaders..."
glslangValidator -S vert -V ./Framebuffer/vertexShader.glsl -o ./Framebuffer/vertexShader.spv
glslangValidator -S frag -V ./Framebuffer/fragmentShader.glsl   -o ./Framebuffer/fragmentShader.spv
