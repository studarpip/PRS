using Moq;
using Neo4j.Driver;
using PRS.Model.Enums;
using PRS.Server.Migrations.Seeders;

public class GenderSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldNotCreate_WhenAllGendersExist()
    {
        var existing = Enum.GetValues<Gender>().Select(g => g.ToString()).ToArray();
        var cursor = new ForEachCursor(existing);

        var txMock = new Mock<IAsyncTransaction>();

        txMock.Setup(t => t.RunAsync("MATCH (g:Gender) RETURN g.name AS name"))
              .ReturnsAsync(cursor);

        txMock.Setup(t => t.RunAsync(It.Is<string>(q => q.TrimStart().StartsWith("MERGE")), It.IsAny<object>()))
              .ReturnsAsync(Mock.Of<IResultCursor>());

        var seeder = new GenderSeeder();

        await seeder.SeedAsync(txMock.Object);

        txMock.Verify(t =>
            t.RunAsync(It.Is<string>(s => s.TrimStart().StartsWith("MERGE")), It.IsAny<object>()),
            Times.Never);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreate_WhenNoGendersExist()
    {
        var cursor = new ForEachCursor(Array.Empty<string>());

        var txMock = new Mock<IAsyncTransaction>();

        txMock.Setup(t => t.RunAsync("MATCH (g:Gender) RETURN g.name AS name"))
              .ReturnsAsync(cursor);

        txMock.Setup(t => t.RunAsync(It.Is<string>(q => q.TrimStart().StartsWith("MERGE")), It.IsAny<object>()))
              .ReturnsAsync(Mock.Of<IResultCursor>());

        var seeder = new GenderSeeder();

        await seeder.SeedAsync(txMock.Object);

        txMock.Verify(t =>
            t.RunAsync(It.Is<string>(s => s.TrimStart().StartsWith("MERGE")), It.IsAny<object>()),
            Times.AtLeastOnce);
    }

    private class ForEachCursor : IResultCursor, IAsyncEnumerable<IRecord>
    {
        private readonly List<IRecord> _records;

        public ForEachCursor(IEnumerable<string> names)
        {
            _records = names
                .Select(name =>
                {
                    var mock = new Mock<IRecord>();
                    mock.Setup(r => r["name"]).Returns(name);
                    return mock.Object;
                })
                .ToList();
        }

        public IAsyncEnumerator<IRecord> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator(_records);

        private class Enumerator : IAsyncEnumerator<IRecord>
        {
            private readonly IEnumerator<IRecord> _enumerator;

            public Enumerator(IEnumerable<IRecord> records)
            {
                _enumerator = records.GetEnumerator();
            }

            public IRecord Current => _enumerator.Current;

            public ValueTask DisposeAsync()
            {
                _enumerator.Dispose();
                return ValueTask.CompletedTask;
            }

            public ValueTask<bool> MoveNextAsync()
                => new(_enumerator.MoveNext());
        }

        public Task<IRecord> SingleAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public bool IsOpen => false;
        public IRecord Current => throw new NotImplementedException();
        public Task<IResultSummary> ConsumeAsync(CancellationToken cancellationToken) => Task.FromResult(Mock.Of<IResultSummary>());
        public Task<IResultSummary> ConsumeAsync() => Task.FromResult(Mock.Of<IResultSummary>());
        public Task<IReadOnlyList<string>> KeysAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IReadOnlyList<IRecord>> ToListAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IRecord> PeekAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> FetchAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string[]> KeysAsync() => throw new NotImplementedException();
        public Task<IRecord> PeekAsync() => throw new NotImplementedException();
        public Task<bool> FetchAsync() => throw new NotImplementedException();
    }
}
