namespace StepCodeDotNet.Interop;

public unsafe partial struct Linked_List_
{
    [NativeTypeName("Link")]
    public Link_* mark;
}

public static unsafe class Linked_list_ex
{
    public delegate void UnsafeAction<T>(T* data) where T : unmanaged;
    public static void For<T>(Linked_List_* list, UnsafeAction<T> action) where T : unmanaged
    {
        for (var _bp = list->mark; (_bp = _bp->next) != list->mark;)
        {
            action((T*)_bp->data);
        }
    }

    public static void For<T>(this Linked_List_ list, UnsafeAction<T> action) where T : unmanaged
    {
        for (var _bp = list.mark; (_bp = _bp->next) != list.mark;)
        {
            action((T*)_bp->data);
        }
    }

    public delegate void UnsafeAction2(void* data);

    public static void For(this Linked_List_ list, UnsafeAction2 action)
    {
        for (var _bp = list.mark; (_bp = _bp->next) != list.mark;)
        {
            action(_bp->data);
        }
    }

    public static void For(Linked_List_* list, UnsafeAction2 action)
    {
        for (var _bp = list->mark; (_bp = _bp->next) != list->mark;)
        {
            action(_bp->data);
        }
    }

    static void LISTdo<T>(Linked_List_* list, UnsafeAction<T> action) where T : unmanaged
    {
        LISTdo_n(list, action);
    }

    static void LISTdo_n<T>(Linked_List_* list, UnsafeAction<T> action) where T : unmanaged
    {
        Linked_List_* _bl = list;
        T* v;
        Link_* _bp;
        if (_bl != null)
        {
            for (_bp = _bl->mark; (_bp = _bp->next) != _bl->mark;)
            {
                v = (T*)_bp->data;
                action(v);
            }
        }
    }


}