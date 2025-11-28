using KartLibrary.Engine.Enums;
using Veldrid;
using Vortice.Direct3D11;

namespace KartCity.Common.Engine.Extension;

public static class VeldridExtension
{
    public static SamplerAddressMode ToVeldridAddressMode(this TextureAddressMode textureAddressMode)
    {
        return textureAddressMode switch
        {
            TextureAddressMode.Border => SamplerAddressMode.Border,
            TextureAddressMode.Clamp => SamplerAddressMode.Clamp,
            TextureAddressMode.Mirror => SamplerAddressMode.Mirror,
            TextureAddressMode.Wrap => SamplerAddressMode.Wrap,
            TextureAddressMode.MirrorOnce => SamplerAddressMode.Mirror,
            _ => SamplerAddressMode.Border
        };
    }

    public static SamplerFilter ToVeldridFilter(D3DTextureFilterType minFilter, D3DTextureFilterType magFilter,
        D3DTextureFilterType mipFilter)
    {
        if (minFilter == D3DTextureFilterType.None)
            minFilter = D3DTextureFilterType.Point;
        if (magFilter == D3DTextureFilterType.None)
            magFilter = D3DTextureFilterType.Point;
        if (mipFilter == D3DTextureFilterType.None)
            mipFilter = D3DTextureFilterType.Point;
        return minFilter switch
        {
            D3DTextureFilterType.Point => magFilter switch
            {
                D3DTextureFilterType.Point => mipFilter switch
                {
                    D3DTextureFilterType.Point => SamplerFilter.MinPoint_MagPoint_MipPoint,
                    D3DTextureFilterType.Linear => SamplerFilter.MinPoint_MagPoint_MipLinear,
                    D3DTextureFilterType.Anisotropic => SamplerFilter.Anisotropic,
                    _ => SamplerFilter.MinPoint_MagPoint_MipPoint
                },
                D3DTextureFilterType.Linear => mipFilter switch
                {
                    D3DTextureFilterType.Point => SamplerFilter.MinPoint_MagLinear_MipPoint,
                    D3DTextureFilterType.Linear => SamplerFilter.MinPoint_MagLinear_MipLinear,
                    D3DTextureFilterType.Anisotropic => SamplerFilter.Anisotropic,
                    _ => SamplerFilter.MinPoint_MagPoint_MipPoint
                },
                D3DTextureFilterType.Anisotropic => mipFilter switch
                {
                    _ => SamplerFilter.Anisotropic
                },
                _ => SamplerFilter.MinPoint_MagPoint_MipPoint
            },
            D3DTextureFilterType.Linear => magFilter switch
            {
                D3DTextureFilterType.Point => mipFilter switch
                {
                    D3DTextureFilterType.Point => SamplerFilter.MinLinear_MagPoint_MipPoint,
                    D3DTextureFilterType.Linear => SamplerFilter.MinLinear_MagPoint_MipLinear,
                    D3DTextureFilterType.Anisotropic => SamplerFilter.Anisotropic,
                    _ => SamplerFilter.MinPoint_MagPoint_MipPoint
                },
                D3DTextureFilterType.Linear => mipFilter switch
                {
                    D3DTextureFilterType.Point => SamplerFilter.MinLinear_MagLinear_MipPoint,
                    D3DTextureFilterType.Linear => SamplerFilter.MinLinear_MagLinear_MipLinear,
                    D3DTextureFilterType.Anisotropic => SamplerFilter.Anisotropic,
                    _ => SamplerFilter.MinPoint_MagPoint_MipPoint
                },
                D3DTextureFilterType.Anisotropic => mipFilter switch
                {
                    _ => SamplerFilter.Anisotropic
                },
                _ => SamplerFilter.MinPoint_MagPoint_MipPoint
            },
            D3DTextureFilterType.Anisotropic => SamplerFilter.Anisotropic,
            _ => SamplerFilter.MinPoint_MagPoint_MipPoint
        };
    }
}