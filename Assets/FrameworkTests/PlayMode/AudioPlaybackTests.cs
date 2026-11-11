using System.Collections;
using System.Linq;
using Blackhole.AudioPlayer.Infrastructure.Adapters;
using Blackhole.AudioPlayer.Infrastructure.ScriptableObjects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Blackhole.Development.Tests
{
    public sealed class AudioPlaybackTests
    {
        [UnityTest]
        public IEnumerator PauseResumeRetainsPositionAndStopClearsPausedPlayback()
        {
            var host = new GameObject("Audio Pool Test");
            var config = ScriptableObject.CreateInstance<AudioConfigSO>();
            var clip = AudioClip.Create("Playback Test", 44100 * 5, 1, 44100, false);
            try
            {
                var pool = host.AddComponent<UnityAudioPool>();
                pool.Initialize(config);
                pool.Initialize(config);
                Assert.That(host.GetComponentsInChildren<AudioSource>().Length, Is.EqualTo(config.SfxChannelCount + 1));
                pool.Play(clip, 0f, 1f, true, true, null);
                pool.Play(clip, 0f, 1f, false, false, null);
                yield return null;

                var music = host.GetComponentsInChildren<AudioSource>().Single(source => source.loop);
                var sfx = host.GetComponentsInChildren<AudioSource>().Single(source => source.clip == clip && !source.loop);
                music.timeSamples = 44100;
                sfx.timeSamples = 44100;
                pool.Play(clip, 0f, 1f, true, true, null);
                Assert.That(music.timeSamples, Is.GreaterThanOrEqualTo(44100));

                pool.PauseAll();
                Assert.That(music.isPlaying || sfx.isPlaying, Is.False);
                Assert.That(sfx.timeSamples, Is.GreaterThanOrEqualTo(44100));
                pool.ResumeAll();
                Assert.That(music.isPlaying && sfx.isPlaying, Is.True);
                Assert.That(sfx.timeSamples, Is.GreaterThanOrEqualTo(44100));

                pool.PauseAll();
                pool.StopAll();
                pool.ResumeAll();
                Assert.That(music.isPlaying || sfx.isPlaying, Is.False);
            }
            finally
            {
                Object.Destroy(host);
                Object.Destroy(config);
                Object.Destroy(clip);
            }
        }
    }
}
