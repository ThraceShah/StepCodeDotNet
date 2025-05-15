// See https://aka.ms/new-console-template for more information
using StepCodeDotNet.Base;

var stepFile = @"D:\model\parasolid_part\cone2.stp";
var creator = StepCodeDotNet.Gen.ap203_configuration_controlled_3d_design_of_mechanical_parts_and_assemblies_mim_lf.StepObjCreator.Instance;
var parser = new StepCodeDotNet.Base.StepParser(creator);
var results = parser.Resolve(stepFile);
Console.WriteLine(results.Length);
