using System;
using System.Collections.Generic;
using System.IO;
using Rumores.Core;

namespace Rumores.Tests
{
    public static class SaveChecks
    {
        static void InDirectory(Action<string> test)
        {
            string dir=Path.Combine(Path.GetTempPath(),"rumores-save-test-"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try {test(dir);} finally {Directory.Delete(dir,true);}
        }
        static GameSession Active(Catalog c) {var g=new GameSession(c);g.NewRun(73);return g;}
        public static Dictionary<string,Action> Cases(Catalog c,ISaveJson json)
        {
            var tests=new Dictionary<string,Action>();
            tests["Archivo: primer inicio sin guardado"] = ()=>InDirectory(dir=>{
                var store=new SaveStorage(dir,json);var g=new GameSession(c,store.Load(c));
                CoreChecks.Assert(g.Run==null&&!store.ReadOnly&&store.Notice==null);
            });
            tests["Archivo: guardar menú vacío y reiniciar tres veces"] = ()=>InDirectory(dir=>{
                var store=new SaveStorage(dir,json);var data=new SaveData();data.settings.volume=.25f;data.mayorTitle=true;
                for(int i=0;i<3;i++){
                    CoreChecks.Assert(store.Write(data));store=new SaveStorage(dir,json);data=store.Load(c);
                    CoreChecks.Assert(data.run==null&&data.settings.volume==.25f&&data.mayorTitle&&store.Notice==null);
                    CoreChecks.Assert(new GameSession(c,data).Run==null);
                }
            });
            tests["Archivo: migra run nula expandida por serializador inline"] = ()=>InDirectory(dir=>{
                var data=new SaveData {run=new RunState()};data.settings.fast=true;
                File.WriteAllText(Path.Combine(dir,"rumores-save.json"),json.Serialize(data));
                var store=new SaveStorage(dir,json);var restored=store.Load(c);
                CoreChecks.Assert(restored.run==null&&restored.settings.fast&&store.Notice==null);
                CoreChecks.Assert(store.Write(restored));CoreChecks.Assert(store.Load(c).run==null);
            });
            tests["Archivo: mapa incompatible recupera respaldo válido"] = ()=>InDirectory(dir=>{
                var store=new SaveStorage(dir,json);var g=Active(c);CoreChecks.Assert(store.Write(g.Save));CoreChecks.Assert(store.Write(g.Save));
                var broken=Active(c).Save;broken.run.map[0].encounterId="encuentro-anterior";
                string path=Path.Combine(dir,"rumores-save.json");string bad=json.Serialize(broken);File.WriteAllText(path,bad);
                var restored=store.Load(c);CoreChecks.Assert(restored.run.seed==73&&store.Notice.Contains("respaldo"));
                var archives=Directory.GetFiles(dir,"*.invalid-*");CoreChecks.Assert(archives.Length==1&&File.ReadAllText(archives[0])==bad);
                CoreChecks.Assert(store.Write(restored));
                File.WriteAllText(path,"broken again");CoreChecks.Assert(new SaveStorage(dir,json).Load(c).run.seed==73);
            });
            tests["Archivo: dos mapas inválidos permiten volver a empezar"] = ()=>InDirectory(dir=>{
                var broken=Active(c).Save;broken.run.map.Clear();var text=json.Serialize(broken);
                var path=Path.Combine(dir,"rumores-save.json");File.WriteAllText(path,text);File.WriteAllText(path+".bak",text);
                var store=new SaveStorage(dir,json);var g=new GameSession(c,store.Load(c));
                CoreChecks.Assert(g.Run==null&&store.Notice!=null&&Directory.GetFiles(dir,"*.invalid-*").Length==2);
                g.NewRun(99);CoreChecks.Assert(store.Write(g.Save));CoreChecks.Assert(new SaveStorage(dir,json).Load(c).run.seed==99);
            });
            tests["Archivo: JSON truncado conserva original y recupera inicio"] = ()=>InDirectory(dir=>{
                string path=Path.Combine(dir,"rumores-save.json");File.WriteAllText(path,"{\"version\":1,");
                var store=new SaveStorage(dir,json);CoreChecks.Assert(new GameSession(c,store.Load(c)).Run==null);
                CoreChecks.Assert(Directory.GetFiles(dir,"*.invalid-*").Length==1);
            });
            tests["Archivo: preserva formato o reglas de versión futura"] = ()=>InDirectory(dir=>{
                foreach(string text in new[]{"{\"fileVersion\":3,\"version\":1,\"rulesVersion\":1}","{\"version\":99,\"rulesVersion\":99}"}){
                    string path=Path.Combine(dir,"rumores-save.json");File.WriteAllText(path,text);
                    var store=new SaveStorage(dir,json);CoreChecks.Assert(store.Load(c).run==null&&store.ReadOnly);
                    CoreChecks.Assert(!store.Write(new SaveData())&&File.ReadAllText(path)==text);
                }
            });
            tests["Archivo: run legítima de semilla cero no se descarta"] = ()=>InDirectory(dir=>{
                var g=new GameSession(c);g.NewRun(0);var store=new SaveStorage(dir,json);CoreChecks.Assert(store.Write(g.Save));
                var restored=new GameSession(c,store.Load(c));CoreChecks.Assert(restored.Run!=null&&restored.Run.seed==0&&restored.Run.map.Count==11);
            });
            tests["Archivo: migración de run antigua mantiene progreso"] = ()=>InDirectory(dir=>{
                var g=Active(c);g.SelectNode(g.AvailableNodes()[0].id);g.ToggleEdge("a","b");
                File.WriteAllText(Path.Combine(dir,"rumores-save.json"),json.Serialize(g.Save));
                var store=new SaveStorage(dir,json);var restored=new GameSession(c,store.Load(c));
                CoreChecks.Assert(restored.Run.phase==Phase.Build&&restored.Run.edges.Count==1);CoreChecks.Assert(store.Write(restored.Save));
                CoreChecks.Assert(new GameSession(c,store.Load(c)).Run.edges.Count==1);
            });
            tests["Archivo: rechazo de documento activo sin run"] = ()=>InDirectory(dir=>{
                var doc=new SaveDocument {hasRun=true,runJson=""};
                File.WriteAllText(Path.Combine(dir,"rumores-save.json"),json.Serialize(doc));
                var store=new SaveStorage(dir,json);CoreChecks.Assert(store.Load(c).run==null&&store.Notice!=null);
            });
            tests["Archivo: run completa restaurada en cada transición"] = ()=>InDirectory(dir=>{
                var store=new SaveStorage(dir,json);var g=Active(c);
                Func<GameSession,GameSession> reload=game=>{
                    CoreChecks.Assert(store.Write(game.Save));
                    return new GameSession(c,new SaveStorage(dir,json).Load(c));
                };
                for(int stage=0;stage<6;stage++){
                    var nodes=g.AvailableNodes();g.SelectNode(nodes[nodes.Count-1].id);g=reload(g);
                    if(g.Run.phase==Phase.Event){g.ChooseEvent(true);g=reload(g);continue;}
                    CoreChecks.BuildSolution(g);g=reload(g);CoreChecks.Assert(g.Simulate().won);g=reload(g);
                    g.AcceptResult();g=reload(g);
                    if(g.Run.phase==Phase.Reward){g.ChooseReward(g.Run.rewardIds[0]);g=reload(g);}
                }
                CoreChecks.Assert(g.Run.phase==Phase.Finished&&g.Run.won&&g.Save.mayorTitle);
            });
            return tests;
        }
    }
}
