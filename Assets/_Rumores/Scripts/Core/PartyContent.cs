using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rumores.Core.Party
{
    [Serializable] public sealed class PartyNews
    {
        public string id,original,subject,place,threat,consequence,lie,question,icebreaker;
        public List<string> mistakes=new List<string>();
    }
    [Serializable] public sealed class PartyAction
    {
        public string id,title,description,personality,kind;
        public List<string> templates=new List<string>();
    }
    [Serializable] public sealed class PartyCatalog
    {
        public int version=2,turnsPerPlayer=2;
        public List<PartyNews> news=new List<PartyNews>();
        public List<PartyAction> actions=new List<PartyAction>();
        public PartyNews News(string id) {return news.Find(n=>n.id==id)??throw new ArgumentException("Noticia desconocida.");}
        public PartyAction Action(string id) {return actions.Find(a=>a.id==id)??throw new ArgumentException("Acción desconocida.");}
        public List<string> Validate()
        {
            var errors=new List<string>();
            if(version!=2||turnsPerPlayer!=2)errors.Add("Versión o número de turnos no soportados.");
            if(news==null||actions==null){errors.Add("Faltan colecciones del catálogo.");return errors;}
            if(news.Count<5||actions.Count!=4)errors.Add("Se necesitan al menos 5 noticias y cuatro maneras de contar el chisme.");
            if(news.Any(n=>n==null)||actions.Any(a=>a==null)){errors.Add("Contenido vacío.");return errors;}
            if(news.Select(n=>n.id).Distinct().Count()!=news.Count||actions.Select(a=>a.id).Distinct().Count()!=actions.Count)errors.Add("IDs duplicados.");
            foreach(var n in news)
            {
                if(string.IsNullOrWhiteSpace(n.id)||string.IsNullOrWhiteSpace(n.original)||string.IsNullOrWhiteSpace(n.subject)||string.IsNullOrWhiteSpace(n.place)||string.IsNullOrWhiteSpace(n.threat)||string.IsNullOrWhiteSpace(n.consequence)||string.IsNullOrWhiteSpace(n.lie)||string.IsNullOrWhiteSpace(n.question)||string.IsNullOrWhiteSpace(n.icebreaker)||n.mistakes==null||n.mistakes.Count==0||n.mistakes.Any(string.IsNullOrWhiteSpace))errors.Add("Noticia incompleta: "+n.id);
            }
            if(new[]{"fear","exaggerate","lie","misread"}.Any(kind=>!actions.Any(a=>a.kind==kind)))errors.Add("Falta una de las cuatro personalidades.");
            foreach(var a in actions)
            {
                if(string.IsNullOrWhiteSpace(a.id)||string.IsNullOrWhiteSpace(a.title)||string.IsNullOrWhiteSpace(a.description)||a.templates==null||a.templates.Count==0||a.templates.Any(string.IsNullOrWhiteSpace))errors.Add("Acción incompleta: "+a.id);
                if(!new[]{"fear","exaggerate","lie","misread"}.Contains(a.kind))errors.Add("Transformación desconocida: "+a.id);
            }
            return errors;
        }
    }
    [Serializable] public sealed class PartyChange
    {
        public int turn;
        public string playerId,actionId,branchId,text,reason,claimKind;
        public bool split;
        public PartyChange Copy() {return new PartyChange {turn=turn,playerId=playerId,actionId=actionId,branchId=branchId,text=text,reason=reason,claimKind=claimKind,split=split};}
    }
    [Serializable] public sealed class PartyRumor
    {
        public string newsId,original,headline,subject;
        public List<PartyChange> history=new List<PartyChange>();
        public PartyRumor Copy() {return new PartyRumor {newsId=newsId,original=original,headline=headline,subject=subject,history=history.Select(c=>c.Copy()).ToList()};}
    }
    public static class PartyTransform
    {
        public static PartyChange Apply(PartyCatalog catalog,PartyRumor rumor,string actionId,int seed,int turn,string playerId,string branchId)
        {
            var n=catalog.News(rumor.newsId);var a=catalog.Action(actionId);
            int variant=(int)((unchecked((uint)seed)+(uint)turn+(uint)catalog.actions.IndexOf(a))%(uint)a.templates.Count);
            string subject=rumor.subject;
            if(a.kind=="misread")
            {
                int count=rumor.history.Count(h=>h.actionId==a.id);
                subject=n.mistakes[count%n.mistakes.Count];
            }
            string text=a.templates[variant].Replace("{subject}",subject).Replace("{place}",n.place).Replace("{threat}",n.threat).Replace("{consequence}",n.consequence).Replace("{lie}",n.lie);
            text=Regex.Replace(Regex.Replace(text,@"\bde el\b","del"),@"\ba el\b","al");
            var change=new PartyChange {turn=turn,playerId=playerId,actionId=actionId,branchId=branchId,text=text,
                claimKind=a.kind=="lie"?"invención":a.kind=="misread"?"confusión":a.kind=="fear"?"susto":"exageración",
                reason=a.kind=="misread"?"Escuchó «"+subject+"» en lugar de «"+rumor.subject+"».":a.description};
            rumor.subject=subject;rumor.headline=text;
            return change;
        }
    }
}
