namespace RhoLoader.Controls;

// Refer to https://stackoverflow.com/questions/75814627/how-to-disable-mouse-wheel-scrolling-while-the-cursor-is-on-the-scrollbar-of-the
public class ScrollBarNativeListener: NativeWindow
{
    private ScrollBar _baseScrollBar;

    public event MouseEventHandler? MouseWheel;
    public event KeyEventHandler? KeyDown;
    
    private static readonly MouseButtons[] MouseFlags = [
        MouseButtons.Left,
        MouseButtons.Right,
        MouseButtons.None,
        MouseButtons.None,
        MouseButtons.Middle,
        MouseButtons.XButton1,
        MouseButtons.XButton2,
    ];
    
    public ScrollBarNativeListener(ScrollBar scrollBar)
    {
        _baseScrollBar = scrollBar;
        _baseScrollBar.HandleCreated += ScrollBarOnHandleCreated;
        _baseScrollBar.HandleDestroyed += ScrollBarOnHandleDestroyed;
    }
    
    private void ScrollBarOnHandleCreated(object? sender, EventArgs e)
    {
        AssignHandle(_baseScrollBar.Handle);
    }
    
    private void ScrollBarOnHandleDestroyed(object? sender, EventArgs e)
    {
       ReleaseHandle();
    }

    protected override void WndProc(ref Message m)
    {
        if(m.Msg is not 0x020A)
            base.WndProc(ref m);
        else
        {
            short wheel = (short)(m.WParam.ToInt64() >>> 16);
            MouseButtons buttons = MouseButtons.None;
            short rawButtons = (short)(m.WParam.ToInt64() & 0xFFFF);
            for (int i = 0; i < 7; i++)
            {
                if((rawButtons & 1) == 1)
                    buttons &= MouseFlags[i];
                rawButtons >>= 1;
            }

            int x = (int)(m.LParam.ToInt64() & 0xFFFF);
            int y = (int)(m.LParam.ToInt64() >>> 16);
            
            HandledMouseEventArgs e = new HandledMouseEventArgs(buttons, 0, x, y, wheel);
            MouseWheel?.Invoke(this, e);
        }
    }
}