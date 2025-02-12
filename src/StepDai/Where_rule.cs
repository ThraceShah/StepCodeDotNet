using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepCodeDotNet;

public class Where_rule: Dictionary_instance
{
    public Express_id _label =string.Empty;
    public string _comment=string.Empty;
    public string comment_() =>_comment;
    public Express_id label_() =>_label;
}
