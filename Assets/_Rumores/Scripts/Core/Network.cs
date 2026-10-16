using System;
using System.Collections.Generic;
using System.Linq;

namespace Rumores.Core
{
    public static class Network
    {
        public static List<string> Validate(Encounter e,IList<Edge> edges,Bonus bonus,bool requireTransmission=true)
        {
            var errors=new List<string>();
            var ids=new HashSet<string>(e.residents.Select(p=>p.id),StringComparer.Ordinal);
            if(ids.Count!=e.residents.Count) errors.Add("Hay habitantes con identificadores duplicados.");
            if(!ids.Contains(e.source)) errors.Add("Falta el habitante de origen.");
            if(requireTransmission && edges.Count==0) errors.Add("Creá al menos una transmisión desde el origen.");
            if(edges.Count>e.budget+bonus.whispers) errors.Add("No alcanzan los Puntos de Susurro.");
            var keys=new HashSet<string>(StringComparer.Ordinal);
            foreach(var edge in edges)
            {
                if(edge==null || !ids.Contains(edge.from ?? "") || !ids.Contains(edge.to ?? "")) { errors.Add("La conexión usa un habitante inexistente."); continue; }
                if(!keys.Add(edge.ToString())) errors.Add("La conexión está repetida.");
                if(edge.from==edge.to) errors.Add("Un habitante no puede contarse a sí mismo.");
                if(!e.potential.Any(p=>p.from==edge.from && p.to==edge.to)) errors.Add("Esa relación social no está disponible: "+edge+".");
                if(edge.to==e.source) errors.Add("El origen no puede recibir conexiones.");
            }
            var valid=edges.Where(x=>x!=null && ids.Contains(x.from ?? "") && ids.Contains(x.to ?? "")).ToList();
            foreach(var id in ids)
            {
                if(valid.Count(x=>x.to==id)>1) errors.Add("Cada destinatario solo puede tener una fuente: "+id+".");
                int max=2+(e.Person(id).personality=="exagerado" ? bonus.exaggeratorOutputs : 0);
                if(valid.Count(x=>x.from==id)>max) errors.Add("Demasiadas salidas desde "+id+" (máximo "+max+").");
            }
            var colors=new Dictionary<string,int>();
            foreach(var id in ids) if(Cycle(id,valid,colors)) { errors.Add("La red contiene un ciclo."); break; }
            var reachable=new HashSet<string>(StringComparer.Ordinal) {e.source};
            var queue=new Queue<string>(); queue.Enqueue(e.source);
            while(queue.Count>0)
            {
                var id=queue.Dequeue();
                foreach(var edge in valid.Where(x=>x.from==id)) if(reachable.Add(edge.to)) queue.Enqueue(edge.to);
            }
            if(valid.Any(x=>!reachable.Contains(x.from)||!reachable.Contains(x.to))) errors.Add("Todos los enlaces deben estar conectados al origen.");
            return errors.Distinct().ToList();
        }
        static bool Cycle(string id,List<Edge> edges,Dictionary<string,int> colors)
        {
            int color; if(colors.TryGetValue(id,out color)) return color==1;
            colors[id]=1;
            foreach(var edge in edges.Where(x=>x.from==id)) if(Cycle(edge.to,edges,colors)) return true;
            colors[id]=2; return false;
        }
        public static List<Edge> RemoveBranch(IList<Edge> edges,string from,string to)
        {
            var removed=new HashSet<string>(StringComparer.Ordinal){to};
            bool changed;
            do { changed=false; foreach(var edge in edges) if(removed.Contains(edge.from) && removed.Add(edge.to)) changed=true; } while(changed);
            return edges.Where(e=>!(e.from==from&&e.to==to)&&!removed.Contains(e.from)).Select(e=>e.Copy()).ToList();
        }
    }
}
