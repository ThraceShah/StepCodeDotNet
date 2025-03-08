namespace StepCodeDotNet.Base;
public interface IStepObjCreater
{
    IStepObj Create(string entityName);
    T Create<T>(string entityName) where T : IStepObj;
}