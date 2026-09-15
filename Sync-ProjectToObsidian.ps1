$CsharpCode = @'
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program {
    static void Main() {
        string solPath = @"C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\ProgramC#\DynamicSepticSystem";
        string vltPath = @"C:\Users\Luxu\Documents\CALANDRIA RESIDENCIAL\ProgramC#\DSSVAULT";
        string targetFolder = Path.Combine(vltPath, "_CodebaseMap");
        
        Directory.CreateDirectory(targetFolder);
        Console.WriteLine("🚀 Procesando arquitectura C#...");

        var exclude = new List<string> { "\\bin\\", "\\obj\\", "\\.vs\\", "\\.git\\", "\\packages\\", "\\node_modules\\" };
        var allFiles = Directory.GetFiles(solPath, "*.*", SearchOption.AllDirectories);
        
        var treeContent = new StringBuilder("# Project Architecture Tree\n```\n");
        var csharpLinks = new List<string>();

        foreach (var file in allFiles) {
            bool skip = false;
            foreach (var dir in exclude) {
                if (file.Contains(dir)) { skip = true; break; }
            }
            if (skip) continue;

            string relPath = file.Replace(solPath, ".");
            treeContent.AppendLine(relPath);

            if (Path.GetExtension(file) == ".cs") {
                string subDir = Path.GetDirectoryName(relPath).Replace(".\\", "").Replace(".", "");
                string destDir = Path.Combine(targetFolder, subDir);
                Directory.CreateDirectory(destDir);

                string noteName = Path.GetFileNameWithoutExtension(file) + ".md";
                string notePath = Path.Combine(destDir, noteName);
                string linkPath = string.IsNullOrEmpty(subDir) ? noteName : \$"{subDir}/{noteName}";
                csharpLinks.Add(\$"- [[{linkPath}| {noteName}]] ({relPath})");

                var skeleton = new StringBuilder(\$"---\ntags: [codebase-map]\n---\n# File: {Path.GetFileName(file)}\n```csharp\n");
                int braces = 0;
                bool inMethod = false;

                foreach (var line in File.ReadLines(file)) {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("using ") || trimmed.StartsWith("namespace ") || string.IsNullOrWhiteSpace(trimmed)) continue;

                    if (trimmed.Contains("{")) braces++;
                    if (braces > 1) {
                        if (!inMethod) {
                            skeleton.AppendLine("    { /* Codigo omitido para ahorrar tokens */ }");
                            inMethod = true;
                        }
                        if (trimmed.Contains("}")) braces--;
                        if (braces <= 1) inMethod = false;
                        continue;
                    }
                    if (trimmed.Contains("}")) braces--;
                    skeleton.AppendLine(line);
                }
                skeleton.AppendLine("```");
                File.WriteAllText(notePath, skeleton.ToString(), Encoding.UTF8);
            }
        }
        treeContent.AppendLine("```");
        File.WriteAllText(Path.Combine(targetFolder, "_ProjectTree.md"), treeContent.ToString(), Encoding.UTF8);

        var indexContent = new StringBuilder("# Master System Context Map\n- [[_ProjectTree|Ver mapa de carpetas]]\n\n## Componentes\n");
        indexContent.AppendLine(string.Join("\n", csharpLinks));
        File.WriteAllText(Path.Combine(targetFolder, "_SystemContext.md"), indexContent.ToString(), Encoding.UTF8);

        Console.WriteLine("✅ Mapeo completado de forma nativa en Obsidian!");
    }
}
'@

$CompilerPath = (Get-Command csc.exe -ErrorAction SilentlyContinue).Source
if (-not $CompilerPath) { $CompilerPath = (Get-ChildItem "C:\Windows\Microsoft.NET\Framework64\v4.0.*" -Filter csc.exe -Recurse | Select-Object -First 1).FullName }

if ($CompilerPath) {
    Set-Content -Path "$env:TEMP\Mapper.cs" -Value $CsharpCode -Encoding utf8
    & $CompilerPath /nologo /out:"$env:TEMP\Mapper.exe" "$env:TEMP\Mapper.cs"
    & "$env:TEMP\Mapper.exe"
} else {
    Write-Host "No se encontro el compilador csc.exe. Por favor compila este codigo en Visual Studio como herramienta de consola." -ForegroundColor Red
}