namespace Sabl;

internal class ExampleContext : Context
{
    public ExampleContext(IContext? parent, string? name = null) : base(parent, name) { }

    public override object Value(object key) => throw null!;
}
