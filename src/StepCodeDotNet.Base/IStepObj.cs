namespace StepCodeDotNet.Base;
public interface IStepObj
{
    int line_id { get; set; }
    void Init(IExpress expression, Dictionary<int, IStepObj> refMap);
}
