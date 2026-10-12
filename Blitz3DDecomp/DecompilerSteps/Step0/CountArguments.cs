using System.Globalization;
using System.Text.RegularExpressions;

namespace Blitz3DDecomp;

static class CountArguments
{
    public static void Process(Function function)
    {
        if (!function.AssemblySections.Any()) { return; }
        var leaveSection = function.AssemblySections.Last(s => s.Name.Contains($"_leave{function.CoreSymbolName}", StringComparison.Ordinal));
        var retInstruction = leaveSection.Instructions[^1];
        int parameterCount;
        if (retInstruction.DestArg.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            var retValueStr = retInstruction.DestArg[2..];
            var retValue = int.Parse(retValueStr, NumberStyles.HexNumber);
            parameterCount = retValue / 4;
        }
        else
        {
            var parameterOffsets = function.AssemblySections
                .SelectMany(s => s.Instructions.ToArray())
                .SelectMany(i => new[] { i.DestArg, i.SrcArg1, i.SrcArg2 })
                .SelectMany(arg => Regex.Matches(arg, @"\[ebp\+0x([0-9a-f]+)\]", RegexOptions.IgnoreCase)
                    .Cast<Match>()
                    .Select(match => int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)))
                .ToArray();

            var maxParameterOffset = parameterOffsets.DefaultIfEmpty(0).Max();
            parameterCount = maxParameterOffset < 0x14
                ? 0
                : ((maxParameterOffset - 0x14) / 4) + 1;
        }

        function.Parameters.AddRange(Enumerable.Range(0, parameterCount)
            .Select(i => new Function.Parameter(function, $"arg{i}", i) { DeclType = DeclType.Unknown }));
    }
}
