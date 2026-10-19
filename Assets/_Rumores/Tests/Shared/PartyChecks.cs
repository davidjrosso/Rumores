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
        public static List<PartyPlayer> Roster(int count) {return Enumerable.Range(0,count).Select(i=>new PartyPlayer {name="Amigo "+(i+1),avatar=i%8}).ToList();}
        static PartyGame New(PartyCatalog c,int count=2,int seed=0) {return new PartyGame(c,Roster(count),seed);}
        static PartyChange Play(PartyGame g,string action,string branch="A",bool split=false)
        {g.Ready(g.Token);var change=g.Confirm(g.Token,action,branch,split);g.Next(g.Token);return change;}
        static void Finish(PartyGame g) {while(g.Phase!=PartyPhase.Finished)Play(g,"mentir");}
        public static Dictionary<string,Action> Cases(PartyCatalog c)
        {
            return new Dictionary<string,Action> {
                ["Party: noticias argentinas con preguntas y textos breves"]=()=>{
                    Check(c.Validate().Count==0,string.Join(";",c.Validate()));Check(c.news.Count>=20);
                    foreach(var n in c.news){Check(n.original.Length<=130&&n.question.EndsWith("?")&&n.icebreaker.EndsWith("?"));}
                    for(int seed=0;seed<c.news.Count*3;seed++)foreach(var action in c.actions){var change=Play(New(c,2,seed),action.id);Check(!change.text.Contains("{")&&!System.Text.RegularExpressions.Regex.IsMatch(change.text,@"\bde el\b")&&change.text.Length<=240,change.text);}
                },
                ["Party: ronda completa con 2 y 8 en orden y sin división"]=()=>{
                    foreach(int count in new[]{2,8}){var g=New(c,count);for(int t=0;t<count*2;t++){Check(g.Current.id=="player-"+(t%count));Play(g,c.actions[t%4].id);}Check(g.Phase==PartyPhase.Finished&&g.Branches.Count==1);foreach(var p in g.Players)Check(g.Branches[0].rumor.history.Count(h=>h.playerId==p.id)==2);}
                },
                ["Party: cuatro estilos narrativos diferentes"]=()=>{
                    var changes=c.actions.Select(a=>Play(New(c),a.id)).ToList();Check(changes.Select(x=>x.claimKind).Distinct().Count()==4);Check(changes.Select(x=>x.text).Distinct().Count()==4);Check(changes.All(x=>!string.IsNullOrWhiteSpace(x.reason)));
                },
                ["Party: confusión persiste en los textos siguientes"]=()=>{var g=New(c);Play(g,"confundir");var subject=g.Branches[0].rumor.subject;Check(subject!=g.News.subject);Check(Play(g,"panico").text.Contains(subject));},
                ["Party: tres opciones distintas y rotación de estilos"]=()=>{var g=New(c);var seen=new HashSet<string>();for(int t=0;t<4;t++){g.Ready(g.Token);var choices=g.Choices();Check(choices.Count==3&&choices.Select(a=>a.id).Distinct().Count()==3);foreach(var a in choices)seen.Add(a.id);g.Confirm(g.Token,choices[0].id);g.Next(g.Token);}Check(seen.Count==4);},
                ["Party: la vista previa no consume turno ni modifica la historia"]=()=>{var g=New(c);g.Ready(g.Token);string original=g.Branches[0].rumor.headline;for(int i=0;i<3;i++)foreach(var a in g.Choices())g.Preview(a.id);Check(g.Turn==0&&g.Phase==PartyPhase.Choosing&&g.LastChange==null&&g.Branches[0].rumor.headline==original&&g.Branches[0].rumor.history.Count==0);},
                ["Party: confirmar produce exactamente la respuesta elegida"]=()=>{for(int seed=0;seed<60;seed++){var g=New(c,2,seed);while(g.Phase!=PartyPhase.Finished){g.Ready(g.Token);string id=g.Choices()[g.Turn%3].id;string preview=g.Preview(id).text;Check(g.Confirm(g.Token,id).text==preview);g.Next(g.Token);}}},
                ["Party: opciones y vista previa solo durante elección"]=()=>{var g=New(c);Rejected(()=>g.Choices());Rejected(()=>g.Preview("mentir"));Finish(g);Rejected(()=>g.Choices());Rejected(()=>g.Preview("mentir"));},
                ["Party: misma semilla y respuestas producen misma historia"]=()=>{foreach(int seed in new[]{0,-42,int.MinValue,int.MaxValue}){var a=New(c,8,seed);var b=New(c,8,seed);for(int i=0;i<16;i++)Check(Play(a,c.actions[i%4].id).text==Play(b,c.actions[i%4].id).text);}},
                ["Party: bifurcación interna conserva prefijo y copias independientes"]=()=>{
                    var g=New(c);Play(g,"mentir","A",true);var a=g.Branches[0].rumor;var b=g.Branches[1].rumor;Check(a.history[0].text==b.history[0].text&&!ReferenceEquals(a.history[0],b.history[0]));
                    string oldB=b.headline;Play(g,"confundir","A");Check(g.Branches[1].rumor.headline==oldB&&b.history.Count==1);string oldA=g.Branches[0].rumor.headline;Play(g,"panico","B");Check(g.Branches[0].rumor.headline==oldA);
                },
                ["Party: historial y cambio devuelto no comparten referencias"]=()=>{var g=New(c);var change=Play(g,"mentir","A",true);change.text="externo";g.LastChange.text="otra copia";g.Branches[1].rumor.history[0].text="otro";Check(g.Branches[0].rumor.history[0].text!="otro"&&g.Branches[0].rumor.history[0].text!="externo"&&g.Branches[0].rumor.history[0].text!="otra copia");},
                ["Party: segunda división y división final rechazadas sin cambios"]=()=>{var g=New(c);Play(g,"mentir","A",true);g.Ready(g.Token);Rejected(()=>g.Confirm(g.Token,"panico","A",true));Check(g.Branches.Count==2&&g.Branches[0].rumor.history.Count==1);g=New(c);for(int i=0;i<3;i++)Play(g,"panico");g.Ready(g.Token);Rejected(()=>g.Confirm(g.Token,"panico","A",true));Check(g.Branches.Count==1);},
                ["Party: doble confirmación y doble avance rechazados"]=()=>{
                    var g=New(c);string token=g.Token;Rejected(()=>g.Confirm(token,"panico"));g.Ready(token);Rejected(()=>g.Ready(token));g.Confirm(token,"panico");Rejected(()=>g.Confirm(token,"panico"));Check(g.Branches[0].rumor.history.Count==1);g.Next(token);Rejected(()=>g.Next(token));g.Ready(g.Token);Rejected(()=>g.Confirm(token,"panico"));Check(g.Turn==1);
                },
                ["Party: comandos inválidos no consumen turno"]=()=>{var g=New(c);g.Ready(g.Token);Rejected(()=>g.Preview("inexistente"));Rejected(()=>g.Confirm(g.Token,"inexistente"));Rejected(()=>g.Confirm(g.Token,"mentir","Z",false));Check(g.Branches[0].rumor.history.Count==0&&g.Phase==PartyPhase.Choosing);},
                ["Party: apodos vacíos duplicados y largos se pueden corregir"]=()=>{
                    Check(PartyGame.ValidatePlayers(Roster(1)).Count>0&&PartyGame.ValidatePlayers(Roster(9)).Count>0);var roster=Roster(2);roster[0].name=" ";Check(PartyGame.ValidatePlayers(roster).Count>0);roster[0].name=" Ana ";roster[1].name="ANA";Check(PartyGame.ValidatePlayers(roster).Count>0);roster[1].name="Luis";var g=new PartyGame(c,roster,0);Check(g.Players[0].name=="Ana");roster[0].name="cambio";Check(g.Players[0].name=="Ana");roster[0].name=new string('x',17);Check(PartyGame.ValidatePlayers(roster).Count>0);
                },
                ["Party: revancha limpia historia conserva amigos y cambia noticia"]=()=>{
                    var g=New(c);string token=g.Token,news=g.NewsId;Rejected(()=>g.Rematch());Finish(g);g.Rematch();Check(g.Turn==0&&g.Phase==PartyPhase.Handoff&&g.Token!=token&&g.NewsId!=news);Check(g.Branches.Count==1&&g.Branches[0].rumor.history.Count==0&&g.LastChange==null);Check(g.Players[1].name=="Amigo 2"&&g.Players[1].avatar==1);g.Ready(g.Token);Rejected(()=>g.Confirm(token,"panico"));
                },
                ["Party: catálogo incompleto se rechaza antes de jugar"]=()=>{string question=c.news[0].question;try{c.news[0].question="";Check(c.Validate().Count>0);Rejected(()=>New(c));}finally{c.news[0].question=question;}},
                ["Party: 360 rondas con opciones reales y una sola historia"]=()=>{
                    for(int seed=0;seed<60;seed++)foreach(int count in new[]{2,8})for(int option=0;option<3;option++){
                        var g=New(c,count,seed);while(g.Phase!=PartyPhase.Finished){g.Ready(g.Token);g.Confirm(g.Token,g.Choices()[option].id);g.Next(g.Token);}
                        Check(g.Branches.Count==1&&g.Branches[0].rumor.history.Count==count*2);Check(g.Branches[0].rumor.original==g.News.original);Check(g.Branches[0].rumor.headline==g.Branches[0].rumor.history.Last().text);
                    }
                }
            };
        }
    }
}
