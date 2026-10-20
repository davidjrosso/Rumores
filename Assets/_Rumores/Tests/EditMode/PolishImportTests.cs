using NUnit.Framework;
using Rumores.Presentation;
using UnityEditor;
using UnityEngine;

namespace Rumores.Tests
{
    public class PolishImportTests
    {
        [TestCase("Music")][TestCase("Select")][TestCase("Transmit")][TestCase("Surprise")]
        [TestCase("Panic")][TestCase("Reveal")][TestCase("Celebrate")][TestCase("Defeat")]
        public void AudioLoadsAndUsesTwoDimensionalPcmSource(string name)
        {
            var clip=Resources.Load<AudioClip>("Audio/"+name);Assert.NotNull(clip);Assert.Greater(clip.samples,0);
            Assert.AreEqual(1,clip.channels);Assert.AreEqual(44100,clip.frequency);
            var importer=(AudioImporter)AssetImporter.GetAtPath("Assets/_Rumores/Resources/Audio/"+name+".wav");
            Assert.AreEqual(AudioCompressionFormat.PCM,importer.defaultSampleSettings.compressionFormat);
        }
        [Test]
        public void LocalizedUiFormatsNamesWithoutTreatingThemAsFormatStrings()
        {
            Assert.AreEqual("Te toca, {0}<b>Ana</b>",UiStrings.T("turn.title","{0}<b>Ana</b>"));
            Assert.AreEqual("Turno 1 de 4 · Vuelta 1 de 2",UiStrings.T("progress.turn",1,4,1));
        }
    }
}
