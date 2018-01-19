using System;

namespace Miriot.Common
{
    public class Constants
    {
        public static readonly Guid SERVICE_UUID = new Guid("4D93B7BA-A8CD-4C95-A11E-4B9B3EFB3417");
        public static readonly Guid SERVICE_READ_UUID = new Guid("4R93B7BA-A8CD-4C95-A11E-4B9B3EFB3417");
        public static readonly Guid SERVICE__WWRITE_UUID = new Guid("4W93B7BA-A8CD-4C95-A11E-4B9B3EFB3417");
        public const UInt16 SERVICE_ATTRIBUTE_ID = 0x100;
        public const byte SERVICE_ATTRIBUTE_TYPE = (4 << 3) | 5;
        public const string SERVICE_NAME = "Miriot";
    }
}
