namespace StepCodeDotNet.Base;
using System.Collections.Frozen;
public interface IStepObjCreator
{
    // IStepObj Create(string entityName);
    // T Create<T>(string entityName) where T : IStepObj;
    // int ToEnum(string value);
    // bool IsBuiltInType(string typeName);
    // FrozenSet<string> GetInitArgTypes(string entityName);

    IStepObj[] CreateStepObjs(List<LineExpress> lineExpresses);
}
