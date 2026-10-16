using System;
using System.Collections.Generic;
using System.Linq;

namespace Rumores.Core
{
    public static class CatalogValidator
    {
        public static List<string> Validate(Catalog c)
        {
            var errors=new List<string>();
            if(c.rulesVersion!=1) errors.Add("Versión de reglas no soportada.");
            if(c.encounters.Count(e=>!e.boss)<5 || c.encounters.Count(e=>e.boss)!=1) errors.Add("Se necesitan cinco encuentros y un jefe.");
            if(c.encounters.Select(e=>e.id).Distinct().Count()!=c.encounters.Count) errors.Add("IDs de encuentros repetidos.");
            if(c.news.Select(e=>e.id).Distinct().Count()!=c.news.Count) errors.Add("IDs de noticias repetidos.");
            if(c.rewards.Count<3 || c.rewards.Select(e=>e.id).Distinct().Count()!=c.rewards.Count) errors.Add("Catálogo de recompensas inválido.");
            foreach(var e in c.encounters)
            {
                try
                {
                    if(e.residents.Count<8||e.residents.Count>12) errors.Add(e.id+": debe tener 8–12 habitantes.");
                    foreach(var p in e.residents) c.Personality(p.personality);
                    if(e.required.Count==0) errors.Add(e.id+": faltan objetivos.");
                    errors.AddRange(Network.Validate(e,e.solution,new Bonus()).Select(s=>e.id+": "+s));
                    foreach(var news in e.newsOptions.Concat(new[]{e.newsId}).Distinct())
                    {
                        var result=Simulator.Run(c,e,e.solution,new Bonus(),news);
                        var evaluation=Objectives.Evaluate(e,result,e.solution.Count);
                        if(!evaluation.won) errors.Add(e.id+" / "+news+": la solución testigo falla: "+string.Join(", ",evaluation.conditions.Where(x=>!x.passed).Select(x=>x.label+" ("+x.actual+")")));
                    }
                }
                catch(Exception ex) {errors.Add(e.id+": "+ex.Message);}
            }
            return errors;
        }
    }
}
