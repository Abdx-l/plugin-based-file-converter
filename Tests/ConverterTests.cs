using Xunit;
public class ConverterTests{
 [Fact]public void EveryRoundTrip(){var r=new Registry();var source=new IntermediateDocument(new(){new(){"name","note"},new(){"عمر","a,b\nquoted \"value\""},new(){"",""}});foreach(var a in new[]{"csv","json","sketch"})foreach(var b in new[]{"csv","json","sketch"}){var first=r.Get(a).Read(r.Get(a).Write(source));var second=r.Get(b).Read(r.Get(b).Write(first));Assert.Equal(System.Text.Json.JsonSerializer.Serialize(source),System.Text.Json.JsonSerializer.Serialize(second));}}
 [Fact]public void MalformedCsv(){Assert.Throws<FormatException>(()=>new Csv().Read("\"open"));}
}
