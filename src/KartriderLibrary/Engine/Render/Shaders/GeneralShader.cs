using KartLibrary.Game.Engine.Render;
using KartLibrary.Resource;
using Veldrid;
using Veldrid.SPIRV;

namespace KartLibrary.Engine.Render.Shaders;

public class GeneralShader: VeldridShader
{
    private GeneralShader(ShaderSetDescription shaderSetDesc, ResourceLayout shaderResLayout) : base(shaderSetDesc, shaderResLayout)
    {
            
    }

    public ResourceSet CreateShaderResourceSet
    (
        GraphicsDevice renderDevice,
        DeviceBuffer modelMatBuffer,
        TextureView surfaceTextureView,
        Sampler surfaceSampler
    )
    {
        ResourceFactory resourceFactory = renderDevice.ResourceFactory;

        ResourceSet resourceSet = resourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                ShaderResourceLayout,
                modelMatBuffer,
                surfaceTextureView,
                surfaceSampler
            ));
        return resourceSet;
    }

    public ShaderSetDescription GetShaderDescWithConst(bool useTexture, bool useColorMixing)
    {
        return ShaderSetDesc with
        {
            Specializations = new SpecializationConstant[]
            {
                new SpecializationConstant(0, useTexture),
                new SpecializationConstant(1, useColorMixing),
            }
        };
    }
    
    public static GeneralShader CreateGeneralShader(GraphicsDevice renderDevice)
    {
        ResourceFactory resourceFactory = renderDevice.ResourceFactory;
        
        byte[]? vertexShader = AssemblyResourceManager.GetEmbeddedResource($"KartLibrary.Engine.Render.Shaders.General.vertexShader.glsl");
        byte[]? fragmentShader = AssemblyResourceManager.GetEmbeddedResource($"KartLibrary.Engine.Render.Shaders.General.fragmentShader.glsl");
        if (vertexShader is null || fragmentShader is null)
            throw new Exception($"Cannot create GeneralShader shader!");
        ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertexShader, "main");
        ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragmentShader, "main");
        Shader[] shaders = resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        VertexLayoutDescription vertexLayoutDescription = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        );
        ShaderSetDescription shaderSetDescription = new ShaderSetDescription(
            new VertexLayoutDescription[] { vertexLayoutDescription },
            shaders,
            new SpecializationConstant[]
            {
                new SpecializationConstant(0, false),
                new SpecializationConstant(1, true),
            }
        );
        ResourceLayout shaderResourceLayout = resourceFactory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ModelInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));
        return new GeneralShader(shaderSetDescription, shaderResourceLayout);
    }
}