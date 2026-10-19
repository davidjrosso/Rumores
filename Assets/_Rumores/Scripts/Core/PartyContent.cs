using System;
using System.Collections.Generic;
using System.Linq;

namespace Rumores.Core.Party
{
    [Serializable] public sealed class PartyOption
    {
        public string id,actionId,text;
    }
    [Serializable] public sealed class PartyNews
    {
        public string id,category,original,anchor,question,icebreaker;
        public List<PartyOption> options=new List<PartyOption>();
    }
    [Serializable] public sealed class PartyAction
    {
        public string id,title,description,kind;
    }
    [Serializable] public sealed class PartyCatalog
    {
        public const int MaxStoryLength=240;
        public int version=3,turnsPerPlayer=2;
        public List<PartyNews> news=new List<PartyNews>();
        public List<PartyAction> actions=new List<PartyAction>();
        public PartyNews News(string id) {return news.Find(n=>n.id==id)??throw new ArgumentException("Noticia desconocida.");}
        public PartyAction Action(string id) {return actions.Find(a=>a.id==id)??throw new ArgumentException("Acción desconocida.");}
        public List<string> Validate()
        {
            var errors=new List<string>();
            if(version!=3||turnsPerPlayer!=2)errors.Add("Versión o número de turnos no soportados.");
            if(news==null||actions==null){errors.Add("Faltan colecciones del catálogo.");return errors;}
            if(news.Count<5||actions.Count!=6)errors.Add("Se necesitan al menos 5 noticias y seis maneras de contar el chisme.");
            if(news.Any(n=>n==null)||actions.Any(a=>a==null)){errors.Add("Contenido vacío.");return errors;}
            if(news.Select(n=>n.id).Distinct().Count()!=news.Count||actions.Select(a=>a.id).Distinct().Count()!=actions.Count)errors.Add("IDs duplicados.");
            string[] kinds={"fear","exaggerate","lie","misread","theory","meme"};
            if(kinds.Any(kind=>!actions.Any(a=>a.kind==kind)))errors.Add("Falta una de las seis maneras de contar.");
            foreach(var a in actions)
                if(string.IsNullOrWhiteSpace(a.id)||string.IsNullOrWhiteSpace(a.title)||string.IsNullOrWhiteSpace(a.description)||!kinds.Contains(a.kind))errors.Add("Acción incompleta: "+a.id);
            var ids=new HashSet<string>();
            foreach(var n in news)
            {
                if(string.IsNullOrWhiteSpace(n.id)||string.IsNullOrWhiteSpace(n.category)||string.IsNullOrWhiteSpace(n.original)||string.IsNullOrWhiteSpace(n.anchor)||string.IsNullOrWhiteSpace(n.question)||string.IsNullOrWhiteSpace(n.icebreaker))errors.Add("Noticia incompleta: "+n.id);
                if(n.options==null||n.options.Any(o=>o==null)){errors.Add("Faltan continuaciones: "+n.id);continue;}
                foreach(var a in actions)if(n.options.Count(o=>o.actionId==a.id)<3)errors.Add("Faltan variantes de "+a.id+" en "+n.id);
                if(n.options.Select(o=>o.text).Distinct().Count()!=n.options.Count)errors.Add("Continuaciones repetidas en "+n.id);
                foreach(var o in n.options)
                {
                    if(string.IsNullOrWhiteSpace(o.id)||!ids.Add(o.id))errors.Add("ID de continuación vacío o duplicado: "+o.id);
                    if(!actions.Any(a=>a.id==o.actionId))errors.Add("Acción desconocida en "+o.id);
                    if(string.IsNullOrWhiteSpace(o.text)||o.text.Contains("{")||o.text.Contains("}"))errors.Add("Texto incompleto en "+o.id);
                }
                // Every pair can be displayed together: validate the worst case before Unity loads it.
                if((n.anchor??"").Length+2+n.options.Select(o=>(o.text??"").Length).OrderByDescending(x=>x).Take(2).Sum()>MaxStoryLength)errors.Add("Continuaciones demasiado largas en "+n.id);
            }
            return errors;
        }
    }
    [Serializable] public sealed class PartyChange
    {
        public int turn;
        public string playerId,actionId,branchId,text,reason,claimKind,detailId,detail,parentDetailId;
        public bool split;
        public PartyChange Copy() {return new PartyChange {turn=turn,playerId=playerId,actionId=actionId,branchId=branchId,text=text,reason=reason,claimKind=claimKind,detailId=detailId,detail=detail,parentDetailId=parentDetailId,split=split};}
    }
    [Serializable] public sealed class PartyRumor
    {
        public string newsId,original,headline;
        public List<PartyChange> history=new List<PartyChange>();
        public PartyRumor Copy() {return new PartyRumor {newsId=newsId,original=original,headline=headline,history=history.Select(c=>c.Copy()).ToList()};}
    }
    public static class PartyTransform
    {
        public static PartyChange Apply(PartyCatalog catalog,PartyRumor rumor,string actionId,int seed,int turn,string playerId,string branchId)
        {
            var n=catalog.News(rumor.newsId);var a=catalog.Action(actionId);
            var options=n.options.Where(o=>o.actionId==a.id).ToList();
            var used=new HashSet<string>(rumor.history.Select(h=>h.detailId));
            var fresh=options.Where(o=>!used.Contains(o.id)).ToList();
            var previous=rumor.history.LastOrDefault();
            // Exhausted styles can repeat, but never repeat the immediately preceding detail.
            if(fresh.Count==0)fresh=options.Where(o=>previous==null||o.id!=previous.detailId).ToList();
            uint cycle=unchecked((uint)seed)/(uint)catalog.news.Count;
            int variant=(int)((cycle+(uint)turn+(uint)catalog.actions.IndexOf(a))%(uint)fresh.Count);
            var option=fresh[variant];
            // Keep the incident and the previous contribution visible, without growing the card forever.
            string text=n.anchor+" "+(previous==null?"":previous.detail+" ")+option.text;
            var change=new PartyChange {turn=turn,playerId=playerId,actionId=actionId,branchId=branchId,text=text,
                claimKind=a.kind,reason=a.description,detailId=option.id,detail=option.text,parentDetailId=previous==null?"":previous.detailId};
            rumor.headline=text;
            return change;
        }
    }
}
