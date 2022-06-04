using System.Reflection;

namespace Reflection
{
    /// <summary>
    /// In this kata you have to add the results of a all members from an object, that are/return strings.
    /// Use only the own members of the object, and not derived members.
    /// When a member is a method, then use only if it is without parameters.
    /// Every member (The member! NOT the content of the Member!) should only be considered once.
    /// The results should be ordered descending by the length, and then ascending by content.
    /// For null-objects return an empty string.
    /// </summary>
    public class AddTheMemberResults
    {
        public static string ConcatStringMembers(object TestObject) =>
            String.Join("",
                TestObject?
                    .GetType()
                    .GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Distinct()
                    .Select(member =>
                    {
                        if (member.MemberType == MemberTypes.Method && TestObject.GetType().GetMethod(member.Name).GetParameters().Count() == 0 && TestObject.GetType().GetMethod(member.Name).Invoke(TestObject, null).GetType() == typeof(string))
                            return TestObject.GetType().GetMethod(member.Name).Invoke(TestObject, null).ToString();
                        else if (member.MemberType == MemberTypes.Field && TestObject.GetType().GetField(member.Name).GetValue(TestObject).GetType() == typeof(string))
                            return TestObject.GetType().GetField(member.Name).GetValue(TestObject).ToString();
                        else return string.Empty;
                    })
                    .OrderByDescending(member => member.Length)
                    .ThenBy(member => member)
                    .ToArray()
                ?? new string[0]);
    }
}