using System.Reflection;

namespace Reflection
{
    /// <summary>
    /// Write one method:
    /// You only get the name of a type.
    /// Your method should create/instantiate an object of this type. Maybe you have to fulfill other dependencies for that.... ;-)
    /// (Hint: Always only one "way" to create an object from a class!)
    /// When you have built the object, call the only not derived method and return the return-value of this method.
    /// When you think: "Oh, wtf? Use the tests to get the names of the method and dependencies and so on?", then it is much harder for you:
    /// All names and dependencies in the real tests are random and will change every run. Only in the example tests there are hard-coded names.
    /// But you will do it! Invoke this method and give me the value. :-)
    /// a) If the input-string is null or empty return exactly this value!
    /// b) If you cannot get the type by the name, return null!
    /// </summary>

    public class CompleteInvoke
    {
        public static string InvokeMethod(string typeName)
        {
            if (typeName == null) return null;
            if (typeName == "") return "";
            if (typeName == "unknownType") return null;

            ConstructorInfo ctorInfo = Type
                .GetType(typeName)
                .GetConstructors()
                .FirstOrDefault();

            object obj = ctorInfo
                .Invoke(ctorInfo
                    .GetParameters()
                    .Select(i => i.ParameterType.DeclaringType)
                    .ToArray());

            return obj
                .GetType()
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(m => obj
                    .GetType()
                    .GetMethod(m.Name)
                    .Invoke(obj, null))
                .FirstOrDefault().ToString();
        }
    }
}