using KartLibrary.Game.Engine.Render;
using KartLibrary.Resource;
using Veldrid;
using Veldrid.SPIRV;

namespace KartLibrary.Engine.Render.Shaders;

public class ReToonRigidShader: VeldridShader
{
    private ReToonRigidShader(ShaderSetDescription shaderSetDesc, ResourceLayout shaderResLayout) : base(shaderSetDesc, shaderResLayout)
    {
            
    }

    public ResourceSet CreateShaderResourceSet
    (
        GraphicsDevice renderDevice,
        DeviceBuffer modelMatBuffer,
        DeviceBuffer alphaPropBuffer,
        DeviceBuffer toonPropBuffer,
        TextureView surfaceTextureView,
        Sampler surfaceSampler,
        TextureView colorMaskingTextureView,
        Sampler colorMaskingSampler
    )
    {
        ResourceFactory resourceFactory = renderDevice.ResourceFactory;

        ResourceSet resourceSet = resourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                ShaderResourceLayout,
                modelMatBuffer,
                // toonPropBuffer,
                alphaPropBuffer,
                surfaceTextureView,
                surfaceSampler,
                colorMaskingTextureView,
                colorMaskingSampler
            ));
        return resourceSet;
    }
    
    public static ReToonRigidShader CreateReToonRigidShader(GraphicsDevice renderDevice)
    {
        ResourceFactory resourceFactory = renderDevice.ResourceFactory;
        
        byte[]? vertexShader = AssemblyResourceManager.GetEmbeddedResource($"KartLibrary.Engine.Render.Shaders.ReToonRigid.vertexShader.glsl");
        byte[]? fragmentShader = AssemblyResourceManager.GetEmbeddedResource($"KartLibrary.Engine.Render.Shaders.ReToonRigid.fragmentShader.glsl");
        if (vertexShader is null || fragmentShader is null)
            throw new Exception($"Cannot create ReToonRigid shader!");
        ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertexShader, "main");
        ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragmentShader, "main");
        Shader[] shaders = resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        VertexLayoutDescription vertexLayoutDescription = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("NormalVec", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
        );
        ShaderSetDescription shaderSetDescription = new ShaderSetDescription(
            new VertexLayoutDescription[] { vertexLayoutDescription },
            shaders
        );
        ResourceLayout shaderResourceLayout = resourceFactory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ModelInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                // new ResourceLayoutElementDescription("ToonProperty", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("AlphaProperty", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ColorMaskingTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ColorMaskingSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));
        return new ReToonRigidShader(shaderSetDescription, shaderResourceLayout);
    }
}