using System;
using System.Collections.Generic;
using System.Linq;

namespace Rumores.Core
{
    [Serializable] public sealed class Stats
    {
        public int importance, panic, credibility, distortion;
        public Stats() { }
        public Stats(int i, int p, int c, int d) { importance=i; panic=p; credibility=c; distortion=d; Clamp(); }
        public Stats Copy() { return new Stats(importance, panic, credibility, distortion); }
        public void Clamp() { importance=Limit(importance); panic=Limit(panic); credibility=Limit(credibility); distortion=Limit(distortion); }
        public static int Limit(int n) { return Math.Max(0, Math.Min(100,n)); }
        public void Add(int i,int p,int c,int d) { importance+=i; panic+=p; credibility+=c; distortion+=d; Clamp(); }
        public override string ToString() { return "IMP " + importance + "  PAN " + panic + "  CRE " + credibility + "  DIS " + distortion; }
    }
    [Serializable] public sealed class Claim
    {
        public string text, authorId, kind;
        public Claim Copy() { return new Claim {text=text,authorId=authorId,kind=kind}; }
    }
    [Serializable] public sealed class Rumor
    {
        public string newsId, originalText;
        public Stats stats=new Stats();
        public List<string> tags=new List<string>(), path=new List<string>(), history=new List<string>();
        public List<Claim> claims=new List<Claim>();
        public Rumor Copy() { return new Rumor {newsId=newsId,originalText=originalText,stats=stats.Copy(),tags=new List<string>(tags),path=new List<string>(path),history=new List<string>(history),claims=claims.Select(c=>c.Copy()).ToList()}; }
    }
    [Serializable] public sealed class Resident { public string id,name,personality; public float x,y; }
    [Serializable] public sealed class Edge
    {
        public string from,to;
        public Edge() { }
        public Edge(string f,string t) { from=f; to=t; }
        public Edge Copy() { return new Edge(from,to); }
        public override string ToString() { return from+">"+to; }
    }
    [Serializable] public sealed class Personality
    {
        public string id,name,description,phrase;
        public int importance,panic,credibility,distortion,panicThreshold,thresholdImportance;
    }
    [Serializable] public sealed class SemanticRule { public string tag,replaceTag,text; }
    [Serializable] public sealed class News { public string id,text; public Stats stats; public List<string> tags=new List<string>(); }
    [Serializable] public sealed class Objective
    {
        public string kind,label,target;
        public int value;
    }
    [Serializable] public sealed class Encounter
    {
        public string id,name,description,source,newsId;
        public int budget,efficientEdges,difficulty;
        public bool boss;
        public List<string> newsOptions=new List<string>();
        public List<Resident> residents=new List<Resident>();
        public List<Edge> potential=new List<Edge>(), solution=new List<Edge>();
        public List<Objective> required=new List<Objective>(), secondary=new List<Objective>();
        public Resident Person(string id) { return residents.Find(p=>p.id==id); }
    }
    [Serializable] public sealed class Reward
    {
        public string id,name,description,effect;
        public int amount,maxStacks;
    }
    [Serializable] public sealed class Catalog
    {
        public int rulesVersion=1;
        public List<Personality> personalities=new List<Personality>();
        public List<SemanticRule> semantics=new List<SemanticRule>();
        public List<News> news=new List<News>();
        public List<Encounter> encounters=new List<Encounter>();
        public List<Reward> rewards=new List<Reward>();
        public Encounter Encounter(string id) { return encounters.Find(x=>x.id==id) ?? throw new ArgumentException("Encuentro desconocido: "+id); }
        public News News(string id) { return news.Find(x=>x.id==id) ?? throw new ArgumentException("Noticia desconocida: "+id); }
        public Personality Personality(string id) { return personalities.Find(x=>x.id==id) ?? throw new ArgumentException("Personalidad desconocida: "+id); }
        public Reward Reward(string id) { return rewards.Find(x=>x.id==id) ?? throw new ArgumentException("Mejora desconocida: "+id); }
    }
    [Serializable] public sealed class Bonus
    {
        public int whispers,exaggeratorImportance,liarCredibility,exaggeratorOutputs;
        public Bonus Copy() { return new Bonus {whispers=whispers,exaggeratorImportance=exaggeratorImportance,liarCredibility=liarCredibility,exaggeratorOutputs=exaggeratorOutputs}; }
    }
    [Serializable] public sealed class Step
    {
        public string residentId,effect,reason;
        public int depth;
        public Rumor before,after;
        public List<string> recipients=new List<string>();
    }
    [Serializable] public sealed class SimulationResult
    {
        public List<Step> steps=new List<Step>();
        public int reach;
        public Step At(string id) { return steps.Find(s=>s.residentId==id); }
    }
    [Serializable] public sealed class ConditionResult { public string label; public bool passed; public double actual; public int threshold; }
    [Serializable] public sealed class Evaluation
    {
        public bool won;
        public int stars;
        public List<ConditionResult> conditions=new List<ConditionResult>();
    }
}
