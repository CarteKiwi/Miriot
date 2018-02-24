using System;

namespace Miriot.Common
{
    public class Constants
    {
        public static readonly Guid SERVICE_UUID = new Guid("4D93B7BA-A8CD-4C95-A11E-4B9B3EFB3417");
        public static readonly Guid SERVICE_READ_UUID = new Guid("3929F0D8-D461-43B0-BCF3-DF228CDD4A35");
        public static readonly Guid SERVICE__WWRITE_UUID = new Guid("08BCF119-62F8-4D2A-840F-EC50C96F612A");
        public const UInt16 SERVICE_ATTRIBUTE_ID = 0x100;
        public const byte SERVICE_ATTRIBUTE_TYPE = (4 << 3) | 5;
        public const string SERVICE_NAME = "Miriot";
    }
}
