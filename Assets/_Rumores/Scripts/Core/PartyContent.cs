using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rumores.Core.Party
{
    [Serializable] public sealed class PartyValues
    {
        public int panic=15,importance=20,credibility=85,reach=1;
        public PartyValues Copy() {return new PartyValues {panic=panic,importance=importance,credibility=credibility,reach=reach};}
        public void Clamp() {panic=Stats.Limit(panic);importance=Stats.Limit(importance);credibility=Stats.Limit(credibility);reach=Math.Max(1,Math.Min(20,reach));}
        public override string ToString() {return "Pánico "+panic+" · Importancia "+importance+" · Credibilidad "+credibility+" · "+reach+" vecinos";}
    }
    [Serializable] public sealed class PartyNews
    {
        public string id,original,subject,place,threat,consequence,lie;
        public List<string> mistakes=new List<string>();
        public PartyValues initial=new PartyValues();
    }
    [Serializable] public sealed class PartyAction
    {
        public string id,title,description,personality,kind;
        public int panic,importance,credibility,reach;
        public List<string> templates=new List<string>();
    }
    [Serializable] public sealed class PartyGoal
    {
        public string id,title,description,metric;
        public int minimum;
        public bool Met(PartyValues before,PartyValues after)
        {
            switch(metric)
            {
                case "panic":return after.panic-before.panic>=minimum;
                case "credibility":return after.credibility>=minimum&&after.credibility>=before.credibility;
                case "reach":return after.reach-before.reach>=minimum;
                default:throw new InvalidOperationException("Objetivo desconocido: "+metric);
            }
        }
    }
    [Serializable] public sealed class PartyCatalog
    {
        public int version=1,turnsPerPlayer=2;
        public List<PartyNews> news=new List<PartyNews>();
        public List<PartyAction> actions=new List<PartyAction>();
        public List<PartyGoal> goals=new List<PartyGoal>();
        public PartyNews News(string id) {return news.Find(n=>n.id==id)??throw new ArgumentException("Noticia desconocida.");}
        public PartyAction Action(string id) {return actions.Find(a=>a.id==id)??throw new ArgumentException("Acción desconocida.");}
        public PartyGoal Goal(string id) {return goals.Find(g=>g.id==id)??throw new ArgumentException("Objetivo desconocido.");}
        public List<string> Validate()
        {
            var errors=new List<string>();
            if(version!=1||turnsPerPlayer!=2)errors.Add("Versión o número de turnos no soportados.");
            if(news==null||actions==null||goals==null){errors.Add("Faltan colecciones del catálogo.");return errors;}
            if(news.Count<5||actions.Count<4||actions.Count>6||goals.Count<1)errors.Add("Se necesitan al menos 5 noticias, entre 4 y 6 acciones y al menos un objetivo.");
            if(news.Any(n=>n==null)||actions.Any(a=>a==null)||goals.Any(g=>g==null)){errors.Add("Contenido vacío.");return errors;}
            if(news.Select(n=>n.id).Distinct().Count()!=news.Count||actions.Select(a=>a.id).Distinct().Count()!=actions.Count||goals.Select(g=>g.id).Distinct().Count()!=goals.Count)errors.Add("IDs duplicados.");
            foreach(var n in news)
            {
                if(string.IsNullOrWhiteSpace(n.id)||string.IsNullOrWhiteSpace(n.original)||string.IsNullOrWhiteSpace(n.subject)||string.IsNullOrWhiteSpace(n.place)||string.IsNullOrWhiteSpace(n.threat)||string.IsNullOrWhiteSpace(n.consequence)||string.IsNullOrWhiteSpace(n.lie)||n.initial==null||n.mistakes==null||n.mistakes.Count==0||n.mistakes.Any(string.IsNullOrWhiteSpace))errors.Add("Noticia incompleta: "+n.id);
                if(n.initial!=null&&(n.initial.panic<0||n.initial.panic>100||n.initial.importance<0||n.initial.importance>100||n.initial.credibility<0||n.initial.credibility>100||n.initial.reach<1||n.initial.reach>20))errors.Add("Valores iniciales fuera de rango: "+n.id);
            }
            if(new[]{"fear","exaggerate","lie","misread"}.Any(kind=>!actions.Any(a=>a.kind==kind)))errors.Add("Falta una de las cuatro personalidades.");
            foreach(var a in actions)
            {
                if(string.IsNullOrWhiteSpace(a.id)||string.IsNullOrWhiteSpace(a.title)||string.IsNullOrWhiteSpace(a.description)||a.templates==null||a.templates.Count==0||a.templates.Any(string.IsNullOrWhiteSpace))errors.Add("Acción incompleta: "+a.id);
                if(!new[]{"fear","exaggerate","lie","misread","calm","spread"}.Contains(a.kind))errors.Add("Transformación desconocida: "+a.id);
                if(Math.Abs((long)a.panic)>100||Math.Abs((long)a.importance)>100||Math.Abs((long)a.credibility)>100||Math.Abs((long)a.reach)>20)errors.Add("Efecto fuera de rango: "+a.id);
                if(a.panic==0&&a.importance==0&&a.credibility==0&&a.reach==0)errors.Add("Acción sin efecto: "+a.id);
            }
            foreach(var g in goals)if(string.IsNullOrWhiteSpace(g.id)||string.IsNullOrWhiteSpace(g.title)||string.IsNullOrWhiteSpace(g.description)||!new[]{"panic","credibility","reach"}.Contains(g.metric)||g.minimum<0)errors.Add("Objetivo inválido: "+g.id);
            return errors;
        }
    }
    [Serializable] public sealed class PartyChange
    {
        public int turn,points;
        public string playerId,actionId,branchId,text,reason,detailKey,claimKind;
        public bool contribution,variety,goal,split;
        public PartyValues before,after;
        public PartyChange Copy() {return new PartyChange {turn=turn,points=points,playerId=playerId,actionId=actionId,branchId=branchId,text=text,reason=reason,detailKey=detailKey,claimKind=claimKind,contribution=contribution,variety=variety,goal=goal,split=split,before=before.Copy(),after=after.Copy()};}
        public string ScoreExplanation {get {return (contribution?"+1 aporte nuevo":"+0 detalle repetido")+" · "+(variety?"+1 acción diferente":"+0 acción repetida")+" · "+(goal?"+1 objetivo":"+0 objetivo");}}
    }
    [Serializable] public sealed class PartyRumor
    {
        public string newsId,original,headline,subject;
        public PartyValues values=new PartyValues();
        public List<PartyChange> history=new List<PartyChange>();
        public PartyRumor Copy() {return new PartyRumor {newsId=newsId,original=original,headline=headline,subject=subject,values=values.Copy(),history=history.Select(c=>c.Copy()).ToList()};}
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
            var before=rumor.values.Copy();var after=before.Copy();
            after.panic+=a.panic;after.importance+=a.importance;after.credibility+=a.credibility;after.reach+=a.reach;after.Clamp();
            string key=actionId+":"+subject; // Cosmetic wording variants are not new contributions.
            var change=new PartyChange {turn=turn,playerId=playerId,actionId=actionId,branchId=branchId,text=text,detailKey=key,
                claimKind=a.kind=="lie"?"invención":a.kind=="misread"?"confusión":a.kind=="calm"?"rectificación":"interpretación",before=before,after=after,
                contribution=!rumor.history.Any(h=>h.playerId==playerId&&h.detailKey==key),reason=a.description+" "+(a.kind=="misread"?"Confundió «"+rumor.subject+"» con «"+subject+"».":a.kind=="lie"?"El detalle añadido es inventado.":"")};
            rumor.subject=subject;rumor.values=after.Copy();rumor.headline=text;
            return change;
        }
        public static string Reaction(PartyValues v)
        {
            if(v.panic>=65)return "¡Sálvese quien pueda! El pueblo se esconde detrás de una maceta.";
            if(v.credibility<=40)return "Nadie lo cree, pero ya están vendiendo camisetas del chisme.";
            if(v.importance>=65)return "Hasta las palomas exigen una asamblea extraordinaria.";
            if(v.reach>=10)return "El chisme ya aparece en el menú de la panadería.";
            return "Tres vecinos asienten muy serios. Una gallina pide más detalles.";
        }
    }
}
