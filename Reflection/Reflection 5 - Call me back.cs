using System.Reflection;

namespace Reflection
{
    /// <summary>
    /// In this kata you have to write a method without parameters and without a return value. Doesn't make sense? Ohhh, you will see, it will!
    /// Write this method
    /// void Activator()
    /// The only thing, this method have to do, is to call a method from the testclass.(The testclass contains the test, which calls your method "Activator".)
    /// The method you have to call is the only static method of the class. It sets an internal field, so for the test there is a prove of the call.
    /// </summary>
    internal class Reflection_5___Call_me_back
    {
        public class ReflectionTests
        {
            private static bool setting;

            public static void Activate()
            {
                setting = true;
            }
        }

        public static class Reflection
        {
            public static void Activator()
            {
                Type c = Type.GetType("ReflectionTests");
                c
                    .GetMethod(c
                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Select(m => m.Name)
                        .FirstOrDefault())
                    .Invoke(System.Activator.CreateInstance(c), null);
            }
        }
    }
}