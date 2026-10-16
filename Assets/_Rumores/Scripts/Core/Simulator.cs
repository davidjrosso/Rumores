using System;
using System.Collections.Generic;
using System.Linq;

namespace Rumores.Core
{
    public static class Simulator
    {
        public static Rumor Transform(Catalog catalog,Resident person,Rumor input,Bonus bonus)
        {
            var r=input.Copy(); var p=catalog.Personality(person.personality);
            r.stats.Add(p.importance+(p.id=="exagerado" ? bonus.exaggeratorImportance:0),p.panic,
                p.credibility+(p.id=="mentiroso" ? bonus.liarCredibility:0),p.distortion);
            if(p.id=="miedoso" && r.stats.panic>p.panicThreshold) r.stats.Add(p.thresholdImportance,0,0,0);
            string phrase=p.phrase;
            string kind="interpretación";
            if(p.id=="ignorante")
            {
                var rule=catalog.semantics.Find(s=>r.tags.Contains(s.tag));
                if(rule!=null) { phrase=rule.text; r.tags.Remove(rule.tag); if(!r.tags.Contains(rule.replaceTag)) r.tags.Add(rule.replaceTag); }
                kind="confusión";
            }
            if(p.id=="mentiroso") kind="invención";
            if(!string.IsNullOrEmpty(phrase)) r.claims.Add(new Claim {text=phrase,authorId=person.id,kind=kind});
            r.path.Add(person.id);
            r.history.Add(person.name+" · "+p.name+": "+input.stats+" → "+r.stats+". "+phrase);
            return r;
        }
        sealed class Delivery { public string id; public Rumor rumor; public int depth; }
        public static SimulationResult Run(Catalog catalog,Encounter encounter,IList<Edge> edges,Bonus bonus,string newsId=null)
        {
            var errors=Network.Validate(encounter,edges,bonus);
            if(errors.Count>0) throw new InvalidOperationException(string.Join("\n",errors));
            var n=catalog.News(newsId ?? encounter.newsId);
            var initial=new Rumor {newsId=n.id,originalText=n.text,stats=n.stats.Copy(),tags=new List<string>(n.tags)};
            var queue=new Queue<Delivery>(); queue.Enqueue(new Delivery {id=encounter.source,rumor=initial,depth=0});
            var result=new SimulationResult();
            while(queue.Count>0)
            {
                var d=queue.Dequeue(); var person=encounter.Person(d.id);
                var after=Transform(catalog,person,d.rumor,bonus);
                var targets=edges.Where(e=>e.from==d.id).Select(e=>e.to).OrderBy(id=>id,StringComparer.Ordinal).ToList();
                result.steps.Add(new Step {residentId=d.id,depth=d.depth,before=d.rumor.Copy(),after=after.Copy(),effect=catalog.Personality(person.personality).name,reason=after.history.Last(),recipients=targets});
                foreach(var target in targets) queue.Enqueue(new Delivery {id=target,rumor=after.Copy(),depth=d.depth+1});
            }
            result.reach=result.steps.Count;
            return result;
        }
    }
    public static class Objectives
    {
        public static ConditionResult Check(Objective o,SimulationResult result,int population)
        {
            double actual; bool upper=false;
            var steps=result.steps;
            switch(o.kind)
            {
                case "reach": actual=result.reach; break;
                case "importance": actual=steps.Sum(s=>s.after.stats.importance)/(double)population; break;
                case "panic": actual=steps.Sum(s=>s.after.stats.panic)/(double)population; break;
                case "credibility": actual=steps.Min(s=>s.after.stats.credibility); break;
                case "distortion": actual=steps.Max(s=>s.after.stats.distortion); upper=true; break;
                case "target": actual=steps.Any(s=>s.residentId==o.target)?1:0; break;
                case "depth": actual=steps.Max(s=>s.depth)+1; break;
                case "contrast":
                    var leaves=steps.Where(s=>s.recipients.Count==0).ToList();
                    actual=leaves.Count<2 ? 0 : leaves.Max(s=>s.after.stats.panic)-leaves.Min(s=>s.after.stats.panic); break;
                default: throw new ArgumentException("Objetivo desconocido: "+o.kind);
            }
            return new ConditionResult {label=o.label,actual=actual,threshold=o.value,passed=upper?actual<=o.value:actual>=o.value};
        }
        public static Evaluation Evaluate(Encounter e,SimulationResult result,int edgeCount)
        {
            var answer=new Evaluation {conditions=e.required.Select(o=>Check(o,result,e.residents.Count)).ToList()};
            answer.won=answer.conditions.All(c=>c.passed);
            if(answer.won) answer.stars=1+(edgeCount<=e.efficientEdges?1:0)+(e.secondary.Count>0 && e.secondary.All(o=>Check(o,result,e.residents.Count).passed)?1:0);
            return answer;
        }
    }
}
