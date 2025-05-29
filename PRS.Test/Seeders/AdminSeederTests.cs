using Moq;
using Neo4j.Driver;
using PRS.Server.Helpers.Interfaces;
using PRS.Server.Migrations.Seeders;

public class AdminSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldNotCreateAdmin_IfAlreadyExists()
    {
        var encryptionMock = new Mock<IEncryptionHelper>();

        var recordMock = new Mock<IRecord>();
        recordMock.Setup(r => r["exists"]).Returns(true);

        var txMock = new Mock<IAsyncTransaction>();
        txMock.Setup(t => t.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
              .ReturnsAsync(new SingleRecordCursor(recordMock.Object));

        var seeder = new AdminSeeder(encryptionMock.Object);

        await seeder.SeedAsync(txMock.Object);

        txMock.Verify(t =>
            t.RunAsync(It.Is<string>(s => s.TrimStart().StartsWith("CREATE")), It.IsAny<object>()),
            Times.Never);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateAdmin_IfNotExists()
    {
        var encryptionMock = new Mock<IEncryptionHelper>();
        encryptionMock.Setup(e => e.Encrypt(It.IsAny<string>())).Returns("encrypted-email");

        var recordMock = new Mock<IRecord>();
        recordMock.Setup(r => r["exists"]).Returns(false);

        var txMock = new Mock<IAsyncTransaction>();
        txMock.SetupSequence(t => t.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
              .ReturnsAsync(new SingleRecordCursor(recordMock.Object))
              .ReturnsAsync(Mock.Of<IResultCursor>());

        var seeder = new AdminSeeder(encryptionMock.Object);

        await seeder.SeedAsync(txMock.Object);

        txMock.Verify(t =>
            t.RunAsync(It.Is<string>(s => s.TrimStart().StartsWith("CREATE")), It.IsAny<object>()),
            Times.Once);
    }

    private class SingleRecordCursor : IResultCursor
    {
        private readonly IRecord _record;

        public SingleRecordCursor(IRecord record) => _record = record;

        public Task<IRecord> SingleAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_record);

        public IAsyncEnumerator<IRecord> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new SingleRecordEnumerator(_record);

        private class SingleRecordEnumerator : IAsyncEnumerator<IRecord>
        {
            private readonly IRecord _record;
            private bool _moved;

            public SingleRecordEnumerator(IRecord record)
            {
                _record = record;
                _moved = false;
            }

            public IRecord Current => _record;

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;

            public ValueTask<bool> MoveNextAsync()
            {
                if (_moved) return new ValueTask<bool>(false);
                _moved = true;
                return new ValueTask<bool>(true);
            }
        }

        public bool IsOpen => false;
        public IRecord Current => throw new NotImplementedException();
        public Task<IResultSummary> ConsumeAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IReadOnlyList<string>> KeysAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IReadOnlyList<IRecord>> ToListAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IRecord> PeekAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> FetchAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string[]> KeysAsync() => throw new NotImplementedException();
        public Task<IResultSummary> ConsumeAsync() => throw new NotImplementedException();
        public Task<IRecord> PeekAsync() => throw new NotImplementedException();
        public Task<bool> FetchAsync() => throw new NotImplementedException();
    }
}