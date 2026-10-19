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
        var endings=new HashSet<string>();int rounds=0,repeated=0,linked=0;
        for(int seed=0;seed<catalog.news.Count*3;seed++)foreach(int count in new[]{2,8})for(int option=0;option<6;option++)
        {
            var game=new PartyGame(catalog,PartyChecks.Roster(count),seed);var phrases=new HashSet<string>();
            while(game.Phase!=PartyPhase.Finished){game.Ready(game.Token);var change=game.Confirm(game.Token,game.Choices(option/3)[option%3].id);if(!phrases.Add(change.detailId))repeated++;if(!string.IsNullOrEmpty(change.parentDetailId))linked++;game.Next(game.Token);}
            endings.Add(game.Branches[0].rumor.headline);rounds++;
        }
        report.AppendLine(rounds+" rondas completas ("+(catalog.news.Count*3)+" semillas × 2/8 amigos × 6 elecciones), "+endings.Count+" finales distintos, "+linked+" aportes enlazados al anterior y "+repeated+" detalles reutilizados dentro de una misma ronda.");
        report.AppendLine().AppendLine("Se ofrecen tres respuestas reales por turno y confirmar conserva exactamente el texto elegido. No hay puntos, atributos ni objetivos calculados. Hay seis maneras de contar, visibles de a tres con «Ver otras 3». Cada noticia tiene 18 continuaciones propias. La carta conserva el episodio y el aporte anterior; las variantes aún no usadas de ese estilo tienen prioridad. La repetición narrativa y la diversión necesitan validación con personas.");
        foreach(int seed in new[]{0,6,7,8,12,18,20,24,30})
        {
            var game=new PartyGame(catalog,PartyChecks.Roster(2),seed);
            report.AppendLine().AppendLine("## "+game.News.original).AppendLine().AppendLine(game.News.question).AppendLine();
            while(game.Phase!=PartyPhase.Finished){game.Ready(game.Token);var choice=game.Choices(game.Turn%2)[game.Turn%3];string name=game.Current.name;var change=game.Confirm(game.Token,choice.id);report.AppendLine("- "+name+": "+change.text);game.Next(game.Token);}
            report.AppendLine().AppendLine("Para charlar: "+game.News.icebreaker);
        }
        report.AppendLine().AppendLine("## Revisión editorial y límites").AppendLine();
        report.AppendLine("Se revisaron las 36 noticias, sus preguntas y 648 continuaciones por tema. Las pruebas estructurales verifican pertenencia al mismo episodio, conservación del aporte previo, longitud y variantes; no pueden demostrar sentido común o humor. Algunas secuencias vuelven a la misma idea al agotar las tres variantes de un estilo. El titular conserva los dos aportes más recientes; el historial conserva todos.");
        report.AppendLine().AppendLine("Las referencias a actualidad son escenas ficticias sobre QR, IA, delivery, precios, videollamadas y grupos. No son titulares reales ni una lista verificada de tendencias del día. Los guiños históricos y de nostalgia aparecen en anécdotas inventadas.");
        report.AppendLine().AppendLine("Pendiente: Unity EditMode/PlayMode, ajuste visual en pantallas pequeñas y prueba con amigos. La lista de avatares conserva la corrección de CanvasRenderer.");
        return report.ToString();
    }
}
