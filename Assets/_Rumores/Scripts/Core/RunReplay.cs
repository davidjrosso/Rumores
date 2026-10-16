using System;
using System.Collections.Generic;
using System.Linq;

namespace Rumores.Core
{
    public static class RunReplay
    {
        // Replays recorded decisions through the same state machine; mismatched results are rejected.
        public static GameSession Play(Catalog catalog,int seed,IList<Decision> log)
        {
            var game=new GameSession(catalog);game.NewRun(seed);
            foreach(var decision in log)
            {
                switch(decision.kind)
                {
                    case "node":game.SelectNode(decision.value);break;
                    case "network":
                        game.Clear();
                        var pending=decision.value.Split(new[]{','},StringSplitOptions.RemoveEmptyEntries).Select(ParseEdge).ToList();
                        var reached=new HashSet<string>{game.Encounter.source};
                        while(pending.Count>0)
                        {
                            var edge=pending.Find(e=>reached.Contains(e.from));
                            if(edge==null)throw new InvalidOperationException("El registro contiene una red desconectada.");
                            var error=game.ToggleEdge(edge.from,edge.to);if(error!=null)throw new InvalidOperationException(error);
                            reached.Add(edge.to);pending.Remove(edge);
                        }
                        game.Simulate();break;
                    case "result":
                        if(game.Run.lastEvaluation==null||(game.Run.lastEvaluation.won?"win":"loss")!=decision.value)throw new InvalidOperationException("El resultado no coincide con el registro.");
                        break;
                    case "retry":game.Retry();break;
                    case "continue":game.AcceptResult();break;
                    case "reward":if(game.Run.phase==Phase.Result)game.AcceptResult();game.ChooseReward(decision.value);break;
                    case "event":if(decision.value!="rest"&&decision.value!="whisper")throw new InvalidOperationException("Evento desconocido.");game.ChooseEvent(decision.value=="rest");break;
                    case "finish":game.AcceptResult();break;
                    default:throw new InvalidOperationException("Decisión desconocida: "+decision.kind);
                }
            }
            if(!game.Run.decisions.Select(d=>d.kind+":"+d.value).SequenceEqual(log.Select(d=>d.kind+":"+d.value)))throw new InvalidOperationException("Las decisiones no coinciden con la versión actual de reglas.");
            return game;
        }
        static Edge ParseEdge(string text)
        {var parts=text.Split('>');if(parts.Length!=2)throw new InvalidOperationException("Formato de conexión inválido.");return new Edge(parts[0],parts[1]);}
    }
}
