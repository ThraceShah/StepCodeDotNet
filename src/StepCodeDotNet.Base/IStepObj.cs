namespace StepCodeDotNet.Base;

public interface IStepBaseObj;
public interface IStepObj : IStepBaseObj
{
    int line_tag { get; set; }
}
public interface IComplexObj
{
    bool Is<SubEntity>(out SubEntity result) where SubEntity : class, IStepObj;
}

public interface IInitableObj
{
    void Init(IStepObjCreator creator, ReadOnlySpan<IStepToken> args, Dictionary<int, IStepObj> refMap);
}
