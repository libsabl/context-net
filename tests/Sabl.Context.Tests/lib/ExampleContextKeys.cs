using System.Collections.Generic;
using System.Text;

namespace Sabl;

public static class ExampleContextKeys
{
    public static readonly ContextKey<string> String = new("string");
    public static readonly ContextKey<IList<int>> IntList = new("list<int>");
    public static readonly ContextKey<IDictionary<string, object>> Map = new("map");
    public static readonly ContextKey<Encoding> Encoding = new();
}
