using System.Collections;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using StepCodeDotNet.Base;
using StepCodeDotNet.Interop;
using static StepCodeDotNet.Interop.IExpress;
using static StepCodeDotNet.Interop.ScopeEx;
using static StepCodeDotNet.Interop.VariableEx;
namespace Exp2CSharp;

interface IStepDefine
{
    string Name { get; set; }
}

interface IStepDataType : IStepDefine
{
    public List<IStepDefine> SuperTypes { get; set; }
}

class StepEnum : IStepDataType
{
    public string Name { get; set; } = string.Empty;
    public List<string> Values { get; set; } = [];
    public List<IStepDefine> SuperTypes { get; set; } = [];
}

class StepAggregate : IStepDataType
{
    public string Name { get; set; } = string.Empty;
    public IStepDefine? ValueType { get; set; }
    public List<IStepDefine> SuperTypes { get; set; } = [];
}

class StepSelect : IStepDataType
{
    public string Name { get; set; } = string.Empty;
    public List<IStepDataType> UnionTypes { get; set; } = [];
    public List<IStepDefine> SuperTypes { get; set; } = [];

}

interface IStepAttribute
{
    string Name { get; }
    bool IsCollection { get; }
}

record StepAttribute(IStepDefine Type, string Name, StepEntity SubEntity);

class StepEntity : IStepDataType
{
    public string Name { get; set; } = string.Empty;
    public List<IStepDefine> SuperTypes { get; set; } = [];
    public List<StepAttribute> Attributes { get; set; } = [];
    public List<StepAttribute> DerivedAttributes { get; set; } = [];
    public List<StepAttribute> SelfAttributes { get; set; } = [];

}

class StepBaseDefine : IStepDataType
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<IStepDefine> SuperTypes { get; set; } = [];
}

class StepSchema : IStepDefine
{
    public string Name { get; set; } = string.Empty;
}

unsafe class ExpResolver2
{
    const string CSPROJ = """
    <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>disable</Nullable>
        <NoWarn>CS8981</NoWarn>
    </PropertyGroup>
    <ItemGroup>
        <Reference Include="StepCodeDotNet.Base">
            <HintPath>StepCodeDotNet.Base.dll</HintPath>
        </Reference>
    </ItemGroup>
    </Project>
    """;

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

    readonly string outputDir;
    readonly string globalOutputFile;
    string schemaName;
    readonly string schemaPath;
    readonly Dictionary<string, string> typeMap = new()
    {
        ["REAL"] = "System.Double",
        ["INTEGER"] = "System.Int32",
        ["STRING"] = "System.String",
        ["BOOLEAN"] = "System.Boolean",
        ["NUMBER"] = "System.Double",
        ["BINARY"] = "System.Byte[]",
    };

    readonly Dictionary<string, string> typeValueGetFuncMap = new()
    {
        ["REAL"] = "GetRealValue",
        ["INTEGER"] = "GetIntegerValue",
        ["STRING"] = "GetStringValue",
        ["BOOLEAN"] = "GetBooleanValue",
        ["NUMBER"] = "GetRealValue",
        ["BINARY"] = "GetBinaryValue",
    };

    HashSet<string> complexSubEntities = [];

    (List<string> lefts, List<string> rights, List<string> complexies) complexEntities = ([], [], []);

    Dictionary<string, StepBaseDefine> baseNameDict = [];

    string NameSpace => $"StepCodeDotNet.Gen.{schemaName}";

    public ExpResolver2(string schemaPath, string outputDir)
    {
        this.schemaPath = schemaPath;
        this.outputDir = outputDir;
        schemaName = Path.GetFileNameWithoutExtension(schemaPath);
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        globalOutputFile = Path.Combine(outputDir, "GlobalUsing.cs");
        if (File.Exists(globalOutputFile))
        {
            File.Delete(globalOutputFile);
        }
    }

    public void Resolve()
    {
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
        PrintGlobalDef();
        //PrintSchema(model);
        NewSchemaResolver(model);
        EXPRESScleanup();
        EXPRESSdestroy(model);
        PrintConstFiles();
        CopyStepCodeDotNetBase();
    }

    void CopyStepCodeDotNetBase()
    {
        var baseDir = AppContext.BaseDirectory;
        var baseDll = Path.Combine(baseDir, "StepCodeDotNet.Base.dll");
        var destDll = Path.Combine(outputDir, "StepCodeDotNet.Base.dll");
        Console.WriteLine($"Copy {baseDll} to {destDll}");
        File.Copy(baseDll, destDll, true);
    }

    void PrintConstFiles()
    {
        var csprojFile = Path.Combine(outputDir, $"{NameSpace}.csproj");
        using var writer = new StreamWriter(csprojFile);
        writer.WriteLine(CSPROJ);
    }


    void PrintGlobalDef()
    {
        using var writer = new StreamWriter(globalOutputFile);
        writer.WriteLine("global using System;");
        writer.WriteLine("global using System.Collections.Generic;");
        writer.WriteLine("global using System.IO;");
        writer.WriteLine("global using System.Linq;");
        writer.WriteLine("global using System.Text;");
        writer.WriteLine("global using StepCodeDotNet.Gen;");
        writer.WriteLine("global using StepCodeDotNet.Base;");
    }
    static string BaseTypeToString(Scope_* type) => TYPEget_body(type)->type switch
    {
        type_enum.integer_ => "INTEGER",
        type_enum.real_ => "REAL",
        type_enum.number_ => "NUMBER",
        type_enum.string_ => "STRING",
        type_enum.boolean_ => "BOOLEAN",
        type_enum.logical_ => "LOGICAL",
        type_enum.binary_ => "BINARY",
        _ => type->symbol.Name
    };

