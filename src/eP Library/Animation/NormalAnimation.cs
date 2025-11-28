namespace eP.Animation;

public class NormalAnimation<T>: IAnimation<T>
{
    public double Duration { get; set; }

    public ValueController<T> ValueController { get; set; }
    
    public Easing Easing { get; set; } = Easings.None;
    
    public T FromValue { get; set; }
    
    public T ToValue { get; set; }

    public NormalAnimation(ValueController<T> valueController, T fromValue, T toValue, double duration)
        :this(valueController, fromValue, toValue, duration, Easings.Linear)
    {
        
    }
    
    public NormalAnimation(ValueController<T> valueController, T fromValue, T toValue, double duration, Easing easing)
    {
        ValueController = valueController;
        FromValue = fromValue;
        ToValue = toValue;
        Duration = duration;
        Easing = easing;
    }
    
    public T ApplyAnimation(double time)
    {
        if (time >= Duration)
            return ToValue;
        if (time < 0)
            return FromValue;
        if (Duration == 0)
            return ToValue;

        double t = Easing(time / Duration);
        return ValueController(FromValue, ToValue, t);
    }
}

public static class NormalAnimationExtension
{
    public static void ApplyNormalAnimation<T>(this AnimatableType<T> animatableType, T to, double duration)
    {
        animatableType.ClearAnimation();
        T from = animatableType.Value;
        
        animatableType.ApplyAnimation(new NormalAnimation<T>(animatableType.ValueController, from, to, duration));
    }
    
    public static void ApplyNormalAnimation<T>(this AnimatableType<T> animatableType, T to, double duration, Easing easing)
    {
        animatableType.ClearAnimation();
        T from = animatableType.Value;
        
        animatableType.ApplyAnimation(new NormalAnimation<T>(animatableType.ValueController, from, to, duration, easing));
    }
    
    public static void ApplyNormalAnimation<T>(this AnimatableType<T> animatableType, T from, T to, double duration)
    {
        animatableType.ApplyAnimation(new NormalAnimation<T>(animatableType.ValueController, from, to, duration));
    }
    
    public static void ApplyNormalAnimation<T>(this AnimatableType<T> animatableType, T from, T to, double duration, Easing easing)
    {
        animatableType.ApplyAnimation(new NormalAnimation<T>(animatableType.ValueController, from, to, duration, easing));
    }
}