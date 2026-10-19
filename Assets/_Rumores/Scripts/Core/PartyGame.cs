using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rumores.Core.Party
{
    public enum PartyPhase { Handoff, Choosing, Revealing, Finished }
    [Serializable] public sealed class PartyPlayer
    {
        public string id,name;
        public int avatar;
        public PartyPlayer Fresh(int index) {return new PartyPlayer {id="player-"+index,name=name,avatar=avatar};}
    }
    [Serializable] public sealed class PartyBranch {public string id,label;public PartyRumor rumor;}
    public sealed class PartyGame
    {
        readonly PartyCatalog catalog;
        public IReadOnlyList<PartyPlayer> Players {get {return players.AsReadOnly();}}
        public IReadOnlyList<PartyBranch> Branches {get {return branches.AsReadOnly();}}
        readonly List<PartyPlayer> players;
        readonly List<PartyBranch> branches=new List<PartyBranch>();
        public PartyPhase Phase {get;private set;}
        public int Seed {get;private set;}
        public int RoundNumber {get;private set;}
        public int Turn {get;private set;}
        public int TotalTurns {get {return players.Count*catalog.turnsPerPlayer;}}
        public string Token {get {return RoundNumber+":"+Turn;}}
        public string NewsId {get;private set;}
        public PartyNews News {get {return catalog.News(NewsId);}}
        public PartyPlayer Current {get {if(Phase==PartyPhase.Finished)throw new InvalidOperationException("La ronda terminó.");return players[Turn%players.Count];}}
        public PartyChange LastChange {get;private set;}
        public bool CanSplit {get {return branches.Count==1&&Turn<TotalTurns-1;}}
        public static string CleanName(string name) {return Regex.Replace(name??"",@"\s+"," ").Trim();}
        public static List<string> ValidatePlayers(IList<PartyPlayer> roster)
        {
            var errors=new List<string>();
            if(roster==null||roster.Count<2||roster.Count>8)return new List<string>{"Registrá entre 2 y 8 participantes."};
            var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for(int i=0;i<roster.Count;i++)
            {
                if(roster[i]==null){errors.Add("Falta el participante "+(i+1)+".");continue;}
                var name=CleanName(roster[i].name);
                if(name.Length==0)errors.Add("Escribí un apodo para el participante "+(i+1)+".");
                else if(name.Length>16)errors.Add("El apodo "+name+" supera 16 caracteres.");
                else if(!names.Add(name))errors.Add("El apodo «"+name+"» está repetido. Agregale un número o una inicial.");
                if(roster[i].avatar<0||roster[i].avatar>7)errors.Add("Elegí uno de los ocho avatares.");
            }
            return errors;
        }
        public PartyGame(PartyCatalog catalog,IList<PartyPlayer> roster,int seed)
        {
            var errors=catalog.Validate().Concat(ValidatePlayers(roster)).ToList();
            if(errors.Count>0)throw new ArgumentException(string.Join("\n",errors));
            this.catalog=catalog;players=roster.Select((p,i)=>p.Fresh(i)).ToList();
            foreach(var p in players)p.name=CleanName(p.name);
            Seed=seed;RoundNumber=1;Begin();
        }
        void Begin()
        {
            int newsIndex=(int)(unchecked((uint)Seed)%(uint)catalog.news.Count);
            NewsId=catalog.news[newsIndex].id;
            Turn=0;Phase=PartyPhase.Handoff;LastChange=null;
            branches.Clear();branches.Add(new PartyBranch {id="A",label="La plaza",rumor=new PartyRumor {newsId=News.id,original=News.original,headline=News.original}});
        }
        void Require(PartyPhase phase,string token)
        {if(Phase!=phase||token!=Token)throw new InvalidOperationException("Ese turno ya cambió. Esperá la pantalla del jugador actual.");}
        public void Ready(string token) {Require(PartyPhase.Handoff,token);Phase=PartyPhase.Choosing;}
        public IReadOnlyList<PartyAction> Choices(int page=0)
        {
            if(Phase!=PartyPhase.Choosing)throw new InvalidOperationException("Esperá tu turno para elegir.");
            if(page<0||page>1)throw new ArgumentOutOfRangeException(nameof(page));
            uint cycle=unchecked((uint)Seed)/(uint)catalog.news.Count;
            int first=(int)((unchecked((uint)Seed)%(uint)catalog.news.Count+cycle+(uint)Turn)%(uint)catalog.actions.Count);
            return Enumerable.Range(0,3).Select(i=>catalog.actions[(first+page*3+i)%catalog.actions.Count]).ToList().AsReadOnly();
        }
        public PartyChange Preview(string actionId)
        {
            if(Phase!=PartyPhase.Choosing)throw new InvalidOperationException("Esperá tu turno para elegir.");
            return PartyTransform.Apply(catalog,branches[0].rumor.Copy(),actionId,Seed,Turn,Current.id,"A");
        }
        public PartyChange Confirm(string token,string actionId) {return Confirm(token,actionId,"A",false);}
        public PartyChange Confirm(string token,string actionId,string branchId,bool split)
        {
            Require(PartyPhase.Choosing,token);
            var branch=branches.Find(b=>b.id==branchId);if(branch==null)throw new InvalidOperationException("Elegí una de las ramas disponibles.");
            if(split&&!CanSplit)throw new InvalidOperationException("La ronda admite una división y debe quedar otro turno para cambiar las ramas.");
            catalog.Action(actionId); // Validate the command before mutating state.
            var next=branch.rumor.Copy();
            var change=PartyTransform.Apply(catalog,next,actionId,Seed,Turn,Current.id,branchId);
            change.split=split;next.history.Add(change.Copy());branch.rumor=next;
            if(split)branches.Add(new PartyBranch {id="B",label="El mercado",rumor=next.Copy()});
            LastChange=change.Copy();Phase=PartyPhase.Revealing;
            return change.Copy();
        }
        public void Next(string token)
        {
            Require(PartyPhase.Revealing,token);Turn++;
            Phase=Turn==TotalTurns?PartyPhase.Finished:PartyPhase.Handoff;
        }
        public void Rematch()
        {
            if(Phase!=PartyPhase.Finished)throw new InvalidOperationException("Terminá la ronda antes de pedir revancha.");
            RoundNumber++;Seed=unchecked(Seed+1);Begin();
        }
    }
}
