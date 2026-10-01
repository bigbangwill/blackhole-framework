using System.IO;
using Blackhole.AudioPlayer.Infrastructure.ScriptableObjects;
using Blackhole.Framework.Composition;
using Blackhole.Framework.Composition.Editor;
using NUnit.Framework;
using UnityEditor;

namespace Blackhole.Development.Tests
{
    public sealed class FrameworkConfigurationTests
    {
        [Test]
        public void FrameworkConfigurationPassesBuildValidation() => Assert.That(FrameworkStartupValidator.Validate(), Is.Empty);

        [Test]
        public void RefreshPreservesStartingModuleAndAudioConfiguration()
        {
            BlackholeProjectSettingsSO settings = AssetDatabase.LoadAssetAtPath<BlackholeProjectSettingsSO>(FrameworkProjectSetup.SettingsPath);
            var startingModule = settings.StartingModule;
            const string soundLibraryPath = "Assets/BlackholeFramework/AudioPlayer/Settings/SoundLibrary.asset";
            string soundLibrary = File.ReadAllText(soundLibraryPath);
            FrameworkProjectSetup.RegenerateSettings();
            Assert.That(settings.StartingModule, Is.EqualTo(startingModule));
            Assert.That(File.ReadAllText(soundLibraryPath), Is.EqualTo(soundLibrary));
            Assert.That(AssetDatabase.LoadAssetAtPath<SoundLibrarySO>(soundLibraryPath), Is.Not.Null);
        }
    }
}
