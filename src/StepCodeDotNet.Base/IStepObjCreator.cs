namespace StepCodeDotNet.Base;
using System.Collections.Frozen;
public interface IStepObjCreator
{
    IStepObj[] CreateStepObjs(List<LineExpress> lineExpresses);
}
