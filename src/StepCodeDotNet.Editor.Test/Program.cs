// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using StepCodeDotNet.Base;

var stepFile = @"D:\model\protofiles\大模型\42u-wof-rack-top-assy-v2.STEP";
var creator = StepCodeDotNet.Gen.config_control_design.StepObjCreator.Instance;
var parser = new StepCodeDotNet.Base.StepParser(creator);
var watch = new Stopwatch();
watch.Start();
var results = parser.Resolve(stepFile);
watch.Stop();
Console.WriteLine($"Parsing took {watch.ElapsedMilliseconds} ms");
Console.WriteLine(results.Length);
