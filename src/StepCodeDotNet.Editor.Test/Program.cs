// See https://aka.ms/new-console-template for more information
using StepCodeDotNet.Editor;

var stepFile = @"D:\code\csharp\myopensource\StepCodeDotNet\stepfiles\cube.STEP";
var creator = new StepCodeDotNet.Gen.ap203.StepObjCreater();
var stepResolver = new StepResolver(creator);
stepResolver.Resolve(stepFile);
