using KartLibrary.Game.Engine.Render;
using KartLibrary.Resource;
using Veldrid;
using Veldrid.SPIRV;

namespace KartLibrary.Engine.Render.Shaders;

public class FramebufferShader: VeldridShader
{
    private FramebufferShader(ShaderSetDescription shaderSetDesc, ResourceLayout shaderResLayout) : base(shaderSetDesc, shaderResLayout)
    {
            
    }

    public ResourceSet CreateShaderResourceSet
    (
        GraphicsDevice renderDevice,
        Texture surfaceTexture,
        Sampler surfaceSampler,
        TextureView depthTextureView,
        Sampler depthSampler,
        DeviceBuffer framebufferInfoBuffer
    )
    {
        ResourceFactory resourceFactory = renderDevice.ResourceFactory;

        ResourceSet resourceSet = resourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                ShaderResourceLayout,
                surfaceTexture,
                surfaceSampler,
                depthTextureView,
                depthSampler,
                framebufferInfoBuffer
            ));
        return resourceSet;
    }
    
    public static FramebufferShader CreateReToonRigidShader(GraphicsDevice renderDevice)
    {
        ResourceFactory resourceFactory = renderDevice.ResourceFactory;
        
        byte[]? vertexShader = AssemblyResourceManager.GetEmbeddedResource($"KartLibrary.Engine.Render.Shaders.Framebuffer.vertexShader.glsl");
        byte[]? fragmentShader = AssemblyResourceManager.GetEmbeddedResource($"KartLibrary.Engine.Render.Shaders.Framebuffer.fragmentShader.glsl");
        if (vertexShader is null || fragmentShader is null)
            throw new Exception($"Cannot create Framebuffer shader!");
        ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertexShader, "main");
        ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragmentShader, "main");
        Shader[] shaders = resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
        VertexLayoutDescription vertexLayoutDescription = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
        );
        ShaderSetDescription shaderSetDescription = new ShaderSetDescription(
            new VertexLayoutDescription[] { vertexLayoutDescription },
            shaders
        );
        ResourceLayout shaderResourceLayout = resourceFactory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("DepthTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("DepthSampler", ResourceKind.Sampler, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("FramebufferInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment)
            ));
        return new FramebufferShader(shaderSetDescription, shaderResourceLayout);
    }
}