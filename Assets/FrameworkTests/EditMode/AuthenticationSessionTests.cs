using Blackhole.Authentication.Application;
using Blackhole.LocalStorageFile.Contract;
using Blackhole.SharedKernel.Contract;
using NUnit.Framework;

namespace Blackhole.Development.Tests
{
    public sealed class AuthenticationSessionTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void HeaderRequestRestoresLocalSessionOrFallsBackToRepository(bool hasLocalSession)
        {
            var repository = new TokenRepository();
            var storage = new Storage { HasSession = hasLocalSession };
            var service = new AuthenticationService(null, repository, storage, new Serializer());
            var header = service.GetAccessTokenHeader();
            Assert.That(header.IsSuccess, Is.True);
            Assert.That(header.Value.Value, Is.EqualTo(hasLocalSession ? "Bearer local-access" : "Bearer repository-access"));
            Assert.That(repository.Reads, Is.EqualTo(hasLocalSession ? 0 : 1));
            Assert.That(storage.Saves, Is.EqualTo(1));
            Assert.That(repository.Saved.AccessToken, Is.EqualTo(hasLocalSession ? "local-access" : "repository-access"));
            service.GetAccessTokenHeader();
            Assert.That(storage.Saves, Is.EqualTo(1), "An initialized session should not be restored twice.");
        }

        [Test]
        public void MissingSessionReturnsNotFoundWithoutWritingStorage()
        {
            var repository = new TokenRepository { HasSession = false };
            var storage = new Storage();
            var service = new AuthenticationService(null, repository, storage, new Serializer());
            Assert.That(service.GetAccessTokenHeader().Error, Is.EqualTo(SessionTokenError.NotFound));
            Assert.That(storage.Saves, Is.Zero);
        }

        private sealed class TokenRepository : ISessionTokenDtoRepository
        {
            public bool HasSession = true;
            public int Reads;
            public SessionTokenDto Saved;
            public Result<SessionTokenDto, SessionTokenError> GetSessionTokenDto()
            {
                Reads++;
                return HasSession ? Result<SessionTokenDto, SessionTokenError>.Success(new SessionTokenDto("repository-access", "refresh", "Bearer", 3600)) : Result<SessionTokenDto, SessionTokenError>.Failure(SessionTokenError.NotFound);
            }
            public void SetSessionTokenDto(SessionTokenDto token) => Saved = token;
            public void ClearSessionTokenDto() => HasSession = false;
        }

        private sealed class Storage : ISaveData
        {
            public bool HasSession;
            public int Saves;
            public Result<StorageSaveResults> SaveData(string key, byte[] data) { Saves++; return Result<StorageSaveResults>.Success(); }
            public Result<byte[], StorageSaveResults> LoadData(string key) => HasSession ? Result<byte[], StorageSaveResults>.Success(new byte[] { 1 }) : Result<byte[], StorageSaveResults>.Failure(StorageSaveResults.NotFound);
            public bool DataExists(string key) => HasSession;
            public Result<StorageSaveResults> DeleteData(string key) => Result<StorageSaveResults>.Success();
        }

        private sealed class Serializer : IDataSerializer
        {
            public Result<byte[], SerializationResults> SerializeData<T>(T data) => Result<byte[], SerializationResults>.Success(new byte[] { 1 });
            public Result<T, SerializationResults> DeserializeData<T>(byte[] data) => Result<T, SerializationResults>.Success((T)(object)new SessionTokenDto("local-access", "refresh", "Bearer", 3600));
        }
    }
}
