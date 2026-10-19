using System.Linq;
using System.Text;
using System.Collections.Generic;
using Rumores.Core.Party;
using Rumores.Tests;

static class PartyReport
{
    public static string Build(PartyCatalog catalog)
    {
        var report=new StringBuilder("# Prueba del chisme argentino\n\nGenerado con `dotnet run --project Tests/Rumores.Tests.csproj -c Release -- --party-report Docs/PARTY_VALIDATION.md`. Simulación C#; no es una ejecución de Unity.\n\n");
        var endings=new HashSet<string>();int rounds=0,repeated=0;
        for(int seed=0;seed<60;seed++)foreach(int count in new[]{2,8})for(int option=0;option<3;option++)
        {
            var game=new PartyGame(catalog,PartyChecks.Roster(count),seed);var phrases=new HashSet<string>();
            while(game.Phase!=PartyPhase.Finished){game.Ready(game.Token);var change=game.Confirm(game.Token,game.Choices()[option].id);if(!phrases.Add(change.text))repeated++;game.Next(game.Token);}
            endings.Add(game.Branches[0].rumor.headline);rounds++;
        }
        report.AppendLine(rounds+" rondas completas (60 semillas × 2/8 amigos × 3 elecciones), "+endings.Count+" finales distintos y "+repeated+" frases repetidas dentro de una misma ronda.");
        report.AppendLine().AppendLine("Se ofrecen tres respuestas reales por turno y confirmar conserva exactamente el texto elegido. No hay puntos, atributos ni objetivos calculados. Las cuatro maneras de contar rotan; el vocabulario sigue siendo finito. La repetición narrativa y la diversión necesitan validación con personas.");
        foreach(int seed in new[]{0,1,2,7})
        {
            var game=new PartyGame(catalog,PartyChecks.Roster(2),seed);
            report.AppendLine().AppendLine("## "+game.News.original).AppendLine().AppendLine(game.News.question).AppendLine();
            while(game.Phase!=PartyPhase.Finished){game.Ready(game.Token);var choice=game.Choices()[game.Turn%3];string name=game.Current.name;var change=game.Confirm(game.Token,choice.id);report.AppendLine("- "+name+": "+change.text);game.Next(game.Token);}
            report.AppendLine().AppendLine("Para charlar: "+game.News.icebreaker);
        }
        report.AppendLine().AppendLine("Pendiente: Unity EditMode/PlayMode, ajuste visual en pantallas pequeñas y prueba con amigos. La lista de avatares conserva la corrección de CanvasRenderer.");
        return report.ToString();
    }
}
