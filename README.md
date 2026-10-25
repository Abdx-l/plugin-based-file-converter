# Plugin-Based File Converter

C# intermediate-document converter with reflection-discovered plugins, CSV/JSON/SKETCH1 formats, multiline quoted CSV fields, Unicode preservation, and all-format-pair round-trip tests.

## Run

`dotnet test Tests/Tests.csproj`. Convert: `dotnet run -- input.csv json output.json`. Plugin assemblies in plugins/ implement IFormat; built-in formats share the same interfaces.
