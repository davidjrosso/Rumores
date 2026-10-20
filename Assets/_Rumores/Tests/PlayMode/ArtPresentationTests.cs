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
        public IEnumerator NextAndRematchWaitOnlyForPageTransition()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            var app=UnityEngine.Object.FindObjectOfType<PartyApp>();app.FastAnimations=false;
            yield return Click("Play");yield return Click("StartRound");
            for(int turn=0;turn<4;turn++)
            {
                yield return Click("Ready");yield return Click("Action_"+app.Game.Choices()[0].id);yield return Click("Confirm");
                Assert.AreEqual(turn,app.Game.Turn);
                Assert.IsTrue(UnityEngine.Object.FindObjectOfType<RumorSignal>().IsPlaying);
                GameObject.Find("Next").GetComponent<Button>().onClick.Invoke();
                Assert.AreEqual(turn,app.Game.Turn,"La transición debe bloquear el avance accidental.");
                yield return Click("Next"); // The short UI transition gates input, not the longer character animation.
                yield return null;
                Assert.AreEqual(turn+1,app.Game.Turn);
            }
            yield return Click("Rematch");yield return null;Assert.AreEqual(0,app.Game.Turn);
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
            var art=Resources.Load<RumoresArt>("RumoresArt");
            string[] ids={"miedoso","exagerado","mentiroso","ignorante"};
            for(int i=0;i<ids.Length;i++)
            {
                var old=preview.Character;yield return Click("PreviewCharacter_"+i);yield return null;
                Assert.IsTrue(old==null);Assert.AreSame(art.Find(ids[i]),preview.Character.GetComponentInChildren<Image>().sprite);
            }
            for(int i=0;i<9;i++){yield return Click("Preview_"+i);yield return null;}
            Assert.IsTrue(preview.Signal.IsPlaying);yield return Click("PreviewFast");
            LogAssert.NoUnexpectedReceived();
        }
#endif
        [UnityTest]
        public IEnumerator CelebrationAndDefeatEffectsNeverCaptureInputAndClearOnDisable()
        {
            var go=new GameObject("BurstTest",typeof(RectTransform));var burst=go.AddComponent<ReactionBurstGraphic>();
            try
            {
                foreach(bool celebrate in new[]{true,false})
                {
                    burst.Play(celebrate,false);Assert.IsFalse(burst.raycastTarget);
                    yield return new WaitForSecondsRealtime(.8f);Assert.IsFalse(burst.IsPlaying);
                    burst.Play(celebrate,false);go.SetActive(false);Assert.IsFalse(burst.IsPlaying);
                    go.SetActive(true);Assert.IsFalse(burst.IsPlaying);
                }
                burst.Play(true,true);yield return new WaitForSecondsRealtime(.2f);Assert.IsFalse(burst.IsPlaying);
            }
            finally {UnityEngine.Object.DestroyImmediate(go);}
        }
        [UnityTest]
        public IEnumerator CardsBindTheSixIconsWithoutChangingChoices()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            var app=UnityEngine.Object.FindObjectOfType<PartyApp>();yield return Click("Play");yield return Click("StartRound");yield return Click("Ready");
            var seen=new System.Collections.Generic.HashSet<string>();
            for(int page=0;page<2;page++)
            {
                foreach(var card in UnityEngine.Object.FindObjectsOfType<PartyActionCard>())
                {
                    var icon=card.GetComponentInChildren<ActionIconGraphic>();Assert.NotNull(icon);
                    Assert.AreEqual(card.name.Substring("Action_".Length),icon.ActionId);
                    Assert.IsFalse(icon.raycastTarget);seen.Add(icon.ActionId);
                }
                if(page==0){yield return Click("MoreChoices");yield return null;}
            }
            Assert.AreEqual(6,seen.Count);Assert.AreEqual(0,app.Game.Turn);
            LogAssert.NoUnexpectedReceived();
        }
        static IEnumerator Click(string name)
        {
            var app=UnityEngine.Object.FindObjectOfType<PartyApp>();float deadline=Time.realtimeSinceStartup+2;
            while(app!=null&&app.IsTransitioning){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            var go=GameObject.Find(name);Assert.NotNull(go,name);go.GetComponent<Button>().onClick.Invoke();
        }
    }
}
