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

unsafe partial class ExpResolver
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

    const string LISTENTITYINIT = """
                        if (listExpress.ExpressList.Count == 1)
                        {
                            $1 = new List<$2>();
                            switch (listExpress.ExpressList[0])
                            {
                                case RefExpress refExpress:
                                    var refEntity = refMap[refExpress.RefLineNumber];
                                    if (refEntity is StepComplex stepComplex)
                                    {
                                        foreach (var item in stepComplex.complex)
                                        {
                                            if (item is $2 ___value)
                                            {
                                                $1.Add(___value);
                                            }
                                        }
                                    }
                                    else if (refEntity is $2 ___value2)
                                    {
                                        $1.Add(___value2);
                                    }
                                    else
                                    {
                                        throw new NotSupportedException();
                                    }
                                    break;
                                case ListExpress listExpress2:
                                    $1 = listExpress2.ExpressList.Select(x => StepObjCreator.Instance.Get<$2>(x, refMap)).ToList();
                                    break;
                                case EntityExpress entityExpress2:
                                    $1 = new List<$2> { StepObjCreator.Instance.Get<$2>(entityExpress2, refMap) };
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                        }
                        else
                        {
                            $1 = listExpress.ExpressList.Select(x => StepObjCreator.Instance.Get<$2>(x, refMap)).ToList();
                        }
    """;

    const string STEPOBJCREATORGET = """
        public T Get<T>(IExpress express, Dictionary<int, IStepObj> refMap)
        {
            switch (express)
            {
                case IntegerExpress integerExpress:
                    return (T)(object)(INTEGER)integerExpress.Value;
                case RealExpress realExpress:
                    return (T)(object)(REAL)realExpress.Value;
                case StringExpress stringExpress:
                    return (T)(object)(STRING)stringExpress.Value;
                case EnumExpress enumExpress:
                    return (T)(object)ToEnum(enumExpress.Value);
                case BooleanExpress booleanExpress:
                    return (T)(object)(BOOLEAN)booleanExpress.Value;
                case EntityExpress entityExpress:
                    if (IsBuiltInType(entityExpress.EntityName))
                    {
                        return Get<T>(entityExpress.Args[0], refMap);
                    }
                    var r = Create(entityExpress.EntityName);
                    r.Init(entityExpress, refMap);
                    return (T)r;
                case RefExpress refExpress:
                    return (T)refMap[refExpress.RefLineNumber];
                case DollarExpress:
                case AsteriskExpress:
                    return default;
                default:
                    throw new NotImplementedException();
            }
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
                        var r = Create(entityExpress.EntityName);
                        stepObjs[i] = r;
                        refMap.Add(lineExpress.LineNumber, r);
                        break;
                    case ListExpress listExpress:
                        var complex = new StepComplexImp() { line_id = lineExpress.LineNumber, complex = listExpress.ExpressList.ToArray() };
                        refMap.Add(lineExpress.LineNumber, complex);
                        stepObjs[i] = complex;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                indexMap.Add(lineExpress, i);
            }
            foreach (var (lineExp, index) in indexMap)
            {
                var stepObj = stepObjs[index];
                if (stepObj is not StepComplex)
                {
                    stepObj.Init(lineExp.Body, refMap);
                }
            }

            return stepObjs;
        }
    """;

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

    readonly string outputDir;
    readonly string enumOutputDir;
    readonly string typeOutputDir;
    readonly string entityOutputDir;
    readonly string entityImpOutputDir;
    readonly string aggregateOutputDir;
    readonly string globalOutputFile;
    readonly string schemaName;
    readonly string schemaPath;
    readonly Dictionary<string, List<string>> entitySelectsMap = [];
    readonly Dictionary<string, string> typeMap = new()
    {
        ["REAL"] = "System.Double",
        ["INTEGER"] = "System.Int32",
        ["STRING"] = "System.String",
        ["BOOLEAN"] = "System.Boolean",
        ["NUMBER"] = "System.Double",
    };
    readonly HashSet<nint> aggregates = [];
    readonly HashSet<nint> selects = [];
    readonly List<nint> entities = [];
    readonly Dictionary<string, string> entityInterfaceMap = [];
    readonly HashSet<string> enumNames = [];
    readonly Dictionary<string, string> enumValueMap = [];
    readonly HashSet<string> builtInTypes = ["REAL", "INTEGER", "STRING", "BOOLEAN", "NUMBER"];
    readonly Dictionary<string, List<string>> entityArgTypesMap = [];
    readonly HashSet<string> complexProperties = [];
    readonly Dictionary<string, List<string>> globalUsingTypesMap = [];
    readonly Regex listRegex = ListRegex();

    string NameSpace => $"StepCodeDotNet.Gen.{schemaName}";

    public ExpResolver(string schemaPath, string outputDir)
    {
        this.schemaPath = schemaPath;
        this.outputDir = outputDir;
        schemaName = Path.GetFileNameWithoutExtension(schemaPath);
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
        aggregateOutputDir = Path.Combine(outputDir, "Aggregates");
        if (!Directory.Exists(aggregateOutputDir))
        {
            Directory.CreateDirectory(aggregateOutputDir);
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
        PrintSchema(model);
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
        File.Copy(baseDll, destDll, true);
    }

    void PrintConstFiles()
    {
        var csprojFile = Path.Combine(outputDir, $"{schemaName}.csproj");
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

    void PrintBaseDef()
    {
        foreach (var (key, value) in typeMap)
        {
            //writer.WriteLine($"global using {key}={value};");
            using var entityWriter = new StreamWriter(Path.Combine(entityImpOutputDir, $"{key}.cs"));
            entityWriter.WriteLine($"namespace {NameSpace};");
            HashSet<string> superNames = [];
            if (entitySelectsMap.TryGetValue(key, out var selects))
            {
                foreach (var select in selects)
                {
                    superNames.Add(select);
                }
            }
            if (globalUsingTypesMap.TryGetValue(key, out var types))
            {
                foreach (var type in types)
                {
                    if (entitySelectsMap.TryGetValue(type, out var typeSelects))
                    {
                        foreach (var select in typeSelects)
                        {
                            superNames.Add(select);
                        }
                    }

                }
            }
            if (superNames.Count > 0)
            {
                entityWriter.WriteLine($"public struct {key} : {string.Join(", ", superNames)}");
            }
            else
            {
                entityWriter.WriteLine($"public struct {key}");
            }
            entityWriter.WriteLine("{");
            entityWriter.WriteLine($"    public {value} Value;");
            entityWriter.WriteLine($"     public static implicit operator {key}({value} value)");
            entityWriter.WriteLine("    {");
            entityWriter.WriteLine("        return new() { Value = value };");
            entityWriter.WriteLine("    }");
            entityWriter.WriteLine($"    public static implicit operator {value}({key} obj)");
            entityWriter.WriteLine("    {");
            entityWriter.WriteLine("        return obj.Value;");
            entityWriter.WriteLine("    }");
            entityWriter.WriteLine("}");
        }

    }

    void PrintEnumDef(Scope_* t)
    {
        var enumName = t->symbol.Name;
        enumNames.Add(enumName);
        var fileName = Path.Combine(enumOutputDir, $"{enumName}.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.WriteLine($"public enum {enumName}");
        writer.WriteLine("{");
        LISTdo_links(TYPEget_body(t)->list, link =>
        {
            var expr = (Expression_*)link->data;
            writer.WriteLine($"    {expr->symbol.Name},");
            enumValueMap[expr->symbol.Name] = $"{enumName}.{expr->symbol.Name}";
        });
        writer.WriteLine("}");
    }

    void PrintSelectDef()
    {
        foreach (Scope_* t in selects)
        {
            var selectName = t->symbol.Name;
            var fileName = Path.Combine(typeOutputDir, $"{selectName}.cs");
            using var writer = new StreamWriter(fileName);
            writer.WriteLine($"namespace {NameSpace};");
            writer.Write($"public interface {selectName}");
            if (entitySelectsMap.TryGetValue(selectName, out var selects))
            {
                writer.Write($": {string.Join(", ", selects)}");
            }
            writer.WriteLine();
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
    }

    void RecordSelectDef(Scope_* t)
    {
        selects.Add((nint)t);
    }

    void RecordAggregateDef(Scope_* t)
    {
        aggregates.Add((nint)t);
    }

    void PrintAggregateDef()
    {
        foreach (Scope_* t in aggregates)
        {
            var aggregateName = t->symbol.Name;
            var baseType = TYPEget_base_type(t);
            var fileName = Path.Combine(aggregateOutputDir, $"{aggregateName}.cs");
            var bodyType = TYPEget_body(t);
            using var writer = new StreamWriter(fileName);
            writer.WriteLine($"namespace {NameSpace};");
            var baseTypeString = GetTypeNameString(baseType);
            var typeString = CollectionTypeToString(bodyType->type, baseTypeString);
            List<string> superNames = [];
            superNames.Add(typeString);
            if (entitySelectsMap.TryGetValue(aggregateName, out var selects))
            {
                superNames.AddRange(selects);
            }
            writer.WriteLine($"public class {aggregateName} : {string.Join(", ", superNames)}");
            writer.WriteLine("{");
            writer.WriteLine("}");
        }
    }

    string CollectionTypeToString(type_enum collectionType, string contentType) => collectionType switch
    {
        type_enum.bag_ => $"List<{contentType}>",
        type_enum.set_ => $"List<{contentType}>",
        type_enum.list_ => $"List<{contentType}>",
        type_enum.array_ => $"List<{contentType}>",
        _ => throw new NotImplementedException()
    };

    string BaseTypeToString(Scope_* type) => TYPEget_body(type)->type switch
    {
        type_enum.integer_ => "INTEGER",
        type_enum.real_ => "REAL",
        type_enum.number_ => "NUMBER",
        type_enum.string_ => "STRING",
        type_enum.boolean_ => "BOOLEAN",
        type_enum.logical_ => "LOGICAL",
        _ => type->symbol.Name
    };

    string GetTypeNameString(Scope_* type)
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
            var typeString = CollectionTypeToString(bodyType->type, GetAttrTypeName(GetTypeNameString(baseType)));
            return typeString;
        }
        else
        {
            return BaseTypeToString(type);
        }
    }

    void PrintUsingTypeDef(Scope_* type)
    {
        using var globalWriter = new StreamWriter(globalOutputFile, true);
        var baseType = BaseTypeToString(type);
        if (globalUsingTypesMap.TryGetValue(baseType, out var types))
        {
            types.Add(type->symbol.Name);
        }
        else
        {
            globalUsingTypesMap[baseType] = [type->symbol.Name];
        }
        globalWriter.WriteLine($"global using {type->symbol.Name}=StepCodeDotNet.Gen.ap203.{baseType};");
    }

    void PrintTypeDef(Scope_* type)
    {
        if (TYPEis_enumeration(type))
        {
            PrintEnumDef(type);
        }
        else if (TYPEis_select(type))
        {
            RecordSelectDef(type);
        }
        else if (TYPEis_aggregate(type))
        {
            RecordAggregateDef(type);
        }
        else
        {
            builtInTypes.Add(type->symbol.Name);
            PrintUsingTypeDef(type);
        }
    }


    string EntityNameToInterfaceName(string entityName)
    {
        var name = char.ToUpper(entityName[0]) + entityName[1..];
        return $"I{name}";
    }


    void GenerateEnityInterfaceName(Scope_* entity)
    {
        var entityName = entity->symbol.Name;
        var interfaceName = EntityNameToInterfaceName(entityName);
        entityInterfaceMap.Add(entityName, interfaceName);
    }

    void PrintEntityDef(Scope_* entity)
    {
        var entityName = entity->symbol.Name;
        var interfaceName = entityInterfaceMap[entityName];
        var fileName = Path.Combine(entityOutputDir, $"{interfaceName}.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.Write($"public interface {interfaceName}");
        List<string> superNames = [];
        if (entitySelectsMap.TryGetValue(entityName, out var selects))
        {
            superNames.AddRange(selects);
        }
        var supertypes = ENTITYget_supertypes(entity);
        if (LISTempty(supertypes) is false)
        {
            (*supertypes).For<Scope_>(super =>
            {
                superNames.Add(EntityNameToInterfaceName(super->symbol.Name));
            });
        }
        else
        {
            superNames.Add("IStepObj");
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
                    var typeName = GetAttrTypeName(GetTypeNameString(attr->type));
                    var attrName = attr->name->symbol.Name;
                    writer.WriteLine($"    {typeName} {attrName} {{ get; set; }}");
                    complexProperties.Add($"{typeName} {interfaceName}.{attrName} {{ get; set; }}");
                }
            });
        }
        writer.WriteLine("}");

        entities.Add((nint)entity);

    }

    string GetAttrTypeName(string typeName)
    {
        if (entityInterfaceMap.TryGetValue(typeName, out var interfaceName))
        {
            return interfaceName;
        }
        return typeName;
    }

    void PrintComplexImpDef()
    {
        var fileName = Path.Combine(outputDir, "StepComplexImp.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.Write("public class StepComplexImp : StepComplex");
        var intefaces = new HashSet<string>();
        foreach (var (key, value) in entityInterfaceMap)
        {
            intefaces.Add(value);
        }
        foreach (var (key, value) in entitySelectsMap)
        {
            foreach (var select in value)
            {
                intefaces.Add(select);
            }
        }
        if (intefaces.Count > 0)
        {
            writer.Write($",{string.Join(", ", intefaces)}");
        }
        writer.WriteLine();
        writer.WriteLine("{");
        foreach (var complexProperty in complexProperties)
        {
            writer.WriteLine($"    {complexProperty}");
        }
        writer.WriteLine("    public override void Init(IExpress expression, Dictionary<int, IStepObj> refMap)");
        writer.WriteLine("    {");
        writer.WriteLine("        throw new NotImplementedException();");
        writer.WriteLine("    }");
        writer.WriteLine("}");
    }



    void PrintEntityImpes()
    {
        foreach (Scope_* entity in entities)
        {
            var entityName = entity->symbol.Name;
            var fileName = Path.Combine(entityImpOutputDir, $"{entityName}_imp.cs");
            using var writer = new StreamWriter(fileName);
            writer.WriteLine($"namespace {NameSpace};");
            writer.WriteLine($"public class {entityName}_imp : {entityInterfaceMap[entityName]}");
            writer.WriteLine("{");
            writer.WriteLine("    public int line_id { get; set; }");
            var initArugments = new List<(string, string)>
            {
                ("int", "line_id")
            };
            var initArugments2 = new List<(string, string)>
            {
                ("int", "line_id")
            };
            var argsTypes = new List<string>();
            argsTypes.Add("Int32");
            var allAttributes = ENTITYget_all_attributes2(entity);
            var attrsSet = new HashSet<string>();
            foreach (Variable_* attr in allAttributes)
            {
                if (VARis_derived(attr) is false)
                {
                    var typeName = GetAttrTypeName(GetTypeNameString(attr->type));
                    var attrName = attr->name->symbol.Name;
                    if (attrsSet.Add(attrName))
                    {
                        writer.WriteLine($"    public {typeName} {attrName} {{ get; set; }}");
                        initArugments.Add((typeName, attrName));
                        if (enumNames.Contains(typeName))
                        {
                            initArugments2.Add(("int", attrName));
                        }
                        else
                        {
                            initArugments2.Add((typeName, attrName));
                        }
                        argsTypes.Add(typeName);
                    }
                }
            }
            entityArgTypesMap[entityName] = argsTypes;
            writer.WriteLine("    public IExpress[] complex { get; set; }");
            string initArugmentsString = string.Join(", ", initArugments.Select(x => $"{x.Item1} {x.Item2} = default"));
            writer.WriteLine($"    public void Init({initArugmentsString})");
            writer.WriteLine("    {");
            foreach (var arugment in initArugments)
            {
                writer.WriteLine($"        this.{arugment.Item2} = {arugment.Item2};");
            }
            writer.WriteLine("    }");
            string initArugmentsString2 = string.Join(", ", initArugments2.Select(x => $"{x.Item1} {x.Item2} = default"));
            if (initArugmentsString2 != initArugmentsString)
            {
                writer.WriteLine($"    public void Init({initArugmentsString2})");
                writer.WriteLine("    {");
                foreach (var arugment in initArugments)
                {
                    if (enumNames.Contains(arugment.Item1))
                    {
                        writer.WriteLine($"        this.{arugment.Item2} = ({arugment.Item1}){arugment.Item2};");
                    }
                    else
                    {
                        writer.WriteLine($"        this.{arugment.Item2} = {arugment.Item2};");
                    }
                }
                writer.WriteLine("    }");
            }

            writer.WriteLine($"    public static implicit operator {entityName}_imp(StepComplex complex)");
            writer.WriteLine("    {");
            writer.WriteLine($"        return new {entityName}_imp() {{ line_id = complex.line_id, complex = complex.complex }};");
            writer.WriteLine("    }");
            writer.WriteLine($"    public static implicit operator StepComplex({entityName}_imp obj)");
            writer.WriteLine("    {");
            writer.WriteLine($"        return new StepComplexImp() {{ line_id = obj.line_id, complex = obj.complex }};");
            writer.WriteLine("    }");


            writer.WriteLine("    public void Init(IExpress expression, Dictionary<int, IStepObj> refMap)");
            writer.WriteLine("    {");
            writer.WriteLine("        switch (expression)");
            writer.WriteLine("        {");
            writer.WriteLine("            case EntityExpress entityExpress:");
            writer.WriteLine("            {");
            writer.WriteLine("                var argExps = entityExpress.Args;");
            for (int i = 1; i < initArugments.Count; i++)
            {
                var arugment = initArugments[i];
                int x = i - 1;
                writer.WriteLine($"                if({x} >= argExps.Count)");
                writer.WriteLine("                {");
                writer.WriteLine($"                    break;");
                writer.WriteLine("                }");
                if (arugment.Item1.StartsWith("List<"))
                {
                    var match = listRegex.Match(arugment.Item1);
                    var typeName = match.Groups[1].Value;
                    writer.WriteLine("                {");
                    writer.WriteLine($"                    var listExpress = (ListExpress)argExps[{x}];");
                    if (builtInTypes.Contains(typeName))
                    {
                        writer.WriteLine($"                this.{arugment.Item2} = ((ListExpress)argExps[{x}]).ExpressList.Select(x=>StepObjCreator.Instance.Get<{typeName}>(x,refMap)).ToList();");
                    }
                    else
                    {
                        writer.WriteLine(LISTENTITYINIT.Replace("$1", $"this.{arugment.Item2}").Replace("$2", typeName));

                    }
                    writer.WriteLine("                }");
                }
                else
                {
                    writer.WriteLine($"                this.{arugment.Item2} = StepObjCreator.Instance.Get<{arugment.Item1}>(argExps[{x}],refMap);");
                }
            }
            writer.WriteLine("                break;");
            writer.WriteLine("            }");
            writer.WriteLine("            case ListExpress listExpress:");
            writer.WriteLine("            {");
            writer.WriteLine("                this.complex=listExpress.ExpressList.ToArray();");
            writer.WriteLine("                break;");
            writer.WriteLine("            }");
            writer.WriteLine("            default:");
            writer.WriteLine("                throw new NotImplementedException();");
            writer.WriteLine("        }");
            writer.WriteLine("    }");
            writer.WriteLine("}");

        }
    }


    void PrintStaticInflect()
    {
        var fileName = Path.Combine(outputDir, "StepObjCreator.cs");
        using var writer = new StreamWriter(fileName);
        writer.WriteLine($"namespace {NameSpace};");
        writer.WriteLine("using System.Collections.Frozen;");
        writer.WriteLine("public class StepObjCreator:IStepObjCreator");
        writer.WriteLine("{");
        writer.WriteLine("    private static readonly FrozenDictionary<string,FrozenSet<string>> initArgTypes;");
        writer.WriteLine("    private static readonly StepObjCreator instance = new();");
        writer.WriteLine("    static StepObjCreator()");
        writer.WriteLine("    {");
        writer.WriteLine("        var dict=new Dictionary<string,FrozenSet<string>>();");
        foreach (var (key, value) in entityArgTypesMap)
        {
            writer.WriteLine("        {");
            writer.Write($"            string[] value=[");
            foreach (var argType in value)
            {
                writer.Write($"\"{argType}\",");
            }
            writer.WriteLine("];");
            writer.WriteLine($"            dict.Add(\"{key}\",FrozenSet.Create<string>(value));");
            writer.WriteLine("        }");
        }
        writer.WriteLine("        initArgTypes= dict.ToFrozenDictionary();");
        writer.WriteLine("    }");

        writer.WriteLine("    public IStepObj Create(string entityName) => entityName.ToLower() switch");
        writer.WriteLine("    {");
        foreach (Scope_* entity in entities)
        {
            var entityName = entity->symbol.Name;
            writer.WriteLine($"        \"{entityName}\" => new {entityName}_imp(),");
        }
        writer.WriteLine("        _ => throw new NotImplementedException()");
        writer.WriteLine("    };");
        writer.WriteLine("    public T Create<T>(string entityName) where T : IStepObj => (T)Create(entityName);");
        writer.WriteLine("    public int ToEnum(string value)=> value.ToLower() switch");
        writer.WriteLine("    {");
        foreach (var (key, value) in enumValueMap)
        {
            writer.WriteLine($"        \"{key}\" => (int){value},");
        }
        writer.WriteLine("        _ => throw new NotImplementedException()");
        writer.WriteLine("    };");
        writer.WriteLine("    public bool IsBuiltInType(string typeName) => typeName.ToLower() switch");
        writer.WriteLine("    {");
        foreach (var typeName in builtInTypes)
        {
            writer.WriteLine($"        \"{typeName.ToLower()}\" => true,");
        }
        writer.WriteLine("        _ => false");
        writer.WriteLine("    };");
        writer.WriteLine("    public FrozenSet<string> GetInitArgTypes(string entityName)=>initArgTypes[entityName];");

        writer.WriteLine("    public static StepObjCreator Instance=>instance ;");

        writer.WriteLine(STEPOBJCREATORGET);
        writer.WriteLine("}");
    }

    void PrintSchema(Scope_* express)
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
                    PrintSelectDef();
                    SCOPEdo_entities(schema, &de, GenerateEnityInterfaceName);
                    SCOPEdo_entities(schema, &de, PrintEntityDef);
                    PrintEntityImpes();
                    PrintStaticInflect();
                    PrintAggregateDef();
                    PrintComplexImpDef();
                    PrintBaseDef();
                    schema->search_id = PROCESSED;
                    complete = complete && (schema->search_id == PROCESSED);
                }
            }

        }
    }

    [GeneratedRegex(@"List<(.+)>")]
    private static partial Regex ListRegex();
}