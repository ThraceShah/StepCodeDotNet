namespace StepCodeDotNet.Interop;

public unsafe partial struct Link_
{
    [NativeTypeName("struct Link_ *")]
    public Link_* next;

    [NativeTypeName("struct Link_ *")]
    public Link_* prev;

    public void* data;
}