    static string EntityNameToInterfaceName(string entityName)
    {
        // var name = char.ToUpper(entityName[0]) + entityName[1..];
        // return $"I{name}";
        return entityName;
    }


    void NewSchemaResolver(Scope_* express)
    {
        Scope_* schema = null;
        HashEntry de = new();

        HASHlistinit_by_type(express->symbol_table, &de, OBJ_SCHEMA);
        schema = (Scope_*)DICTdo(&de);
        schemaName = schema->symbol.Name;
        var dict = new Dictionary<nint, IStepDefine>
        {
            [(nint)schema] = new StepSchema() { Name = schema->symbol.Name }
        };
        var nameDict = new Dictionary<string, IStepDataType>()
        {
            {"REAL", new StepBaseDefine() { Name = "REAL",Type = "REAL" } },
            {"INTEGER", new StepBaseDefine() { Name = "INTEGER", Type = "INTEGER" } },
            {"STRING", new StepBaseDefine() { Name = "STRING", Type = "STRING" } },
            {"BOOLEAN", new StepBaseDefine() { Name = "BOOLEAN", Type = "BOOLEAN" } },
            {"NUMBER", new StepBaseDefine() { Name = "NUMBER", Type = "NUMBER" } },
            {"BINARY", new StepBaseDefine() { Name = "BINARY", Type = "BINARY" } },
            {"LOGICAL", new StepEnum() { Name = "LOGICAL", Values = ["TRUE", "FALSE","UNKNOWN"] }},
        };
        var enumsDict = new Dictionary<nint, StepEnum>();
        var selectsDict = new Dictionary<nint, StepSelect>();
        var aggregatesDict = new Dictionary<nint, StepAggregate>();
        var baseDict = new Dictionary<nint, StepBaseDefine>();
        var entitiesDict = new Dictionary<nint, StepEntity>();
        SCOPEdo_types(schema, &de, (Scope_* type) =>
        {
            if (TYPEis_enumeration(type))
            {
                var obj = new StepEnum();
                dict[(nint)type] = obj;
                enumsDict[(nint)type] = obj;
                nameDict[type->symbol.Name] = obj;
            }
            else if (TYPEis_select(type))
            {
                var obj = new StepSelect();
                dict[(nint)type] = obj;
                selectsDict[(nint)type] = obj;
                nameDict[type->symbol.Name] = obj;
            }
            else if (TYPEis_aggregate(type))
            {
                var obj = new StepAggregate();
                dict[(nint)type] = obj;
                aggregatesDict[(nint)type] = obj;
                nameDict[type->symbol.Name] = obj;
            }
            else
            {
                var obj = new StepBaseDefine();
                dict[(nint)type] = obj;
                baseDict[(nint)type] = obj;
                nameDict[type->symbol.Name] = obj;
            }
        });
        SCOPEdo_entities(schema, &de, (Scope_* entity) =>
        {
            var obj = new StepEntity();
            dict[(nint)entity] = obj;
            entitiesDict[(nint)entity] = obj;
            nameDict[entity->symbol.Name] = obj;
        });
        foreach (var (p, obj) in enumsDict)
        {
            var t = (Scope_*)p;
            obj.Name = t->symbol.Name;
            LISTdo_links(TYPEget_body(t)->list, link =>
            {
                var expr = (Expression_*)link->data;
                obj.Values.Add(expr->symbol.Name);
            });
        }
        foreach (var (p, obj) in selectsDict)
        {
            var t = (Scope_*)p;
            obj.Name = t->symbol.Name;
            LISTdo_links(TYPEget_body(t)->list, link =>
            {
                var expr = (Expression_*)link->data;
                var unionType = nameDict[expr->symbol.Name];
                obj.UnionTypes.Add(unionType);
                unionType.SuperTypes.Add(obj);
            });
        }
        foreach (var (p, obj) in baseDict)
        {
            var t = (Scope_*)p;
            obj.Name = t->symbol.Name;
            obj.Type = BaseTypeToString(t);
        }
        foreach (var (p, obj) in aggregatesDict)
        {
            var t = (Scope_*)p;
            obj.Name = t->symbol.Name;
            var baseType = TYPEget_base_type(t);
            if (dict.TryGetValue((nint)baseType, out var baseObj))
            {
                obj.ValueType = baseObj;
            }
            else if (nameDict.TryGetValue(baseType->symbol.Name, out var baseObj2))
            {
                obj.ValueType = baseObj2;
            }
            else
            {
                throw new NotImplementedException($"Base type {baseType->symbol.Name} not found in dict");
            }
            LISTdo_links(TYPEget_body(t)->list, link =>
            {
                var expr = (Expression_*)link->data;
                obj.SuperTypes.Add(dict[(nint)expr]);
            });
        }
        foreach (var (p, obj) in entitiesDict)
        {
            var t = (Scope_*)p;
            obj.Name = t->symbol.Name;

            var supertypes = ENTITYget_supertypes(t);
            if (LISTempty(supertypes) is false)
            {
                (*supertypes).For<Scope_>(super =>
                {
                    var superType = dict[(nint)super];
                    obj.SuperTypes.Add(superType);
                });
            }
            var attributes = ENTITYget_attributes(t);
            if (LISTempty(attributes) is false)
            {
                (*attributes).For<Variable_>(attr =>
                {
                    if (IsSelfAttr(attr) is true)
                    {
                        var stepAttr = GetStepAttribute(attr, obj, nameDict);
                        obj.SelfAttributes.Add(stepAttr);
                    }
                    else if (VARis_derived(attr) is false)
                    {
                        var stepAttr = GetStepAttribute(attr, obj, nameDict);
                        obj.Attributes.Add(stepAttr);
                    }
                    else
                    {
                        var stepAttr = GetStepAttribute(attr, obj, nameDict);
                        obj.DerivedAttributes.Add(stepAttr);
                    }
                });
            }
        }
        PrintEnums(enumsDict);
        PrintSelects(selectsDict);
        PrintAggregates(aggregatesDict);
        this.baseNameDict = nameDict.Where(x => x.Value is StepBaseDefine).ToDictionary(x => x.Key, x => (StepBaseDefine)x.Value);
        PrintBaseDef(baseNameDict);
        PrintIEntities(entitiesDict);
        PrintIEntityImp(entitiesDict);
        PrintComplexImp2(entitiesDict);
        PrintStaticInflect(entitiesDict, baseNameDict);
    }

