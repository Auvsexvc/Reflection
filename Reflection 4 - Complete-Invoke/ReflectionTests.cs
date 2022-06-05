using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NUnit.Framework;
using Reflection;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

[TestFixture]
public class ReflectionTests
{
    [Test]
    public void NullTest()
    {
        Assert.AreEqual(null, CompleteInvoke.InvokeMethod(null));
    }

    [Test]
    public void EmptyTest()
    {
        Assert.AreEqual("", CompleteInvoke.InvokeMethod(""));
    }

    [Test]
    public void UnknownTypeTest()
    {
        Assert.AreEqual(null, CompleteInvoke.InvokeMethod("unknownType"));
    }

    [Test]
    public void SmallObjectTest()
    {
        var returnValue = CompleteInvoke.InvokeMethod("testClass");
        Assert.AreEqual(Helper.returnvalue, returnValue);
    }

    [Test]
    public void DynamicObjectTest()
    {
        Random rand = new Random((int)DateTime.Now.Ticks);
        string class1Name = Helper.RandomString;
        string class2Name = Helper.RandomString;
        string class3Name = Helper.RandomString;
        string methodName = Helper.RandomString;

        string expectedReturnValue = Helper.RandomString;

        string stringToCompileStart = "using System; public class " + class1Name + " {";

        string stringToCompileEnd = "}";

        StringBuilder sb = new StringBuilder();
        sb.Append(stringToCompileStart);
        var countDirectDependencies = rand.Next(1, 3);
        if (countDirectDependencies == 1)
        {
            sb.Append("public " + class1Name + "(" + class2Name + " i) { } ");
        }
        else
        {
            sb.Append("public " + class1Name + "(" + class2Name + " i1, " + class3Name + " i2) { } ");
        }
        sb.Append("public string " + methodName + "() { return \"" + expectedReturnValue + "\"; }");

        sb.Append("}");

        sb.Append("public class " + class2Name + " {");

        if (countDirectDependencies == 1)
        {
            sb.Append("public " + class2Name + "(" + class3Name + " i) { } ");
            sb.Append("}");
        }
        else
        {
            sb.Append("public " + class2Name + "() { } ");
            sb.Append("}");
        }

        sb.Append("public class " + class3Name + " {");
        sb.Append("public " + class3Name + "() { } ");

        sb.Append(stringToCompileEnd);

        var stringToCompile = sb.ToString();

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
                var assemblyQualifiedName = class1Name + "," + assembly.FullName;
                var returnValue = CompleteInvoke.InvokeMethod(assemblyQualifiedName);

                Assert.AreEqual(expectedReturnValue, returnValue);
            }
            else
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                Assert.Fail("Compilation Failed!\n" + string.Join("\n", failures.Select(diagnostic => string.Format("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage()))));
            }
        }
    }
}