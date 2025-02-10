using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using StepCodeDotNet.Interop;
using static StepCodeDotNet.Interop.Express;
namespace Exp2CSharp;

unsafe class Program
{
    const int EOF = -1;
    static char* sc_optarg;
    static int sc_optind = 0;
    static char* next = null;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static void print_file(Scope_* express)
    {

    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static int success(Scope_* express)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static int Handle_FedPlus_Args(int i, sbyte* arg)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static void exp2cxx_usage()
    {
        Console.WriteLine("Usage: exp2cxx [options] file1 [file2 ...]");
    }

    static void EXPRESSinit_init()
    {
        EXPRESSbackend = &print_file;
        EXPRESSsucceed = &success;
        EXPRESSgetopt = &Handle_FedPlus_Args;
        ERRORusage_function = &exp2cxx_usage;
    }

    static void Main(string[] args)
    {
        int c;
        int rc;
        char* cp;
        int no_warnings = 1;
        int resolve = 1;
        int result;

        bool buffer_messages = false;
        Express_ model;

        EXPRESSprogram_name = Environment.GetCommandLineArgs()[0];
        ERRORusage_function = null;

        EXPRESSinit_init();

        EXPRESSinitialize();

        if (EXPRESSinit_args != null)
        {
            byte*[] argv = new byte*[args.Length + 1];
            argv[0] = (byte*)Marshal.StringToHGlobalAnsi(Environment.GetCommandLineArgs()[0]);
            for (int i = 0; i < args.Length; i++)
            {
                argv[i + 1] = (byte*)Marshal.StringToHGlobalAnsi(args[i]);
            }
            fixed (byte** argvPtr = argv)
            {
                EXPRESSinit_args(args.Length, argvPtr);
            }
            foreach (var ptr in argv)
            {
                Marshal.FreeHGlobal((nint)ptr);
            }
        }
    }
}
