using System.Linq.Expressions;
using System.Reflection;

namespace Sabl;

/// <summary>
/// Utility that uses reflection to extract the internal count of cancellation 
/// callback registrations on the <see cref="CancellationTokenSource"/>
/// </summary>
internal static class CTSExtensions
{
    private static readonly Func<CancellationTokenSource, int> _readRegCount;

    static CTSExtensions()
    {
        _readRegCount = BuildRegCounter();
    }

    private static Func<CancellationTokenSource, int> BuildRegCounter()
    {
        var bf = BindingFlags.Public | BindingFlags.NonPublic;
        var t_CTS = typeof(CancellationTokenSource);
        var t_CTS_Registrations = t_CTS.GetNestedType("Registrations", bf)!; // internal sealed class CancellationTokenSource.Registrations
        var t_CTS_CBNode = t_CTS.GetNestedType("CallbackNode", bf)!;         // internal sealed class CancellationTokenSource.CallbackNode

        var f_registrations = t_CTS.GetField("_registrations", BindingFlags.Instance | bf)!;       // CancellationTokenSource: private Registrations? _registrations;
        var f_Callbacks = t_CTS_Registrations.GetField("Callbacks", BindingFlags.Instance | bf)!;  // Registrations: public CallbackNode? Callbacks;
        var f_Next = t_CTS_CBNode.GetField("Next", BindingFlags.Instance | bf)!;                   // CallbackNode: public CallbackNode? Next;

        var p_CTS = Expression.Parameter(t_CTS, "cts");                  // (CancellationTokenSource cts)
        var pl_regs = Expression.Parameter(t_CTS_Registrations, "regs"); // (Registrations regs)
        var pl_node = Expression.Parameter(t_CTS_CBNode, "node");        // (CallbackNode node)
        var pl_cnt = Expression.Parameter(typeof(int), "cnt");           // (int cnt)

        LabelTarget lbl_rtrn = Expression.Label(typeof(int), "<return>");

        // Registrations regs = ctx._registrations;
        var xp_asnRegs = Expression.Assign(pl_regs, Expression.Field(p_CTS, f_registrations));
        
        // if(regs == null) { return 0 }
        var xp_nullRegs =
            Expression.IfThen(
                Expression.Equal(pl_regs, Expression.Constant(null)),
                Expression.Return(lbl_rtrn, Expression.Constant(0))
            );

        // CallbackNode node = regs.Callbacks;
        var xp_asnCbs = Expression.Assign(pl_node, Expression.Field(pl_regs, f_Callbacks));

        // if(node == null) { return 0 }
        var xp_nullCbs =
            Expression.IfThen(
                Expression.Equal(Expression.Constant(null), pl_node),
                Expression.Return(lbl_rtrn, Expression.Constant(0))
            );

        // do {
        //   int cnt;
        //   if(node == null) { return cnt }
        //   cnt++;
        //   node = node.Next;
        // }
        var xp_loop =
            Expression.Loop(
                Expression.Block(
                    new ParameterExpression[] { pl_cnt },
                    Expression.IfThen(
                        Expression.Equal(pl_node, Expression.Constant(null)),
                        Expression.Return(lbl_rtrn, pl_cnt)
                    ),
                    Expression.PostIncrementAssign(pl_cnt),
                    Expression.Assign(pl_node, Expression.Field(pl_node, f_Next))
                )
            );

        // return 0;
        var xp_dfltRetur = Expression.Label(lbl_rtrn, Expression.Constant(0));

        // int GetRegistrationCount(CancellationTokenSource cts) {
        //    Registrations regs;
        //    CallbackNode node;
        //
        //    regs = ctx._registrations;
        //    if(regs == null) { return 0 }
        //    
        //    node = regs.Callbacks;
        //    if(node == null) { return 0 }
        //
        //    do {
        //      int cnt;
        //      if(node == null) { return cnt }
        //      cnt++;
        //      node = node.Next;
        //    }
        //    return 0;
        // }
        var exp = Expression.Lambda<Func<CancellationTokenSource, int>>(
            Expression.Block(
                new ParameterExpression[] {
                    pl_regs,
                    pl_node,
                },
                xp_asnRegs,
                xp_nullRegs,
                xp_asnCbs,
                xp_nullCbs,
                xp_loop,
                xp_dfltRetur
            ),
            "GetRegistrationCount",
            new[] { p_CTS }
        );
        
        return exp.Compile();
    }

    /// <summary>Get the count of cancellation callback registrations on the <see cref="CancellationTokenSource"/></summary>
    public static int GetRegistrationCount(this CancellationTokenSource cts)
    {
        return _readRegCount.Invoke(cts);
    }
}
