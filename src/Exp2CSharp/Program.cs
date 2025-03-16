using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using StepCodeDotNet.Interop;
using static StepCodeDotNet.Interop.IExpress;
using static StepCodeDotNet.Interop.ScopeEx;
using static StepCodeDotNet.Interop.VariableEx;
namespace Exp2CSharp;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            System.Console.WriteLine("Usage: Exp2CSharp <schema file> <output file>");
            return;
        }
        var schemaPath = Path.Combine(Environment.CurrentDirectory, args[0]);
        if (!File.Exists(schemaPath))
        {
            System.Console.WriteLine($"File {schemaPath} not found");
            return;
        }
        var outputPath = Path.Combine(Environment.CurrentDirectory, args[1]);
        var expResolver = new ExpResolver(args[0], outputPath);
        expResolver.Resolve();
    }
}
