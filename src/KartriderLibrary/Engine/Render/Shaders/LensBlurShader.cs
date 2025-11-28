using KartLibrary.Game.Engine.Render;
using KartLibrary.Resource;
using Veldrid;
using Veldrid.SPIRV;

namespace KartLibrary.Engine.Render.Shaders;

public class LensBlurShader: VeldridShader
{
    private LensBlurShader(ShaderSetDescription shaderSetDesc, ResourceLayout shaderResLayout) : base(shaderSetDesc, shaderResLayout)
    {
            
    }

    public ResourceSet CreateShaderResourceSet
    (
        GraphicsDevice renderDevice,
        TextureView surfaceTextureView,
        Sampler surfaceSampler
    )
    {
        ResourceFactory resourceFactory = renderDevice.ResourceFactory;
        ResourceSet resourceSet = resourceFactory.CreateResourceSet(
            new ResourceSetDescription(
                ShaderResourceLayout,
                surfaceTextureView,
                surfaceSampler
            ));
        return resourceSet;
    }
    
    public static LensBlurShader CreateLensBlurShader(GraphicsDevice renderDevice)
    {
        ResourceFactory resourceFactory = renderDevice.ResourceFactory;
        
        byte[]? vertexShader = AssemblyResourceManager.GetEmbeddedResource($"KartLibrary.Engine.Render.Shaders.LensBlur.vertexShader.glsl");
        byte[]? fragmentShader = AssemblyResourceManager.GetEmbeddedResource($"KartLibrary.Engine.Render.Shaders.LensBlur.fragmentShader.glsl");
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
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));
        return new LensBlurShader(shaderSetDescription, shaderResourceLayout);
    }
}