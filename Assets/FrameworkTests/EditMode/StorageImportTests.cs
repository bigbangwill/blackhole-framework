using System;
using System.IO;
using Blackhole.EncryptionSaveData.Application;
using Blackhole.EncryptionSaveData.Infrastructure;
using Blackhole.LocalStorageFile.Application;
using Blackhole.LocalStorageFile.Contract;
using Blackhole.LocalStorageFile.Infrastructure;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Blackhole.Development.Tests
{
    public sealed class StorageImportTests
    {
        private string root;
        private LocalSaveSettingSO settings;
        private EncryptionSettingSO encryptionSettings;
        private SaveDataService storage;

        [SetUp]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), "BlackholeStorageTests", Guid.NewGuid().ToString("N"));
            settings = ScriptableObject.CreateInstance<LocalSaveSettingSO>();
            var serialized = new SerializedObject(settings);
            serialized.FindProperty("extendedPath").stringValue = root;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            encryptionSettings = ScriptableObject.CreateInstance<EncryptionSettingSO>();
            storage = new SaveDataService(new StorageData(settings));
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(settings);
            UnityEngine.Object.DestroyImmediate(encryptionSettings);
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        [Test]
        public void NestedSaveKeysRoundTripAndOverwriteWithoutChangingTheSaveFormat()
        {
            Assert.That(storage.SaveData("profiles/player", new byte[] { 1, 2 }).IsSuccess, Is.True);
            Assert.That(File.Exists(Path.Combine(root, "profiles", "player.sav")), Is.True);
            Assert.That(storage.SaveData("profiles/player", new byte[] { 3 }).IsSuccess, Is.True);
            Assert.That(storage.LoadData("profiles/player").Value, Is.EqualTo(new byte[] { 3 }));
            Assert.That(storage.DeleteData("profiles/player").IsSuccess, Is.True);
            Assert.That(storage.LoadData("profiles/player").Error, Is.EqualTo(StorageSaveResults.NotFound));
        }

        [TestCase("../outside")]
        [TestCase("..\\outside")]
        [TestCase("")]
        [TestCase(null)]
        public void InvalidKeysReturnErrorsWithoutWritingOutsideTheSaveDirectory(string key)
        {
            Assert.That(storage.SaveData(key, new byte[] { 1 }).Error, Is.EqualTo(StorageSaveResults.InvalidKey));
            Assert.That(storage.DataExists(key), Is.False);
        }

        [Test]
        public void InvalidDataAndUnavailableStorageReturnActionableErrors()
        {
            Assert.That(storage.SaveData("profile", null).Error, Is.EqualTo(StorageSaveResults.InvalidData));
            var denied = new SaveDataService(new FailingStorage(new UnauthorizedAccessException()));
            Assert.That(denied.SaveData("profile", new byte[] { 1 }).Error, Is.EqualTo(StorageSaveResults.AccessDenied));
            var tooLong = new SaveDataService(new FailingStorage(new PathTooLongException()));
            Assert.That(tooLong.SaveData("profile", new byte[] { 1 }).Error, Is.EqualTo(StorageSaveResults.PathTooLong));
            Assert.That(denied.LoadData("profile").Error, Is.EqualTo(StorageSaveResults.StorageError));
            Assert.That(denied.DeleteData("profile").Error, Is.EqualTo(StorageSaveResults.StorageError));
        }

        [Test]
        public void EncryptedSavesRoundTripAndRetainUnderlyingStorageFailures()
        {
            var encryptor = new EncryptionData(encryptionSettings);
            var encrypted = new SaveEncryptData(encryptor, storage);
            byte[] payload = { 3, 7, 11 };
            Assert.That(encrypted.SaveData("profiles/encrypted", payload).IsSuccess, Is.True);
            Assert.That(encrypted.LoadData("profiles/encrypted").Value, Is.EqualTo(payload));
            Assert.That(storage.LoadData("profiles/encrypted").Value, Is.Not.EqualTo(payload));
            Assert.That(encrypted.DeleteData("profiles/encrypted").IsSuccess, Is.True);
            Assert.That(encrypted.DeleteData("profiles/encrypted").Error, Is.EqualTo(StorageSaveResults.NotFound));
            Assert.That(encrypted.LoadData("profiles/encrypted").Error, Is.EqualTo(StorageSaveResults.NotFound));
            var denied = new SaveEncryptData(encryptor, new SaveDataService(new FailingStorage(new UnauthorizedAccessException())));
            Assert.That(denied.SaveData("profile", payload).Error, Is.EqualTo(StorageSaveResults.AccessDenied));
        }

        [Test]
        public void CorruptEncryptedDataIsReportedWithoutThrowing()
        {
            var encryptor = new EncryptionData(encryptionSettings);
            Assert.That(encryptor.EncryptData(null).Error, Is.EqualTo(StorageSaveResults.EncryptionFail));
            Assert.That(encryptor.DecryptData(new byte[17]).Error, Is.EqualTo(StorageSaveResults.CorruptedData));
            byte[] encrypted = encryptor.EncryptData(new byte[] { 42 }).Value;
            encrypted[15] ^= 1;
            Assert.That(encryptor.DecryptData(encrypted).Error, Is.EqualTo(StorageSaveResults.DecryptionFail));
        }

        private sealed class FailingStorage : IDataStorage
        {
            private readonly Exception error;
            public FailingStorage(Exception error) => this.error = error;
            public void WriteData(string key, byte[] data) => throw error;
            public byte[] ReadData(string key) => throw error;
            public bool DataExits(string key) => throw error;
            public void DeleteData(string key) => throw error;
        }
    }
}
