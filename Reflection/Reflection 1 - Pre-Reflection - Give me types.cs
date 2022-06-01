namespace Reflection
{
    /// <summary>
    /// So, there is a list of objects and you have to get the type of every object.
    /// There is no return value, so you have to return the types over the list.
    /// If an object is null, so there is no type! So you do not have to set anything for null-Objects.
    /// </summary>
    internal class Reflection_1___Pre_Reflection___Give_me_types
    {
        public static void GetTypes(List<Tuple<object, Type>> objectTypes)
        {
            List<Tuple<object, Type>> tempList = new List<Tuple<object, Type>>();

            tempList = objectTypes
                .Select(t => t.Item1 is null
                    ? (Tuple.Create(t.Item1, t.Item2))
                    : (Tuple.Create(t.Item1, t.Item1.GetType())))
                .ToList();

            objectTypes.Clear();
            objectTypes.AddRange(tempList);
        }
    }
}