using System.Collections;
using System.Linq;
using NUnit.Framework;
using Rumores.Core.Party;
using Rumores.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Rumores.Tests
{
    public class PolishTests
    {
        static IEnumerator Ready()
        {
            var app=Object.FindObjectOfType<PartyApp>();float deadline=Time.realtimeSinceStartup+3;
            while(app!=null&&app.IsTransitioning){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
        }
        static void Press(string name){GameObject.Find(name).GetComponent<Button>().onClick.Invoke();}
        [UnityTest]
        public IEnumerator RevealedTurnSurvivesPauseMenuAndSettingsWithoutRepeatingIt()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return Ready();
            var app=Object.FindObjectOfType<PartyApp>();
            Press("Play");yield return Ready();Press("StartRound");yield return Ready();Press("Ready");yield return Ready();
            Press("Action_"+app.Game.Choices()[0].id);Press("Confirm");yield return Ready();
            string text=app.Game.LastChange.text,token=app.Game.Token;
            app.SendMessage("OnApplicationPause",true);yield return Ready();
            Assert.IsFalse(Object.FindObjectsOfType<Text>().Any(t=>t.text==text));
            Press("Home");yield return Ready();Press("Settings");yield return Ready();
            Press("Back");yield return Ready();Press("Continue");yield return Ready();Press("Ready");yield return Ready();
            Assert.AreEqual(PartyPhase.Revealing,app.Game.Phase);Assert.AreEqual(token,app.Game.Token);
            Assert.AreEqual(text,app.Game.LastChange.text);Assert.AreEqual(1,app.Game.Branches[0].rumor.history.Count);
            Press("Next");yield return Ready();Assert.AreEqual(1,app.Game.Turn);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]
        public IEnumerator NewGameFromMenuDiscardsTheOldRoundAndOldCallbacks()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return Ready();
            var app=Object.FindObjectOfType<PartyApp>();
            Press("Play");yield return Ready();Press("StartRound");yield return Ready();Press("Ready");yield return Ready();
            Press("Action_"+app.Game.Choices()[0].id);Press("Confirm");yield return Ready();
            Press("Next");yield return Ready();Press("Home");yield return Ready();
            var old=app.Game;Assert.AreEqual(1,old.Turn);Assert.AreEqual(1,old.Branches[0].rumor.history.Count);
            Press("Play");yield return Ready();
            var start=GameObject.Find("StartRound").GetComponent<Button>();
            start.onClick.Invoke();var current=app.Game;start.onClick.Invoke();
            Assert.AreSame(current,app.Game);Assert.AreNotSame(old,app.Game);
            Assert.AreEqual(0,app.Game.Turn);Assert.AreEqual(PartyPhase.Handoff,app.Game.Phase);
            Assert.AreEqual(0,app.Game.Branches[0].rumor.history.Count);Assert.IsNull(app.Game.LastChange);
            yield return Ready();LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]
        public IEnumerator TransitionAndPrivacyRejectEarlyClicksWithoutChangingRules()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return Ready();
            var app=Object.FindObjectOfType<PartyApp>();app.FastAnimations=false;
            Press("Play");Assert.IsTrue(app.IsTransitioning);Press("StartRound");Assert.IsNull(app.Game);
            yield return Ready();Press("StartRound");yield return Ready();
            Assert.IsFalse(Object.FindObjectsOfType<Text>().Any(t=>t.text==app.Game.News.original));
            Press("Ready");yield return Ready();string token=app.Game.Token;
            Press("Action_"+app.Game.Choices()[0].id);
            app.SendMessage("OnApplicationPause",true);yield return Ready();
            Assert.AreEqual(PartyPhase.Choosing,app.Game.Phase);Assert.AreEqual(token,app.Game.Token);
            Assert.IsFalse(Object.FindObjectsOfType<Text>().Any(t=>t.text==app.Game.News.original));
            Press("Ready");yield return Ready();Assert.AreEqual(PartyPhase.Choosing,app.Game.Phase);
            Assert.IsTrue(GameObject.Find("Confirm").GetComponent<Button>().interactable);
            var confirm=GameObject.Find("Confirm").GetComponent<Button>();confirm.onClick.Invoke();confirm.onClick.Invoke();
            Assert.AreEqual(1,app.Game.Branches[0].rumor.history.Count);
            Assert.AreEqual(PartyPhase.Revealing,app.Game.Phase);
            Press("Next");Assert.AreEqual(0,app.Game.Turn);
            yield return Ready();var next=GameObject.Find("Next").GetComponent<Button>();next.onClick.Invoke();next.onClick.Invoke();
            Assert.AreEqual(1,app.Game.Turn);Assert.AreEqual(PartyPhase.Handoff,app.Game.Phase);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]
        public IEnumerator IndependentVolumesPersistAndAllClipsLoad()
        {
            bool hadMusic=PlayerPrefs.HasKey(PartyAudio.MusicKey),hadEffects=PlayerPrefs.HasKey(PartyAudio.EffectsKey);
            float oldMusic=PlayerPrefs.GetFloat(PartyAudio.MusicKey),oldEffects=PlayerPrefs.GetFloat(PartyAudio.EffectsKey);
            try
            {
                yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return Ready();
                var audio=Object.FindObjectOfType<PartyAudio>();Assert.IsTrue(audio.Ready);
                Press("Settings");yield return Ready();
                GameObject.Find("MusicVolume").GetComponent<Slider>().value=0;
                GameObject.Find("EffectsVolume").GetComponent<Slider>().value=.75f;
                Assert.AreEqual(0,audio.MusicVolume);Assert.AreEqual(.75f,audio.EffectsVolume);
                Press("Back");yield return Ready();
                yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return Ready();
                audio=Object.FindObjectOfType<PartyAudio>();Assert.AreEqual(0,audio.MusicVolume);Assert.AreEqual(.75f,audio.EffectsVolume);
                audio.SetEffects(0);audio.Play(PartySound.Celebrate);
                Assert.AreEqual(0,audio.GetComponents<AudioSource>().Single(s=>!s.loop).volume);
            }
            finally
            {
                if(hadMusic)PlayerPrefs.SetFloat(PartyAudio.MusicKey,oldMusic);else PlayerPrefs.DeleteKey(PartyAudio.MusicKey);
                if(hadEffects)PlayerPrefs.SetFloat(PartyAudio.EffectsKey,oldEffects);else PlayerPrefs.DeleteKey(PartyAudio.EffectsKey);
                PlayerPrefs.Save();
            }
        }
        [UnityTest]
        public IEnumerator LongCardsUseNaturalHeightAndFooterStaysOutsideScroll()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return Ready();
            Press("Play");yield return Ready();Press("StartRound");yield return Ready();Press("Ready");yield return Ready();
            Canvas.ForceUpdateCanvases();
            var scroll=GameObject.Find("ChoicesScroll").GetComponent<ScrollRect>();
            Assert.Greater(scroll.content.rect.height,scroll.viewport.rect.height);
            foreach(var card in Object.FindObjectsOfType<PartyActionCard>())
            {
                Assert.AreSame(scroll.content,card.transform.parent);
                foreach(var text in card.GetComponentsInChildren<Text>())
                    Assert.GreaterOrEqual(text.rectTransform.rect.height+1,text.preferredHeight,"Texto recortado: "+text.text);
            }
            Assert.IsNull(GameObject.Find("Confirm").GetComponentInParent<ScrollRect>());
            var app=Object.FindObjectOfType<PartyApp>();Press("Action_"+app.Game.Choices()[0].id);
            Assert.IsTrue(Object.FindObjectsOfType<Text>().Any(t=>t.text==UiStrings.T("card.selected")));
            Assert.IsTrue(GameObject.Find("Confirm").GetComponentInChildren<Text>().text.Contains(app.Game.Choices()[0].title));
        }
        [TestCase(360,640)][TestCase(360,800)][TestCase(720,1080)][TestCase(720,1280)][TestCase(800,1280)]
        public void CardLayoutFitsPortraitSizesWithSafeAreaInsets(int width,int height)
        {
            // Layout harness using real Unity text/layout components, not a device screenshot.
            var go=new GameObject("LayoutHarness",typeof(RectTransform),typeof(Canvas));
            try
            {
                go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
                float scale=width/720f;var rect=(RectTransform)go.transform;
                rect.sizeDelta=new Vector2(720,(height-56)/scale);
                var flow=PartyLayout.Flow(rect,"Flow",new Vector2(.03f,.23f),new Vector2(.97f,.86f));
                var catalog=Resources.Load<PartyCatalogAsset>("PartyCatalog").Load();
                var game=new PartyGame(catalog,new[]{new PartyPlayer {name="Ana",avatar=0},new PartyPlayer {name="Juan",avatar=1}},0);
                game.Ready(game.Token);
                foreach(var action in game.Choices())
                {
                    var item=Ui.Rect(flow,"Card",Vector2.zero,Vector2.one).gameObject.AddComponent<PartyActionCard>();
                    item.Bind(action,game.Preview(action.id).text,()=>{});
                }
                Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(flow);Canvas.ForceUpdateCanvases();
                foreach(var text in go.GetComponentsInChildren<Text>())
                    Assert.GreaterOrEqual(text.rectTransform.rect.height+1,text.preferredHeight,width+"x"+height+": "+text.text);
                var footer=Ui.Button(rect,"Confirm","Contarlo así",new Vector2(.03f,.02f),new Vector2(.97f,.12f),()=>{});
                Assert.GreaterOrEqual(((RectTransform)footer.transform).rect.height*scale,44);
            }
            finally {Object.DestroyImmediate(go);}
        }
    }
}
