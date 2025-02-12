using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepCodeDotNet;

public class TypeDescriptor
{
    protected readonly string? _name;
    protected string _altname=string.Empty;
    protected SchRename? altNames;
    protected PrimitiveType _fundamentalType = PrimitiveType.UNKNOWN_TYPE;
    protected Schema? _originatingSchema;
    protected TypeDescriptor? _referentType;
    protected string? _description;

    public List<Where_rule>? _where_rules;
    public List<Where_rule>? where_rules_() => _where_rules;
    public void where_rules_(List<Where_rule> wrl) => _where_rules = wrl;
    protected bool PossName(string nm) => OurName(nm) || AltName(nm);
    protected bool OurName(string nm) => string.Compare(nm, _name, true) == 0;
    protected bool AltName(string nm) => (altNames is not null) && altNames.choice(nm);
    public TypeDescriptor(string nm, PrimitiveType ft, string d)
    {
        _name = nm;
        _fundamentalType = ft;
        _description = d;
    }
    public TypeDescriptor(string nm, PrimitiveType ft,
                        Schema origSchema, string d)
    {
        _name = nm;
        _fundamentalType = ft;
        _originatingSchema = origSchema;
        _description = d;
    }
    public TypeDescriptor()
    {
    }
    public static implicit operator bool(TypeDescriptor? td) => td is not null;

