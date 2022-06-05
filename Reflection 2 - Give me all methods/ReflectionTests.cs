using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NUnit.Framework;
using Reflection;
using System.Reflection;
using System.Runtime.Loader;

[TestFixture]
public class ReflectionTests
{
    [Test]
    public void NullTest()
    {
        Assert.AreEqual(0, GiveMeAllMethods.GetMethodNames(null).Length);
    }

    [Test]
    public void NewObjectTest()
    {
        var testObject = new object();
        var methodNameArray = GiveMeAllMethods.GetMethodNames(testObject);
        Assert.IsTrue(methodNameArray.Contains("ToString"));
    }

    [Test]
    public void ReflObjectTest()
    {
        Random rand = new Random((int)DateTime.Now.Ticks);

        string stringToCompile = "using System; public class Refl { static void Main(string[] args) { Console.WriteLine(new Refl().Output()); Console.WriteLine(new Refl().AddInts(1,2)); } public string Output() { return \"Test-Output\"; } public int AddInts(int i1, int i2) {return i1 + i2;}}";

        string AddIntsName = "AddInts" + rand.Next(0, 9) + "(";
        stringToCompile = stringToCompile.Replace("AddInts(", AddIntsName);

        string className = "Refl";

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(stringToCompile);

        string assemblyName = Path.GetRandomFileName();
        var refPaths = new[] {
        typeof(Object).GetTypeInfo().Assembly.Location,
        typeof(Console).GetTypeInfo().Assembly.Location,
        Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll"),
    };
        MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);

            if (result.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);

                var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                var instance = assembly.CreateInstance(className);

                var methodNameArray = GiveMeAllMethods.GetMethodNames(instance);
                var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                var mis = instance.GetType().GetMethods(bindingFlags);
                var miss = mis.Select(o => o.Name).ToArray();

                CollectionAssert.AreEqual(miss.OrderBy(o => o), methodNameArray.OrderBy(o => o), "Different count of Methods " + methodNameArray.Count() + " vs " + miss.Count());
            }
            else
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                Assert.Fail("Compilation Failed!\n" + string.Join("\n", failures.Select(diagnostic => string.Format("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage()))));
            }
        }
    }
}