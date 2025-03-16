namespace StepCodeDotNet.Base;
public class StepComplex : IStepObj
{
    public int line_id { get; set; }
    public IStepObj[] complex { get; set; }
    public void Init(Express expression, Dictionary<int, IStepObj> refMap)
    {
        throw new NotImplementedException();
    }
}
