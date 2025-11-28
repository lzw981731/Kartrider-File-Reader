using System.Numerics;
using KartLibrary.Engine.Render.DataModel;

namespace KartLibrary.Engine.Render.Renderables;

public class BallRenderable: GeneralRenderable
{
    protected override RenderGeneralVertex[] CreateRenderVertices()
    {
        List<RenderGeneralVertex> vertices = new List<RenderGeneralVertex>();
        int precision = 20;
        for (int z = -precision; z < precision; z++)
        {
            float curZ = (z / (float)precision);
            float nextZ = ((z + 1) / (float)precision);
            float currentRadius = MathF.Sqrt(1 - MathF.Pow(curZ, 2));
            float nextRadius = MathF.Sqrt(1 - MathF.Pow(nextZ, 2));
            for (int j = 0; j < 360; j++)
            {
                float x = MathF.Cos(j / 180f * MathF.PI);
                float y = MathF.Sin(j / 180f * MathF.PI);
                float x1 = x * currentRadius;
                float x2 = x * nextRadius;
                float y1 = y * currentRadius;
                float y2 = y * nextRadius;
                vertices.Add(new RenderGeneralVertex()
                {
                    Position = new Vector3(x1, y1, curZ),
                    Color = new Vector4(currentRadius, 0, 0.5f, 1.0f)
                });
                vertices.Add(new RenderGeneralVertex()
                {
                    Position = new Vector3(x2, y2, nextZ),
                    Color = new Vector4(currentRadius, 0, 0.5f, 1.0f)
                });
            }
        }

        return vertices.ToArray();
    }
    
    
}