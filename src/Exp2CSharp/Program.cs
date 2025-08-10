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
            string[][] defaultArgs = [
                ["data/ap203e2/ap203e2_mim_lf.exp","src/StepCodeDotNet.Gen/ap203e2"],
                ["data/ap214e3/AP214E3_2010.exp","src/StepCodeDotNet.Gen/ap214e3_2010"],
                ["data/ap242/242_n8324_mim_lf.exp","src/StepCodeDotNet.Gen/ap242"]];
            foreach (var arg in defaultArgs)
            {
                Run(arg);
            }
            return;
        }
        Run(args);
    }

    static void Run(string[] args)
    {
        var schemaPath = Path.Combine(Environment.CurrentDirectory, args[0]);
        if (!File.Exists(schemaPath))
        {
            System.Console.WriteLine($"File {schemaPath} not found");
            return;
        }
        var outputPath = Path.Combine(Environment.CurrentDirectory, args[1]);
        var expResolver = new ExpResolver2(args[0], outputPath);
        expResolver.Resolve();

    }
}
