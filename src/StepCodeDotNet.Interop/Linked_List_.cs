namespace StepCodeDotNet.Interop;

public unsafe partial struct Linked_List_
{
    [NativeTypeName("Link")]
    public Link_* mark;
}

public static unsafe class Linked_list_ex
{
    public delegate void UnSafeAction<T>(T* data) where T : unmanaged;
    public static void For<T>(Linked_List_* list, UnSafeAction<T> action) where T : unmanaged
    {
        for (var _bp = list->mark; (_bp = _bp->next) != list->mark;)
        {
            action((T*)_bp->data);
        }
    }

    public static void For<T>(this Linked_List_ list, UnSafeAction<T> action) where T : unmanaged
    {
        for (var _bp = list.mark; (_bp = _bp->next) != list.mark;)
        {
            action((T*)_bp->data);
        }
    }

    public delegate void UnSafeAction2(void* data);

    public static void For(this Linked_List_ list, UnSafeAction2 action)
    {
        for (var _bp = list.mark; (_bp = _bp->next) != list.mark;)
        {
            action(_bp->data);
        }
    }

    public static void For(Linked_List_* list, UnSafeAction2 action)
    {
        for (var _bp = list->mark; (_bp = _bp->next) != list->mark;)
        {
            action(_bp->data);
        }
    }



}