using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rumores.Core;
using Rumores.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Rumores.Tests
{
    public class FlowTests
    {
        static void Click(string name)
        {
            var go=GameObject.Find(name);Assert.NotNull(go,"Falta el botón "+name);
            var button=go.GetComponent<Button>();Assert.IsTrue(button.interactable,name+" desactivado");button.onClick.Invoke();
        }
        [UnityTest] public IEnumerator TwoPlayersCompleteRoundThroughButtons() {yield return CompleteParty(2);}
        [UnityTest] public IEnumerator EightPlayersCompleteRoundThroughButtons() {yield return CompleteParty(8);}
        [TestCase(typeof(AvatarGraphic))]
        [TestCase(typeof(BoardGraphics))]
        public void CustomGraphicsCreateTheirRequiredRenderer(Type graphicType)
        {
            var go=new GameObject("GraphicDependencyTest",typeof(RectTransform));
            try
            {
                go.SetActive(false);
                var graphic=(MaskableGraphic)go.AddComponent(graphicType);
                Assert.NotNull(go.GetComponent<CanvasRenderer>(),graphicType.Name+" necesita CanvasRenderer antes de activarse.");
                Assert.AreSame(go.GetComponent<CanvasRenderer>(),graphic.canvasRenderer);
            }
            finally {UnityEngine.Object.DestroyImmediate(go);}
        }
        [UnityTest]
        public IEnumerator RosterAvatarsSurviveClippingRebuildAndReturnToMenu()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            for(int cycle=0;cycle<3;cycle++)
            {
                Click("Play");yield return null;
                Canvas.ForceUpdateCanvases();
                var avatars=UnityEngine.Object.FindObjectsOfType<AvatarGraphic>();
                Assert.AreEqual(2,avatars.Length);
                foreach(var avatar in avatars)
                {
                    Assert.NotNull(avatar.GetComponent<CanvasRenderer>());
                    Assert.NotNull(avatar.GetComponentInParent<RectMask2D>());
                }
                // These buttons rebuild the masked roster, disabling and destroying old avatars.
                Click("Avatar_0");yield return null;
                Assert.IsTrue(avatars.All(avatar=>avatar==null));
                Click("AddPlayer");yield return null;
                Assert.AreEqual(3,UnityEngine.Object.FindObjectsOfType<AvatarGraphic>().Length);
                Click("Remove_2");yield return null;
                Click("Back");yield return null;
                Assert.AreEqual(0,UnityEngine.Object.FindObjectsOfType<AvatarGraphic>().Length);
                LogAssert.NoUnexpectedReceived();
            }
        }
        IEnumerator CompleteParty(int count)
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            var app=UnityEngine.Object.FindObjectOfType<PartyApp>();Assert.NotNull(app);app.FastAnimations=true;
            Click("HowToPlay");yield return null;Click("Back");yield return null;
            Click("Settings");yield return null;Click("ToggleAnimations");yield return null;Click("Back");yield return null;
            Click("Play");yield return null;
            for(int i=2;i<count;i++){Click("AddPlayer");yield return null;}
            GameObject.Find("PlayerName_0").GetComponent<InputField>().text=" ";Click("StartRound");yield return null;Assert.IsNull(app.Game);
            GameObject.Find("PlayerName_0").GetComponent<InputField>().text="Jugador 2";Click("StartRound");yield return null;Assert.IsNull(app.Game);
            GameObject.Find("PlayerName_0").GetComponent<InputField>().text="Ana";
            Click("Avatar_0");yield return null;Click("StartRound");yield return null;
            Assert.AreEqual(count,app.Game.Players.Count);Assert.AreEqual(1,app.Game.Players[0].avatar);
            for(int turn=0;turn<count*2;turn++)
            {
                Assert.AreEqual("player-"+(turn%count),app.Game.Current.id);
                Click("Ready");yield return null;
                Assert.IsFalse(GameObject.Find("Confirm").GetComponent<Button>().interactable);
                Assert.IsNull(GameObject.Find("Split"));Assert.IsNull(GameObject.Find("Branch_B"));
                Assert.AreEqual(3,UnityEngine.Object.FindObjectsOfType<PartyActionCard>().Length);
                int page=turn%2;
                if(page==1)
                {
                    Click("Action_"+app.Game.Choices()[0].id);yield return null;
                    Click("MoreChoices");yield return null;
                    Assert.IsFalse(GameObject.Find("Confirm").GetComponent<Button>().interactable);
                    Assert.AreEqual(turn,app.Game.Turn);
                    Assert.AreEqual(3,UnityEngine.Object.FindObjectsOfType<PartyActionCard>().Length);
                }
                string action=app.Game.Choices(page)[turn%3].id;
                string preview=app.Game.Preview(action).text;
                Assert.IsTrue(GameObject.Find("Action_"+action).GetComponentsInChildren<Text>().Any(t=>t.text==preview));
                Click("Action_"+action);yield return null;
                if(turn==1){Click("History");yield return null;Click("BackToTurn");yield return null;}
                var confirm=GameObject.Find("Confirm").GetComponent<Button>();confirm.onClick.Invoke();
                int events=app.Game.Branches[0].rumor.history.Count;confirm.onClick.Invoke();Assert.AreEqual(events,app.Game.Branches[0].rumor.history.Count);Assert.AreEqual(preview,app.Game.LastChange.text);
                yield return null;Assert.AreEqual(Rumores.Core.Party.PartyPhase.Revealing,app.Game.Phase);
                Click("Next");yield return null;
            }
            Assert.AreEqual(Rumores.Core.Party.PartyPhase.Finished,app.Game.Phase);Assert.AreEqual(1,app.Game.Branches.Count);Assert.AreEqual(count*2,app.Game.Branches[0].rumor.history.Count);
            Click("History");yield return null;Click("BackToTurn");yield return null;
            string oldNews=app.Game.NewsId;Click("Rematch");yield return null;
            Assert.AreEqual(0,app.Game.Turn);Assert.AreEqual(1,app.Game.Branches.Count);Assert.AreNotEqual(oldNews,app.Game.NewsId);
            Assert.AreEqual(0,app.Game.Branches[0].rumor.history.Count);Assert.AreEqual("Ana",app.Game.Current.name);
        }
        [Test]
        public void JsonUtilityAndAtomicSaveResumeRewardAndRecoverBackup()
        {
            string directory=Path.Combine(Application.temporaryCachePath,"rumores-test-"+Guid.NewGuid().ToString("N"));
            try
            {
                var catalog=Resources.Load<RumoresCatalog>("RumoresCatalog").Load();var g=new GameSession(catalog);g.NewRun(73);
                g.SelectNode(g.AvailableNodes()[0].id);foreach(var edge in g.Encounter.solution)Assert.IsNull(g.ToggleEdge(edge.from,edge.to));
                g.Simulate();g.AcceptResult();var storage=new LocalSave(directory);Assert.IsTrue(storage.Write(g.Save));
                var restored=new GameSession(catalog,storage.Load(catalog));Assert.AreEqual(Phase.Reward,restored.Run.phase);
                restored.ChooseReward(restored.Run.rewardIds[0]);Assert.IsTrue(storage.Write(restored.Save));
                File.WriteAllText(Path.Combine(directory,"rumores-save.json"),"damaged");
                var backup=storage.Load(catalog);Assert.AreEqual(Phase.Reward,backup.run.phase);Assert.NotNull(storage.Notice);
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]
        public void FutureSaveIsPreservedAndWriteBlocked()
        {
            string directory=Path.Combine(Application.temporaryCachePath,"rumores-test-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
            try
            {
                string path=Path.Combine(directory,"rumores-save.json");File.WriteAllText(path,"{\"version\":99,\"rulesVersion\":99}");
                var storage=new LocalSave(directory);storage.Load(Resources.Load<RumoresCatalog>("RumoresCatalog").Load());
                Assert.IsTrue(storage.ReadOnly);Assert.IsFalse(storage.Write(new SaveData()));Assert.IsTrue(File.ReadAllText(path).Contains("99"));
            }
            finally{Directory.Delete(directory,true);}
        }
    }
}
