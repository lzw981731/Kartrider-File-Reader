using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.Game.Engine
{
    public class FrameTimeSource: ITimeSource
    {
        private float? _firstFrameTime;

        private float? _lastFrameTime;

        private float _playSpeed = 1.0f;
        
        private DateTime _baseDateTime = DateTime.Now;
        
        public float PlaySpeed
        {
            get => _playSpeed;
            set
            {
                float? orgTime = (_lastFrameTime - _firstFrameTime) * _playSpeed;
                _playSpeed = value;
                if (orgTime is not null)
                    orgTime /= _playSpeed;
                _firstFrameTime = _lastFrameTime - orgTime;
            }
        }
        
        public FrameTimeSource() 
        {
            
        }

        public void OnUpdateFrame()
        {
            if (_firstFrameTime is null)
            {
                _lastFrameTime = _firstFrameTime = (float)(DateTime.Now - _baseDateTime).TotalMilliseconds;
            }
            else
            {
                _lastFrameTime = (float)(DateTime.Now - _baseDateTime).TotalMilliseconds;
            }
        }

        public float GetTimeStamp()
        {
            return ((_lastFrameTime - _firstFrameTime) * _playSpeed) ?? 0;
        }

        public void ResetTimeStamp()
        {
            _firstFrameTime = null;
        }
    }
}
