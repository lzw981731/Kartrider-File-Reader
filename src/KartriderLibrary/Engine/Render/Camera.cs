using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine.Render
{
    public class Camera
    {
        private Vector3 _cameraPos;
        private Vector3 _cameraTarget;
        private Vector3 _cameraUp;
        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;
        private Matrix4x4 _farProjectionViewMatrix;
        private Matrix4x4 _projViewMatCache;
        private Matrix4x4 _farProjViewMatCache;
        private float _fieldOfView = MathF.PI / 2f;
        // View modified focus on modifies like camera position, target, up and direction.
        private bool _viewModified = true;
        // Proj Modified focus on modifies like FOV, near, far and aspect radio.
        private bool _projModified = true;
        private Vector2 _sceneSize = new Vector2(1, 1);
        private bool _lockToTarget = false;
        private float _near = 1f;
        private float _far = 100000f;

        public virtual Vector3 CameraPosition
        {
            get => _cameraPos;
            set
            {
                _cameraPos = value;
                _viewModified = true;
            }
        }
        public virtual Vector3 CameraTarget
        {
            get => _cameraTarget;
            set
            {
                _cameraTarget = value;
                _viewModified = true;
            }
        }
        public virtual Vector3 CameraDirection
        {
            get => Vector3.Normalize(_cameraPos - _cameraTarget);
            set
            {
                if (_lockToTarget)
                {
                    Vector3 camToTarget = _cameraTarget - _cameraPos;
                    float len = camToTarget.Length();
                    _cameraPos = _cameraTarget + len * Vector3.Normalize(value);
                }
                else
                    _cameraTarget = _cameraPos - value;
                _viewModified = true;
            }
        }
        public virtual Vector3 CameraUp
        {
            get => _cameraUp;
            set
            {
                _cameraUp = Vector3.Normalize(value);
                _viewModified = true;
            }
        }
        public virtual Matrix4x4 ViewMatrix => _viewMatrix;
        public virtual Matrix4x4 ProjectionMatrix => _projectionMatrix;
        
        [ImmutableObject(true)]
        public virtual ref Matrix4x4 ViewMatrixRef => ref _viewMatrix; 
        
        [ImmutableObject(true)]
        public virtual ref Matrix4x4 ProjectionMatrixRef => ref _projectionMatrix; 
        
        public virtual Matrix4x4 FarObjectProjectionViewMatrix => _farProjectionViewMatrix;
        
        public virtual ref Matrix4x4 FarObjectProjectionViewMatrixRef => ref _farProjViewMatCache;
        
        
        [ImmutableObject(true)]
        public virtual ref Matrix4x4 ProjectionViewMatrixRef => ref _projViewMatCache;

        public virtual float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (value >= MathF.PI)
                    _fieldOfView = MathF.PI - 0.25f;
                else if (value <= 0)
                    _fieldOfView = 0.25f;
                else
                    _fieldOfView = value;
                _projModified = true;
            }
        }

        public virtual Vector2 CameraScreenSize => _sceneSize;

        public virtual bool IsViewModified => _viewModified;
        
        public virtual bool IsProjectionModified => _projModified;

        public virtual bool LockToTarget
        {
            get => _lockToTarget;
            set
            {
                _lockToTarget = value;
            }
        }

        public virtual float Far
        {
            get => _far;
            set
            {
                _far = value;
                _projModified = true;
            }
        }
        
        public virtual float Near
        {
            get => _near;
            set
            {
                _near = value;
                _projModified = true;
            }
        }

        public Camera()
        {
            
        }

        public void UpdateMatrix()
        {
            bool hasUpdated = false;
            if( _viewModified)
            {
                _viewMatrix = Matrix4x4.CreateLookAt(_cameraPos, _cameraTarget, _cameraUp);
                _viewModified = false;
                hasUpdated = true;
            }

            if (_projModified)
            {
                _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fieldOfView, _sceneSize.X / _sceneSize.Y , _near, _far);
                _farProjectionViewMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fieldOfView, _sceneSize.X / _sceneSize.Y , 0.1f, 20000f);
                _projModified = false;
                hasUpdated = true;
            }

            if (hasUpdated)
            {
                _projViewMatCache = _viewMatrix * _projectionMatrix;
                _farProjViewMatCache = _farProjectionViewMatrix * _projectionMatrix;
            }
        }
        
        public void UpdateSceneSize(int width, int height) 
        {
            _sceneSize = new Vector2(width, height);
            _projModified = true;
        }

    }
}