    static bool IsSelfAttr(Variable_* attr)
    {
        var nameExpr = attr->name;
        var opCode = nameExpr->e.op_code;
        if (opCode != Op_Code.OP_DOT)
        {
            return false;
        }
        var op1 = nameExpr->e.op1;
        var op2 = nameExpr->e.op2;
        var op1Type = op1->e.op1->type;
        var attrType = attr->type;
        if (op1->e.op_code != Op_Code.OP_GROUP)
        {
            return false;
        }
        if (op1->e.op1->type->u.type->body->type != type_enum.self_)
        {
            return false;
        }
        return true;
    }

    private void PrintEnums(Dictionary<nint, StepEnum> enumsDict)
    {
        var fileName = Path.Combine(outputDir, $"Enums.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");

        foreach (var (_, obj) in enumsDict)
        {
            writer.WriteLine($"public enum {obj.Name.ToUpper()}");
            writer.WriteLine("{");
            foreach (var value in obj.Values)
            {
                writer.WriteLine($"    {value.ToUpper()},");
            }
            writer.WriteLine("}");
        }
    }

    private void PrintSelects(Dictionary<nint, StepSelect> selectsDict)
    {
        var fileName = Path.Combine(outputDir, $"Selects.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        foreach (var (_, obj) in selectsDict)
        {
            writer.Write($"public interface {obj.Name}");
            if (obj.SuperTypes.Count > 0)
            {
                writer.Write($" : {string.Join(", ", obj.SuperTypes.Select(x => x.Name))}");
            }
            writer.WriteLine(";");
        }
    }

    private void PrintAggregates(Dictionary<nint, StepAggregate> aggregatesDict)
    {
        foreach (var (_, obj) in aggregatesDict)
        {
            var fileName = Path.Combine(outputDir, $"{obj.Name}.cs");
            using var writer = new StreamWriter(fileName);
            writer.WriteLine($"namespace {NameSpace};");
            writer.Write($"public class {obj.Name} : List<{obj.ValueType!.Name}>");
            if (obj.SuperTypes.Count > 0)
            {
                writer.WriteLine($",{string.Join(", ", obj.SuperTypes.Select(x => x.Name))}");
            }
            else
            {
                writer.WriteLine();
            }
            writer.WriteLine("{");
            writer.WriteLine("}");
        }
    }

    private static string GetStepTypeStrng(IStepDefine type)
    {
        return type switch
        {
            StepBaseDefine baseType => baseType.Name,
            StepEnum enumType => enumType.Name.ToUpper(),
            StepSelect selectType => selectType.Name,
            StepAggregate aggregateType => GetAggregateTypeStr(aggregateType),
            StepEntity entityType => EntityNameToInterfaceName(entityType.Name),
            _ => throw new NotImplementedException($"Type {type.GetType()} not implemented"),
        };
    }

    private static string GetAggregateTypeStr(StepAggregate type)
    {
        if (type.ValueType is StepAggregate aggregateType)
        {
            return $"{GetAggregateTypeStr(aggregateType)}[]";
        }
        else
        {
            return $"{GetStepTypeStrng(type.ValueType!)}[]";
        }
    }

    private void PrintIEntities(Dictionary<nint, StepEntity> entitiesDict)
    {
        var fileName = Path.Combine(outputDir, $"IEntities.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        foreach (var (_, obj) in entitiesDict)
        {
            var entityName = obj.Name;
            var interfaceName = EntityNameToInterfaceName(entityName);
            writer.Write($"public interface {interfaceName} : IStepObj");
            if (obj.SuperTypes.Count > 0)
            {
                writer.Write($", {string.Join(", ", obj.SuperTypes.Select(x => GetStepTypeStrng(x)))}");
            }
            writer.WriteLine();
            writer.WriteLine("{");
            foreach (var (type, attrName, _) in obj.Attributes)
            {
                writer.WriteLine($"    {GetStepTypeStrng(type)} {attrName} {{ get; set; }}");
            }
            writer.WriteLine("}");
        }
    }

    private static List<StepAttribute> GetEntityAllAttrs(StepEntity stepEntity)
    {
        var stack = new Stack<StepEntity>();
        var attrLists = new Stack<List<StepAttribute>>();
        stack.Push(stepEntity);
        while (stack.Count > 0)
        {
            var sup = stack.Pop();
            if (sup is not StepEntity entity)
            {
                continue;
            }
            attrLists.Push(entity.Attributes);
            foreach (var super in entity.SuperTypes)
            {
                if (super is StepEntity superEntity)
                {
                    stack.Push(superEntity);
                }
            }
        }
        var list = new List<StepAttribute>();
        while (attrLists.Count > 0)
        {
            var attrList = attrLists.Pop();
            foreach (var a in attrList)
            {
                if (list.Any(x => x.Name == a.Name && x.Type.Name == a.Type.Name))
                {
                    continue;
                }
                list.Add(a);
            }
        }
        return list;
    }

    private void PrintIEntityImp(Dictionary<nint, StepEntity> entitiesDict)
    {
        var initFileName = Path.Combine(outputDir, $"EntityImpInit.cs");
        using var initWriter = new StreamWriter(initFileName);
        initWriter.WriteLine($"namespace {NameSpace};");
        initWriter.WriteLine("using System.Collections.Frozen;");
        initWriter.WriteLine("public static class EntityImpInit");
        initWriter.WriteLine("{");

        var initRegFileName = Path.Combine(outputDir, $"EntityImpInitReg.cs");
        using var initRegWriter = new StreamWriter(initRegFileName);
        initRegWriter.WriteLine($"namespace {NameSpace};");
        initRegWriter.WriteLine("using System.Collections.Frozen;");
        initRegWriter.WriteLine("public static class EntityImpInitReg");
        initRegWriter.WriteLine("{");
        initRegWriter.WriteLine($"    public static readonly FrozenDictionary<Type, Action<IStepObjCreator,IStepObj, ReadOnlySpan<IStepToken>, Dictionary<int, IStepObj>>> _entityInitFuncMap = new Dictionary<Type, Action<IStepObjCreator,IStepObj, ReadOnlySpan<IStepToken>, Dictionary<int, IStepObj>>>");
        initRegWriter.WriteLine("    {");

        var fileName = Path.Combine(outputDir, $"EntityImpls.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");

        foreach (var (_, obj) in entitiesDict)
        {
            var entityName = obj.Name;
            var interfaceName = EntityNameToInterfaceName(entityName);
            writer.WriteLine($"public class {entityName}_imp : {interfaceName}");
            writer.WriteLine("{");
            writer.WriteLine($"    public int line_id {{ get; set; }}");
            var allAttrs = GetEntityAllAttrs(obj);
            HashSet<string> attrNames = new();
            foreach (var (type, attrName, subEntity) in allAttrs)
            {
                if (attrNames.Add(attrName))
                {
                    writer.WriteLine($"    public {GetStepTypeStrng(type)} {attrName} {{ get; set; }}");
                }
                else
                {
                    writer.WriteLine($"    {GetStepTypeStrng(type)} {subEntity.Name}.{attrName} {{ get; set; }}");
                }
            }
            writer.WriteLine("}");

            initWriter.WriteLine($"    static readonly Func<IStepObjCreator, {entityName}, ReadOnlySpan<IStepToken>, Dictionary<int, IStepObj>, ReadOnlySpan<IStepToken>>[] {entityName}_Funcs = [");

            for (int x = 0; x < allAttrs.Count; x++)
            {
                initWriter.WriteLine("        (c, o, a, m) =>");
                initWriter.WriteLine("        {");
                var (type, attrName, subEntity) = allAttrs[x];
                if (type is StepBaseDefine baseType)
                {
                    initWriter.WriteLine($"            var r = c.Get{baseType.Type}(a[0]);");
                    initWriter.WriteLine($"            (({subEntity.Name})o).{attrName} = r;");
                    initWriter.WriteLine("            return a[1..];");
                }
                else if (type is StepEnum enumType)
                {
                    initWriter.WriteLine($"            var r = c.GetEnum<{enumType.Name.ToUpper()}>(a[0]);");
                    initWriter.WriteLine($"            (({subEntity.Name})o).{attrName} = r;");
                    initWriter.WriteLine("                return a[1..];");
                }
                else if (type is StepSelect selectType)
                {
                    initWriter.WriteLine($"            var (r,endIndex) = c.GetEntity<{selectType.Name}>(a, m);");
                    initWriter.WriteLine($"            (({subEntity.Name})o).{attrName} = r;");
                    initWriter.WriteLine("            if (endIndex == a.Length)");
                    initWriter.WriteLine("            {");
                    initWriter.WriteLine("                return [];");
                    initWriter.WriteLine("            }");
                    initWriter.WriteLine("            return a[endIndex..];");

                }
                else if (type is StepAggregate aggregateType)
                {
                    initWriter.WriteLine($"            var list = IStepObjCreator.GetListTokens(a, out var valid, out var endIndex);");
                    initWriter.WriteLine($"            var r = c.GetAggregate<{GetStepTypeStrng(aggregateType.ValueType!)}>(list, m);");
                    initWriter.WriteLine($"            (({subEntity.Name})o).{attrName} = r;");
                    initWriter.WriteLine("            if (endIndex == a.Length)");
                    initWriter.WriteLine("            {");
                    initWriter.WriteLine("                return [];");
                    initWriter.WriteLine("            }");
                    initWriter.WriteLine("            return a[endIndex..];");
                }
                else if (type is StepEntity entityType)
                {
                    initWriter.WriteLine($"            var (r,endIndex) = c.GetEntity<{entityType.Name}>(a, m);");
                    initWriter.WriteLine($"            (({subEntity.Name})o).{attrName} = r;");
                    initWriter.WriteLine("            if (endIndex == a.Length)");
                    initWriter.WriteLine("            {");
                    initWriter.WriteLine("                return [];");
                    initWriter.WriteLine("            }");
                    initWriter.WriteLine("            return a[endIndex..];");
                }
                initWriter.WriteLine("        },");
            }
            initWriter.WriteLine("    ];");
            var newEntityName = entityName.Replace("@", "_");
            initWriter.WriteLine($"    public static void Init_{newEntityName}(IStepObjCreator creator, IStepObj obj, ReadOnlySpan<IStepToken> args, Dictionary<int, IStepObj> refMap)");
            initWriter.WriteLine("    {");
            initWriter.WriteLine("        var argExps = args;");
            initWriter.WriteLine($"        var stepObj=({entityName})obj;");
            initWriter.WriteLine($"        foreach (var init in {entityName}_Funcs)");
            initWriter.WriteLine("        {");
            const string initStr = """
                        int skip = 0;
                        while (argExps[skip].TokenType == StepTokenType.Comma)
                        {
                            skip++;
                        }
                        argExps = init(creator, stepObj, argExps[skip..], refMap);
            """;
            initWriter.WriteLine(initStr);
            initWriter.WriteLine("            if (argExps.Length == 0)");
            initWriter.WriteLine("            {");
            initWriter.WriteLine("                return;");
            initWriter.WriteLine("            }");
            initWriter.WriteLine("        }");
            initWriter.WriteLine("    }");

            initRegWriter.WriteLine($"        {{typeof({entityName}_imp), EntityImpInit.Init_{newEntityName}}},");
        }

    }

    private string GetEntityMethodStr(StepEntity entityType)
    {
        if (baseNameDict.ContainsKey(entityType.Name))
        {
            return "GetBaseEntity";
        }
        return "GetEntity";
    }

    private string GetInstanceCreateStr(IStepDefine stepDefine, int i)
    {
        return stepDefine switch
        {
            StepBaseDefine baseType => $"StepObjCreator.Get{baseType.Type}(argExps[{i}])",
            StepEnum enumType => $"StepObjCreator.GetEnum<{enumType.Name.ToUpper()}>(argExps[{i}])",
            StepSelect selectType => $"StepObjCreator.GetEntity<{selectType.Name}>(argExps[{i}],refMap)",
            StepAggregate aggregateType => $"StepObjCreator.GetAggregate<{GetStepTypeStrng(aggregateType.ValueType!)}>(argExps[{i}],refMap)",
            StepEntity entityType => $"StepObjCreator.{GetEntityMethodStr(entityType)}<{GetStepTypeStrng(entityType)}>(argExps[{i}],refMap)",
            _ => throw new NotImplementedException($"Type {stepDefine.GetType()} not implemented"),
        };
    }

    private void PrintBaseDef(Dictionary<string, StepBaseDefine> baseDict)
    {
        var fileName = Path.Combine(outputDir, $"BaseDef.cs");
        using var globalWriter = new StreamWriter(globalOutputFile, true);
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        foreach (var (_, obj) in baseDict)
        {
            if (obj.Name == "LOGICAL")
            {
                continue;
            }
            {
                var key = obj.Name;
                var value = typeMap[obj.Type];
                if (value == "System.String" || value == "System.Byte[]")
                {
                    writer.Write($"public record {obj.Name}({value} Value): IStepBaseObj");
                }
                else
                {
                    writer.Write($"public record struct {obj.Name}({value} Value): IStepBaseObj");
                }
                if (obj.SuperTypes.Count > 0)
                {
                    writer.Write($", {string.Join(", ", obj.SuperTypes.Select(x => x.Name))}");
                }
                writer.WriteLine();
                writer.WriteLine("{");
                writer.WriteLine($"     public static implicit operator {key}({value} value)");
                writer.WriteLine("    {");
                writer.WriteLine("        return new(value);");
                writer.WriteLine("    }");
                writer.WriteLine($"    public static implicit operator {value}({key} obj)");
                writer.WriteLine("    {");
                writer.WriteLine("        return obj.Value;");
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }
    }


    private IStepDataType GetStepDataType(Scope_* type, Dictionary<string, IStepDataType> nameDict)
    {
        if (nameDict.TryGetValue(type->symbol.Name, out var obj))
        {
            return obj;
        }
        else if (TYPEis_aggregate(type))
        {
            var baseType = GetStepDataType(TYPEget_base_type(type), nameDict);
            return new StepAggregate() { Name = type->symbol.Name, ValueType = baseType };
        }
        else
        {
            var baseName = BaseTypeToString(type);
            if (nameDict.TryGetValue(baseName, out var baseObj))
            {
                return baseObj;
            }
            else
            {
                throw new NotImplementedException($"Type {type->symbol.Name} not found in nameDict");
            }
        }
    }

    private StepAttribute GetStepAttribute(Variable_* attr, StepEntity subEntity, Dictionary<string, IStepDataType> nameDict)
    {
        var type = GetStepDataType(attr->type, nameDict);
        var attrName = attr->name->symbol.Name;
        return new StepAttribute(type, attrName, subEntity);
    }

    List<StepEntity> GetExpressionSubOpEntities(Expression_* expression, Dictionary<string, StepEntity> entitiesDict)
    {
        if (expression->type->u.type->body->type == type_enum.oneof_)
        {
            var list = new List<StepEntity>();
            LISTdo<Expression_>(expression->u.list, arg =>
            {
                list.Add(entitiesDict[arg->symbol.Name]);
            });
            return list;
        }
        else if (expression->type->u.type->body->type == type_enum.entity_)
        {
            return [entitiesDict[expression->symbol.Name]];
        }
        throw new NotImplementedException($"Expression type {expression->type->u.type->body->type} not implemented");
    }
    public static HashSet<StepEntity> GetEntityAllSupers(StepEntity stepEntity)
    {
        var stack = new Stack<StepEntity>();
        stack.Push(stepEntity);
        var allSuper = new HashSet<StepEntity>();
        while (stack.Count > 0)
        {
            var sup = stack.Pop();
            allSuper.Add(sup);
            if (sup is not StepEntity entity)
            {
                continue;
            }
            foreach (var super in entity.SuperTypes)
            {
                if (super is StepEntity superEntity)
                {
                    stack.Push(superEntity);
                }
            }
        }
        return allSuper;
    }
    private void PrintComplexImp2(Dictionary<nint, StepEntity> entitiesDict)
    {
        var nameEntities = entitiesDict.Values.ToDictionary(x => x.Name, x => x);
        var fileName = Path.Combine(outputDir, "StepComplexImp.cs");
        using var writer = new StreamWriter(fileName);
        var initFileName = Path.Combine(outputDir, "EntityImpInit.cs");
        using var initWriter = new StreamWriter(initFileName, append: true);
        const string complexInitStr = """
            public static void Init_Complex_Func(IStepObjCreator creator, IStepObj obj, ReadOnlySpan<IStepToken> args, Dictionary<int, IStepObj> refMap)
            {
                var remainArgs = args;
                while (remainArgs.Length > 0)
                {
                    var entityExpress = IStepObjCreator.GetListTokens(remainArgs, out var valid, out var endIndex);
                    if (valid is false && entityExpress.Length < 2)
                    {
                        return;
                    }
                    remainArgs = remainArgs[endIndex..];
                    var entityType = Encoding.ASCII.GetString(entityExpress[0].GetEntityName());
                    var entityArgs = IStepObjCreator.GetEntityArgs(entityExpress[1..], out var validArgs, out _);
                    if (_complexInitFuncs.TryGetValue(entityType, out var initFunc))
                    {
                        initFunc(creator, obj, entityArgs, refMap);
                    }
                }
            }
            static readonly FrozenDictionary<string, Action<IStepObjCreator, IStepObj, ReadOnlySpan<IStepToken>, Dictionary<int, IStepObj>>> _complexInitFuncs = new Dictionary<string, Action<IStepObjCreator, IStepObj, ReadOnlySpan<IStepToken>, Dictionary<int, IStepObj>>>()
            {
        """;
        initWriter.WriteLine(complexInitStr);

        var initRegFileName = Path.Combine(outputDir, "EntityImpInitReg.cs");
        using var initRegWriter = new StreamWriter(initRegFileName, append: true);
        writer.WriteLine($"namespace {NameSpace};");
        writer.WriteLine("using System.Collections.Frozen;");
        foreach (var (p, obj) in entitiesDict)
        {
            var t = (Scope_*)p;
            var subTypeExp = t->u.entity->subtype_expression;
            if (subTypeExp == null)
            {
                continue;
            }
            if (subTypeExp->e.op_code == Op_Code.OP_ANDOR)
            {
                var left = subTypeExp->e.op1;
                var leftType = left->type->u.type->body->type;
                var leftEntities = GetExpressionSubOpEntities(left, nameEntities);
                var right = subTypeExp->e.op2;
                var rightType = right->type->u.type->body->type;
                var rightEntities = GetExpressionSubOpEntities(right, nameEntities);
                foreach (var leftEntity in leftEntities)
                {
                    foreach (var rightEntity in rightEntities)
                    {
                        if (leftEntity.Name == rightEntity.Name)
                        {
                            continue;
                        }
                        var complexName = $"{leftEntity.Name}_and_{rightEntity.Name}";
                        complexEntities.lefts.Add(leftEntity.Name);
                        complexEntities.rights.Add(rightEntity.Name);
                        complexEntities.complexies.Add(complexName);
                        PrintComplexEntityImp(writer, initWriter, initRegWriter, leftEntity, rightEntity);
                    }
                }
            }
        }
        {
            var global_uncertainty_assigned_context = nameEntities["global_uncertainty_assigned_context"];
            var global_unit_assigned_context = nameEntities["global_unit_assigned_context"];
            complexEntities.lefts.Add(global_uncertainty_assigned_context.Name);
            complexEntities.rights.Add(global_unit_assigned_context.Name);
            complexEntities.complexies.Add($"{global_uncertainty_assigned_context.Name}_and_{global_unit_assigned_context.Name}");
            PrintComplexEntityImp(writer, initWriter, initRegWriter, global_uncertainty_assigned_context, global_unit_assigned_context);
        }
        {
            var c1 = nameEntities["representation_relationship_with_transformation"];
            var c2 = nameEntities["shape_representation_relationship"];
            complexEntities.lefts.Add(c1.Name);
            complexEntities.rights.Add(c2.Name);
            complexEntities.complexies.Add($"{c1.Name}_and_{c2.Name}");
            PrintComplexEntityImp(writer, initWriter, initRegWriter, c1, c2);
        }

        initWriter.WriteLine("    }.ToFrozenDictionary();");
        initWriter.WriteLine("}");

        initRegWriter.WriteLine("    }.ToFrozenDictionary();");
        initRegWriter.WriteLine("}");

    }

    string GetComplexName(params StepEntity[] entities)
    {
        var complexName = string.Join("_and_", entities.Select(x => x.Name));
        return complexName;
    }

    void PrintComplexEntityImp2(StreamWriter writer, params StepEntity[] entities)
    {
        var complexName = GetComplexName(entities);
        writer.WriteLine($"public class {complexName} : {string.Join(", ", entities.Select(x => EntityNameToInterfaceName(x.Name)))}");
        writer.WriteLine("{");
        writer.WriteLine($"    public int line_id {{ get; set; }}");
        var leftAllAttrs = GetEntityAllAttrs(entities[0]);
        foreach (var (type, attrName, _) in leftAllAttrs)
        {
            writer.WriteLine($"    public {GetStepTypeStrng(type)} {attrName} {{ get; set; }}");
        }
        for (int i = 1; i < entities.Length; i++)
        {
            var right = entities[i];
            foreach (var (type, attrName, _) in right.Attributes)
            {
                writer.WriteLine($"    public {GetStepTypeStrng(type)} {attrName} {{ get; set; }}");
            }
        }
        var leftSupers = GetEntityAllSupers(entities[0]);
        var supers = new HashSet<StepEntity>(entities);
        supers.UnionWith(leftSupers);
        writer.WriteLine($"    private static readonly FrozenDictionary<string, Action<{complexName}, EntityExpress, Dictionary<int, IStepObj>>> _entityInitFuncMap = new Dictionary<string, Action<{complexName}, EntityExpress, Dictionary<int, IStepObj>>>");
        writer.WriteLine("    {");
        foreach (var super in supers)
        {
            if (super.Attributes.Count == 0)
            {
                continue;
            }
            writer.Write($"        {{\"{super.Name.ToUpper()}\",");
            writer.WriteLine($"{super.Name}_init}},");
        }
        writer.WriteLine("    }.ToFrozenDictionary();");
        foreach (var super in supers)
        {
            if (super.Attributes.Count == 0)
            {
                continue;
            }
            writer.WriteLine($"    private static void {super.Name}_init({complexName} obj, EntityExpress express, Dictionary<int, IStepObj> refMap)");
            writer.WriteLine("    {");
            writer.WriteLine("        var argExps = express.Args;");
            writer.WriteLine("        switch (argExps.Count)");
            writer.WriteLine("        {");
            for (int x = 0; x < super.Attributes.Count; x++)
            {
                int y = x + 1;
                writer.WriteLine($"            case {y}:");
                for (int i = 0; i < y; i++)
                {
                    var (type, attrName, _) = super.Attributes[i];
                    writer.WriteLine($"                obj.{attrName} = {GetInstanceCreateStr(type, i)};");
                }
                writer.WriteLine("                return;");
            }
            writer.WriteLine("            default:");
            writer.WriteLine("                return;");
            writer.WriteLine("        }");
            writer.WriteLine("    }");
        }
        writer.WriteLine("    public void Init(IExpress expression, Dictionary<int, IStepObj> refMap)");
        writer.WriteLine("    {");
        writer.WriteLine("        var complexExpress = (ComplexExpress)expression;");
        writer.WriteLine("        foreach (EntityExpress express in complexExpress.ExpressList)");
        writer.WriteLine("        {");
        writer.WriteLine("            var entityName = express.EntityName.ToUpper();");
        writer.WriteLine("            if (_entityInitFuncMap.TryGetValue(entityName, out var initFunc))");
        writer.WriteLine("            {");
        writer.WriteLine("                initFunc(this, express, refMap);");
        writer.WriteLine("            }");
        writer.WriteLine("        }");
        writer.WriteLine("    }");
        writer.WriteLine("}");
    }


    void PrintComplexEntityImp(StreamWriter writer, StreamWriter initWriter, StreamWriter initRegWriter, StepEntity left, StepEntity right)
    {
        var complexName = $"{left.Name}_and_{right.Name}_imp";
        writer.WriteLine($"public class {complexName} : {EntityNameToInterfaceName(left.Name)}, {EntityNameToInterfaceName(right.Name)}");
        writer.WriteLine("{");
        writer.WriteLine($"    public int line_id {{ get; set; }}");
        var leftAllAttrs = GetEntityAllAttrs(left);
        foreach (var (type, attrName, _) in leftAllAttrs)
        {
            writer.WriteLine($"    public {GetStepTypeStrng(type)} {attrName} {{ get; set; }}");
        }
        foreach (var (type, attrName, _) in right.Attributes)
        {
            writer.WriteLine($"    public {GetStepTypeStrng(type)} {attrName} {{ get; set; }}");
        }
        writer.WriteLine("}");

        var leftSupers = GetEntityAllSupers(left);
        var supers = new HashSet<StepEntity>
        {
            left,
            right
        };
        supers.UnionWith(leftSupers);

        foreach (var super in supers)
        {
            if (super.Attributes.Count == 0)
            {
                continue;
            }
            if (complexSubEntities.Add(super.Name) is false)
            {
                continue;
            }
            initWriter.Write($"        {{\"{super.Name.ToUpper()}\",");
            initWriter.WriteLine($"Init_{super.Name}}},");
        }
        initRegWriter.WriteLine($"        {{typeof({complexName}), EntityImpInit.Init_Complex_Func}},");


        // writer.WriteLine($"    private static readonly FrozenDictionary<string, Action<{complexName}, EntityExpress, Dictionary<int, IStepObj>>> _entityInitFuncMap = new Dictionary<string, Action<{complexName}, EntityExpress, Dictionary<int, IStepObj>>>");
        // writer.WriteLine("    {");
        // foreach (var super in supers)
        // {
        //     if (super.Attributes.Count == 0)
        //     {
        //         continue;
        //     }
        //     writer.Write($"        {{\"{super.Name.ToUpper()}\",");
        //     writer.WriteLine($"{super.Name}_init}},");
        // }
        // writer.WriteLine("    }.ToFrozenDictionary();");
        // foreach (var super in supers)
        // {
        //     if (super.Attributes.Count == 0)
        //     {
        //         continue;
        //     }
        //     writer.WriteLine($"    private static void {super.Name}_init({complexName} obj, EntityExpress express, Dictionary<int, IStepObj> refMap)");
        //     writer.WriteLine("    {");
        //     writer.WriteLine("        var argExps = express.Args;");
        //     writer.WriteLine("        switch (argExps.Count)");
        //     writer.WriteLine("        {");
        //     for (int x = 0; x < super.Attributes.Count; x++)
        //     {
        //         int y = x + 1;
        //         writer.WriteLine($"            case {y}:");
        //         for (int i = 0; i < y; i++)
        //         {
        //             var (type, attrName, _) = super.Attributes[i];
        //             writer.WriteLine($"                obj.{attrName} = {GetInstanceCreateStr(type, i)};");
        //         }
        //         writer.WriteLine("                return;");
        //     }
        //     writer.WriteLine("            default:");
        //     writer.WriteLine("                return;");
        //     writer.WriteLine("        }");
        //     writer.WriteLine("    }");
        // }
    }


    private void PrintStaticInflect(Dictionary<nint, StepEntity> entitiesDict, Dictionary<string, StepBaseDefine> baseDict)
    {
        var fileName = Path.Combine(outputDir, "StepObjCreator.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Generic;");
        writer.WriteLine("using System.Collections;");
        writer.WriteLine("using System.Collections.Frozen;");
        writer.WriteLine("using System.Runtime.CompilerServices;");
        writer.WriteLine("public class StepObjCreator:IStepObjCreator");
        writer.WriteLine("{");
        writer.WriteLine($"    private const int NAMESPACE_LENGTH = {NameSpace.Length};");
        writer.WriteLine("    private static readonly Encoding _gb18030;");
        writer.WriteLine("    static StepObjCreator()");
        writer.WriteLine("    {");
        writer.WriteLine("        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);");
        writer.WriteLine("        _gb18030 = Encoding.GetEncoding(\"GB18030\");");
        writer.WriteLine("    }");
        writer.WriteLine("    public Encoding Encoding => _gb18030;");
        writer.WriteLine("    public IStepBaseObj Create(ReadOnlySpan<char> entityName) => entityName switch");
        writer.WriteLine("    {");
        foreach (var entity in entitiesDict.Values)
        {
            writer.WriteLine($"        \"{entity.Name.ToUpper()}\" => new {entity.Name}_imp(),");
        }
        writer.WriteLine("        _ => default");
        writer.WriteLine("    };");

        writer.WriteLine("    public IStepBaseObj Create(ReadOnlySpan<char> entityName, ReadOnlySpan<IStepToken> argTokens) => entityName switch");
        writer.WriteLine("    {");
        foreach (var entity in entitiesDict.Values)
        {
            writer.WriteLine($"        \"{entity.Name.ToUpper()}\" => new {entity.Name}_imp(),");
        }
        foreach (var baseDef in baseDict.Values)
        {
            if (baseDef.Name == "LOGICAL" || baseDef.Type == "BINARY")
            {
                continue;
            }
            if (baseDef.Type == "STRING")
            {
                writer.WriteLine($"        \"{baseDef.Name.ToUpper()}\" => new {baseDef.Name}(Encoding.GetString(argTokens[0].{typeValueGetFuncMap[baseDef.Type]}())),");
            }
            else
            {
                writer.WriteLine($"        \"{baseDef.Name.ToUpper()}\" => new {baseDef.Name}(argTokens[0].{typeValueGetFuncMap[baseDef.Type]}()),");
            }

        }
        writer.WriteLine("        _ => default");
        writer.WriteLine("    };");

        writer.Write("    static readonly FrozenSet<string> _leftNames = [");
        foreach (var entity in complexEntities.lefts)
        {
            writer.Write($"\"{entity.ToUpper()}\",");
        }
        writer.WriteLine("];");
        writer.Write("    static readonly FrozenSet<string> _rightNames = [");
        foreach (var entity in complexEntities.rights)
        {
            writer.Write($"\"{entity.ToUpper()}\",");
        }
        writer.WriteLine("];");
        writer.Write("    static readonly FrozenSet<string> _complexNames = [");
        foreach (var entity in complexEntities.complexies)
        {
            writer.Write($"\"{entity.ToUpper()}\",");
        }
        writer.WriteLine("];");
        writer.WriteLine("    FrozenSet<string> IStepObjCreator.LeftNames=> _leftNames;");
        writer.WriteLine("    FrozenSet<string> IStepObjCreator.RightNames=> _rightNames;");
        writer.WriteLine("    FrozenSet<string> IStepObjCreator.ComplexNames=> _complexNames;");

        writer.WriteLine("    public IStepObj CreateComplex(ReadOnlySpan<char> complexName) => complexName switch");
        writer.WriteLine("    {");
        foreach (var entity in complexEntities.complexies)
        {
            writer.WriteLine($"        \"{entity.ToUpper()}\" => new {entity}_imp(),");
        }
        writer.WriteLine("        _ => default");
        writer.WriteLine("    };");
        const string InitStepObjStr = """
            public void InitStepObj(IStepObj obj, ReadOnlySpan<IStepToken> argTokens, Dictionary<int, IStepObj> refMap)
            {
                var type = obj.GetType();
                if (EntityImpInitReg._entityInitFuncMap.TryGetValue(type, out var initFunc))
                {
                    initFunc(this, obj, argTokens, refMap);
                }
            }
        """;
        writer.WriteLine(InitStepObjStr);
        writer.WriteLine("}");
    }

}

