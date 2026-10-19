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
            string[] actions={"panico","exagerar","mentir","confundir","calmar","difundir"};
            for(int turn=0;turn<count*2;turn++)
            {
                Assert.AreEqual("player-"+(turn%count),app.Game.Current.id);
                Click("Ready");yield return null;
                Assert.IsFalse(GameObject.Find("Confirm").GetComponent<Button>().interactable);
                if(turn==0){Click("Split");yield return null;}
                if(turn%2==1){Click("Branch_B");yield return null;}
                Click("Action_"+actions[turn%actions.Length]);yield return null;
                if(turn==1){Click("History");yield return null;Click("BackToTurn");yield return null;}
                var confirm=GameObject.Find("Confirm").GetComponent<Button>();confirm.onClick.Invoke();
                int score=app.Game.Current.score;confirm.onClick.Invoke();Assert.AreEqual(score,app.Game.Current.score);
                yield return null;Assert.AreEqual(Rumores.Core.Party.PartyPhase.Revealing,app.Game.Phase);
                Click("Next");yield return null;
            }
            Assert.AreEqual(Rumores.Core.Party.PartyPhase.Finished,app.Game.Phase);Assert.AreEqual(2,app.Game.Branches.Count);Assert.IsTrue(app.Game.Winners().Count>0);
            Assert.AreNotEqual(app.Game.Branches[0].rumor.headline,app.Game.Branches[1].rumor.headline);
            string oldNews=app.Game.NewsId;Click("Rematch");yield return null;
            Assert.AreEqual(0,app.Game.Turn);Assert.AreEqual(1,app.Game.Branches.Count);Assert.AreNotEqual(oldNews,app.Game.NewsId);
            Assert.IsTrue(app.Game.Players.All(p=>p.score==0));Assert.AreEqual("Ana",app.Game.Current.name);
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
