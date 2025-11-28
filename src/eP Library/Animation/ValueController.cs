namespace eP.Animation;

public delegate T ValueController<T>(T value, T newValue, double t);

public static class ValueControllers
{
    public static readonly ValueController<float> FloatValueController = (x, y, t) => (float)((x) * (1 - t) + (y) * t);
    public static readonly ValueController<double> DoubleValueController = (x, y, t) => ((x) * (1 - t) + (y) * t);
    public static readonly ValueController<int> Int32ValueController = (x, y, t) => (int)((x) * (1 - t) + (y) * t);
    public static readonly ValueController<long> Int64ValueController = (x, y, t) => (long)((x) * (1 - t) + (y) * t);
}