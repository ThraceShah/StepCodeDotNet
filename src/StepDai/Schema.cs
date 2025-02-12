using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepCodeDotNet;

public class Schema: Dictionary_instance
{
    protected string? _name;
    protected EntityDescriptorList? _entList; // list of entities in the schema
    protected EntityDescriptorList? _entsWithInverseAttrs;
    protected TypeDescriptorList? _typeList; // list of types in the schema
    protected TypeDescriptorList? _unnamed_typeList; // list of unnamed types in the schema (for cleanup)
    public string? Name()=> _name;
    public void Name(string? value) => _name = value;
}
