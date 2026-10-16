using System;
using System.Collections.Generic;
using System.Linq;
using Rumores.Core;

namespace Rumores.Tests
{
    public static class CoreChecks
    {
        public static void Assert(bool condition,string message="Aserción fallida") {if(!condition) throw new Exception(message);}
        static void Throws(Action action) {bool threw=false; try{action();}catch(InvalidOperationException){threw=true;} Assert(threw,"Se esperaba rechazo de estado/red.");}
        static Rumor Input(int panic=15) {return new Rumor {newsId="test",originalText="Una cabra",stats=new Stats(25,panic,95,0),tags=new List<string>{"animal"}};}
        static Resident Person(string profile) {return new Resident {id=profile,name=profile,personality=profile};}
        static string Fingerprint(SimulationResult r) {return string.Join("|",r.steps.Select(s=>s.residentId+":"+s.after.stats+":"+string.Join(",",s.after.history)+":"+string.Join(",",s.recipients)));}
        static string MapKey(List<MapNode> map) {return string.Join("|",map.Select(n=>n.id+":"+n.kind+":"+n.encounterId+":"+n.newsId+":"+string.Join(",",n.next)));}
        static GameSession Start(Catalog c,int seed=42) {var g=new GameSession(c);g.NewRun(seed);g.SelectNode(g.AvailableNodes().First(n=>n.kind!="event").id);return g;}
        public static void BuildSolution(GameSession g)
        {
            foreach(var e in g.Encounter.solution) Assert(g.ToggleEdge(e.from,e.to)==null,"Solución no construible: "+e);
        }
        public static Dictionary<string,Action> Cases(Catalog c)
        {
            var tests=new Dictionary<string,Action>();
            tests["Catálogo: todas las soluciones y variantes son ganables"]=()=>{var errors=CatalogValidator.Validate(c);Assert(errors.Count==0,string.Join("\n",errors));};
            tests["Miedoso: +18 y umbral estrictamente mayor a 40"]=()=>{var a=Simulator.Transform(c,Person("miedoso"),Input(22),new Bonus());Assert(a.stats.panic==40&&a.stats.importance==25);var b=Simulator.Transform(c,Person("miedoso"),Input(23),new Bonus());Assert(b.stats.importance==30);};
            tests["Exagerado: cuatro modificadores exactos"]=()=>{var a=Simulator.Transform(c,Person("exagerado"),Input(),new Bonus());Assert(a.stats.importance==39&&a.stats.panic==25&&a.stats.credibility==87&&a.stats.distortion==12);};
            tests["Mentiroso: inventa y modifica atributos"]=()=>{var a=Simulator.Transform(c,Person("mentiroso"),Input(),new Bonus());Assert(a.stats.importance==30&&a.stats.distortion==18&&a.stats.credibility==83&&a.claims[0].kind=="invención");};
            tests["Ignorante: reinterpretación semántica determinista"]=()=>{var a=Simulator.Transform(c,Person("ignorante"),Input(),new Bonus());Assert(a.stats.distortion==14&&a.stats.credibility==95&&a.tags.Contains("catástrofe")&&!a.tags.Contains("animal")&&a.claims[0].kind=="confusión");};
            tests["Ignorante: prioridad de etiquetas definida por catálogo"]=()=>{var a=Input();a.tags=new List<string>{"máquina","animal"};Assert(Simulator.Transform(c,Person("ignorante"),a,new Bonus()).claims[0].text==c.semantics[0].text);};
            tests["Clamping: 500 transformaciones permanecen en rango"]=()=>{var r=Input();for(int i=0;i<500;i++){r=Simulator.Transform(c,Person(c.personalities[i%4].id),r,new Bonus());var s=r.stats;Assert(new[]{s.importance,s.panic,s.credibility,s.distortion}.All(v=>v>=0&&v<=100));}Assert(r.stats.distortion==100&&r.stats.credibility==0);};
            tests["Transformar conserva intacta la entrada"]=()=>{var r=Input();Simulator.Transform(c,Person("ignorante"),r,new Bonus());Assert(r.tags.SequenceEqual(new[]{"animal"})&&r.claims.Count==0&&r.path.Count==0&&r.stats.distortion==0);};
            tests["Bifurcación: aislamiento profundo de atributos e historial"]=()=>{var e=c.Encounter("plaza");var r=Simulator.Run(c,e,new[]{new Edge("a","b"),new Edge("a","c")},new Bonus());var b=r.At("b").after;var other=r.At("c").after;var before=other.stats.distortion;b.stats.distortion=99;b.tags.Clear();b.claims[0].text="mutación";b.history.Clear();b.path.Clear();Assert(other.stats.distortion==before&&other.claims[0].text!="mutación"&&other.history.Count==2&&other.path.Count==2&&other.tags.Count>0);};
            tests["Orden BFS: origen antes que ramas; destino ordinal"]=()=>{var e=c.Encounter("plaza");var r=Simulator.Run(c,e,new[]{new Edge("b","e"),new Edge("a","c"),new Edge("a","b")},new Bonus());Assert(r.steps.Select(s=>s.residentId).SequenceEqual(new[]{"a","b","c","e"}));Assert(r.At("e").after.path.SequenceEqual(new[]{"a","b","e"}));};
            tests["Determinismo: orden de aristas no cambia resultados"]=()=>{var e=c.Encounter("plaza");Assert(Fingerprint(Simulator.Run(c,e,e.solution,new Bonus()))==Fingerprint(Simulator.Run(c,e,e.solution.AsEnumerable().Reverse().ToList(),new Bonus())));};
            tests["Alcance incluye origen sin duplicados"]=()=>{var e=c.Encounter("plaza");var r=Simulator.Run(c,e,e.solution,new Bonus());Assert(r.reach==e.solution.Count+1&&r.steps.Select(s=>s.residentId).Distinct().Count()==r.reach);};
            tests["Historial conserva anterior, posterior, razón y destinatarios"]=()=>{var e=c.Encounter("plaza");var r=Simulator.Run(c,e,e.solution,new Bonus());Assert(r.steps.All(s=>s.before!=s.after&&!string.IsNullOrEmpty(s.reason)&&s.after.history.Count==s.depth+1));Assert(r.steps[0].recipients.SequenceEqual(new[]{"b","c"}));};
            tests["Red rechaza ciclos, incluso desconectados"]=()=>{var e=c.Encounter("plaza");var errors=Network.Validate(e,new[]{new Edge("b","e"),new Edge("e","b")},new Bonus());Assert(errors.Any(s=>s.Contains("ciclo")));};
            tests["Red rechaza relaciones inexistentes"]=()=>{Assert(Network.Validate(c.Encounter("plaza"),new[]{new Edge("a","j")},new Bonus()).Any(s=>s.Contains("relación")));};
            tests["Red rechaza habitantes inexistentes"]=()=>{Assert(Network.Validate(c.Encounter("plaza"),new[]{new Edge("a","unknown")},new Bonus()).Any(s=>s.Contains("inexistente")));};
            tests["Red rechaza fusiones de ramas"]=()=>{Assert(Network.Validate(c.Encounter("plaza"),new[]{new Edge("a","b"),new Edge("a","c"),new Edge("b","f"),new Edge("c","f")},new Bonus()).Any(s=>s.Contains("una fuente")));};
            tests["Red rechaza entrada al origen"]=()=>{Assert(Network.Validate(c.Encounter("plaza"),new[]{new Edge("b","a")},new Bonus()).Any(s=>s.Contains("origen no")));};
            tests["Red rechaza tres salidas sin habilidad"]=()=>{Assert(Network.Validate(c.Encounter("plaza"),new[]{new Edge("a","b"),new Edge("a","c"),new Edge("a","d")},new Bonus()).Any(s=>s.Contains("salidas")));};
            tests["Red rechaza presupuesto excedido"]=()=>{var e=c.Encounter("plaza");var all=new[]{"ab","ac","be","bf","cg","ei","fj"}.Select(s=>new Edge(s[0].ToString(),s[1].ToString())).ToList();Assert(Network.Validate(e,all,new Bonus()).Any(s=>s.Contains("Susurro")));Assert(!Network.Validate(e,all,new Bonus{whispers=1}).Any(s=>s.Contains("Susurro")));};
            tests["Red rechaza subárbol desconectado"]=()=>{Assert(Network.Validate(c.Encounter("plaza"),new[]{new Edge("b","e")},new Bonus()).Any(s=>s.Contains("conectados")));};
            tests["Red vacía: edición permitida, simulación rechazada"]=()=>{var e=c.Encounter("plaza");Assert(Network.Validate(e,new List<Edge>(),new Bonus(),false).Count==0);Throws(()=>Simulator.Run(c,e,new List<Edge>(),new Bonus()));};
            tests["Red rechaza arista duplicada"]=()=>{Assert(Network.Validate(c.Encounter("plaza"),new[]{new Edge("a","b"),new Edge("a","b")},new Bonus()).Any(s=>s.Contains("repetida")));};
            tests["Eliminar rama quita descendientes y conserva hermanas"]=()=>{var r=Network.RemoveBranch(new[]{new Edge("a","b"),new Edge("a","c"),new Edge("b","e"),new Edge("e","i")},"a","b");Assert(r.Count==1&&r[0].to=="c");};
            tests["Objetivos colectivos incluyen ceros de no alcanzados"]=()=>{var e=c.Encounter("plaza");var r=Simulator.Run(c,e,new[]{new Edge("a","b")},new Bonus());var x=Objectives.Check(new Objective{kind="importance",value=7},r,10);Assert(Math.Abs(x.actual-6.4)<0.001&&!x.passed);};
            tests["Objetivos: todas las condiciones obligatorias cuentan"]=()=>{var e=c.Encounter("archivo");var r=Simulator.Run(c,e,new[]{new Edge("a","d"),new Edge("d","h"),new Edge("h","j"),new Edge("a","b")},new Bonus());Assert(r.reach==5&&!Objectives.Evaluate(e,r,4).won);};
            tests["Estrellas: límites 0–3 y criterios de nivel"]=()=>{foreach(var e in c.encounters){var r=Simulator.Run(c,e,e.solution,new Bonus());var result=Objectives.Evaluate(e,r,e.solution.Count);Assert(result.stars>=1&&result.stars<=3);}var p=c.Encounter("plaza");Assert(Objectives.Evaluate(p,Simulator.Run(c,p,new[]{new Edge("a","b")},new Bonus()),1).stars==0);};
            tests["Mapas: 1000 semillas reproducibles, rutas hasta jefe"]=()=>{for(int seed=-500;seed<500;seed++){var map=RunGenerator.Generate(c,seed);Assert(MapKey(map)==MapKey(RunGenerator.Generate(c,seed)));Assert(map.Count==11&&map.Count(n=>n.kind=="boss")==1);foreach(var n in map){Assert(n.next.All(id=>map.Any(t=>t.id==id&&t.stage==n.stage+1)));if(n.stage<5)Assert(n.next.Count>0);}for(int s=0;s<5;s++){var alternatives=map.Where(n=>n.stage==s).ToList();Assert(alternatives.Count==2&&alternatives[0].encounterId!=alternatives[1].encounterId);}}};
            tests["Semillas distintas generan contenido distinto"]=()=>{Assert(MapKey(RunGenerator.Generate(c,1))!=MapKey(RunGenerator.Generate(c,2)));};
            tests["Flujo: no permite saltarse nodos"]=()=>{var g=new GameSession(c);g.NewRun(3);Throws(()=>g.SelectNode("5-0"));};
            tests["Edición: deshacer y limpiar conservan red válida"]=()=>{var g=Start(c);Assert(g.ToggleEdge("a","b")==null);g.Clear();Assert(g.Run.edges.Count==0);g.Undo();Assert(g.Run.edges.Count==1);g.Undo();Assert(g.Run.edges.Count==0);};
            tests["Resultado: bloquea edición y doble simulación"]=()=>{var g=Start(c);BuildSolution(g);g.Simulate();Throws(()=>g.ToggleEdge("a","d"));Throws(()=>g.Simulate());Throws(()=>g.Retry());};
            tests["Paciencia: reintento paga cada fracaso y termina al tercero"]=()=>{var g=Start(c);g.ToggleEdge("a","b");for(int i=2;i>=0;i--){g.Simulate();Assert(g.Run.patience==i);if(i>0)g.Retry();}Assert(g.Run.phase==Phase.Finished&&!g.Run.won);};
            tests["Fracaso: avanzar no concede recompensa"]=()=>{var g=Start(c);g.ToggleEdge("a","b");g.Simulate();g.AcceptResult();Assert(g.Run.phase==Phase.Map&&g.Run.stage==1&&g.Run.chosenRewards.Count==0&&g.Run.patience==2);};
            tests["Recompensas: elección única y parámetros aplicados"]=()=>{foreach(var reward in c.rewards){var g=Start(c);BuildSolution(g);g.Simulate();g.AcceptResult();g.Run.rewardIds=new List<string>{reward.id};g.Run.patience=2;g.ChooseReward(reward.id);Assert(g.Run.phase==Phase.Map&&g.Run.chosenRewards.Count==1);switch(reward.effect){case "whispers":Assert(g.Run.bonus.whispers==1);break;case "importance":Assert(g.Run.bonus.exaggeratorImportance==6);break;case "credibility":Assert(g.Run.bonus.liarCredibility==6);break;case "outputs":Assert(g.Run.bonus.exaggeratorOutputs==1);break;case "heal":Assert(g.Run.patience==3);break;}Throws(()=>g.ChooseReward(reward.id));}};
            tests["Pregonero: tercera salida es legal solo en exagerado"]=()=>{var e=c.Encounter("plaza");var edges=new[]{new Edge("a","b"),new Edge("b","e"),new Edge("b","f"),new Edge("b","g")};Assert(Network.Validate(e,edges,new Bonus()).Any(s=>s.Contains("salidas")));Assert(Network.Validate(e,edges,new Bonus{exaggeratorOutputs=1}).Count==0);};
            tests["Bonos modifican personalidad sin exceder límites"]=()=>{var b=new Bonus{exaggeratorImportance=6,liarCredibility=6};Assert(Simulator.Transform(c,Person("exagerado"),Input(),b).stats.importance==45);Assert(Simulator.Transform(c,Person("mentiroso"),Input(),b).stats.credibility==89);};
            tests["Nueva run reinicia mejoras y conserva ajustes"]=()=>{var g=Start(c);g.Run.bonus.whispers=4;g.Save.settings.fast=true;g.NewRun(7);Assert(g.Run.bonus.whispers==0&&g.Run.patience==3&&g.Run.chosenRewards.Count==0&&g.Save.settings.fast);};
            tests["Evento: recuperar o susurro lleva al siguiente mapa"]=()=>{foreach(bool rest in new[]{false,true}){var g=new GameSession(c);g.NewRun(42);g.Run.stage=1;g.SelectNode("1-1");g.Run.patience=2;g.ChooseEvent(rest);Assert(g.Run.stage==2&&g.Run.phase==Phase.Map);Assert(rest?g.Run.patience==3:g.Run.bonus.whispers==1);Throws(()=>g.ChooseEvent(rest));}};
            tests["Integración: 100 runs completas, rutas y eventos"]=()=>{for(int seed=1;seed<=100;seed++){var g=new GameSession(c);g.NewRun(seed);for(int stage=0;stage<6;stage++){var nodes=g.AvailableNodes();g.SelectNode(nodes[seed%nodes.Count].id);if(g.Run.phase==Phase.Event){g.ChooseEvent(seed%2==0);continue;}BuildSolution(g);Assert(g.Simulate().won);g.AcceptResult();if(g.Run.phase==Phase.Reward)g.ChooseReward(g.Run.rewardIds[seed%g.Run.rewardIds.Count]);}Assert(g.Run.phase==Phase.Finished&&g.Run.won&&g.Save.mayorTitle&&g.Run.visited.Count==6);}};
            tests["Jefe fallido: reintentar o abandonar, nunca victoria"]=()=>{var g=new GameSession(c);g.NewRun(8);g.Run.stage=5;g.SelectNode("5-0");g.ToggleEdge("a","b");g.Simulate();Assert(!g.Run.lastEvaluation.won&&g.Run.phase==Phase.Result);g.AcceptResult();Assert(g.Run.phase==Phase.Finished&&!g.Run.won&&!g.Save.mayorTitle);};
            tests["Guardado: rechaza versiones futuras y mapas alterados"]=()=>{var g=Start(c);Assert(SaveRules.Validate(g.Save,c).Count==0);g.Save.version=99;Assert(SaveRules.Validate(g.Save,c).Count>0);g.Save.version=1;g.Run.map[0].encounterId="no";Assert(SaveRules.Validate(g.Save,c).Count>0);};
            tests["Guardado: restaurar resultado no cobra paciencia otra vez"]=()=>{var g=Start(c);g.ToggleEdge("a","b");g.Simulate();var restored=new GameSession(c,g.Save);Assert(restored.Run.patience==2&&restored.Run.phase==Phase.Result);restored.Retry();restored.Simulate();Assert(restored.Run.patience==1);};
            tests["Decisiones: misma semilla y acciones reproducen log y final"]=()=>{Func<string> play=()=>{var g=Start(c,13);BuildSolution(g);g.Simulate();g.AcceptResult();g.ChooseReward(g.Run.rewardIds[0]);return string.Join("|",g.Run.decisions.Select(d=>d.kind+":"+d.value))+g.Run.bonus.whispers;};Assert(play()==play());};
            tests["Replay: run con evento, fracaso, reintento y victoria"] = ()=>{
                var g=new GameSession(c);g.NewRun(19);
                for(int stage=0;stage<6;stage++){
                    var nodes=g.AvailableNodes();g.SelectNode(nodes.Last().id);
                    if(g.Run.phase==Phase.Event){g.ChooseEvent(stage==1);continue;}
                    if(stage==0){g.ToggleEdge("a","b");g.Simulate();g.Retry();g.Clear();}
                    BuildSolution(g);Assert(g.Simulate().won);g.AcceptResult();if(g.Run.phase==Phase.Reward)g.ChooseReward(g.Run.rewardIds[0]);
                }
                var replay=RunReplay.Play(c,19,g.Run.decisions);
                Assert(replay.Run.won&&replay.Run.patience==g.Run.patience&&replay.Run.totalStars==g.Run.totalStars&&replay.Run.chosenRewards.SequenceEqual(g.Run.chosenRewards));
            };
            tests["Replay: detecta resultado manipulado"] = ()=>{
                var g=Start(c);BuildSolution(g);g.Simulate();g.Run.decisions.Last().value="loss";
                Throws(()=>RunReplay.Play(c,g.Run.seed,g.Run.decisions));
            };
            tests["Guardado: rechaza registros nulos y recorrido incompleto"]=()=>{
                var g=Start(c);g.Run.map[0]=null;Assert(SaveRules.Validate(g.Save,c).Count>0);
                g=Start(c);g.Run.visited.Add("0-0");Assert(SaveRules.Validate(g.Save,c).Count>0);
            };
            tests["Guardado: reconstruye historial determinista y rechaza victoria falsa"]=()=>{
                var g=Start(c);BuildSolution(g);g.Simulate();g.Run.lastSimulation.steps.Clear();
                var restored=new GameSession(c,g.Save);Assert(restored.Run.lastSimulation.reach>0&&restored.Run.lastSimulation.steps.Count>0);
                g=Start(c);g.ToggleEdge("a","b");g.Simulate();g.Run.lastEvaluation.won=true;Assert(SaveRules.Validate(g.Save,c).Count>0);
            };
            return tests;
        }
    }
}
