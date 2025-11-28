namespace RayCityLibrary.IO
{
    /// <summary>
    /// The implement of KartObject. This attribute can be used on the class that derived of KartObject.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RayCityObjectImplementAttribute : Attribute
    {
        public CreateObjectFunc? CreateObjectMethod;

        public RayCityObjectImplementAttribute()
        {
            this.CreateObjectMethod = null;
            
        }

        public RayCityObjectImplementAttribute(CreateObjectFunc? createObjectMethod)
        {
            this.CreateObjectMethod = createObjectMethod;
        }
    }

    public delegate RayCityObject CreateObjectFunc();
}
