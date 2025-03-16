namespace StepCodeDotNet.Base;
public interface IStepObj
{
    int line_id { get; set; }
    public IExpress[] complex { get; set; }
    void Init(IExpress expression, Dictionary<int, IStepObj> refMap);
}
