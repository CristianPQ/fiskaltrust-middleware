﻿using System;

namespace fiskaltrust.Middleware.Localization.QueueME.Exceptions
{
    public class CashDepositOutstandingException : Exception
    {
        public CashDepositOutstandingException(string message)
            : base(message)
        {
        }
    }
}
