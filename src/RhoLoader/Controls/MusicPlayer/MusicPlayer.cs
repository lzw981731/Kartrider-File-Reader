using System.Drawing.Drawing2D;
using System.IO;
using eP.Animation;
using NAudio.Vorbis;
using NAudio.Wave;

namespace RhoLoader.Controls.MusicPlayer;

public partial class MusicPlayer : UserControl
{
    private WaveOutEvent? _outputDevice;
    private LoopableAudioReader? _audioReader;

    private AnimatableType<float> _boxPosition = new AnimatableType<float>(10, ValueControllers.FloatValueController);
    
    public MusicPlayer()
    {
        InitializeComponent();
        
        _updateTimer.Start();
    }

    public void LoadMusic(byte[] data)
    {
        var memoryStream = new MemoryStream(data);

        _outputDevice = new WaveOutEvent();
        _audioReader = new LoopableAudioReader(new VorbisWaveReader(memoryStream));
        _audioReader.Looping = true;
        
        _outputDevice.PlaybackStopped += OutputDeviceOnPlaybackStopped;
        _outputDevice.Init(_audioReader);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.FillRectangle(new SolidBrush(Color.BlueViolet), _boxPosition.Value, 30, 30, 30);
    }

    private void ActionPlayPause(object sender, EventArgs e)
    {
        if (_audioReader is not null)
        {
            if(_outputDevice?.PlaybackState is PlaybackState.Playing)
                _outputDevice?.Pause();
            else if(_outputDevice?.PlaybackState is PlaybackState.Paused or PlaybackState.Stopped)
                _outputDevice?.Play();
        }
    }

    private void OutputDeviceOnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        
    }

    private void ActionUpdateTimer(object sender, EventArgs e)
    {
        Refresh();
    }

    private void ActionDebug(object sender, EventArgs e)
    {
        _boxPosition.ApplyNormalAnimation(10, 300, 520, Easings.EaseInOutBack);
    }
}

//
// Refer to https://www.markheath.net/post/looped-playback-in-net-with-naudio
//
public class LoopableAudioReader(WaveStream baseStream, bool autoDisposing = false) : WaveStream
{
    public WaveStream BaseWaveStream = baseStream;
    
    public bool Looping { get; set; } = false;

    public override WaveFormat WaveFormat => BaseWaveStream.WaveFormat;

    public override long Length => BaseWaveStream.Length;

    public override long Position
    {
        get => BaseWaveStream.Position;
        set => BaseWaveStream.Position = value;
    }

    private bool _autoDisposing = autoDisposing;
    
    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int bytesRead = BaseWaveStream.Read(buffer, offset + totalRead, count - totalRead);

            if (bytesRead == 0)
            {
                if (BaseWaveStream.Position == 0 || !Looping)
                    break;

                BaseWaveStream.Position = 0;
            }
            
            totalRead += bytesRead;
        }

        return totalRead;
    }

    protected override void Dispose(bool disposing)
    {
        if(_autoDisposing && disposing)
            BaseWaveStream.Dispose();
        
        base.Dispose(disposing);
    }
}