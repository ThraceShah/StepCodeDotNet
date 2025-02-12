using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepCodeDotNet;

public class SchRename
{
    private string schName = string.Empty;
    private string newName = string.Empty;
    public SchRename(string sch="",string newnm = "")
    {
        schName = sch;
        newName = newnm;
    }
    public string objName()
    {
        return schName;
    }
    public static bool  operator <(SchRename left,SchRename right)
    {
        return string.Compare(left.schName, right.schName) < 0;
    }
    public static bool operator >(SchRename self, SchRename schrnm)
    {
        return string.Compare(self.schName, schrnm.schName) > 0;
    }


    public bool choice(string nm)
    {
        if (string.Compare(schName, nm,true) == 0)
        {
            return true;
        }
        if(next is not null)
        {
            return next.choice(nm);
        }
        return false;
    }
    public string? rename(string schnm,ref string newnm)
    {
        if (string.Compare(schnm, schName, true) == 0)
        {
            newnm = newName;
            return newName;
        }
        if (next is not null)
        {
            return next.rename(schnm,ref newnm);
        }
        return null;
    }

    public SchRename? next;
}

