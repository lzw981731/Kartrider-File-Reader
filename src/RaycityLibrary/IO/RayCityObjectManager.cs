using System.Collections.Concurrent;
using System.Reflection;
using KartLibrary.IO;

namespace RayCityLibrary.IO
{
    public static class RayCityObjectManager
    {
        private static bool _initialized = false;
        private static Mutex _initMutex = new Mutex();
        private static ConcurrentDictionary<uint, RayCityObjectInfo> _registeredClasses = new();
        private static ConcurrentDictionary<Type, uint> _registeredClassesRev = new();
        /// <summary>
        /// Initialize <see cref="RayCityObjectManager"/>. Notes that it will register all classes that have <see cref="RayCityObjectImplementAttribute"/> attribute.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                _initMutex.WaitOne();
                if (_initialized)
                    return;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach(Assembly assembly in assemblies)
                foreach (TypeInfo type in 
                         assembly.GetTypes().Select(x => x).Where(x => x.IsSubclassOf(typeof(RayCityObject)) && x.GetCustomAttribute(typeof(RayCityObjectImplementAttribute), false) is not null))
                    RegisterClass(type);
                _initialized = true;
            }
            finally
            {
                _initMutex.ReleaseMutex();
            }
        }

        public static void RegisterClass<TRegisterClass>() where TRegisterClass : RayCityObject, new()
        {
            Type type = typeof(TRegisterClass);
            RegisterClass(type);
        }

        public static void RegisterClass(Type type)
        {
            Type? baseType = type.BaseType;
            while(baseType != null && baseType != typeof(RayCityObject))
                baseType = baseType.BaseType;
            if (baseType is null)
                throw new Exception("");
            ConstructorInfo? constructorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, new Type[0]);
            if (constructorInfo == null)
                throw new Exception("");
            RayCityObjectInfo rayCityObjectInfo = new(type, constructorInfo);
            RayCityObject newObj = rayCityObjectInfo.CreateObject();
            uint classStamp = newObj.ClassStamp;
            if (!_registeredClasses.TryAdd(classStamp, rayCityObjectInfo))
                throw new Exception($"Class: {type} has registered.");
            
            _registeredClassesRev.TryAdd(type, classStamp);
        }

        public static void RegisterAssemblyClasses(Assembly assembly)
        {
            assembly.GetTypes();
            IEnumerable<TypeInfo> foundTypes = assembly.DefinedTypes.Where(x => x.GetCustomAttributes<RayCityObjectImplementAttribute>().Count() > 0);
            foreach (TypeInfo typeInfo in foundTypes)
                RegisterClass(typeInfo.UnderlyingSystemType);
        }

        public static bool ContainsClass(uint classStamp)
        {
            if (!_initialized)
                Initialize();
            return _registeredClasses.ContainsKey(classStamp);
        }

        public static uint GetClassStamp(Type type)
        {
            return _registeredClassesRev[type];
        }

        public static T CreateObject<T>(uint ClassStamp) where T : RayCityObject
        {
            if (!_initialized)
                Initialize();
            if (!_registeredClasses.ContainsKey(ClassStamp))
                throw new Exception($"cannot found type: {ClassStamp:x8}");
            RayCityObjectInfo rayCityObjectInfo = _registeredClasses[ClassStamp];
            if (!rayCityObjectInfo.CanBeConvertTo(typeof(T)))
                throw new InvalidCastException($"{rayCityObjectInfo.BaseType.Name} cannot be convert to {typeof(T).Name}");
            return (T)rayCityObjectInfo.CreateObject();
        }

        public static RayCityObject CreateObject(uint ClassStamp)
        {
            if (!_initialized)
                Initialize();
            if (!_registeredClasses.ContainsKey(ClassStamp))
                throw new Exception($"cannot found type: {ClassStamp:x8}");
            RayCityObjectInfo rayCityObjectInfo = _registeredClasses[ClassStamp];
            return rayCityObjectInfo.CreateObject();
        }
    }

    internal record class RayCityObjectInfo(Type BaseType, ConstructorInfo ConstructorInfo)
    {
        public RayCityObject CreateObject()
        {
            return (RayCityObject)ConstructorInfo.Invoke(new object[0]);
        }

        public bool CanBeConvertTo(Type targetType)
        {
            Type? superType = targetType;
            while(superType != null)
            {
                if(superType == targetType)
                    return true;
                superType = superType.BaseType;
            }
            return false;
        }
    }
}
