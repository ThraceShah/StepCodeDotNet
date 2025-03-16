namespace StepCodeDotNet.Base;
public interface IStepObj
{
    int line_id { get; set; }
    IStepObj[] complex { get; set; }

    void Init(Express expression, Dictionary<int, IStepObj> refMap);
}
