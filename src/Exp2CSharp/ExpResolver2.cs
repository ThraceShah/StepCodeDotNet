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

record StepAttribute(IStepDefine Type, string Name);

class StepEntity : IStepDataType
{
    public string Name { get; set; } = string.Empty;
    public List<IStepDefine> SuperTypes { get; set; } = [];
    public List<StepAttribute> Attributes { get; set; } = [];
    public List<StepAttribute> DerivedAttributes { get; set; } = [];
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
    };

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
            {"LOGICAL", new StepEnum() { Name = "LOGICAL", Values = ["TRUE", "FALSE","UNKNOWN"] }}
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
            obj.ValueType = dict[(nint)TYPEget_base_type(t)];
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
                    if (VARis_derived(attr) is false)
                    {
                        var stepAttr = GetStepAttribute(attr, nameDict);
                        obj.Attributes.Add(stepAttr);
                    }
                    else
                    {
                        var stepAttr = GetStepAttribute(attr, nameDict);
                        obj.DerivedAttributes.Add(stepAttr);
                    }
                });
            }
        }
        PrintEnums(enumsDict);
        PrintSelects(selectsDict);
        PrintAggregates(aggregatesDict);
        var baseNameDict = nameDict.Where(x => x.Value is StepBaseDefine).ToDictionary(x => x.Key, x => (StepBaseDefine)x.Value);
        PrintBaseDef(baseNameDict);
        PrintIEntities(entitiesDict);
        PrintIEntityImp(entitiesDict);
        PrintComplexImp(entitiesDict);
        PrintStaticInflect(entitiesDict, baseNameDict);
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
            return $"List<{GetAggregateTypeStr(aggregateType)}>";
        }
        else
        {
            return $"List<{GetStepTypeStrng(type.ValueType!)}>";
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
            foreach (var (type, attrName) in obj.Attributes)
            {
                writer.WriteLine($"    {GetStepTypeStrng(type)} {attrName} {{ get; set; }}");
            }
            writer.WriteLine("}");
        }
    }

    public static List<StepAttribute> GetEntityAllAttrs(StepEntity stepEntity)
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
                if (list.Any(x => x.Name == a.Name))
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
            foreach (var (type, attrName) in allAttrs)
            {
                writer.WriteLine($"    public {GetStepTypeStrng(type)} {attrName} {{ get; set; }}");
            }
            writer.WriteLine("    public void Init(IExpress expression, Dictionary<int, IStepObj> refMap)");
            writer.WriteLine("    {");
            writer.WriteLine("        var entityExpress = (EntityExpress)expression;");
            writer.WriteLine("        var argExps = entityExpress.Args;");
            writer.WriteLine("        switch (argExps.Count)");
            writer.WriteLine("        {");
            for (int x = 0; x < allAttrs.Count; x++)
            {
                int y = x + 1;
                writer.WriteLine($"            case {y}:");
                for (int i = 0; i < y; i++)
                {
                    var (type, attrName) = allAttrs[i];
                    writer.WriteLine($"                this.{attrName} = {GetInstanceCreateStr(type, i)};");
                }
                writer.WriteLine("                return;");
            }
            writer.WriteLine("            default:");
            writer.WriteLine("                return;");
            writer.WriteLine("        }");
            writer.WriteLine("    }");

            writer.WriteLine("}");
        }
    }

    private string GetInstanceCreateStr(IStepDefine stepDefine, int i)
    {
        return stepDefine switch
        {
            StepBaseDefine baseType => $"StepObjCreator.Get{baseType.Type}(argExps[{i}])",
            StepEnum enumType => $"StepObjCreator.GetEnum<{enumType.Name.ToUpper()}>(argExps[{i}])",
            StepSelect selectType => $"StepObjCreator.GetEntity<{selectType.Name}>(argExps[{i}],refMap)",
            StepAggregate aggregateType => $"StepObjCreator.GetAggregate<{GetStepTypeStrng(aggregateType.ValueType!)}>(argExps[{i}],refMap)",
            StepEntity entityType => $"StepObjCreator.GetEntity<{GetStepTypeStrng(entityType)}>(argExps[{i}],refMap)",
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
                if (value == "System.String")
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

    private StepAttribute GetStepAttribute(Variable_* attr, Dictionary<string, IStepDataType> nameDict)
    {
        var type = GetStepDataType(attr->type, nameDict);
        var attrName = attr->name->symbol.Name;
        return new StepAttribute(type, attrName);
    }

    private void PrintComplexImp(Dictionary<nint, StepEntity> entitiesDict)
    {
        var fileName = Path.Combine(outputDir, "StepComplexImp.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.Write("public class StepComplexImp : StepComplex");
        var intefaces = new HashSet<string>();
        foreach (var entity in entitiesDict.Values)
        {
            intefaces.Add(EntityNameToInterfaceName(entity.Name));
        }
        if (intefaces.Count > 0)
        {
            writer.Write($",{string.Join(", ", intefaces)}");
        }
        writer.WriteLine();
        writer.WriteLine("{");
        foreach (var entity in entitiesDict.Values)
        {
            foreach (var (type, attrName) in entity.Attributes)
            {
                writer.WriteLine($"    {GetStepTypeStrng(type)} {EntityNameToInterfaceName(entity.Name)}.{attrName} {{ get; set; }}");
            }
        }
        writer.WriteLine("    public override void Init(IExpress expression, Dictionary<int, IStepObj> refMap)");
        writer.WriteLine("    {");
        writer.WriteLine("        switch (expression)");
        writer.WriteLine("        {");
        writer.WriteLine("            case ListExpress listExpress:");
        writer.WriteLine("                this.complex = [..listExpress.ExpressList];");
        writer.WriteLine("                break;");
        writer.WriteLine("            default:");
        writer.WriteLine("                throw new NotImplementedException();");
        writer.WriteLine("        }");
        writer.WriteLine("    }");
        writer.WriteLine("}");

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
        writer.WriteLine("    private static readonly StepObjCreator instance = new();");
        writer.WriteLine("    private static IStepBaseObj Create(EntityExpress express) => express.EntityName switch");
        writer.WriteLine("    {");
        foreach (var entity in entitiesDict.Values)
        {
            writer.WriteLine($"        \"{entity.Name.ToUpper()}\" => new {entity.Name}_imp(),");
        }
        foreach (var baseDef in baseDict.Values)
        {
            if (baseDef.Name == "LOGICAL")
            {
                continue;
            }
            writer.WriteLine($"        \"{baseDef.Name.ToUpper()}\" => new {baseDef.Name}(((IExpress<{typeMap[baseDef.Type]}>)express.Args[0]).Value),");
        }
        writer.WriteLine("        _ => default");
        writer.WriteLine("    };");

        writer.WriteLine("    private static IStepBaseObj Create(string entityName, IExpress express) => entityName switch");
        writer.WriteLine("    {");
        foreach (var entity in entitiesDict.Values)
        {
            writer.WriteLine($"        \"{entity.Name.ToUpper()}\" => new {entity.Name}_imp(),");
        }
        foreach (var baseDef in baseDict.Values)
        {
            if (baseDef.Name == "LOGICAL")
            {
                continue;
            }
            writer.WriteLine($"        \"{baseDef.Name.ToUpper()}\" => new {baseDef.Name}(((IExpress<{typeMap[baseDef.Type]}>)express).Value),");
        }
        writer.WriteLine("        _ => default");
        writer.WriteLine("    };");

        writer.WriteLine("    public static StepObjCreator Instance=>instance ;");

        writer.WriteLine(STEPOBJCREATORGET);
        writer.WriteLine("}");
    }



    const string STEPOBJCREATORGET = """
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetEnum<T>(IExpress express) where T : struct, Enum
        {
            var enumExpress = (EnumExpress)express;
            return Enum.Parse<T>(enumExpress.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetREAL(IExpress express)
        {
            if (express is RealExpress realExpress)
            {
                return realExpress.Value;
            }
            else if (express is IntegerExpress integerExpress)
            {
                return integerExpress.Value;
            }
            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetINTEGER(IExpress express)
        {
            var integerExpress = (IntegerExpress)express;
            return integerExpress.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetSTRING(IExpress express)
        {
            var stringExpress = (StringExpress)express;
            return stringExpress.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBOOLEAN(IExpress express)
        {
            var booleanExpress = (BooleanExpress)express;
            return booleanExpress.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetEntity<T>(IExpress express, Dictionary<int, IStepObj> refMap)
        {
            if (express is EntityExpress entityExpress)
            {
                var r = Create(entityExpress);
                if (r is IStepObj stepObj)
                {
                    stepObj.Init(entityExpress, refMap);
                }
                return (T)r;
            }
            else if (express is RefExpress refExpress)
            {
                return (T)refMap[refExpress.RefLineNumber];
            }
            return default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<T> GetRefAggregate<T>(IExpress express, Dictionary<int, IStepObj> refMap)
        {
            var listExpress = (ListExpress)express;
            var result = new List<T>(listExpress.ExpressList.Count);
            foreach (var item in listExpress.ExpressList)
            {
                result.Add((T)refMap[((RefExpress)item).RefLineNumber]);
            }
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object GetRefAggregateObjs(IExpress express, Dictionary<int, IStepObj> refMap, Type listType)
        {
            var listExpress = (ListExpress)express;
            var result = Activator.CreateInstance(listType, listExpress.ExpressList.Count);
            foreach (var item in listExpress.ExpressList)
            {
                ((IList)result).Add(refMap[((RefExpress)item).RefLineNumber]);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetList(IExpress express, Dictionary<int, IStepObj> refMap, Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            if (express is ListExpress listExpress)
            {
                if (listExpress.ExpressList.Count == 0)
                {
                    return Activator.CreateInstance(listType);
                }
                var firstElement = listExpress.ExpressList[0];

                if (firstElement is RefExpress)
                {
                    return GetRefAggregateObjs(express, refMap, listType);
                }
                else if (firstElement is ListExpress)
                {
                    var elementElementType = elementType.GenericTypeArguments[0];
                    var result = Activator.CreateInstance(listType, listExpress.ExpressList.Count);
                    foreach (var item in listExpress.ExpressList)
                    {
                        var element = GetList(item, refMap, elementElementType);
                        ((IList)result).Add(element);
                    }
                }
                else
                {
                    var typeName = elementType.Name.ToUpper();
                    var result = Activator.CreateInstance(listType, listExpress.ExpressList.Count);
                    foreach (var item in listExpress.ExpressList)
                    {
                        var r = Create(typeName, item);
                        if (r is IStepObj stepObj)
                        {
                            stepObj.Init(item, refMap);
                        }
                        ((IList)result).Add(r);
                    }
                    return result;
                }
            }
            return default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> GetAggregate<T>(IExpress express, Dictionary<int, IStepObj> refMap)
        {
            if (express is ListExpress listExpress)
            {
                if (listExpress.ExpressList.Count == 0)
                {
                    return [];
                }
                var firstElement = listExpress.ExpressList[0];
                if (firstElement is RefExpress)
                {
                    return GetRefAggregate<T>(express, refMap);
                }
                else if (firstElement is ListExpress)
                {
                    var elementElementType = typeof(T).GenericTypeArguments[0];
                    var result = new List<T>(listExpress.ExpressList.Count);
                    foreach (var item in listExpress.ExpressList)
                    {
                        var element = GetList(item, refMap, elementElementType);
                        ((IList)result).Add(element);
                    }
                    return result;
                }
                else
                {
                    var typeName = typeof(T).Name.ToUpper();
                    var result = new List<T>(listExpress.ExpressList.Count);
                    foreach (var item in listExpress.ExpressList)
                    {
                        var r = Create(typeName, item);
                        if (r is IStepObj stepObj)
                        {
                            stepObj.Init(item, refMap);
                        }
                        result.Add((T)r);
                    }
                    return result;

                }
            }
            return default;
        }

        public IStepObj[] CreateStepObjs(List<LineExpress> lineExpresses)
        {
            var refMap = new Dictionary<int, IStepObj>();
            var stepObjs = new IStepObj[lineExpresses.Count];
            var indexMap = new Dictionary<LineExpress, int>();
            for (int i = 0; i < lineExpresses.Count; i++)
            {
                var lineExpress = lineExpresses[i];
                switch (lineExpress.Body)
                {
                    case EntityExpress entityExpress:
                        var r = Create(entityExpress);
                        if (r is IStepObj stepObj)
                        {
                            stepObj.line_id = lineExpress.LineNumber;
                            stepObjs[i] = stepObj;
                            refMap.Add(lineExpress.LineNumber, stepObj);
                        }
                        else
                        {
                            continue;
                        }
                        break;
                    case ListExpress:
                        var complex = new StepComplexImp() { line_id = lineExpress.LineNumber };
                        stepObjs[i] = complex;
                        refMap.Add(lineExpress.LineNumber, complex);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                indexMap.Add(lineExpress, i);
            }
            foreach (var (lineExp, index) in indexMap)
            {
                var stepObj = stepObjs[index];
                stepObj.Init(lineExp.Body, refMap);
            }
            return stepObjs;
        }
    """;
}

