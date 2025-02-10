using System;
using System.Runtime.InteropServices;
using System.Text;

namespace StepCodeDotNet
{
    public enum Severity
    {
        SEVERITY_MAX = -5,
        SEVERITY_DUMP = -4,
        SEVERITY_EXIT = -3,   // fatal
        SEVERITY_BUG = -2,    // non-recoverable error -- probably bug
        SEVERITY_INPUT_ERROR = -1,  // non-recoverable error
        SEVERITY_WARNING = 0,    // recoverable error
        SEVERITY_INCOMPLETE = 1,    // incomplete data
        SEVERITY_USERMSG = 2,    // possibly an error
        SEVERITY_NULL = 3 // no error or message
    }

    public enum DebugLevel
    {
        DEBUG_OFF = 0,
        DEBUG_USR = 1,
        DEBUG_ALL = 2
    }

    public unsafe class ErrorDescriptor
    {
        private string _userMsg = string.Empty;
        private string _detailMsg = string.Empty;
        private Severity _severity;
        private static DebugLevel _debug_level;

        public ErrorDescriptor(Severity s = Severity.SEVERITY_NULL, DebugLevel d = DebugLevel.DEBUG_OFF)
        {
            _severity = s;
            if (d != DebugLevel.DEBUG_OFF)
            {
                _debug_level = d;
            }
        }

        public void PrintContents()
        {
            Console.WriteLine($"Severity: {severityString()}");
            if (!string.IsNullOrEmpty(_userMsg))
            {
                Console.WriteLine("User message in parens:");
                Console.WriteLine($"({_userMsg})");
            }
            if (!string.IsNullOrEmpty(_detailMsg))
            {
                Console.WriteLine("Detailed message in parens:");
                Console.WriteLine($"({_detailMsg})");
            }
        }

        public void ClearErrorMsg()
        {
            _severity = Severity.SEVERITY_NULL;
            _userMsg = string.Empty;
            _detailMsg = string.Empty;
        }

        public Severity severity() => _severity;

        public Severity severity(Severity s) => _severity = s;

        public string severityString()
        {
            return _severity switch
            {
                Severity.SEVERITY_NULL => "SEVERITY_NULL",
                Severity.SEVERITY_USERMSG => "SEVERITY_USERMSG",
                Severity.SEVERITY_INCOMPLETE => "SEVERITY_INCOMPLETE",
                Severity.SEVERITY_WARNING => "SEVERITY_WARNING",
                Severity.SEVERITY_INPUT_ERROR => "SEVERITY_INPUT_ERROR",
                Severity.SEVERITY_BUG => "SEVERITY_BUG",
                Severity.SEVERITY_EXIT => "SEVERITY_EXIT",
                Severity.SEVERITY_DUMP => "SEVERITY_DUMP",
                Severity.SEVERITY_MAX => "SEVERITY_MAX",
                _ => "UNKNOWN"
            };
        }

        public Severity GetCorrSeverity(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                s = s.ToUpper();
                return s switch
                {
                    "SEVERITY_NULL" => Severity.SEVERITY_NULL,
                    "SEVERITY_USERMSG" => Severity.SEVERITY_USERMSG,
                    "SEVERITY_INCOMPLETE" => Severity.SEVERITY_INCOMPLETE,
                    "SEVERITY_WARNING" => Severity.SEVERITY_WARNING,
                    "SEVERITY_INPUT_ERROR" => Severity.SEVERITY_INPUT_ERROR,
                    "SEVERITY_BUG" => Severity.SEVERITY_BUG,
                    "SEVERITY_EXIT" => Severity.SEVERITY_EXIT,
                    "SEVERITY_DUMP" => Severity.SEVERITY_DUMP,
                    "SEVERITY_MAX" => Severity.SEVERITY_MAX,
                    _ => Severity.SEVERITY_BUG
                };
            }
            Console.Error.WriteLine("Internal error:  " + nameof(ErrorDescriptor) + " " + nameof(GetCorrSeverity));
            Console.Error.WriteLine("Calling ErrorDescriptor::GetCorrSeverity() with null string");
            return Severity.SEVERITY_BUG;
        }

        public string UserMsg() => _userMsg;

        public void UserMsg(string msg) => _userMsg = msg;

        public void AppendToUserMsg(string msg) => _userMsg += msg;

        public void PrependToUserMsg(string msg) => _userMsg = msg + _userMsg;

        public string DetailMsg() => _detailMsg;

        public void DetailMsg(string msg) => _detailMsg = msg;

        public void AppendToDetailMsg(string msg) => _detailMsg += msg;

        public void PrependToDetailMsg(string msg) => _detailMsg = msg + _detailMsg;

        public Severity AppendFromErrorArg(ErrorDescriptor err)
        {
            GreaterSeverity(err.severity());
            AppendToDetailMsg(err.DetailMsg());
            AppendToUserMsg(err.UserMsg());
            return severity();
        }

        public Severity GreaterSeverity(Severity s) => _severity = (s < _severity) ? s : _severity;

        public DebugLevel debug_level() => _debug_level;

        public void debug_level(DebugLevel d) => _debug_level = d;

        public void SetOutput(Stream o) { }

    }
}