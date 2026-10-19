using System;
using System.Collections.Generic;
using System.Linq;
using Rumores.Core.Party;

namespace Rumores.Tests
{
    public static class PartyChecks
    {
        static void Check(bool value,string message="Contrato party incumplido") {if(!value)throw new Exception(message);}
        static void Rejected(Action action) {try{action();}catch(ArgumentException){return;}catch(InvalidOperationException){return;}throw new Exception("Se aceptó una operación inválida");}
        public static List<PartyPlayer> Roster(int count) {return Enumerable.Range(0,count).Select(i=>new PartyPlayer {name="Vecino "+(i+1),avatar=i%8}).ToList();}
        static PartyGame New(PartyCatalog c,int count=2,int seed=0) {return new PartyGame(c,Roster(count),seed);}
        static PartyChange Play(PartyGame g,string action,string branch="A",bool split=false)
        {g.Ready(g.Token);var change=g.Confirm(g.Token,action,branch,split);g.Next(g.Token);return change;}
        static void Finish(PartyGame g) {while(g.Phase!=PartyPhase.Finished)Play(g,"difundir");}
        public static Dictionary<string,Action> Cases(PartyCatalog c)
        {
            return new Dictionary<string,Action> {
                ["Party: catálogo completo y plantillas resueltas"]=()=>{
                    Check(c.Validate().Count==0,string.Join(";",c.Validate()));
                    for(int seed=0;seed<c.news.Count*3;seed++)foreach(var action in c.actions){var g=New(c,2,seed);var change=Play(g,action.id);Check(!change.text.Contains("{")&&!change.text.Contains("de el ")&&!change.text.Contains("a el ")&&change.text.Length<240);Check(!string.IsNullOrWhiteSpace(change.reason));}
                },
                ["Party: ronda completa con 2 y 8, orden y dos turnos por jugador"]=()=>{
                    foreach(int count in new[]{2,8}){var g=New(c,count);for(int t=0;t<count*2;t++){Check(g.Current.id=="player-"+(t%count));Check(g.Turn==t);Play(g,c.actions[t%c.actions.Count].id);}Check(g.Phase==PartyPhase.Finished);Check(g.Players.All(p=>p.usedActions.Count==2));Check(g.Winners().Count>=1);}
                },
                ["Party: cuatro personalidades alteran datos y narración de forma distinta"]=()=>{
                    var fear=Play(New(c),"panico");var exaggeration=Play(New(c),"exagerar");var lie=Play(New(c),"mentir");var confusion=Play(New(c),"confundir");
                    Check(fear.after.panic>exaggeration.after.panic);Check(exaggeration.after.importance>fear.after.importance);Check(lie.after.credibility<confusion.after.credibility);Check(lie.claimKind=="invención"&&confusion.claimKind=="confusión");Check(new[]{fear.text,exaggeration.text,lie.text,confusion.text}.Distinct().Count()==4);
                },
                ["Party: confusión persiste en la siguiente transformación"]=()=>{var g=New(c);Play(g,"confundir");var subject=g.Branches[0].rumor.subject;Check(subject!=g.News.subject);var next=Play(g,"panico");Check(next.text.Contains(subject));},
                ["Party: calmar y difundir tienen efectos distintos"]=()=>{var a=Play(New(c),"calmar");var b=Play(New(c),"difundir");Check(a.after.panic<a.before.panic&&a.after.credibility>a.before.credibility);Check(b.after.reach-b.before.reach==4);},
                ["Party: mismo catálogo y semilla producen mismo resultado"]=()=>{var a=New(c,8,-42);var b=New(c,8,-42);for(int i=0;i<16;i++){string id=c.actions[i%c.actions.Count].id;var x=Play(a,id);var y=Play(b,id);Check(x.text==y.text&&x.points==y.points&&x.after.ToString()==y.after.ToString());}},
                ["Party: bifurcación conserva prefijo y evolución independiente"]=()=>{
                    var g=New(c);Play(g,"mentir","A",true);Check(g.Branches.Count==2);var a=g.Branches[0].rumor;var b=g.Branches[1].rumor;
                    Check(a.history[0].text==b.history[0].text&&!ReferenceEquals(a.history[0],b.history[0]));Check(!ReferenceEquals(a.values,b.values));
                    var oldB=b.headline;Play(g,"confundir","A");Check(g.Branches[1].rumor.headline==oldB&&g.Branches[1].rumor.history.Count==1);
                    var oldA=g.Branches[0].rumor.headline;Play(g,"calmar","B");Check(g.Branches[0].rumor.headline==oldA);Check(g.Branches[0].rumor.subject!=g.Branches[1].rumor.subject);
                },
                ["Party: copias profundas incluyen valores de eventos"]=()=>{var g=New(c);Play(g,"panico","A",true);g.Branches[1].rumor.history[0].after.panic=99;g.Branches[1].rumor.history[0].text="otro";Check(g.Branches[0].rumor.history[0].after.panic!=99&&g.Branches[0].rumor.history[0].text!="otro");},
                ["Party: bifurcación no duplica puntos ni turnos"]=()=>{var a=New(c);var b=New(c);var x=Play(a,"mentir","A",true);var y=Play(b,"mentir");Check(x.points==y.points&&a.Players[0].score==b.Players[0].score&&a.Turn==b.Turn);},
                ["Party: segunda división y división final rechazadas sin efectos"]=()=>{
                    var g=New(c);Play(g,"mentir","A",true);g.Ready(g.Token);int score=g.Current.score;Rejected(()=>g.Confirm(g.Token,"panico","A",true));Check(g.Branches.Count==2&&g.Current.score==score&&g.Phase==PartyPhase.Choosing);
                    g=New(c);for(int i=0;i<3;i++)Play(g,"panico");g.Ready(g.Token);Rejected(()=>g.Confirm(g.Token,"panico","A",true));Check(g.Branches.Count==1);
                },
                ["Party: doble confirmación y doble avance rechazados"]=()=>{
                    var g=New(c);string token=g.Token;Rejected(()=>g.Confirm(token,"panico","A",false));g.Ready(token);Rejected(()=>g.Ready(token));g.Confirm(token,"panico","A",false);int score=g.Players[0].score;Rejected(()=>g.Confirm(token,"panico","A",false));Check(g.Players[0].score==score);g.Next(token);Rejected(()=>g.Next(token));g.Ready(g.Token);Rejected(()=>g.Confirm(token,"panico","A",false));Check(g.Turn==1&&g.Phase==PartyPhase.Choosing);
                },
                ["Party: comando inválido no consume turno ni cambia rumor"]=()=>{var g=New(c);g.Ready(g.Token);Rejected(()=>g.Confirm(g.Token,"inexistente","A",false));Rejected(()=>g.Confirm(g.Token,"mentir","Z",false));Check(g.Current.score==0&&g.Branches[0].rumor.history.Count==0&&g.Phase==PartyPhase.Choosing);},
                ["Party: apodos vacíos duplicados límites y normalización"]=()=>{
                    Check(PartyGame.ValidatePlayers(Roster(1)).Count>0&&PartyGame.ValidatePlayers(Roster(9)).Count>0);var roster=Roster(2);roster[0].name=" ";Check(PartyGame.ValidatePlayers(roster).Count>0);roster[0].name=" Ana ";roster[1].name="ANA";Check(PartyGame.ValidatePlayers(roster).Count>0);roster[1].name="Luis";var g=new PartyGame(c,roster,0);Check(g.Players[0].name=="Ana");roster[0].name="cambio";Check(g.Players[0].name=="Ana");roster[0].name=new string('x',17);Check(PartyGame.ValidatePlayers(roster).Count>0);
                },
                ["Party: puntuación verificable aporte variedad y objetivo"]=()=>{var g=New(c);var first=Play(g,"panico");Check(first.points==3&&first.contribution&&first.variety&&first.goal);Play(g,"panico");var again=Play(g,"panico");Check(again.points==1&&!again.contribution&&!again.variety&&again.goal);Check(g.Players[0].score==4);},
                ["Party: otro participante no pierde el punto por una contribución ajena"]=()=>{var g=New(c);var a=Play(g,"exagerar");var b=Play(g,"exagerar");Check(a.contribution&&b.contribution&&a.points==b.points);},
                ["Party: cambiar solo redacción no vuelve a premiar aporte"]=()=>{var g=New(c);var a=Play(g,"exagerar");Play(g,"exagerar");var b=Play(g,"exagerar");Check(a.text!=b.text&&!b.contribution&&!b.variety);},
                ["Party: cada objetivo recompensa una decisión diferente"]=()=>{var panic=New(c,2,0);var trust=New(c,2,1);var reach=New(c,2,2);Check(Play(panic,"panico").goal);Check(Play(trust,"calmar").goal);Check(Play(reach,"difundir").goal);Check(!Play(New(c,2,1),"panico").goal&&!Play(New(c,2,0),"calmar").goal);},
                ["Party: ganador coincide con máximo y empates se comparten"]=()=>{var g=New(c);Rejected(()=>g.Winners());Finish(g);Check(g.Winners().All(p=>p.score==g.Players.Max(x=>x.score)));var tied=New(c);Play(tied,"confundir");Play(tied,"confundir");Play(tied,"calmar");Play(tied,"mentir");Check(tied.Players[0].score==tied.Players[1].score&&tied.Winners().Count==2);},
                ["Party: revancha limpia estado conserva participantes cambia noticia y token"]=()=>{
                    var g=New(c);string oldToken=g.Token,oldNews=g.NewsId;Rejected(()=>g.Rematch());Play(g,"mentir","A",true);Finish(g);g.Rematch();Check(g.Turn==0&&g.Phase==PartyPhase.Handoff&&g.Token!=oldToken&&g.NewsId!=oldNews);Check(g.Branches.Count==1&&g.Branches[0].rumor.history.Count==0&&g.LastChange==null);Check(g.Players.All(p=>p.score==0&&p.usedActions.Count==0));Check(g.Players[1].name=="Vecino 2"&&g.Players[1].avatar==1);g.Ready(g.Token);Rejected(()=>g.Confirm(oldToken,"panico","A",false));
                },
                ["Party: límites numéricos incluso con repetición extrema"]=()=>{
                    foreach(var action in c.actions){var g=New(c);var rumor=g.Branches[0].rumor.Copy();for(int t=0;t<100;t++){var e=PartyTransform.Apply(c,rumor,action.id,0,t,"p","A");rumor.history.Add(e);Check(rumor.values.panic>=0&&rumor.values.panic<=100&&rumor.values.importance>=0&&rumor.values.importance<=100&&rumor.values.credibility>=0&&rumor.values.credibility<=100&&rumor.values.reach>=1&&rumor.values.reach<=20);}}
                },
                ["Party: 432 rondas combinadas y auditoría de puntos por evento"]=()=>{
                    for(int seed=0;seed<36;seed++)foreach(int count in new[]{2,8})for(int strategy=0;strategy<6;strategy++){
                        var g=New(c,count,seed);var events=new List<PartyChange>();while(g.Phase!=PartyPhase.Finished){int t=g.Turn;events.Add(Play(g,c.actions[(t+strategy)%6].id,t>0&&t%2==1?"B":"A",t==0));}
                        foreach(var p in g.Players)Check(p.score==events.Where(e=>e.playerId==p.id).Sum(e=>e.points)&&p.score<=6);
                        Check(events.Count==count*2&&g.Branches.Count==2);Check(g.Branches.All(b=>b.rumor.history.Select(h=>h.turn).Distinct().Count()==b.rumor.history.Count));
                    }
                }
            };
        }
    }
}
