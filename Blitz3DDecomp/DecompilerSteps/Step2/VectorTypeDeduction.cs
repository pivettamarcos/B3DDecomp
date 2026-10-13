using B3DDecompUtils;
using Blitz3DDecomp.LowLevel;

namespace Blitz3DDecomp.DecompilerSteps.Step2;

static class VectorTypeDeduction
{
    private static void ProcessSection(Function function, AssemblySection section)
    {
        for (int i = 2; i < section.Instructions.Length - 1; i++)
        {
            var instruction = section.Instructions[i];
            if (instruction.Name != "call"
                || !instruction.DestArg.Contains("_builtIn__bbVecAlloc", StringComparison.Ordinal))
            {
                continue;
            }

            var vecTypeToRegister = section.Instructions[i - 2];
            var vecType = DeclType.FromDesc(vecTypeToRegister.SrcArg1[1..]);

            var registerToArg = section.Instructions[i - 1];

            Instruction? resultToVariable = null;
            for (var resultIndex = i + 1; resultIndex <= i + 3 && resultIndex < section.Instructions.Length; resultIndex++)
            {
                var candidate = section.Instructions[resultIndex];
                if (candidate.Name == "mov" && candidate.SrcArg1.StartsWith("eax", StringComparison.OrdinalIgnoreCase))
                {
                    resultToVariable = candidate;
                    break;
                }
            }
            if (resultToVariable is null) { continue; }

            var variable = function.InstructionArgumentToVariable(resultToVariable.DestArg);

            if (vecTypeToRegister.Name == "mov"
                && vecType.IsArrayType
                && vecTypeToRegister.DestArg == registerToArg.SrcArg1
                 && variable != null
                  && (variable.DeclType == DeclType.Unknown || variable is GlobalVariable))
            {
                variable.Trace = variable.Trace.Append($"{function}: {variable.Name} is {vecType} because {vecTypeToRegister}");
                variable.DeclType = vecType;
            }
        }
    }

    public static void Process(Function function)
    {
        foreach (var section in function.AssemblySections)
        {
            ProcessSection(function, section);
        }
    }
}
