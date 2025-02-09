using System.Runtime.CompilerServices;

namespace StepCodeDotNet.Interop;

public partial struct Hash_Table_
{
    [NativeTypeName("unsigned int")]
    public uint p;

    [NativeTypeName("unsigned int")]
    public uint maxp;

    [NativeTypeName("unsigned int")]
    public uint KeyCount;

    [NativeTypeName("unsigned int")]
    public uint SegmentCount;

    [NativeTypeName("unsigned int")]
    public uint MinLoadFactor;

    [NativeTypeName("unsigned int")]
    public uint MaxLoadFactor;

    [NativeTypeName("Segment[256]")]
    public _Directory_e__FixedBuffer Directory;

    public unsafe partial struct _Directory_e__FixedBuffer
    {
        public Element_** e0;
        public Element_** e1;
        public Element_** e2;
        public Element_** e3;
        public Element_** e4;
        public Element_** e5;
        public Element_** e6;
        public Element_** e7;
        public Element_** e8;
        public Element_** e9;
        public Element_** e10;
        public Element_** e11;
        public Element_** e12;
        public Element_** e13;
        public Element_** e14;
        public Element_** e15;
        public Element_** e16;
        public Element_** e17;
        public Element_** e18;
        public Element_** e19;
        public Element_** e20;
        public Element_** e21;
        public Element_** e22;
        public Element_** e23;
        public Element_** e24;
        public Element_** e25;
        public Element_** e26;
        public Element_** e27;
        public Element_** e28;
        public Element_** e29;
        public Element_** e30;
        public Element_** e31;
        public Element_** e32;
        public Element_** e33;
        public Element_** e34;
        public Element_** e35;
        public Element_** e36;
        public Element_** e37;
        public Element_** e38;
        public Element_** e39;
        public Element_** e40;
        public Element_** e41;
        public Element_** e42;
        public Element_** e43;
        public Element_** e44;
        public Element_** e45;
        public Element_** e46;
        public Element_** e47;
        public Element_** e48;
        public Element_** e49;
        public Element_** e50;
        public Element_** e51;
        public Element_** e52;
        public Element_** e53;
        public Element_** e54;
        public Element_** e55;
        public Element_** e56;
        public Element_** e57;
        public Element_** e58;
        public Element_** e59;
        public Element_** e60;
        public Element_** e61;
        public Element_** e62;
        public Element_** e63;
        public Element_** e64;
        public Element_** e65;
        public Element_** e66;
        public Element_** e67;
        public Element_** e68;
        public Element_** e69;
        public Element_** e70;
        public Element_** e71;
        public Element_** e72;
        public Element_** e73;
        public Element_** e74;
        public Element_** e75;
        public Element_** e76;
        public Element_** e77;
        public Element_** e78;
        public Element_** e79;
        public Element_** e80;
        public Element_** e81;
        public Element_** e82;
        public Element_** e83;
        public Element_** e84;
        public Element_** e85;
        public Element_** e86;
        public Element_** e87;
        public Element_** e88;
        public Element_** e89;
        public Element_** e90;
        public Element_** e91;
        public Element_** e92;
        public Element_** e93;
        public Element_** e94;
        public Element_** e95;
        public Element_** e96;
        public Element_** e97;
        public Element_** e98;
        public Element_** e99;
        public Element_** e100;
        public Element_** e101;
        public Element_** e102;
        public Element_** e103;
        public Element_** e104;
        public Element_** e105;
        public Element_** e106;
        public Element_** e107;
        public Element_** e108;
        public Element_** e109;
        public Element_** e110;
        public Element_** e111;
        public Element_** e112;
        public Element_** e113;
        public Element_** e114;
        public Element_** e115;
        public Element_** e116;
        public Element_** e117;
        public Element_** e118;
        public Element_** e119;
        public Element_** e120;
        public Element_** e121;
        public Element_** e122;
        public Element_** e123;
        public Element_** e124;
        public Element_** e125;
        public Element_** e126;
        public Element_** e127;
        public Element_** e128;
        public Element_** e129;
        public Element_** e130;
        public Element_** e131;
        public Element_** e132;
        public Element_** e133;
        public Element_** e134;
        public Element_** e135;
        public Element_** e136;
        public Element_** e137;
        public Element_** e138;
        public Element_** e139;
        public Element_** e140;
        public Element_** e141;
        public Element_** e142;
        public Element_** e143;
        public Element_** e144;
        public Element_** e145;
        public Element_** e146;
        public Element_** e147;
        public Element_** e148;
        public Element_** e149;
        public Element_** e150;
        public Element_** e151;
        public Element_** e152;
        public Element_** e153;
        public Element_** e154;
        public Element_** e155;
        public Element_** e156;
        public Element_** e157;
        public Element_** e158;
        public Element_** e159;
        public Element_** e160;
        public Element_** e161;
        public Element_** e162;
        public Element_** e163;
        public Element_** e164;
        public Element_** e165;
        public Element_** e166;
        public Element_** e167;
        public Element_** e168;
        public Element_** e169;
        public Element_** e170;
        public Element_** e171;
        public Element_** e172;
        public Element_** e173;
        public Element_** e174;
        public Element_** e175;
        public Element_** e176;
        public Element_** e177;
        public Element_** e178;
        public Element_** e179;
        public Element_** e180;
        public Element_** e181;
        public Element_** e182;
        public Element_** e183;
        public Element_** e184;
        public Element_** e185;
        public Element_** e186;
        public Element_** e187;
        public Element_** e188;
        public Element_** e189;
        public Element_** e190;
        public Element_** e191;
        public Element_** e192;
        public Element_** e193;
        public Element_** e194;
        public Element_** e195;
        public Element_** e196;
        public Element_** e197;
        public Element_** e198;
        public Element_** e199;
        public Element_** e200;
        public Element_** e201;
        public Element_** e202;
        public Element_** e203;
        public Element_** e204;
        public Element_** e205;
        public Element_** e206;
        public Element_** e207;
        public Element_** e208;
        public Element_** e209;
        public Element_** e210;
        public Element_** e211;
        public Element_** e212;
        public Element_** e213;
        public Element_** e214;
        public Element_** e215;
        public Element_** e216;
        public Element_** e217;
        public Element_** e218;
        public Element_** e219;
        public Element_** e220;
        public Element_** e221;
        public Element_** e222;
        public Element_** e223;
        public Element_** e224;
        public Element_** e225;
        public Element_** e226;
        public Element_** e227;
        public Element_** e228;
        public Element_** e229;
        public Element_** e230;
        public Element_** e231;
        public Element_** e232;
        public Element_** e233;
        public Element_** e234;
        public Element_** e235;
        public Element_** e236;
        public Element_** e237;
        public Element_** e238;
        public Element_** e239;
        public Element_** e240;
        public Element_** e241;
        public Element_** e242;
        public Element_** e243;
        public Element_** e244;
        public Element_** e245;
        public Element_** e246;
        public Element_** e247;
        public Element_** e248;
        public Element_** e249;
        public Element_** e250;
        public Element_** e251;
        public Element_** e252;
        public Element_** e253;
        public Element_** e254;
        public Element_** e255;

        public ref Element_** this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (Element_*** pThis = &e0)
                {
                    return ref pThis[index];
                }
            }
        }
    }
}
