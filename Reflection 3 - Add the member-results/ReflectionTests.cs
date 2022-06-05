using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NUnit.Framework;
using Reflection;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

public class testClass
{
    public string Output1()
    {
        return "Output";
    }

    public string Output2()
    {
        return "It";
    }
}

[TestFixture]
public class ReflectionTests
{
    [Test]
    public void NullTest()
    {
        Assert.AreEqual("", AddTheMemberResults.ConcatStringMembers(null));
    }

    [Test]
    public void SmallObjectTest()
    {
        var testObject = new testClass();
        var concattedString = AddTheMemberResults.ConcatStringMembers(testObject);
        Assert.AreEqual("OutputIt", concattedString);
    }

    [Test]
    public void BigObjectTest()
    {
        Random rand = new Random((int)DateTime.Now.Ticks);

        string stringToCompileStart = "using System; public class Refl {";

        string stringToCompileEnd = "}";

        StringBuilder sb = new StringBuilder();
        sb.Append(stringToCompileStart);
        sb.Append("public string Output(int i) { return \"Test-Output\" + i; } public int AddInts(int i1, int i2) {return i1 + i2;}");
        var r = rand.Next(0, 9);
        sb.Append("public string MH" + r + "() { return \"MH" + r + "\"; }");
        sb.Append("public string MH" + (r + 1) + " { get { return \"MH" + (r + 1) + "\"; } }");
        sb.Append("public string MH" + (r + 2) + " = \"MH" + (r + 2) + "\";");

        var membersCount = rand.Next(12, 20);
        for (int i = 0; i < membersCount; i++)
        {
            int memberType = rand.Next(0, 3);
            int memberTypeLength = rand.Next(4, 10);
            string memberName = "";
            for (int j = 0; j < memberTypeLength; j++)
            {
                memberName += (char)rand.Next(65, 90);
            }

            int stringMemberContentLength = rand.Next(4, 10);
            string stringMemberContent = "";
            for (int j = 0; j < stringMemberContentLength; j++)
            {
                stringMemberContent += (char)rand.Next(65, 90);
            }

            int intMemberContent = rand.Next(0, 100);
            double doubleMemberContent = Math.Round(rand.NextDouble(), 2);

            string member = "";
            switch (rand.Next(0, 3))
            {
                case 0: // Method
                    if (memberType == 0) // string
                    {
                        member = "public string " + memberName + "() { return \"" + stringMemberContent + "\"; }";
                    }
                    if (memberType == 1) // int
                    {
                        member = "public int " + memberName + "() { return " + intMemberContent + "; }";
                    }
                    if (memberType == 2) // double
                    {
                        member = "public double " + memberName + "() { return " + doubleMemberContent + "; }";
                    }

                    break;

                case 1: // Property
                    if (memberType == 0) // string
                    {
                        member = "public string " + memberName + " { get { return \"" + stringMemberContent + "\"; } }";
                    }
                    if (memberType == 1) // int
                    {
                        member = "public int " + memberName + " { get { return " + intMemberContent + "; } }";
                    }
                    if (memberType == 2) // double
                    {
                        member = "public double " + memberName + " { get { return " + doubleMemberContent + "; } }";
                    }

                    break;

                case 2: // Field
                    if (memberType == 0) // string
                    {
                        member = "public string " + memberName + " = \"" + stringMemberContent + "\";";
                    }
                    if (memberType == 1) // int
                    {
                        member = "public int " + memberName + " = " + intMemberContent + ";";
                    }
                    if (memberType == 2) // double
                    {
                        member = "public double " + memberName + " = " + doubleMemberContent + ";";
                    }

                    break;
            }
            sb.Append(member);
        }

        sb.Append(stringToCompileEnd);

        var stringToCompile = sb.ToString();
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

                var concattedString = AddTheMemberResults.ConcatStringMembers(instance);

                var members = instance.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                List<string> list = new List<string>();
                foreach (var mi in members)
                {
                    if (mi is MethodInfo)
                    {
                        if (((MethodInfo)mi).ReturnType == typeof(string))
                        {
                            if (((MethodInfo)mi).GetParameters().Length == 0)
                            {
                                list.Add((string)((MethodInfo)mi).Invoke(instance, new object[0]));
                            }
                        }
                    }
                    if (mi is FieldInfo)
                    {
                        if (((FieldInfo)mi).FieldType == typeof(string))
                        {
                            list.Add((string)((FieldInfo)mi).GetValue(instance));
                        }
                    }
                    /* // not necessary; is already added by the "get_"-Methods for the Properties
                    if(mi is PropertyInfo)
                    {
                        if(((PropertyInfo)mi).PropertyType == typeof(string))
                        {
                            list.Add((string)((PropertyInfo)mi).GetValue(instance));
                        }
                    }*/
                }

                var expected = string.Concat(list.OrderByDescending(s => s.Length).ThenBy(s => s));

                Assert.AreEqual(expected, concattedString);
            }
            else
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                Assert.Fail("Compilation Failed!\n" + string.Join("\n", failures.Select(diagnostic => string.Format("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage()))));
            }
        }
    }
}