using System.Linq.Expressions;
using System.Reflection;

namespace Sabl;

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
        var t_CTS_Registrations = t_CTS.GetNestedType("Registrations", bf)!;
        var t_CTS_CBNode = t_CTS.GetNestedType("CallbackNode", bf)!;

        var f_registrations = t_CTS.GetField("_registrations", BindingFlags.Instance | bf)!;
        var f_Callbacks = t_CTS_Registrations.GetField("Callbacks", BindingFlags.Instance | bf)!;
        var f_Next = t_CTS_CBNode.GetField("Next", BindingFlags.Instance | bf)!;


        var p_CTS = Expression.Parameter(t_CTS, "cts");

        var pl_regs = Expression.Parameter(t_CTS_Registrations, "regs");
        var pl_cnt = Expression.Parameter(typeof(int), "cnt");
        var pl_node = Expression.Parameter(t_CTS_CBNode, "node");

        LabelTarget lbl_rtrn = Expression.Label(typeof(int), "<return>");

        var xp_asnRegs = Expression.Assign(pl_regs, Expression.Field(p_CTS, f_registrations));
        var xp_nullRegs =
            Expression.IfThen(
                Expression.Equal(pl_regs, Expression.Constant(null)),
                Expression.Return(lbl_rtrn, Expression.Constant(0))
            );

        var xp_asnCbs = Expression.Assign(pl_node, Expression.Field(pl_regs, f_Callbacks));
        var xp_nullCbs =
            Expression.IfThen(
                Expression.Equal(Expression.Constant(null), pl_node),
                Expression.Return(lbl_rtrn, Expression.Constant(0))
            );

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

        var xp_dfltRetur = Expression.Label(lbl_rtrn, Expression.Constant(0));

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
            p_CTS
        ); ;

        return exp.Compile();
    }

    public static int GetRegistrationCount(this CancellationTokenSource cts)
    {
        return _readRegCount.Invoke(cts);
    }
}
