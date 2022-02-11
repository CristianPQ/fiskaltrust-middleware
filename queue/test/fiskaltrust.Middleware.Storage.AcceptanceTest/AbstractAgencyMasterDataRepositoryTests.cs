﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using fiskaltrust.Middleware.Contracts.Repositories;
using FluentAssertions;
using Xunit;
using fiskaltrust.storage.V0.MasterData;

namespace fiskaltrust.Middleware.Storage.AcceptanceTest
{
    public abstract class AbstractAgencyMasterDataRepositoryTests : IDisposable
    {
        public abstract Task<IMasterDataRepository<AgencyMasterData>> CreateRepository(IEnumerable<AgencyMasterData> entries);
        public abstract Task<IMasterDataRepository<AgencyMasterData>> CreateReadOnlyRepository(IEnumerable<AgencyMasterData> entries);

        public virtual void DisposeDatabase() { return; }

        public void Dispose() => DisposeDatabase();

        [Fact]
        public async Task GetAsync_ShouldReturnAllEntitiesThatExistInRepository()
        {
            var expectedEntries = StorageTestFixtureProvider.GetFixture().CreateMany<AgencyMasterData>(10);

            var sut = await CreateReadOnlyRepository(expectedEntries);
            var actualEntries = await sut.GetAsync();

            actualEntries.Should().BeEquivalentTo(expectedEntries);
        }

        [Fact]
        public async Task GetAsync_ShouldNotReturnNull()
        {
            var sut = await CreateReadOnlyRepository(new List<AgencyMasterData>());
            var actualEntries = await sut.GetAsync();

            actualEntries.Should().NotBeNull();
            actualEntries.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntry_ToTheDatabase()
        {
            var entries = StorageTestFixtureProvider.GetFixture().CreateMany<AgencyMasterData>(10).ToList();
            var entryToInsert = StorageTestFixtureProvider.GetFixture().Create<AgencyMasterData>();

            var sut = await CreateRepository(entries);
            await sut.CreateAsync(entryToInsert);

            var actualEntries = await sut.GetAsync();
            actualEntries.Should().HaveCount(11);
            actualEntries.Should().ContainEquivalentOf(entryToInsert);
        }

        [Fact]
        public async Task ClearAsync_ShouldRemoveAlllEntries_FromTheDatabase()
        {
            var entries = StorageTestFixtureProvider.GetFixture().CreateMany<AgencyMasterData>(1).ToList();

            var sut = await CreateRepository(entries);
            await sut.ClearAsync();
            var actualEntries = await sut.GetAsync();

            actualEntries.Should().NotBeNull();
            actualEntries.Should().BeEmpty();
        }
    }
}
