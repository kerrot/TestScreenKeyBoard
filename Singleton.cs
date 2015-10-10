using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


public static class Singleton<T>
       where T : class
{
    static volatile T _instance;
    static object _lock = new object();

    static Singleton()
    {
    }

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        ConstructorInfo constructor = null;

                        try
                        {
                            // Binding flags exclude public constructors.
                            constructor = typeof(T).GetConstructor(BindingFlags.Instance |
                                          BindingFlags.NonPublic, null, new Type[0], null);
                        }
                        catch (Exception exception)
                        {
                            throw new Exception(exception.Message);
                        }

                        if (constructor == null || constructor.IsAssembly)
                        {
                            throw new Exception(string.Format("A private or " +
                                  "protected constructor is missing for '{0}'.", typeof(T).Name));
                        }

                        _instance = (T)constructor.Invoke(null);
                    }
                }
            }


            return _instance;
        }
    }
}    

