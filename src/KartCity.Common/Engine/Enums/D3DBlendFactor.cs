using Veldrid;

namespace KartCity.Common.Engine.Enums
{
    public enum D3DBlendFactor
    {
        Zero = 1,
        One = 2,
        SourceColor = 3,
        InverseSourceColor = 4,
        SourceAlpha = 5,
        InverseSourceAlpha = 6,
        DestinationAlpha = 7,
        InverseDestinationAlpha = 8,
        DestinationColor = 9,
        InverseDestinationColor = 10,
        SourceAlphaSaturate = 11,
        BothSourceAlpha = 12,
        BothInverseSourceAlpha = 13,
        BlendFactor = 14,
        InverseBlendFactor = 15,
        SourceColor2 = 16,
        InverseSourceColor2 = 17,
    }

    public static class BlendFactorUtility
    {
        public static BlendFactor ConvertFromD3DBlendFactor(D3DBlendFactor blendFactor)
        {
            switch(blendFactor)
            {
                case D3DBlendFactor.Zero:
                    return BlendFactor.Zero;
                case D3DBlendFactor.One:
                    return BlendFactor.One;
                case D3DBlendFactor.SourceAlpha: 
                    return BlendFactor.SourceAlpha;
                case D3DBlendFactor.InverseSourceAlpha:
                    return BlendFactor.InverseSourceAlpha;
                case D3DBlendFactor.DestinationAlpha:
                    return BlendFactor.DestinationAlpha;
                case D3DBlendFactor.InverseDestinationAlpha:
                    return BlendFactor.InverseDestinationAlpha;
                case D3DBlendFactor.SourceColor:
                    return BlendFactor.SourceColor;
                case D3DBlendFactor.InverseSourceColor:
                    return BlendFactor.InverseSourceColor;
                case D3DBlendFactor.DestinationColor:
                    return BlendFactor.DestinationColor;
                case D3DBlendFactor.InverseDestinationColor:
                    return BlendFactor.InverseDestinationColor;
                case D3DBlendFactor.BlendFactor:
                    return BlendFactor.BlendFactor;
                case D3DBlendFactor.InverseBlendFactor:
                    return BlendFactor.InverseBlendFactor;
                default:
                    throw new NotImplementedException();
            }
        }   
    }
}
