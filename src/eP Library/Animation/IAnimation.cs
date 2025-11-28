namespace eP.Animation;

public interface IAnimation<T>
{
    double Duration { get; }

    T ApplyAnimation(double time);
}