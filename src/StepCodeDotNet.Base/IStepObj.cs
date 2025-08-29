namespace StepCodeDotNet.Base;

public interface IStepBaseObj;
public interface IStepObj : IStepBaseObj
{
    int type_id { get; }
    int line_tag { get; set; }

}

public interface ISubComplexObj : IStepBaseObj
{
    int type_id { get; }
}
public interface IComplexObj : IStepObj
{
    ISubComplexObj[] sub_entities { get; }
    bool add_sub_entity(ISubComplexObj subEntity);
    bool remove_sub_entity(ISubComplexObj subEntity);
}

public interface IInitableObj
{
    void Init(IStepObjCreator creator, ReadOnlySpan<IStepToken> args, Dictionary<int, IStepObj> refMap);
}
