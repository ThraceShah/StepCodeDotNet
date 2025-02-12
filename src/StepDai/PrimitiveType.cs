using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepCodeDotNet;
public enum PrimitiveType
{
    sdaiINTEGER = 0x0001,
    sdaiREAL = 0x0002,
    sdaiBOOLEAN = 0x0004,
    sdaiLOGICAL = 0x0008,
    sdaiSTRING = 0x0010,
    sdaiBINARY = 0x0020,
    sdaiENUMERATION = 0x0040,
    sdaiSELECT = 0x0080,
    sdaiINSTANCE = 0x0100,
    sdaiAGGR = 0x0200,
    sdaiNUMBER = 0x0400,
    // The elements defined below are not part of part 23
    // (IMS: these should not be used as bitmask fields)
    ARRAY_TYPE,     // DAS
    BAG_TYPE,       // DAS
    SET_TYPE,       // DAS
    LIST_TYPE,      // DAS
    GENERIC_TYPE,
    REFERENCE_TYPE,
    UNKNOWN_TYPE
};