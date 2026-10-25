using System.Reflection;using System.Text;using System.Text.Json;
if(args.Length==3){var registry=new Registry();registry.Load("plugins");var input=Path.GetExtension(args[0]).TrimStart('.');var output=args[1];var document=registry.Get(input).Read(File.ReadAllText(args[0]));File.WriteAllText(args[2],registry.Get(output).Write(document));}
else Console.WriteLine("Usage: converter <input-file> <output-format> <output-file>");
public record IntermediateDocument(List<List<string>> Rows);
public interface IFormatReader{IntermediateDocument Read(string text);}
public interface IFormatWriter{string Write(IntermediateDocument document);}
public interface IFormat:IFormatReader,IFormatWriter{string Extension{get;}}
public class Registry{
 readonly Dictionary<string,IFormat> formats=new(StringComparer.OrdinalIgnoreCase);
 public Registry(){foreach(var f in new IFormat[]{new Csv(),new JsonFormat(),new Sketch()})formats[f.Extension]=f;}
 public void Load(string folder){if(!Directory.Exists(folder))return;foreach(var file in Directory.GetFiles(folder,"*.dll"))foreach(var type in Assembly.LoadFrom(Path.GetFullPath(file)).GetTypes().Where(t=>typeof(IFormat).IsAssignableFrom(t)&&!t.IsAbstract&&!t.IsInterface))if(Activator.CreateInstance(type)is IFormat format)formats[format.Extension]=format;}
 public IFormat Get(string extension)=>formats.TryGetValue(extension,out var f)?f:throw new ArgumentException("Unknown format "+extension);
}
public class Csv:IFormat{
 public string Extension=>"csv";
 public IntermediateDocument Read(string text){
 var rows=new List<List<string>>();var row=new List<string>();var field=new StringBuilder();bool quote=false,atStart=true;
 for(int i=0;i<text.Length;i++){char c=text[i];
 if(quote){if(c=='"'){if(i+1<text.Length&&text[i+1]=='"'){field.Append('"');i++;}else quote=false;}else field.Append(c);}
 else if(c=='"'&&atStart){quote=true;atStart=false;}
 else if(c==','){row.Add(field.ToString());field.Clear();atStart=true;}
 else if(c=='\n'||c=='\r'){if(c=='\r'&&i+1<text.Length&&text[i+1]=='\n')i++;row.Add(field.ToString());rows.Add(row);row=new();field.Clear();atStart=true;}
 else{field.Append(c);atStart=false;}}
 if(quote)throw new FormatException("Unclosed quote");
 if(field.Length>0||row.Count>0||(!atStart&&text.Length>0)){row.Add(field.ToString());rows.Add(row);}
 return new(rows);
 }
 public string Write(IntermediateDocument d)=>string.Join("\r\n",d.Rows.Select(row=>string.Join(",",row.Select(f=>"\""+f.Replace("\"","\"\"")+"\""))))+(d.Rows.Count>0?"\r\n":"");
}
public class JsonFormat:IFormat{public string Extension=>"json";public IntermediateDocument Read(string t)=>new(JsonSerializer.Deserialize<List<List<string>>>(t)??new());public string Write(IntermediateDocument d)=>JsonSerializer.Serialize(d.Rows);}
public class Sketch:IFormat{
 public string Extension=>"sketch";
 public IntermediateDocument Read(string text){var lines=text.Split('\n',StringSplitOptions.RemoveEmptyEntries);if(lines.Length==0||lines[0].Trim()!="SKETCH1")throw new FormatException("Invalid sketch header");return new(lines.Skip(1).Select(l=>JsonSerializer.Deserialize<List<string>>(l)??new()).ToList());}
 public string Write(IntermediateDocument d)=>"SKETCH1\n"+string.Join("\n",d.Rows.Select(r=>JsonSerializer.Serialize(r)))+"\n";
}
