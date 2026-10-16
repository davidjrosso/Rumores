using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
var files=Directory.GetFiles("Assets","*.cs",SearchOption.AllDirectories);
int errors=0;
foreach(var file in files){
    var syntax=CSharpSyntaxTree.ParseText(File.ReadAllText(file),new CSharpParseOptions(LanguageVersion.CSharp9),file);
    foreach(var d in syntax.GetDiagnostics().Where(d=>d.Severity==DiagnosticSeverity.Error)){Console.WriteLine(d);errors++;}
}
Console.WriteLine($"C# 9 syntax: {files.Length} files, {errors} errors. This does not check Unity API types or run Unity.");
return errors==0?0:1;
