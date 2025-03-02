using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using StepCodeDotNet.Interop;
using static StepCodeDotNet.Interop.Express;
using static StepCodeDotNet.Interop.ScopeEx;
using static StepCodeDotNet.Interop.VariableEx;
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

    // static void Main(string[] args)
    // {
    //     int c;
    //     int rc;
    //     char* cp;
    //     int no_warnings = 1;
    //     int resolve = 1;
    //     int result;

    //     bool buffer_messages = false;
    //     Express_ model;

    //     EXPRESSprogram_name = Environment.GetCommandLineArgs()[0];
    //     ERRORusage_function = null;

    //     EXPRESSinit_init();

    //     EXPRESSinitialize();

    //     if (EXPRESSinit_args != null)
    //     {
    //         byte*[] argv = new byte*[args.Length + 1];
    //         argv[0] = (byte*)Marshal.StringToHGlobalAnsi(Environment.GetCommandLineArgs()[0]);
    //         for (int i = 0; i < args.Length; i++)
    //         {
    //             argv[i + 1] = (byte*)Marshal.StringToHGlobalAnsi(args[i]);
    //         }
    //         fixed (byte** argvPtr = argv)
    //         {
    //             EXPRESSinit_args(args.Length, argvPtr);
    //         }
    //         foreach (var ptr in argv)
    //         {
    //             Marshal.FreeHGlobal((nint)ptr);
    //         }
    //     }
    // }

    const int NOTKNOWN = 1;
    const int UNPROCESSED = 2;
    const int CANTPROCESS = 3;
    const int CANPROCESS = 4;
    const int PROCESSED = 5;

    delegate void UnsafeAction<T>(T* ptr) where T : unmanaged;
    static void SCOPEdo_entities(Scope_* s, HashEntry* de, UnsafeAction<Scope_> action)
    {
        HASHlistinit_by_type(s->symbol_table, de, (sbyte)'e');
        Scope_* ent;
        while (null != (ent = (Scope_*)DICTdo(de)))
        {
            action(ent);
        }
    }
    static void SCOPEdo_types(Scope_* s, HashEntry* de, UnsafeAction<Scope_> action)
    {
        HASHlistinit_by_type(s->symbol_table, de, (sbyte)'t');
        Scope_* ent;
        while (null != (ent = (Scope_*)DICTdo(de)))
        {
            action(ent);
        }
    }

    static void initializeMarks(Scope_* express)
    {
        HashEntry de_sch, de_ent, de_type;
        Scope_* schema;

        HASHlistinit_by_type(express->symbol_table, &de_sch, OBJ_SCHEMA);
        while ((schema = (Scope_*)DICTdo(&de_sch)) != null)
        {
            schema->search_id = UNPROCESSED;
            schema->clientData = (int*)NativeMemory.Alloc(sizeof(int));
            *(int*)schema->clientData = 0;
            SCOPEdo_entities(schema, &de_ent, (ent) => ent->search_id = NOTKNOWN);
            SCOPEdo_types(schema, &de_type, (type) => type->search_id = NOTKNOWN);
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

    static void LISTdo_links(Linked_List_* list, UnsafeAction<Link_> action)
    {
        Linked_List_* __i = list;
        Link_* link;
        if (__i != (Linked_List_*)((void*)0))
        {
            for ((link) = __i->mark; ((link) = (link)->next) != __i->mark;)
            {
                action(link);
            }
        }
    }

    static void numberAttributes(Scope_* scope)
    {
        int count = 0;
        Linked_List_* list = SCOPEget_entities_superclass_order(scope);
        LISTdo<Scope_>(list, (e) =>
        {
            LISTdo_n<Variable_>(ENTITYget_attributes(e), v =>
            {
                v->idx = count++;
            });
        });
    }

    static void unsetObjs(Scope_* schema)
    {
        HashEntry de;

        SCOPEdo_types(schema, &de, t =>
        {
            if (t->search_id == CANTPROCESS)
            {
                t->search_id = NOTKNOWN;
            }

        });
        SCOPEdo_entities(schema, &de, ent =>
        {
            if (ent->search_id == CANTPROCESS)
            {
                ent->search_id = NOTKNOWN;
            }
        });
    }

    static string outputDir = string.Empty;
    static string enumOutputDir = string.Empty;
    static string typeOutputDir = string.Empty;
    static string entityOutputDir = string.Empty;
    static string entityImpOutputDir = string.Empty;
    static string globalOutputFile = string.Empty;
    static string schemaName = string.Empty;

    static string NameSpace => $"Exp2CSharp.Gen.{schemaName}";
    static Dictionary<string, List<string>> entitySelectsMap = new();
    static void Main()
    {
        var schemaPath = @"D:\code\fsharp\STEPSharp\schemas\ap203\ap203.exp";
        Span<byte> span = Encoding.ASCII.GetBytes(schemaPath);
        EXPRESSinitialize();
        Scope_* model = EXPRESScreate();
        EXPRESSparse(model, null, (sbyte*)Unsafe.AsPointer(ref span.GetPinnableReference()));
        if (ERRORoccurred)
        {
            EXPRESSdestroy(model);
            Console.WriteLine("Error parsing schema");
            return;
        }
        EXPRESSresolve(model);
        if (ERRORoccurred)
        {
            int result = EXPRESS_fail(model);
            EXPRESScleanup();
            EXPRESSdestroy(model);
            Console.WriteLine("Error resolving schema");
            return;
        }
        schemaName = Path.GetFileNameWithoutExtension(schemaPath);
        outputDir = Path.Combine(Environment.CurrentDirectory, $"../StepCodeDotNet.Gen/{schemaName}");
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        enumOutputDir = Path.Combine(outputDir, "Enums");
        if (!Directory.Exists(enumOutputDir))
        {
            Directory.CreateDirectory(enumOutputDir);
        }
        typeOutputDir = Path.Combine(outputDir, "Types");
        if (!Directory.Exists(typeOutputDir))
        {
            Directory.CreateDirectory(typeOutputDir);
        }
        entityOutputDir = Path.Combine(outputDir, "Entities");
        if (!Directory.Exists(entityOutputDir))
        {
            Directory.CreateDirectory(entityOutputDir);
        }
        entityImpOutputDir = Path.Combine(outputDir, "EntitiesImp");
        if (!Directory.Exists(entityImpOutputDir))
        {
            Directory.CreateDirectory(entityImpOutputDir);
        }
        globalOutputFile = Path.Combine(outputDir, "GlobalUsing.cs");
        if (File.Exists(globalOutputFile))
        {
            File.Delete(globalOutputFile);
        }
        PrintBaseDef();
        PrintSchema(model);
        EXPRESScleanup();
        EXPRESSdestroy(model);
    }

    static Dictionary<string, string> typeMap = new()
    {
        ["REAL"] = "System.Double",
        ["INTEGER"] = "System.Int32",
        ["STRING"] = "System.String",
        ["BOOLEAN"] = "System.Boolean",
        ["NUMBER"] = "System.Double",
    };

    static void PrintBaseDef()
    {
        using var writer = new StreamWriter(globalOutputFile);
        writer.WriteLine("global using System;");
        writer.WriteLine("global using System.Collections.Generic;");
        writer.WriteLine("global using System.IO;");
        writer.WriteLine("global using System.Linq;");
        writer.WriteLine("global using System.Text;");
        writer.WriteLine("global using Exp2CSharp.Gen;");
        foreach (var (key, value) in typeMap)
        {
            writer.WriteLine($"global using {key}={value};");
        }
    }

    static void PrintEnumDef(Scope_* t)
    {
        var enumName = t->symbol.Name;
        var fileName = Path.Combine(enumOutputDir, $"{enumName}.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.WriteLine($"public enum {enumName}");
        writer.WriteLine("{");
        LISTdo_links(TYPEget_body(t)->list, link =>
        {
            var expr = (Expression_*)link->data;
            writer.WriteLine($"    {expr->symbol.Name},");
        });
        writer.WriteLine("}");
    }

    static void PrintSelectDef(Scope_* t)
    {
        var selectName = t->symbol.Name;
        var fileName = Path.Combine(typeOutputDir, $"{selectName}.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.WriteLine($"public interface {selectName}");
        writer.WriteLine("{");
        LISTdo_links(TYPEget_body(t)->list, link =>
        {
            var expr = (Expression_*)link->data;
            var key = expr->symbol.Name;
            if (!entitySelectsMap.ContainsKey(key))
            {
                entitySelectsMap[key] = [];
            }
            entitySelectsMap[key].Add(selectName);
        });
        writer.WriteLine("}");
    }

    static void PrintAggregateDef(Scope_* t)
    {
        var aggregateName = t->symbol.Name;
        var baseType = TYPEget_base_type(t);
        var fileName = Path.Combine(typeOutputDir, $"{aggregateName}.cs");
        var bodyType = TYPEget_body(t);
        using var globalWriter = new StreamWriter(globalOutputFile, true);
        var baseTypeString = GetTypeNameString(baseType);
        var typeString = CollectionTypeToString(bodyType->type, $"{NameSpace}.{baseTypeString}");
        globalWriter.WriteLine($"global using {aggregateName}=System.Collections.Generic.{typeString};");
    }

    static string CollectionTypeToString(type_enum collectionType, string contentType) => collectionType switch
    {
        type_enum.bag_ => $"List<{contentType}>",
        type_enum.set_ => $"HashSet<{contentType}>",
        type_enum.list_ => $"List<{contentType}>",
        type_enum.array_ => $"{contentType}[]",
        _ => throw new NotImplementedException()
    };

    static string BaseTypeToString(Scope_* type) => TYPEget_body(type)->type switch
    {
        type_enum.integer_ => "INTEGER",
        type_enum.real_ => "REAL",
        type_enum.number_ => "NUMBER",
        type_enum.string_ => "STRING",
        type_enum.boolean_ => "BOOLEAN",
        type_enum.logical_ => "LOGICAL",
        _ => type->symbol.Name
    };

    static string GetTypeNameString(Scope_* type)
    {
        if (TYPEis_enumeration(type))
        {
            return type->symbol.Name;
        }
        else if (TYPEis_select(type))
        {
            return type->symbol.Name;
        }
        else if (TYPEis_aggregate(type))
        {
            var baseType = TYPEget_base_type(type);
            var bodyType = TYPEget_body(type);
            var typeString = CollectionTypeToString(bodyType->type, GetTypeNameString(baseType));
            return typeString;
        }
        else
        {
            return BaseTypeToString(type);
        }
    }

    static void PrintUsingTypeDef(Scope_* type)
    {
        using var globalWriter = new StreamWriter(globalOutputFile, true);
        var baseType = BaseTypeToString(type);
        string typeString = baseType;
        while (typeMap.TryGetValue(typeString, out baseType))
        {
            typeString = baseType;
        }
        globalWriter.WriteLine($"global using {type->symbol.Name}={typeString};");
    }

    static void PrintTypeDef(Scope_* type)
    {
        Scope_* i;
        bool aggrNot1d = true;
        if (TYPEis_enumeration(type))
        {
            PrintEnumDef(type);
        }
        else if (TYPEis_select(type))
        {
            PrintSelectDef(type);
        }
        else if (TYPEis_aggregate(type))
        {
            PrintAggregateDef(type);
        }
        else
        {
            PrintUsingTypeDef(type);
        }
    }

    static unsafe List<nint> entities = new();

    static void PrintEntityDef(Scope_* entity)
    {
        var entityName = entity->symbol.Name;
        var fileName = Path.Combine(entityOutputDir, $"{entityName}.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.Write($"public interface {entityName}");
        List<string> superNames = [];
        if (entitySelectsMap.TryGetValue(entityName, out var selects))
        {
            //writer.Write($": {string.Join(", ", selects)}");
            superNames.AddRange(selects);
        }
        var supertypes = ENTITYget_supertypes(entity);
        if (LISTempty(supertypes) is false)
        {
            (*supertypes).For<Scope_>(super =>
            {
                superNames.Add(super->symbol.Name);
            });
        }
        if (superNames.Count > 0)
        {
            writer.Write($": {string.Join(", ", superNames)}");
        }
        writer.WriteLine();
        writer.WriteLine("{");
        var attributes = ENTITYget_attributes(entity);
        if (LISTempty(attributes) is false)
        {
            (*attributes).For<Variable_>(attr =>
            {
                if (VARis_derived(attr) is false)
                {
                    var typeName = GetTypeNameString(attr->type);
                    var attrName = attr->name->symbol.Name;
                    writer.WriteLine($"    {typeName} {attrName} {{ get; set; }}");
                }
            });
        }
        writer.WriteLine("}");

        entities.Add((nint)entity);

    }

    static void PrintEntityImpes()
    {
        foreach (Scope_* entity in entities)
        {
            var entityName = entity->symbol.Name;
            var fileName = Path.Combine(entityImpOutputDir, $"{entityName}.cs");
            using var writer = new StreamWriter(fileName);
            writer.WriteLine($"namespace {NameSpace};");
            writer.WriteLine($"public class {entityName}_imp : {entityName}");
            writer.WriteLine("{");
            var allAttributes = ENTITYget_all_attributes(entity);
            if (LISTempty(allAttributes) is false)
            {
                var attrsSet = new HashSet<string>();
                (*allAttributes).For<Variable_>(attr =>
                {
                    if (VARis_derived(attr) is false)
                    {
                        var typeName = GetTypeNameString(attr->type);
                        var attrName = attr->name->symbol.Name;
                        if (attrsSet.Add(attrName))
                        {
                            writer.WriteLine($"    public {typeName} {attrName} {{ get; set; }}");
                        }
                    }
                });
            }
            writer.WriteLine("}");

        }
    }

    static void PrintSchema(Scope_* express)
    {
        Scope_* schema = null;
        HashEntry de = new();

        initializeMarks(express);
        HASHlistinit_by_type(express->symbol_table, &de, OBJ_SCHEMA);
        while ((schema = (Scope_*)DICTdo(&de)) != null)
        {
            numberAttributes(schema);
        }
        bool complete = false;
        while (!complete)
        {
            complete = true;
            HASHlistinit_by_type(express->symbol_table, &de, OBJ_SCHEMA);
            while (null != (schema = (Scope_*)DICTdo(&de)))
            {
                if (schema->search_id == UNPROCESSED)
                {
                    unsetObjs(schema);
                    SCOPEdo_types(schema, &de, PrintTypeDef);
                    SCOPEdo_entities(schema, &de, PrintEntityDef);
                    PrintEntityImpes();
                    schema->search_id = PROCESSED;
                    complete = complete && (schema->search_id == PROCESSED);
                }
            }

        }
    }
}
