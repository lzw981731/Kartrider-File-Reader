namespace eP.Animation;

public class AnimatableType<T>(T initialValue, ValueController<T> valueController)
{
    private Lock _animationLock = new Lock();
    private IAnimation<T>? _activeAnimation;
    private long _animationStartTime = 0;
    
    private Lock _valueLock = new Lock();
    private T _value = initialValue;
        
    public ValueController<T> ValueController { get;  } = valueController;

    public T Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    public bool IsAnimationPlaying
    {
        get
        {
            lock (_animationLock)
            {
                return _activeAnimation is not null && GetTime() <= _activeAnimation.Duration;
            }
        }
    }

    public void ApplyAnimation(IAnimation<T> animation)
    {
        lock (_animationLock)
        {
            _activeAnimation = animation;
            _animationStartTime = Environment.TickCount64;
        }
    }

    public void ClearAnimation()
    {
        lock (_animationLock)
        {
            _activeAnimation = null;
        }
    }
    
    private double GetTime() => (Environment.TickCount64 - _animationStartTime);
    
    private T GetValue()
    {
        lock (_animationLock)
        lock(_valueLock)
        {
            double time = GetTime();
            if (_activeAnimation?.Duration < time)
                _activeAnimation = null;
            
            if (_activeAnimation is null)
                return _value;

            _value = _activeAnimation.ApplyAnimation(time);
            
            return _value;
        }
    }
    
    private void SetValue(T newValue)
    {
        lock (_animationLock)
        lock(_valueLock)
        {
            _activeAnimation = null;
            _value = newValue;
        }
    }
}