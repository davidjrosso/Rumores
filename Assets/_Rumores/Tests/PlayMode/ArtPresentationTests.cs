using System;
using System.Collections;
using NUnit.Framework;
using Rumores.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Rumores.Tests
{
    public class ArtPresentationTests
    {
        [UnityTest]
        public IEnumerator ReactionsUseUnscaledTimeAndRestoreTransformWhenDisabled()
        {
            var go=new GameObject("MotionTest",typeof(RectTransform));var rect=(RectTransform)go.transform;
            rect.anchoredPosition=new Vector2(12,23);rect.localScale=new Vector3(.8f,.8f,1);
            var motion=go.AddComponent<CharacterMotion>();float oldTimeScale=Time.timeScale;
            try
            {
                Time.timeScale=0;
                foreach(CharacterReaction reaction in Enum.GetValues(typeof(CharacterReaction)))
                {
                    motion.Play(reaction);yield return new WaitForSecondsRealtime(.07f);
                    bool moved=rect.localScale!=new Vector3(.8f,.8f,1)||rect.anchoredPosition!=new Vector2(12,23)||Quaternion.Angle(rect.localRotation,Quaternion.identity)>.01f;
                    Assert.IsTrue(moved,"Sin movimiento en "+reaction);
                    if(reaction!=CharacterReaction.Idle)
                    {yield return new WaitForSecondsRealtime(CharacterMotion.Duration(reaction)+.05f);Assert.IsFalse(motion.IsReacting);}
                    go.SetActive(false);
                    Assert.AreEqual(new Vector3(.8f,.8f,1),rect.localScale);Assert.AreEqual(new Vector2(12,23),rect.anchoredPosition);
                    Assert.AreEqual(Quaternion.identity,rect.localRotation);Assert.IsFalse(motion.IsReacting);
                    go.SetActive(true);
                }
                motion.Fast=true;motion.Play(CharacterReaction.Panic);yield return null;
                Assert.AreEqual(Quaternion.identity,rect.localRotation);
                yield return new WaitForSecondsRealtime(.2f);Assert.IsFalse(motion.IsReacting);
            }
            finally {Time.timeScale=oldTimeScale;UnityEngine.Object.DestroyImmediate(go);}
        }
        [UnityTest]
        public IEnumerator TransmissionAndSplitEndWithoutLeavingVisibleSymbols()
        {
            var go=new GameObject("SignalTest",typeof(RectTransform));var signal=go.AddComponent<RumorSignal>();
            try
            {
                signal.Play(true,false);yield return new WaitForSecondsRealtime(.1f);
                Assert.AreEqual(2,go.GetComponentsInChildren<SpeechBubbleGraphic>().Length);
                var a=(RectTransform)go.transform.Find("RumorA");var b=(RectTransform)go.transform.Find("RumorB");
                Assert.Greater(a.anchorMin.y,b.anchorMin.y);
                yield return new WaitForSecondsRealtime(.5f);Assert.IsFalse(signal.IsPlaying);
                Assert.AreEqual(0,go.GetComponentsInChildren<SpeechBubbleGraphic>().Length);
                signal.Play(false,true);Assert.AreEqual(1,go.GetComponentsInChildren<SpeechBubbleGraphic>().Length);
                go.SetActive(false);Assert.IsFalse(signal.IsPlaying);
                go.SetActive(true);Assert.AreEqual(0,go.GetComponentsInChildren<SpeechBubbleGraphic>().Length);
            }
            finally {UnityEngine.Object.DestroyImmediate(go);}
        }
        [UnityTest]
        public IEnumerator NextAndRematchDoNotWaitForAnimation()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            var app=UnityEngine.Object.FindObjectOfType<PartyApp>();app.FastAnimations=false;
            Click("Play");Click("StartRound");
            for(int turn=0;turn<4;turn++)
            {
                Click("Ready");Click("Action_"+app.Game.Choices()[0].id);Click("Confirm");
                Assert.AreEqual(turn,app.Game.Turn);
                Assert.IsTrue(UnityEngine.Object.FindObjectOfType<RumorSignal>().IsPlaying);
                Click("Next"); // same frame: the visual effect must never gate the turn
                yield return null;
                Assert.AreEqual(turn+1,app.Game.Turn);
            }
            Click("Rematch");yield return null;Assert.AreEqual(0,app.Game.Turn);
            Assert.AreEqual(0,app.Game.Branches[0].rumor.history.Count);
            LogAssert.NoUnexpectedReceived();
        }
#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator ReviewSceneHasConnectedSpritesAndAllReactionControls()
        {
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Rumores/Scenes/ArtPreview.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            var preview=UnityEngine.Object.FindObjectOfType<ArtPreview>();Assert.NotNull(preview.Character);
            Assert.NotNull(preview.Character.GetComponentInChildren<Image>().sprite);
            for(int i=0;i<9;i++){Click("Preview_"+i);yield return null;}
            Assert.IsTrue(preview.Signal.IsPlaying);Click("PreviewFast");
            LogAssert.NoUnexpectedReceived();
        }
#endif
        static void Click(string name)
        {var go=GameObject.Find(name);Assert.NotNull(go,name);go.GetComponent<Button>().onClick.Invoke();}
    }
}
