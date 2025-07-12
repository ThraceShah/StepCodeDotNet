// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using StepCodeDotNet.Base;

var zipFile = Path.Combine(AppContext.BaseDirectory, "step_file.zip");
var stepFile = Path.Combine(AppContext.BaseDirectory, "step_file.stp");
if (File.Exists(stepFile) is false)
{
    // Unzip the file
    System.IO.Compression.ZipFile.ExtractToDirectory(zipFile, AppContext.BaseDirectory, true);
}
if (!File.Exists(stepFile))
{
    Console.WriteLine($"Step file not found: {stepFile}");
    return;
}
var creator = StepCodeDotNet.Gen.config_control_design.StepObjCreator.Instance;
var parser = new StepCodeDotNet.Base.StepParser(creator);
var watch = new Stopwatch();
watch.Start();
parser.Resolve(stepFile);
var results = parser.GetStepObjs();
watch.Stop();
Console.WriteLine($"Parsing took {watch.ElapsedMilliseconds} ms");
Console.WriteLine(results.Length);
var memoryUsed = StepCodeDotNet.Base.StepParser.GetMemoryUsedMB();
Console.WriteLine($"After parser all Memory used: {memoryUsed} MB");