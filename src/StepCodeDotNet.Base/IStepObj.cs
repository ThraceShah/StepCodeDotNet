namespace StepCodeDotNet.Base;

public interface IStepBaseObj;
public interface IStepObj : IStepBaseObj
{
    int line_id { get; set; }
}
