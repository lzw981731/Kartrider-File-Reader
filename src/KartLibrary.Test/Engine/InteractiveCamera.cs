using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using KartLibrary.Game.Engine.Render;
using Veldrid;
using Veldrid.Sdl2;
using Vortice.Mathematics;

namespace KartRiderLibrary.Tests
{
    public class InteractiveCamera
    {
        private Camera _baseCamera;
        private float moveSpeed = 1.79f;
        private InputCapturer _inputCapturer;

        private Vector2 _mousePosOnClick;
        private Vector3 _cameraDirectionOnClick;
        private Vector3 _cameraUpOnClick;

        private long _prevUpdateTimeStamp = 0;

        private bool _lockToTarget = false;

        public ref float MovingSpeed => ref moveSpeed;
        
        public bool LockToTarget
        {
            get => _lockToTarget;
            set
            {
                _lockToTarget = value;
            }
        }
        
        public bool IsEnabled { get; set; } = true;

        public InteractiveCamera(InputCapturer inputCapturer, Camera baseCamera)
        {
            this._baseCamera = baseCamera;
            _inputCapturer = inputCapturer;
            _inputCapturer.MouseDown += mouseDown;
            _inputCapturer.MouseMove += mouseMove;
            _inputCapturer.MouseWheel += mouseWheel;
            _inputCapturer.WindowSizeChanged += windowSizeChanged;
        }

        private void windowSizeChanged(Vector2 newsize)
        {
            _baseCamera.UpdateSceneSize((int)newsize.X, (int)newsize.Y);
        }

        public void UpdateMovingEvent()
        {
            if (!IsEnabled)
                return;
            if (_prevUpdateTimeStamp != 0)
            {
                float duration = (Environment.TickCount64 - _prevUpdateTimeStamp) / 1000f;
                Vector3 moveVec = new Vector3(0, 0, 0);
                bool hiSpeedMode = _inputCapturer.IsKeyPressed(Key.P);
                if (_inputCapturer.IsKeyPressed(Key.W) || _inputCapturer.IsKeyPressed(Key.Up))
                    moveVec -= Vector3.UnitZ;
                if (_inputCapturer.IsKeyPressed(Key.S) || _inputCapturer.IsKeyPressed(Key.Down))
                    moveVec += Vector3.UnitZ;
                if (_inputCapturer.IsKeyPressed(Key.A) || _inputCapturer.IsKeyPressed(Key.Left))
                    moveVec -= Vector3.UnitX;
                if (_inputCapturer.IsKeyPressed(Key.D) || _inputCapturer.IsKeyPressed(Key.Right))
                    moveVec += Vector3.UnitX;
                if (moveVec != Vector3.Zero)
                {
                    if (_lockToTarget)
                        moveVec.X = 0;
                    Vector3 cameraDirection = _baseCamera.CameraDirection;
                    Vector3 cameraRight = Vector3.Cross(_baseCamera.CameraUp, cameraDirection);
                    if(hiSpeedMode)
                        moveVec *= (moveSpeed * 55f) * duration;
                    else
                        moveVec *= moveSpeed * duration;
                    Vector3 offset = moveVec.X * cameraRight + moveVec.Y * _baseCamera.CameraUp + moveVec.Z * cameraDirection;
                    _baseCamera.CameraPosition += offset;
                    if(!_lockToTarget)
                        _baseCamera.CameraTarget += offset;
                }
            }
            _prevUpdateTimeStamp = Environment.TickCount64;
        }
        private void mouseDown(MouseEvent e)
        {
            if (!IsEnabled)
                return;
            if (e.MouseButton == MouseButton.Left || e.MouseButton == MouseButton.Right)
            {
                _mousePosOnClick = _inputCapturer.MousePosition;
                _cameraDirectionOnClick = Vector3.Normalize(_baseCamera.CameraPosition - _baseCamera.CameraTarget);
                _cameraUpOnClick = Vector3.Normalize(_baseCamera.CameraUp);
            }
        }

