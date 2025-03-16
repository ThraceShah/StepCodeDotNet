// See https://aka.ms/new-console-template for more information
using StepCodeDotNet.Base;

var stepFile = @"D:\code\csharp\myopensource\StepCodeDotNet\stepfiles\cube.STEP";
var creator = StepCodeDotNet.Gen.ap203.StepObjCreator.Instance;
var parser = new StepCodeDotNet.Base.StepParser(creator);
var results = parser.Resolve(stepFile);
Console.WriteLine(results.Length);
