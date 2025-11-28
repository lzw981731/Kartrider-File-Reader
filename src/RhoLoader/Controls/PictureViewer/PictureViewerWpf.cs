using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using eP.Animation;
using Brushes = System.Windows.Media.Brushes;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Pen = System.Drawing.Pen;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace RhoLoader.Controls.PictureViewer;

public partial class PictureViewerWpf : UIElement
{
    private System.Windows.Point _initMousePos;
    private System.Windows.Point _initPicturePos;
    private System.Windows.Point _picturePos;
    private AnimatableType<float> _aniScale = new (1, ValueControllers.FloatValueController);
    private ImageSource? _imageSource;
    
    private DispatcherTimer _updateTimer = new DispatcherTimer();

    private DrawingVisual _drawingVisual = new DrawingVisual();
    
    private Grid _grid;
    private Image _image;

    private DoubleAnimationUsingKeyFrames _animationUsingKeyFrames;
    
    private float _scale = 1;

    public ImageSource? ImageSource
    {
        get => _imageSource;
        set
        {
            _imageSource = value;
            this.Dispatcher.Invoke(InvalidateVisual);
        }
    }
    
    public PictureViewerWpf()
    {
        // _updateTimer.Interval = TimeSpan.FromMilliseconds(1f / 120);
        _updateTimer.Tick += UpdateTimerOnTick;
    }

    private void Drawing(DrawingContext drawingContext)
    {
        if (ImageSource is not null)
        {
            float scale = _aniScale.Value;
            double imageWidth = ImageSource.Width * scale;
            double imageHeight = ImageSource.Height * scale;
            
            drawingContext.DrawRectangle(Brushes.Gray, new System.Windows.Media.Pen(), new Rect(RenderSize));
            
            _picturePos = ClampPicturePosition(scale, _picturePos);

            double imageX = _picturePos.X + (RenderSize.Width - imageWidth) * 0.5f;
            double imageY = _picturePos.Y + (RenderSize.Height - imageHeight) * 0.5f;
            
            drawingContext.DrawImage(
                ImageSource,
                new Rect(imageX, imageY, imageWidth, imageHeight)
            );
        }
    }
    
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        this.Drawing(drawingContext);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _initMousePos = e.GetPosition(this);
            _initPicturePos = _picturePos;
        }
        
    }
    
    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            System.Windows.Point mousePosition = e.GetPosition(this);
            System.Windows.Point newPoint = new System.Windows.Point(
                _initPicturePos.X + mousePosition.X - _initMousePos.X,
                _initPicturePos.Y + mousePosition.Y - _initMousePos.Y
            );
            
            System.Windows.Point clampedNewPoint = ClampPicturePosition(_aniScale.Value, newPoint);
            
            _initPicturePos = new System.Windows.Point(
                clampedNewPoint.X - mousePosition.X + _initMousePos.X,
                clampedNewPoint.Y - mousePosition.Y + _initMousePos.Y
            );

            _picturePos = clampedNewPoint;
            this.InvalidateVisual();
        }
        
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (ImageSource is null)
            return;
        
        _scale = Math.Clamp(_scale * MathF.Pow(1.1f, e.Delta / 120), (float)GetMinScale(), 12);
        float from = _aniScale.Value;
        
        _aniScale.ApplyNormalAnimation(from, _scale, 500, Easings.EaseOutQuint);
        _updateTimer.Start();
    }
    
    private System.Windows.Point ClampPicturePosition(float scale, Point newLocation)
    {
        if (ImageSource is null)
            return newLocation;

        double maxX = Math.Max(ImageSource.Width * scale - RenderSize.Width, 0) / 2;
        double maxY = Math.Max(ImageSource.Height * scale - RenderSize.Height, 0) / 2;
        
        newLocation.X = Math.Clamp(newLocation.X, -maxX, maxX);
        newLocation.Y = Math.Clamp(newLocation.Y, -maxY ,maxY);
        
        return newLocation;
    }

    private void UpdateTimerOnTick(object? sender, EventArgs e)
    {   
        if (_aniScale.IsAnimationPlaying)
            this.InvalidateVisual();
        else
            _updateTimer.Stop();
    }

    protected override Size MeasureCore(Size availableSize)
    {
        return availableSize;
    }

    private double GetMinScale()
    {
        Size size = RenderSize;
        if (ImageSource is null)
            return 1;
        
        if (size.Width == 0 || size.Height == 0)
            return 0.2;
        
        return Math.Min(0.2, Math.Min(ImageSource.Width / size.Width, ImageSource.Height / size.Height));

    }
}