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
        [UnityTest]
        public IEnumerator SceneBootsAndButtonsReachRewardAndMap()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            var app=UnityEngine.Object.FindObjectOfType<RumoresApp>();Assert.NotNull(app);Assert.NotNull(app.Session);
            app.StartRun(42);yield return null;
            string id=app.Session.AvailableNodes().First(n=>n.kind!="event").id;
            GameObject.Find("Node_"+id).GetComponent<Button>().onClick.Invoke();yield return null;
            foreach(var edge in app.Session.Encounter.solution)Assert.IsNull(app.Session.ToggleEdge(edge.from,edge.to));
            app.Session.Save.settings.fast=true;
            GameObject.Find("Simulate").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.AreNotEqual(Phase.Build,app.Session.Run.phase);
            GameObject.Find("Skip").GetComponent<Button>().onClick.Invoke();yield return null;yield return null;
            Assert.AreEqual(Phase.Result,app.Session.Run.phase);Assert.IsTrue(app.Session.Run.lastEvaluation.won);
            GameObject.Find("AcceptResult").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.AreEqual(Phase.Reward,app.Session.Run.phase);
            string reward=app.Session.Run.rewardIds[0];GameObject.Find("Reward_"+reward).GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.AreEqual(Phase.Map,app.Session.Run.phase);Assert.AreEqual(1,app.Session.Run.stage);
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
