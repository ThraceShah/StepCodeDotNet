// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using StepCodeDotNet.Base;
using StepCodeDotNet.Gen;
using StepCodeDotNet.Gen.ap203_configuration_controlled_3d_design_of_mechanical_parts_and_assemblies_mim_lf;

var productDef = new product_definition_imp();
if (productDef is approval_item approval_Item)
{
    Console.WriteLine("It's an approval item");
    var typeName = productDef.GetType().Name;
    Console.WriteLine($"Found stepObj with line_tag 58: {typeName}");
    var targetType = typeof(approval_item);
    Console.WriteLine($"Target type: {targetType.Name}");
    if (targetType.IsAssignableFrom(productDef.GetType()) is false)
    {
        Console.WriteLine($"Warning: {typeName} is not assignable to {targetType.Name}");
    }
}
else
{
    Console.WriteLine("It's NOT an approval item");
}
var zipFile = Path.Combine(AppContext.BaseDirectory, "step_file.zip");
var stepFile = Path.Combine(AppContext.BaseDirectory, "step_file.stp");
stepFile = @"/Users/thrace/code/csharp/StepCodeDotNet/stepfiles/cube.STEP";
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
// var creator = new StepCodeDotNet.Gen.automotive_design.StepObjCreator();
// var creator = new StepCodeDotNet.Gen.ap242_managed_model_based_3d_engineering_mim_lf.StepObjCreator();
var creator = new StepCodeDotNet.Gen.ap203_configuration_controlled_3d_design_of_mechanical_parts_and_assemblies_mim_lf.StepObjCreator();
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