using System.Reflection;

namespace Reflection
{
    /// <summary>
    /// You get an object and should return the names of all(!) methods, that you found for the object.
    /// For using random, the Name of the AddInts-Methods has an additonal number at the end. For null return an empty string array!
    /// </summary>
    internal partial class GiveMeAllMethods
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