using System.Reflection;

namespace Reflection
{
    /// <summary>
    /// You get an object and should return the names of all(!) methods, that you found for the object.
    /// For using random, the Name of the AddInts-Methods has an additonal number at the end. For null return an empty string array!
    /// </summary>
    internal class Reflection_2___Give_me_all_methods
    {
        public class Refl
        {
            public string Output()
            {
                return "Test-Output";
            }

            public int AddInts(int i1, int i2)
            {
                return i1 + i2;
            }
        }

        public static class Reflection
        {
            public static string[] GetMethodNames(object TestObject) =>
                TestObject?
                    .GetType()
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(m => m.Name)
                    .ToArray()
                ?? new string[0];
        }
    }
}