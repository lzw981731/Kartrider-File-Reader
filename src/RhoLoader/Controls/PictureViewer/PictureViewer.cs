using System.ComponentModel;
using System.Diagnostics;
using eP.Animation;

namespace RhoLoader.Controls.PictureViewer;

public partial class PictureViewer : UserControl
{
    private Point _initMousePos;
    private PointF _initPicturePos;
    private PointF _picturePos;
    private AnimatableType<float> _aniScale = new (1, ValueControllers.FloatValueController);

    private float _scale = 1;
    
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Image? Image { get; set; }
    
    public PictureViewer()
    {
        InitializeComponent();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Image is null)
            return;

        float scale = _aniScale.Value;
        float imageWidth = Image.Width * scale;
        float imageHeight = Image.Height * scale;
        
        _picturePos = ClampPicturePosition(scale, _picturePos);

        float imageX = _picturePos.X + (Width - imageWidth) * 0.5f;
        float imageY = _picturePos.Y + (Height - imageHeight) * 0.5f;
        
        e.Graphics.DrawImage(
            Image, 
            new RectangleF(imageX, imageY, imageWidth, imageHeight),
            new RectangleF(0, 0, Image.Width, Image.Height),
            GraphicsUnit.Pixel
        );
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button.HasFlag(MouseButtons.Left))
        {
            _initMousePos = e.Location;
            _initPicturePos = _picturePos;
        }
    }
    
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.Button.HasFlag(MouseButtons.Left))
        {
            PointF newPoint = new PointF(
                _initPicturePos.X + e.X - _initMousePos.X,
                _initPicturePos.Y + e.Y - _initMousePos.Y
            );
            
            Debug.Print($"{newPoint} {_initPicturePos}");
            
            PointF clampedNewPoint = ClampPicturePosition(_aniScale.Value, newPoint);
            
            _initPicturePos = new PointF(
                clampedNewPoint.X - e.X + _initMousePos.X,
                clampedNewPoint.Y - e.Y + _initMousePos.Y
            );

            _picturePos = clampedNewPoint;
            
            Refresh();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (Image is null)
            return;

        _scale = Math.Clamp(_scale * MathF.Pow(1.1f, e.Delta / -120), 1, 5);
        float from = _aniScale.Value;
        
        _aniScale.ApplyNormalAnimation(from, _scale, 500, Easings.EaseOutQuint);
    }

    private PointF ClampPicturePosition(float scale, PointF newLocation)
    {
        if (Image is null)
            return newLocation;

        float maxX = MathF.Max(Image.Width * scale - Width, 0) / 2;
        float maxY = MathF.Max(Image.Height * scale - Height, 0) / 2;
        
        newLocation.X = Math.Clamp(newLocation.X, -maxX, maxX);
        newLocation.Y = Math.Clamp(newLocation.Y, -maxY ,maxY);
        
        return newLocation;
    }

    private void ActionUpdateTimer(object sender, EventArgs e)
    {
        if(_aniScale.IsAnimationPlaying)
            Refresh();
    }
}