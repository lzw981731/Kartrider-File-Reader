using Veldrid;

namespace KartLibrary.Game.Engine.Render;

public class VeldridShader
{
    public ShaderSetDescription ShaderSetDesc { get; set; }
    
    public ResourceLayout ShaderResourceLayout { get; set; }

    public VeldridShader(ShaderSetDescription shaderSetDesc, ResourceLayout shaderResourceLayout)
    {
        ShaderSetDesc = shaderSetDesc;
        ShaderResourceLayout = shaderResourceLayout;
    } 
}