    public virtual string GenerateExpress()
    {
        StringBuilder buf = new();
        buf.Append("TYPE ");
        buf.Append(Name()?.ToLower());
        buf.Append(" = ");
        string? desc = Description();
        if (desc is not null)
        {
            foreach (var c in desc)
            {
                if (c == ',')
                {
                    buf.Append(",\n  ");
                }
                else if (c == '(')
                {
                    buf.Append("\n  (");
                }
                else if (char.IsUpper(c))
                {
                    buf.Append(char.ToLower(c));
                }
                else
                {
                    buf.Append(c);
                }
            }
        }
        buf.Append(";\n");
        if (_where_rules is not null)
        {
            int all_comments = 1;
            int count = _where_rules.Count;
            for (int i = 0; i < count; i++)
            { // print out each UNIQUE rule
                if (_where_rules[i]._label.Length == 0)
                {
                    all_comments = 0;
                }
            }

            if (all_comments != 0)
            {
                buf.Append("  (* WHERE *)\n");
            }
            else
            {
                buf.Append("    WHERE\n");
            }

            for (int i = 0; i < count; i++)
            { // print out each WHERE rule
                if (_where_rules[i]._comment.Length > 0)
                {
                    buf.Append("    ");
                    buf.Append(_where_rules[i].comment_());
                }
                if (_where_rules[i]._label.Length > 0)
                {
                    buf.Append("      ");
                    buf.Append(_where_rules[i].label_());
                }
            }
        }
        buf.Append("END_TYPE;\n");
        return buf.ToString();
    }
    public string? Name(string? schnm = null)
    {
        if (schnm is null)
        {
            return _name;
        }
        if (altNames is not null&& altNames.rename(schnm,ref _altname) is not null)
        {
            return _altname;
        }
        return _name;
    }
    public void AttrTypeName(out string? buf, string? schnm = null)
    {
        var sn = Name(schnm);
        if (sn is not null)
        {
            buf = sn.ToLower();
        }
        else
        {
            buf = _description;
        }
    }
    public SchRename? AltNameList() => altNames;
    public string? TypeString()
    {
        StringBuilder s = new();
        switch (Type())
        {
            case PrimitiveType.REFERENCE_TYPE:
                if (Name() is not null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                if (Description() is not null)
                {
                    s.Append(Description());
                }
                if (_referentType is not null)
                {
                    s.Append(" -- ");
                    s.Append(_referentType.TypeString());
                }
                return s.ToString();

            case PrimitiveType.sdaiINTEGER:
                s.Clear();
                if (_referentType != null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                s.Append("Integer");
                break;
            case PrimitiveType.sdaiSTRING:
                s.Clear();
                if (_referentType != null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                s.Append("String");
                break;
            case PrimitiveType.sdaiREAL:
                s.Clear();
                if (_referentType != null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                s.Append("Real");
                break;
            case PrimitiveType.sdaiENUMERATION:
                s.Append("Enumeration");
                if (Name() is not null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                if (Description() is not null)
                {
                    s.Append(Description());
                }
                break;
            case PrimitiveType.sdaiBOOLEAN:
                s.Clear();
                if (_referentType != null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                s.Append("Boolean: F, T");
                break;
            case PrimitiveType.sdaiLOGICAL:
                s.Clear();
                if (_referentType != null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                s.Append("Logical: F, T, U");
                break;
            case PrimitiveType.sdaiNUMBER:
                s.Clear();
                if (_referentType != null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                s.Append("Number");
                break;
            case PrimitiveType.sdaiBINARY:
                s.Clear();
                if (_referentType != null)
                {
                    s.Append("TYPE ");
                    s.Append(Name());
                    s.Append(" = ");
                }
                s.Append("Binary");
                break;
            case PrimitiveType.sdaiINSTANCE:
                s.Append("Entity");
                if (Name() is not null)
                {
                    s.Append(Name());
                }
                break;
            case PrimitiveType.sdaiAGGR:
            case PrimitiveType.ARRAY_TYPE:
            case PrimitiveType.BAG_TYPE:
            case PrimitiveType.SET_TYPE:
            case PrimitiveType.LIST_TYPE:
                s.Append(Description());
                if (_referentType is not null)
                {
                    s.Append(" -- ");
                    s.Append(_referentType.TypeString());
                }
                break;
            case PrimitiveType.sdaiSELECT:
                s.Append(Description());
                break;
            case PrimitiveType.GENERIC_TYPE:
            case PrimitiveType.UNKNOWN_TYPE:
                s.Append("Unknown");
                break;
        }
        return s.ToString();
    }
    public PrimitiveType Type() => _fundamentalType;
    public void Type(PrimitiveType t) => _fundamentalType = t;
    public PrimitiveType BaseType()
    {
        var td = BaseTypeDescriptor();
        if (td is not null)
        {
            return td.FundamentalType();
        }
        else
        {
            return PrimitiveType.sdaiINSTANCE;
        }
    }
    public TypeDescriptor BaseTypeDescriptor()
    {
        var td = this;
        while (td._referentType is not null)
        {
            td = td._referentType;
        }
        return td;
    }
    public string? BaseTypeName()
    {
        var bt = BaseTypeDescriptor();
        return bt?.Name();
    }
    public PrimitiveType NonRefType()
    {
        var td = NonRefTypeDescriptor();
        if (td is not null)
        {
            return td.FundamentalType();
        }
        else
        {
            return PrimitiveType.UNKNOWN_TYPE;
        }
    }
    public TypeDescriptor NonRefTypeDescriptor()
    {
        var td = this;
        while (td._referentType is not null)
        {
            if (td.Type() != PrimitiveType.REFERENCE_TYPE)
            {
                return td;
            }
            td = td._referentType;
        }
        return td;
    }
    public int IsAggrType() => NonRefType() switch
    {
        PrimitiveType.sdaiAGGR or PrimitiveType.ARRAY_TYPE or
        PrimitiveType.BAG_TYPE or PrimitiveType.SET_TYPE or
        PrimitiveType.LIST_TYPE => 1,
        _ => 0,
    };
    public PrimitiveType AggrElemType()
    {
        var aggrElemTD = AggrElemTypeDescriptor();
        if (aggrElemTD is not null)
        {
            return aggrElemTD.Type();
        }
        return PrimitiveType.UNKNOWN_TYPE;
    }
    public TypeDescriptor? AggrElemTypeDescriptor()
    {
        var aggrTD = NonRefTypeDescriptor();
        var aggrElemTD = aggrTD.ReferentType();
        if (aggrElemTD is not null)
        {
            aggrElemTD = aggrElemTD.NonRefTypeDescriptor();
        }
        return aggrElemTD;
    }
    public PrimitiveType FundamentalType() => _fundamentalType;
    public void FundamentalType(PrimitiveType ft) => _fundamentalType = ft;
    public TypeDescriptor? ReferentType() => _referentType;
    public void ReferentType(TypeDescriptor? rt) => _referentType = rt;
    public Schema? OriginatingSchema() => _originatingSchema;
    public void OriginatingSchema(Schema? os) => _originatingSchema = os;
    public string schemaName()
    {
        if (_originatingSchema is not null)
        {
            return _originatingSchema.Name();
        }
        return string.Empty;
    }
    public string? Description() => _description;
    public void Description(string desc) => _description = desc;
    public virtual TypeDescriptor? IsA(TypeDescriptor other)
    {
        if (this == other)
        {
            return other;
        }
        return null;

    }
    public virtual TypeDescriptor? BaseTypeIsA(TypeDescriptor td)=> NonRefType() switch
    {
        PrimitiveType.sdaiAGGR => AggrElemTypeDescriptor()?.IsA(td),
        _ => IsA(td),
    };
    public virtual TypeDescriptor? IsA(string other)
    {
        if (Name() is null)
        {
            return null;
        }
        if (string.Compare(_name,other,true)==0)
        {   // this is the type
            return this;
        }
        return _referentType?.IsA(other);

    }
    public virtual TypeDescriptor? CanBe(TypeDescriptor td)=>IsA(td);
    public virtual TypeDescriptor? CanBe(string n) => IsA(n);

    public virtual TypeDescriptor? CanBeSet(string n,string? schNm=null) => CurrName(n, schNm) ? this : null;

    public bool CurrName(string other, string? schNm = null ) 
    {
        if (schNm is null || string.IsNullOrEmpty(schNm))
        {
            // If there's no current schema, accept any possible name of this.
            // (I.e., accept its actual name or any substitute):
            return PossName(other);
        }
        if (altNames is not null&& altNames.rename(schNm,ref _altname) is not null)
        {
            // If we have a different name when the current schema = schNm, then
            // other better = the alt name.
            return string.Compare(_altname, other, true) == 0;
        }
        else
        {
            // If we have no designated alternate name when the current schema =
            // schNm, other must = our _name.
            return OurName(other);
        }
    }
    public void addAltName(string schnm, string newnm )
    {
        var newpair = new SchRename(schnm, newnm);
        SchRename? node=altNames;
        SchRename? prev = null;
        while (node is not null && node < newpair)
        {
            prev = node;
            node = node.next;
        }
        newpair.next = node; // node may = NULL
        if (prev is not null)
        {
            // Will be the case if new node should not be first (and above while
            // loop was entered).
            prev.next = newpair;
        }
        else
        {
            // put newpair first
            altNames = newpair;
        }

    }
}

