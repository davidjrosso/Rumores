using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Rumores.Core.Party;
using Rumores.Tests;

static class PartyReport
{
    public static string Build(PartyCatalog catalog)
    {
        var report=new StringBuilder("# Validación simulada del party game\n\nGenerado con `dotnet run --project Tests/Rumores.Tests.csproj -c Release -- --party-report Docs/PARTY_VALIDATION.md`. Usa las reglas C# reales; no ejecuta Unity ni evalúa diversión humana.\n\n");
        report.AppendLine("432 rondas: 36 semillas × 2 tamaños (2 y 8) × 6 secuencias. Cada secuencia rota las seis acciones, divide en el primer turno y alterna ramas A/B. Todos juegan exactamente dos veces.").AppendLine();
        report.AppendLine("| Participantes | Rondas | Empates | Puntos medios/persona | Ganadores por asiento (incluye empates) |").AppendLine("|---|---:|---:|---:|---|");
        var headlines=new HashSet<string>();int capped=0;
        foreach(int count in new[]{2,8})
        {
            int rounds=0,ties=0,points=0;var wins=new int[count];
            for(int seed=0;seed<36;seed++)for(int offset=0;offset<6;offset++)
            {
                var game=new PartyGame(catalog,PartyChecks.Roster(count),seed);
                while(game.Phase!=PartyPhase.Finished)
                {
                    int turn=game.Turn;game.Ready(game.Token);game.Confirm(game.Token,catalog.actions[(turn+offset)%6].id,turn%2==1?"B":"A",turn==0);game.Next(game.Token);
                }
                rounds++;points+=game.Players.Sum(p=>p.score);var winners=game.Winners();if(winners.Count>1)ties++;
                for(int i=0;i<count;i++)if(winners.Contains(game.Players[i]))wins[i]++;
                foreach(var b in game.Branches){headlines.Add(b.rumor.headline);if(b.rumor.values.panic==100||b.rumor.values.credibility==0||b.rumor.values.reach==20)capped++;}
            }
            report.AppendLine("| "+count+" | "+rounds+" | "+ties+" | "+((double)points/(rounds*count)).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+" | "+string.Join(", ",wins)+" |");
        }
        report.AppendLine().AppendLine("Titulares finales distintos: "+headlines.Count+". Ramas que terminan con pánico 100, credibilidad 0 o difusión 20: "+capped+" (sobre 864 ramas).");
        report.AppendLine().AppendLine("Estos recorridos son pruebas estructurales, no jugadores óptimos. Los empates son esperables con 0–6 puntos y se muestran como victoria compartida. La novedad se evalúa por participante y rama: una contribución ajena no consume tu oportunidad. Los umbrales del objetivo siguen dependiendo del orden; validarlo con personas antes de ampliar el balance.");
        report.AppendLine().AppendLine("## Ejemplos reproducibles");
        foreach(int seed in new[]{0,5,21})
        {
            var game=new PartyGame(catalog,PartyChecks.Roster(2),seed);
            report.AppendLine().AppendLine("### Semilla "+seed+" · "+game.Goal.title).AppendLine().AppendLine("Original: "+game.News.original).AppendLine();
            string[] actions={"mentir","confundir","panico","calmar"};
            for(int turn=0;turn<4;turn++)
            {
                game.Ready(game.Token);var change=game.Confirm(game.Token,actions[turn],turn%2==1?"B":"A",turn==0);
                report.AppendLine("- T"+(turn+1)+" · "+change.branchId+" · "+catalog.Action(change.actionId).title+": "+change.text+" (+"+change.points+")");game.Next(game.Token);
            }
            report.AppendLine().AppendLine("Resultado: "+string.Join("; ",game.Players.Select(p=>p.name+" = "+p.score))+". Ganadores: "+string.Join(", ",game.Winners().Select(p=>p.name))+".");
        }
        report.AppendLine().AppendLine("## Observaciones\n\n- Repetir tu transformación sobre el mismo protagonista y rama puede cambiar sus palabras, pero no recibe otro punto de aporte. La prueba de repetición lo exige.\n- Las confusiones sustituyen al protagonista y quedan en las transformaciones siguientes. Las invenciones anteriores se conservan en el historial; el titular muestra la última transformación, no todas las frases concatenadas.\n- El objetivo de pánico puede dejar de puntuar al acercarse a 100; difusión también satura en 20. Calmar y elegir otra rama permiten cambiar el contexto.\n- El vocabulario es finito: 12 noticias, 3 plantillas por acción y 2 malentendidos por noticia. La variedad de redacción no equivale a una nueva regla.\n- Pendientes: importación/renderizado y pruebas PlayMode en Unity, lectura y pulsación en pantallas pequeñas y prueba de diversión/equidad con personas.");
        return report.ToString();
    }
}
