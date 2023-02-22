﻿using fiskaltrust.ifPOS.v1;
using fiskaltrust.Middleware.Contracts.Exceptions;

namespace fiskaltrust.Middleware.Localization.QueueIT.Extensions
{
    public static class ChargeItemExtensions
    {
        public static bool IsPaymentAdjustment(this ChargeItem chargeItem)
        {
            return (chargeItem.ftChargeItemCase & 0xFFFF) switch
            {
                0x0023 or 0x0024 or 0x0025 or 0x0026 or 0x0027 => true,
                _ => false,
            };
        }

        public static int GetVatGroup(this ChargeItem chargeItem)
        {
            // TODO: check VAT rate table on printer at the moment according to xml example
            switch (chargeItem.ftChargeItemCase & 0xFFFF)
            {
                //0
                case 0x0005:
                case 0x000C:
                case 0x0011:
                case 0x0016:
                case 0x001B:
                case 0x0020:
                case 0x0027:
                case 0x003C:
                    return 0;
                //22
                case 0x0001:
                case 0x0008:
                case 0x000D:
                case 0x0012:
                case 0x0017:
                case 0x001C:
                case 0x0023:
                case 0x0028:
                    return 1;
                //10
                case 0x0002:
                case 0x0009:
                case 0x000E:
                case 0x0013:
                case 0x0018:
                case 0x001D:
                case 0x0024:
                case 0x0029:
                    return 2;
                //5
                case 0x0003:
                case 0x000A:
                case 0x000F:
                case 0x0014:
                case 0x0019:
                case 0x001E:
                case 0x0025:
                case 0x003A:
                    return 3;
                //4
                case 0x0004:
                case 0x000B:
                case 0x0010:
                case 0x0015:
                case 0x001A:
                case 0x001F:
                case 0x0026:
                case 0x003B:
                    return 4;
                default:
                    throw new UnknownChargeItemException(chargeItem.ftChargeItemCase);
            }
        }
    }
}
