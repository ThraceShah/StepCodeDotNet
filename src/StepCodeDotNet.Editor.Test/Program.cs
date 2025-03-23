// See https://aka.ms/new-console-template for more information
using StepCodeDotNet.Base;

var stepFile = @"D:\model\parasolid_part\cube_void1.STEP";
var creator = StepCodeDotNet.Gen.ap203.StepObjCreator.Instance;
var parser = new StepCodeDotNet.Base.StepParser(creator);
var results = parser.Resolve(stepFile);
Console.WriteLine(results.Length);
