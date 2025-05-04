// See https://aka.ms/new-console-template for more information
using StepCodeDotNet.Base;

var stepFile = @"D:\model\parasolid_part\cone2.stp";
var creator = StepCodeDotNet.Gen.config_control_design.StepObjCreator.Instance;
var parser = new StepCodeDotNet.Base.StepParser(creator);
var results = parser.Resolve(stepFile);
Console.WriteLine(results.Length);
