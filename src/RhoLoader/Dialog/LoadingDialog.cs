namespace RhoLoader;

public partial class LoadingDialog : Form
{
    private Task? _shownTask;
    private CancellationTokenSource _cancelSource = new CancellationTokenSource();
    private double _targetOpacity = 0;
    public LoadingDialog()
    {
        InitializeComponent();
    }
    
    public void LoadCompleted()
    {
        _targetOpacity = 0.01;
        _cancelSource.Cancel();
        if (this.InvokeRequired)
            this.Invoke(LoadCompleted);
        else
            this.DialogResult = DialogResult.OK;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _cancelSource.Cancel();
        
        _cancelSource =  new CancellationTokenSource();
        this.Opacity = 0.01;
        _targetOpacity = 0.01;
        _shownTask = Task.Run(async () =>
        {
            var cancelToken = _cancelSource.Token;
            await Task.Delay(300, cancelToken);
            if (!cancelToken.IsCancellationRequested)
            {
                _targetOpacity = 1;
                visibleWindow();
            }
        }, _cancelSource.Token);
    }

    private void visibleWindow()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(visibleWindow);
            return;
        }
        this.Opacity = _targetOpacity;
    }
}