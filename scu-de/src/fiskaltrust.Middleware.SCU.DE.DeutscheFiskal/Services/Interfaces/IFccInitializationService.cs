﻿namespace fiskaltrust.Middleware.SCU.DE.DeutscheFiskal.Services.Interfaces
{
    public interface IFccInitializationService
    {
        void Initialize(string fccDirectory);
        bool IsInitialized(string fccDirectory);
    }
}