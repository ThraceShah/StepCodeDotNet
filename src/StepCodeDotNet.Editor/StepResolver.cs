namespace StepCodeDotNet.Editor;

using System.Text;
using System.Text.RegularExpressions;
using StepCodeDotNet.Base;

public partial class StepResolver(IStepObjCreator creater)
{
    public void Resolve
}

// public partial class StepResolver(IStepObjCreator creater)
// {
//     public List<IStepObj> StepEntities { get; set; } = [];


//     public void Resolve(List<LineExpress> expressList)
//     {
//         Dictionary<int, IStepObj> lineObjMap = new();
//         foreach (var lineExpress in expressList)
//         {
//             switch (lineExpress.Body)
//             {
//                 case EntityExpress entityExpress:
//                     {
//                         //generate by EntityExpress
//                         break;
//                     }
//                 case ListExpress listExpress:
//                     {
//                         var complex = new StepComplex();
//                         complex.line_id = lineExpress.LineNumber;
//                         StepEntities.Add(complex);
//                         lineObjMap.Add(lineExpress.LineNumber, complex);
//                         break;
//                     }
//                 default:
//                     throw new Exception($"Invalid line:{lineExpress.LineNumber}");
//             }
//         }
//     }

//     private void InitStepObjs(Dictionary<int, LineExpress> lineExpMap)
//     {
//         foreach (var stepObj in StepEntities)
//         {
//             if (stepObj is StepComplex complex)
//             {
//                 var complexObjs = new List<IStepObj>();
//                 foreach (var stepObj in complex.complex)
//                 {
//                     if (stepObj is not StepComplex)
//                     {
//                         throw new Exception("Invalid complex");
//                     }
//                     complexObjs.Add(stepObj);
//                 }
//                 complex.complex = complexObjs.ToArray();
//             }
//         }
//     }

//     private void InitStepEntiyObj(LineExpress lineExp, Dictionary<int, IStepObj> lineExpMap)
//     {
//     }

//     private void InitStepEntiyObj(EntityExpress entityExp, IStepObj entityObj, Dictionary<int, IStepObj> lineExpMap)
//     {
//         creater.GetInitArgTypes(entityExp.EntityName);
//         object[] args = new object[entityExp.Args.Count];
//         for (int i = 0; i < entityExp.Args.Count; i++)
//         {
//             var argExp = entityExp.Args[i];
//             switch (argExp)
//             {
//                 case StringExpress stringExp:
//                     args[i] = stringExp.Value;
//                     break;
//                 case IntegerExpress integerExp:
//                     args[i] = integerExp.Value;
//                     break;
//                 case RealExpress realExp:
//                     args[i] = realExp.Value;
//                     break;
//                 case BooleanExpress booleanExp:
//                     args[i] = booleanExp.Value;
//                     break;
//                 case EnumExpress enumExp:
//                     args[i] = creater.ToEnum(enumExp.Value);
//                     break;
//                 case RefExpress refExp:
//                     args[i] = lineExpMap[refExp.RefLineNumber];
//                     break;
//                 case EntityExpress subEntityExp:
//                     var entity = creater.Create(subEntityExp.EntityName);
//                     InitStepEntiyObj(subEntityExp, entity, lineExpMap);
//                     args[i] = entity;
//                     break;
//                 case ListExpress listExp:
//                     break;
//                 default:
//                     throw new Exception("Invalid entity");
//             }
//         }


//     }

//     private void InitStepComplexObjs(LineExpress lineExp, Dictionary<int, IStepObj> lineExpMap)
//     {
//         var stepObj = (StepComplex)lineExpMap[lineExp.LineNumber];
//         var listExpress = (ListExpress)lineExp.Body;
//         var complexObjs = new IStepObj[listExpress.ExpressList.Count];
//         for (int i = 0; i < listExpress.ExpressList.Count; i++)
//         {
//             var express = listExpress.ExpressList[i];
//             if (express is EntityExpress entityExpress)
//             {
//                 var entity = creater.Create(entityExpress.EntityName);
//             }
//             else
//             {
//                 throw new Exception("Invalid complex");
//             }
//         }
//     }

// }
