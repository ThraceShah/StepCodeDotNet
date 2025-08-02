namespace StepCodeDotNet.Base;

public interface IStepBaseObj;
public interface IStepObj : IStepBaseObj
{
    int line_tag { get; set; }
}
public interface IComplex<T> : IStepObj where T : IStepObj
{
    List<T> sub_entities { get; }
}
