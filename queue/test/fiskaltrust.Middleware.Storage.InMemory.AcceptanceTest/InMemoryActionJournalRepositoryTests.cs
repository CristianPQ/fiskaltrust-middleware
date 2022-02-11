﻿using fiskaltrust.Middleware.Storage.AcceptanceTest;
using fiskaltrust.Middleware.Storage.InMemory.Repositories;
using fiskaltrust.storage.V0;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fiskaltrust.Middleware.Storage.InMemory.AcceptanceTest
{
    public class InMemoryActionJournalRepositoryTests : AbstractActionJournalRepositoryTests
    {
        public override Task<IReadOnlyActionJournalRepository> CreateReadOnlyRepository(IEnumerable<ftActionJournal> entries) => Task.FromResult< IReadOnlyActionJournalRepository>(new InMemoryActionJournalRepository(entries));

        public override Task<IActionJournalRepository> CreateRepository(IEnumerable<ftActionJournal> entries) => Task.FromResult<IActionJournalRepository>(new InMemoryActionJournalRepository(entries));

    }
}