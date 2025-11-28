using System.IO;
using KartCityStudio.Game.Model;

namespace RhoLoader.Controls.PreviewPanel;

public partial class PreviewPanel : UserControl
{
    private Lock _loadLock = new Lock();
    private Task? _fileLoadTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private Control? _previewControl;
    private Stream? _fileStream;

    public event CreateControlDelegate? CreateControl;
    public event InitControlDelegate? InitControl;
    
    public PreviewPanel()
    {
        InitializeComponent();
    }
    
    public void LoadPreview(IArchiveFile previewFile)
    {
        lock (_loadLock)
        {
            if (!_cancellationTokenSource?.IsCancellationRequested ?? false)
                _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _fileLoadTask = Task.Run(() => BackgroundLoadPreview(previewFile, _cancellationTokenSource.Token));
        }
    }
    
    private void BackgroundLoadPreview(IArchiveFile previewFile, CancellationToken token)
    {
        try
        {
            _fileStream?.Dispose();
            _fileStream = null;
            
            UnloadPreviewControl();
            SetPreparePanelVisibility(true);
            SetPreviewControl(Path.GetExtension(previewFile.Name), token);
            token.ThrowIfCancellationRequested();

            _fileStream = previewFile.CreateStream();
            
            InitControl?.Invoke(previewFile , _previewControl, _fileStream, token);
            token.ThrowIfCancellationRequested();
            SetPreparePanelVisibility(false);
        }
        catch (OperationCanceledException)
        {
            _fileStream?.Dispose();
            _fileStream = null;
            UnloadPreviewControl();
            throw;
        }
    }

    private void SetPreviewControl(string extension, CancellationToken token)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(SetPreviewControl, extension, token);
            return;
        }
        
        UnloadPreviewControl();
        
        token.ThrowIfCancellationRequested();
        
        _previewControl = CreateControl?.Invoke(extension, token);
        if (_previewControl is not null)
        {
            _previewControl.Dock = DockStyle.Fill;
            _previewPanel.Controls.Add(_previewControl);
        }
    }

    private void UnloadPreviewControl()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(UnloadPreviewControl);
            return;
        }
        
        _previewPanel.Controls.Clear();
        try
        {
            _previewControl?.Dispose();
        }
        catch
        {
            
        }
        _previewControl = null;
    }

    private void SetPreparePanelVisibility(bool visibility)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(SetPreparePanelVisibility, visibility);
            return;
        }

        _loadingPanel.Visible = visibility;
    }
}

public delegate Control? CreateControlDelegate(string extension, CancellationToken token);
public delegate void InitControlDelegate(IArchiveFile file, Control? control, Stream stream, CancellationToken token);