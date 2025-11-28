using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace KartRiderLibrary.Tests
{
    public class InputCapturer
    {
        private Dictionary<Key, bool> keyStatus = new Dictionary<Key, bool>();

        private Dictionary<Key, DateTime> keyCheckTime = new Dictionary<Key, DateTime>();
        
        private Dictionary<MouseButton, bool> mouseBtnStatus = new Dictionary<MouseButton, bool>();

        private Dictionary<MouseButton, DateTime> mouseBtnCheckTime = new Dictionary<MouseButton, DateTime>();

        private Vector2 mousePos;

        private Vector2 clientSize;

        public Vector2 MousePosition => mousePos;
        
        public Vector2 ClientSize => clientSize;

        public bool IsShiftPressed => keyStatus[Key.ShiftLeft] || keyStatus[Key.ShiftRight];

        public bool IsAltPressed => keyStatus[Key.AltLeft] || keyStatus[Key.AltRight];

        public bool IsMouseLeftPressed => mouseBtnStatus[MouseButton.Left];

        public bool IsMouseMiddlePressed => mouseBtnStatus[MouseButton.Middle];

        public bool IsMouseRightPressed => mouseBtnStatus[MouseButton.Right];

        public event KeyHandler? KeyDown;

        public event KeyHandler? KeyUp;

        public event MouseMoveHandler? MouseMove;

        public event MouseButtonHandler? MouseDown;

        public event MouseButtonHandler? MouseUp;

        public event MouseWheelHandler? MouseWheel;

        public event WindowSizeChangeHandler? WindowSizeChanged;

        public InputCapturer()
        {
            foreach(Key key in Enum.GetValues<Key>())
            {
                if (!keyStatus.ContainsKey(key))
                {
                    keyStatus.Add(key, false);
                    keyCheckTime.Add(key, DateTime.MinValue);
                }
            }
            foreach (MouseButton button in Enum.GetValues<MouseButton>())
            {
                if (!mouseBtnCheckTime.ContainsKey(button))
                {
                    mouseBtnStatus.Add(button, false);
                    mouseBtnCheckTime.Add(button, DateTime.MinValue);
                }
            }
        }

        public void OnKeyDown(KeyEvent keyEvent)
        {
            keyStatus[keyEvent.Key] = true;
            keyCheckTime[keyEvent.Key] = DateTime.Now;
            KeyDown?.Invoke(keyEvent);
        }

        public void OnKeyUp(KeyEvent keyEvent)
        {
            keyStatus[keyEvent.Key] = false;
            keyCheckTime[keyEvent.Key] = DateTime.MinValue;
            KeyUp?.Invoke(keyEvent);
        }

        public void OnMouseMove(MouseMoveEventArgs e)
        {
            mousePos = e.MousePosition;
            MouseMove?.Invoke(e);
        }

        public void OnMouseButtonDown(MouseEvent e)
        {
            mouseBtnStatus[e.MouseButton] = true;
            MouseDown?.Invoke(e);
        }

        public void OnMouseButtonUp(MouseEvent e)
        {
            mouseBtnStatus[e.MouseButton] = false;
            MouseUp?.Invoke(e);
        }

        public void OnMouseWheel(MouseWheelEventArgs e)
        {
            MouseWheel?.Invoke(e);
        }

        public void OnResize(Vector2 newSize)
        {
            clientSize = newSize;
            WindowSizeChanged?.Invoke(newSize);
        }

        public bool IsKeyPressed(Key key)
        {
            return keyStatus[key];
        }

        public bool IsMouseButtonPressed(MouseButton button)
        {
            return mouseBtnStatus[button];
        }
    }

    public delegate void KeyHandler(KeyEvent keyEvent);

    public delegate void MouseButtonHandler(MouseEvent mouseEvent);

    public delegate void MouseMoveHandler(MouseMoveEventArgs mouseMoveEvent);

    public delegate void MouseWheelHandler(MouseWheelEventArgs mouseWheelEvent);

    public delegate void WindowSizeChangeHandler(Vector2 newSize);
}
