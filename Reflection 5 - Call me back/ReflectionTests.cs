using NUnit.Framework;

namespace Reflection
{
    [TestFixture]
    public class ReflectionTests
    {
        private static bool setting;

        public static void ForActivateCallMe()
        {
            setting = true;
        }

        [Test]
        public void Test()
        {
            CallMeBack.Activator();
            Assert.IsTrue(setting);
        }

        // For this kata there is no chance for random tests
    }
}