namespace StepCodeDotNet.Interop;

public unsafe partial struct freelist_head
{
    public int size_elt;

    public int alloc;

    public int dealloc;

    public int create;

    public void* max;

    public int size;

    public int bytes;

    public freelist* freelist;
}
