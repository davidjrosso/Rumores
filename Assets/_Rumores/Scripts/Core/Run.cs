using System;
using System.Collections.Generic;
using System.Linq;

namespace Rumores.Core
{
    public enum Phase { Map, Build, Result, Reward, Event, Finished }
    [Serializable] public sealed class MapNode
    {
        public string id,kind,encounterId,newsId;
        public int stage;
        public List<string> next=new List<string>();
    }
    public sealed class StableRandom
    {
        uint state;
        public StableRandom(int seed) { state=unchecked((uint)seed); if(state==0) state=0x9e3779b9; }
        public int Next(int count) { if(count<=0) throw new ArgumentOutOfRangeException("count"); state^=state<<13; state^=state>>17; state^=state<<5; return (int)(state%(uint)count); }
    }
    public static class RunGenerator
    {
        public static List<MapNode> Generate(Catalog catalog,int seed)
        {
            var random=new StableRandom(seed);
            var normal=catalog.encounters.Where(e=>!e.boss).OrderBy(e=>e.id,StringComparer.Ordinal).ToList();
            var boss=catalog.encounters.Single(e=>e.boss);
            var map=new List<MapNode>();
            for(int stage=0;stage<6;stage++)
            {
                int first=random.Next(normal.Count);
                int second=(first+1+random.Next(normal.Count-1))%normal.Count;
                for(int slot=0;slot<(stage==5?1:2);slot++)
                {
                    var encounter=stage==5?boss:normal[slot==0?first:second];
                    string news=encounter.newsOptions.Count==0?encounter.newsId:encounter.newsOptions[random.Next(encounter.newsOptions.Count)];
                    var node=new MapNode {id=stage+"-"+slot,stage=stage,kind=stage==5?"boss":slot==1 && (stage==1||stage==3)?"event":"challenge",encounterId=encounter.id,newsId=news};
                    if(stage<5) { node.next.Add((stage+1)+"-0"); if(stage<4) node.next.Add((stage+1)+"-1"); }
                    map.Add(node);
                }
            }
            return map;
        }
    }
    [Serializable] public sealed class Decision { public string kind,value; }
    [Serializable] public sealed class RunState
    {
        public int seed,stage,patience=3,wins,totalStars,failures;
        public Phase phase=Phase.Map;
        public bool won;
        public string currentNode;
        public List<MapNode> map=new List<MapNode>();
        public List<Edge> edges=new List<Edge>();
        public List<Decision> decisions=new List<Decision>();
        public List<string> rewardIds=new List<string>(),chosenRewards=new List<string>(),visited=new List<string>();
        public Bonus bonus=new Bonus();
        public SimulationResult lastSimulation;
        public Evaluation lastEvaluation;
    }
    [Serializable] public sealed class Settings { public float volume=0.5f; public bool fast,tutorial=true; }
    [Serializable] public sealed class SaveData
    {
        public int version=1,rulesVersion=1;
        public Settings settings=new Settings();
        public bool mayorTitle;
        public RunState run;
    }
    public sealed class GameSession
    {
        readonly Catalog catalog;
        readonly Stack<List<Edge>> undo=new Stack<List<Edge>>();
        public SaveData Save {get;private set;}
        public RunState Run { get {return Save.run;} }
        public Encounter Encounter {get { return catalog.Encounter(Node.encounterId); }}
        public MapNode Node {get {return Run.map.Find(n=>n.id==Run.currentNode) ?? throw new InvalidOperationException("No hay encuentro activo."); }}
        public GameSession(Catalog catalog,SaveData save=null)
        {
            this.catalog=catalog; Save=save ?? new SaveData();
            var errors=SaveRules.Validate(Save,catalog); if(errors.Count>0) throw new ArgumentException(string.Join("\n",errors));
        }
        void Require(Phase phase) { if(Run==null || Run.phase!=phase) throw new InvalidOperationException("Acción no disponible en este momento."); }
        void Log(string kind,string value) { Run.decisions.Add(new Decision {kind=kind,value=value}); }
        public void NewRun(int seed)
        {
            Save.run=new RunState {seed=seed,map=RunGenerator.Generate(catalog,seed)};
            undo.Clear();
        }
        public List<MapNode> AvailableNodes()
        {
            if(Run==null) return new List<MapNode>();
            return Run.map.Where(n=>n.stage==Run.stage && (Run.visited.Count==0 || Run.map.Find(p=>p.id==Run.visited.Last()).next.Contains(n.id))).ToList();
        }
        public void SelectNode(string id)
        {
            Require(Phase.Map);
            var node=AvailableNodes().Find(n=>n.id==id); if(node==null) throw new InvalidOperationException("Elegí una ruta conectada a tu posición.");
            Run.currentNode=id; Run.edges.Clear(); undo.Clear(); Run.lastEvaluation=null; Run.lastSimulation=null;
            Run.phase=node.kind=="event"?Phase.Event:Phase.Build;
            Log("node",id);
        }
        public string ToggleEdge(string from,string to)
        {
            Require(Phase.Build);
            var proposal=Run.edges.Any(e=>e.from==from&&e.to==to)?Network.RemoveBranch(Run.edges,from,to):Run.edges.Select(e=>e.Copy()).Concat(new[]{new Edge(from,to)}).ToList();
            var errors=Network.Validate(Encounter,proposal,Run.bonus,false);
            if(errors.Count>0) return string.Join("\n",errors);
            undo.Push(Run.edges.Select(e=>e.Copy()).ToList()); Run.edges=proposal; return null;
        }
        public void Undo() { Require(Phase.Build); if(undo.Count>0) Run.edges=undo.Pop(); }
        public void Clear() { Require(Phase.Build); if(Run.edges.Count>0) {undo.Push(Run.edges.Select(e=>e.Copy()).ToList()); Run.edges.Clear();} }
        public Evaluation Simulate()
        {
            Require(Phase.Build);
            var simulation=Simulator.Run(catalog,Encounter,Run.edges,Run.bonus,Node.newsId);
            var result=Objectives.Evaluate(Encounter,simulation,Run.edges.Count);
            Run.lastSimulation=simulation; Run.lastEvaluation=result;
            Log("network",string.Join(",",Run.edges.OrderBy(e=>e.from,StringComparer.Ordinal).ThenBy(e=>e.to,StringComparer.Ordinal).Select(e=>e.ToString())));
            Log("result",result.won?"win":"loss");
            if(result.won) { Run.wins++; Run.totalStars+=result.stars; }
            else {Run.patience--; Run.failures++;}
            Run.phase=Run.patience==0?Phase.Finished:Phase.Result;
            return result;
        }
        public void Retry()
        {
            Require(Phase.Result);
            if(Run.lastEvaluation.won) throw new InvalidOperationException("El desafío ya fue superado.");
            Run.phase=Phase.Build; Run.lastSimulation=null; Run.lastEvaluation=null; undo.Clear(); Log("retry",Run.currentNode);
        }
        public void AcceptResult()
        {
            Require(Phase.Result);
            if(Encounter.boss)
            {
                Run.won=Run.lastEvaluation.won; Run.phase=Phase.Finished;
                if(Run.won) {Save.mayorTitle=true; Run.visited.Add(Run.currentNode);}
                Log("finish",Run.won?"victory":"abandon"); return;
            }
            if(!Run.lastEvaluation.won) {Log("continue","no_reward"); Advance(); return;}
            var available=catalog.rewards.Where(r=>Run.chosenRewards.Count(id=>id==r.id)<r.maxStacks).OrderBy(r=>r.id,StringComparer.Ordinal).ToList();
            var random=new StableRandom(unchecked(Run.seed+Run.stage*997+17));
            Run.rewardIds.Clear();
            while(Run.rewardIds.Count<3 && available.Count>0) {int i=random.Next(available.Count); Run.rewardIds.Add(available[i].id); available.RemoveAt(i);}
            if(Run.rewardIds.Count==0) Advance(); else Run.phase=Phase.Reward;
        }
        public void ChooseReward(string id)
        {
            Require(Phase.Reward);
            if(!Run.rewardIds.Contains(id)) throw new InvalidOperationException("Esta recompensa no está disponible.");
            Apply(catalog.Reward(id)); Run.chosenRewards.Add(id); Log("reward",id); Run.rewardIds.Clear(); Advance();
        }
        void Apply(Reward r)
        {
            switch(r.effect)
            {
                case "whispers": Run.bonus.whispers+=r.amount; break;
                case "importance": Run.bonus.exaggeratorImportance+=r.amount; break;
                case "credibility": Run.bonus.liarCredibility+=r.amount; break;
                case "outputs": Run.bonus.exaggeratorOutputs+=r.amount; break;
                case "heal": Run.patience=Math.Min(3,Run.patience+r.amount); break;
                default: throw new ArgumentException("Efecto desconocido: "+r.effect);
            }
        }
        public void ChooseEvent(bool rest)
        {
            Require(Phase.Event);
            if(rest) Run.patience=Math.Min(3,Run.patience+1); else Run.bonus.whispers++;
            Log("event",rest?"rest":"whisper"); Advance();
        }
        void Advance()
        {
            Run.visited.Add(Run.currentNode); Run.stage++; Run.currentNode=null;
            Run.edges.Clear(); Run.lastSimulation=null; Run.lastEvaluation=null; Run.phase=Phase.Map; undo.Clear();
        }
    }
    public static class SaveRules
    {
        public static List<string> Validate(SaveData data,Catalog catalog)
        {
            var errors=new List<string>();
            if(data==null) return new List<string>{"Guardado vacío."};
            if(data.version!=1 || data.rulesVersion!=catalog.rulesVersion) errors.Add("Versión de guardado o reglas no compatible.");
            if(data.settings==null) data.settings=new Settings();
            data.settings.volume=float.IsNaN(data.settings.volume)?0.5f:Math.Max(0,Math.Min(1,data.settings.volume));
            var r=data.run; if(r==null) return errors;
            if(r.stage<0||r.stage>5||r.patience<0||r.patience>3||!Enum.IsDefined(typeof(Phase),r.phase)) errors.Add("Estado de run fuera de rango.");
            if(r.bonus==null || r.edges==null || r.map==null || r.visited==null || r.decisions==null || r.rewardIds==null || r.chosenRewards==null) {errors.Add("Faltan datos de la run.");return errors;}
            if(r.bonus.whispers<0||r.bonus.exaggeratorOutputs<0||r.bonus.exaggeratorImportance<0||r.bonus.liarCredibility<0) errors.Add("Mejoras inválidas.");
            if(r.map.Any(n=>n==null) || r.decisions.Any(d=>d==null)) { errors.Add("El guardado contiene registros vacíos."); return errors; }
            var expected=RunGenerator.Generate(catalog,r.seed);
            if(r.map.Count!=expected.Count || expected.Any(n=>!r.map.Any(m=>m.id==n.id&&m.kind==n.kind&&m.stage==n.stage&&m.encounterId==n.encounterId&&m.newsId==n.newsId&&m.next!=null&&m.next.SequenceEqual(n.next)))) { errors.Add("El mapa no coincide con la semilla y versión."); return errors; }
            int expectedVisited=r.phase==Phase.Finished&&r.won?6:r.stage;
            if(r.visited.Count!=expectedVisited || r.visited.Where((id,index)=>!r.map.Any(n=>n.id==id&&n.stage==index)).Any()) errors.Add("El recorrido guardado está incompleto.");
            if(r.phase!=Phase.Map)
            {
                var node=r.map.Find(n=>n.id==r.currentNode);
                if(node==null || node.stage!=r.stage) errors.Add("Encuentro activo inválido.");
                else
                {
                    if((r.phase==Phase.Event)!=(node.kind=="event")) errors.Add("El tipo de encuentro no coincide con la fase.");
                    if(r.phase==Phase.Build) errors.AddRange(Network.Validate(catalog.Encounter(node.encounterId),r.edges,r.bonus,false));
                    if(r.phase==Phase.Result || r.phase==Phase.Reward || r.phase==Phase.Finished)
                    {
                        try
                        {
                            var encounter=catalog.Encounter(node.encounterId);
                            var simulation=Simulator.Run(catalog,encounter,r.edges,r.bonus,node.newsId);
                            var evaluation=Objectives.Evaluate(encounter,simulation,r.edges.Count);
                            if(r.lastEvaluation==null || evaluation.won!=r.lastEvaluation.won || evaluation.stars!=r.lastEvaluation.stars) errors.Add("El resultado guardado no coincide con la red.");
                            else {r.lastSimulation=simulation;r.lastEvaluation=evaluation;}
                            if(r.phase==Phase.Reward && !evaluation.won) errors.Add("Una derrota no puede conceder recompensas.");
                            if(r.won && (!encounter.boss || !evaluation.won || r.phase!=Phase.Finished)) errors.Add("La victoria final no coincide con el encuentro.");
                        }
                        catch(InvalidOperationException ex) {errors.Add(ex.Message);}
                    }
                }
            }
            if((r.phase==Phase.Result||r.phase==Phase.Reward) && (r.lastEvaluation==null||r.lastSimulation==null)) errors.Add("Falta el resultado del desafío.");
            if(r.phase==Phase.Reward && (r.rewardIds.Count==0 || r.rewardIds.Any(id=>!catalog.rewards.Any(x=>x.id==id)))) errors.Add("Recompensas pendientes inválidas.");
            if(r.patience==0 && r.phase!=Phase.Finished) errors.Add("Una run sin paciencia debe estar terminada.");
            return errors;
        }
    }
}
