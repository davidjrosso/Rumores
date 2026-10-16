using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Rumores.Core;
using Rumores.Tests;

class Program
{
    static int Main(string[] args)
    {
        var path=args.Length>0?args[0]:"Assets/_Rumores/Resources/Content.json";
        var options=new JsonSerializerOptions {IncludeFields=true};
        var catalog=JsonSerializer.Deserialize<Catalog>(File.ReadAllText(path),options);
        var cases=CoreChecks.Cases(catalog);
        cases["Persistencia JSON: roundtrip completo y continuación hasta jefe"]=()=>{
            var g=new GameSession(catalog);g.NewRun(937);
            for(int stage=0;stage<6;stage++){
                g.SelectNode(g.AvailableNodes().First(n=>n.kind!="event").id);CoreChecks.BuildSolution(g);g.Simulate();
                g=new GameSession(catalog,JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(g.Save,options),options));
                CoreChecks.Assert(g.Run.lastSimulation.steps.Count>0&&g.Run.lastEvaluation.won);g.AcceptResult();
                if(g.Run.phase==Phase.Reward){g=new GameSession(catalog,JsonSerializer.Deserialize<SaveData>(JsonSerializer.Serialize(g.Save,options),options));g.ChooseReward(g.Run.rewardIds[0]);}
            }
            CoreChecks.Assert(g.Run.won);
        };
        foreach(var test in SaveChecks.Cases(catalog,new SystemSaveJson()))cases.Add(test.Key,test.Value);
        int failed=0;
        foreach(var test in cases){try{test.Value();Console.WriteLine("PASS "+test.Key);}catch(Exception e){failed++;Console.WriteLine("FAIL "+test.Key+"\n"+e);}}
        Console.WriteLine("RESULT: "+(cases.Count-failed)+" passed, "+failed+" failed, "+cases.Count+" executed.");
        return failed==0?0:1;
    }
}

sealed class SystemSaveJson : ISaveJson
{
    readonly JsonSerializerOptions options=new JsonSerializerOptions {IncludeFields=true};
    public string Serialize<T>(T value) {return JsonSerializer.Serialize(value,options);}
    public T Deserialize<T>(string json)
    {
        try {return JsonSerializer.Deserialize<T>(json,options);}
        catch(JsonException ex) {throw new ArgumentException("JSON inválido.",ex);}
    }
}
