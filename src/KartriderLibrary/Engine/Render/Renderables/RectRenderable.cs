using System.Numerics;
using KartLibrary.Engine.Render.DataModel;
using KartLibrary.Game.Engine.Render;
using BoundingBox = KartCity.Common.Engine.BoundingBox;

namespace KartLibrary.Engine.Render.Renderables;

public class RectRenderable: GeneralRenderable
{
    private Vector3 _topLeft;
    private Vector3 _topRight;
    private Vector3 _bottomLeft;
    private Vector3 _bottomRight;
    private BoundingBox _boundingBox;
    private Vector4 _color;
    
    public RectRenderable(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, Vector4 color)
    {
        _topLeft = topLeft;
        _topRight = topRight;
        _bottomLeft = bottomLeft;
        _bottomRight = bottomRight;
        Vector3[] vecArray = [_topLeft, _topRight, _bottomLeft, _bottomRight];
        float minX = vecArray.Select(x => x.X).Min();
        float minY = vecArray.Select(x => x.Y).Min();
        float minZ = vecArray.Select(x => x.Z).Min();
        
        float maxX = vecArray.Select(x => x.X).Max();
        float maxY = vecArray.Select(x => x.Y).Max();
        float maxZ = vecArray.Select(x => x.Z).Max();
        _boundingBox = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
        _color = color;
    }
    
    protected override RenderGeneralVertex[] CreateRenderVertices()
    {
        return
        [
            new RenderGeneralVertex()
            {
                Position = _topLeft,
                Color = _color,
            },
            new RenderGeneralVertex()
            {
                Position = _topRight,
                Color = _color,
            },
            new RenderGeneralVertex()
            {
                Position = _bottomLeft,
                Color = _color,
            },
            new RenderGeneralVertex()
            {
                Position = _bottomRight,
                Color = _color,
            },
        ];
    }

    public override double GetDistance(Camera camera, bool useFar)
    {
        _boundingBox.GetMinMaxZ(ref camera.ProjectionViewMatrixRef, ref ModelMatrixRef, out var minZ, out var maxZ);
        return maxZ < -0.01f ? double.NaN : maxZ;
    }
    
    public override bool IsTranslucencyObject() => _color.W < 1;
}