        private void mouseMove(MouseMoveEventArgs e)
        {
            if (!IsEnabled)
                return;
            if (_inputCapturer.IsMouseLeftPressed || _inputCapturer.IsMouseRightPressed)
            {
                Vector2 mousePos = e.MousePosition;

                
                bool cursorReturn = false;
                float margin = 50f;
                if (mousePos.X <= margin)
                {
                    _mousePosOnClick = new Vector2(_inputCapturer.ClientSize.X - (margin + 10f), mousePos.Y);
                    cursorReturn = true;
                }
                else if (mousePos.X >= _inputCapturer.ClientSize.X - margin)
                {
                    _mousePosOnClick = new Vector2((margin + 10f), mousePos.Y);
                    cursorReturn = true;
                }
                if (mousePos.Y <= margin)
                {
                    _mousePosOnClick = new Vector2(cursorReturn ? _mousePosOnClick.X : mousePos.X, _inputCapturer.ClientSize.Y - (margin + 10f));
                    cursorReturn = true;
                }
                else if (mousePos.Y >= _inputCapturer.ClientSize.Y - margin)
                {
                    _mousePosOnClick = new Vector2(cursorReturn ? _mousePosOnClick.X : mousePos.X, (margin + 10f));
                    cursorReturn = true;
                }
                if (cursorReturn)
                {
                    _cameraDirectionOnClick = Vector3.Normalize(_baseCamera.CameraPosition - _baseCamera.CameraTarget);
                    _cameraUpOnClick = Vector3.Normalize(_baseCamera.CameraUp);
                    mousePos = _mousePosOnClick;
                }

                Vector2 deltaPos = mousePos - _mousePosOnClick;
                Vector2 normalized = deltaPos / _inputCapturer.ClientSize;
                Vector3 cameraRightOnClick = Vector3.Cross(_cameraUpOnClick, _cameraDirectionOnClick);
                if (!_lockToTarget)
                {
                    float zRotate = -normalized.X * (MathF.PI / 2);
                    float camRightRotate = -normalized.Y * (MathF.PI / 2);
                    Quaternion zAxisQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, zRotate);
                    Quaternion camRightQuaternion = Quaternion.CreateFromAxisAngle(cameraRightOnClick, camRightRotate);
                    Quaternion rotateQuaternion = Quaternion.Multiply(zAxisQuaternion, camRightQuaternion);
                    Vector3 rotatedCameraUp = Vector3.Transform(_cameraUpOnClick, rotateQuaternion);
                    Vector3 rotatedCameraDirection = Vector3.Transform(_cameraDirectionOnClick, rotateQuaternion);
                    _baseCamera.CameraUp = rotatedCameraUp;
                    _baseCamera.CameraDirection = rotatedCameraDirection;
                }
                else
                {
                    float zRotate = -normalized.X * (MathF.PI / 2);
                    float camRightRotate = -normalized.Y * (MathF.PI / 2);
                    Quaternion zAxisQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, zRotate);
                    Quaternion camRightQuaternion = Quaternion.CreateFromAxisAngle(cameraRightOnClick, camRightRotate);
                    Quaternion rotateQuaternion = Quaternion.Multiply(zAxisQuaternion, camRightQuaternion);
                    Vector3 rotatedCameraUp = Vector3.Transform(_cameraUpOnClick, rotateQuaternion);
                    Vector3 rotatedCameraDirection = Vector3.Transform(_cameraDirectionOnClick, rotateQuaternion);
                    _baseCamera.CameraUp = rotatedCameraUp;
                    _baseCamera.CameraDirection = rotatedCameraDirection;

                }
            }
        }
        private void mouseWheel(MouseWheelEventArgs mouseWheelEvent)
        {
            if (!IsEnabled)
                return;
            float changeSpeed = (12f / 180F * MathF.PI);
            float angleChange = mouseWheelEvent.WheelDelta * ((changeSpeed));
            float newFOV = _baseCamera.FieldOfView + angleChange;
            if (newFOV >= (120f / 180f) * MathF.PI)
                newFOV = (120f / 180f) * MathF.PI;
            else if (newFOV <= (15f / 180f) * MathF.PI)
                newFOV = (15f / 180f) * MathF.PI;
            _baseCamera.FieldOfView = newFOV;
        }
    }
}
