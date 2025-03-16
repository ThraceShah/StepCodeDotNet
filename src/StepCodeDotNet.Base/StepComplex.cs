namespace StepCodeDotNet.Base;
public abstract class StepComplex : IStepObj
{
    public int line_id { get; set; }
    public IExpress[] complex { get; set; }
    public abstract void Init(IExpress expression, Dictionary<int, IStepObj> refMap);
}
