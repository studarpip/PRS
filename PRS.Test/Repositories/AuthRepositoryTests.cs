using Moq;
using Neo4j.Driver;
using PRS.Model.Enums;
using PRS.Server.Repositories;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

public class AuthRepositoryTests
{
    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenExists()
    {
        var userId = Guid.NewGuid();

        var record = new FakeRecord(new Dictionary<string, object>
        {
            ["id"] = userId.ToString(),
            ["username"] = "testuser",
            ["password"] = "hashedpass",
            ["role"] = "User"
        });

        var cursor = new StubCursor(new List<IRecord> { record });

        var session = new Mock<IAsyncSession>();
        session.Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
               .ReturnsAsync(cursor);

        var driver = new Mock<IDriver>();
        driver.Setup(d => d.AsyncSession())
              .Returns(session.Object);

        var repo = new AuthRepository(driver.Object);

        var result = await repo.GetByUsernameAsync("testuser");

        Assert.NotNull(result);
        Assert.Equal("testuser", result!.Username);
        Assert.Equal("hashedpass", result.Password);
        Assert.Equal(Role.User, result.Role);
        Assert.Equal(userId, result.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenNotFound()
    {
        var cursor = new StubCursor(new List<IRecord>());

        var session = new Mock<IAsyncSession>();
        session.Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<object>()))
               .ReturnsAsync(cursor);

        var driver = new Mock<IDriver>();
        driver.Setup(d => d.AsyncSession())
              .Returns(session.Object);

        var repo = new AuthRepository(driver.Object);

        var result = await repo.GetByUsernameAsync("nonexistent");

        Assert.Null(result);
    }

    private class StubCursor : IResultCursor, IAsyncEnumerable<IRecord>
    {
        private readonly List<IRecord> _records;

        public StubCursor(List<IRecord> records)
        {
            _records = records;
        }

        public Task<IReadOnlyList<IRecord>> ToListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<IRecord>>(_records);

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
        public Task<IRecord> PeekAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> FetchAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<string[]> KeysAsync() => throw new NotImplementedException();
        public Task<IRecord> PeekAsync() => throw new NotImplementedException();
        public Task<bool> FetchAsync() => throw new NotImplementedException();
    }

    private class FakeRecord : IRecord
    {
        private readonly Dictionary<string, object> _values;

        public FakeRecord(Dictionary<string, object> values)
        {
            _values = values;
        }

        public object this[string key] => _values[key];
        public KeyValuePair<string, object> this[int index] => _values.ElementAt(index);
        object IRecord.this[int index] => throw new NotImplementedException();
        public IReadOnlyList<string> Keys => _values.Keys.ToList();
        public IReadOnlyList<object> Values => _values.Values.ToList();
        public int Count => _values.Count;
        IReadOnlyDictionary<string, object> IRecord.Values => throw new NotImplementedException();
        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => throw new NotImplementedException();
        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => throw new NotImplementedException();
        public bool ContainsKey(string key) => _values.ContainsKey(key);
        public T Get<T>(string key) => throw new NotImplementedException();
        public T GetCaseInsensitive<T>(string key) => throw new NotImplementedException();
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new NotImplementedException();
        public bool TryGet<T>(string key, out T value) => throw new NotImplementedException();
        public bool TryGetCaseInsensitive<T>(string key, out T value) => throw new NotImplementedException();
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => throw new NotImplementedException();
        public object ValuesFor(string key) => _values[key];
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
