namespace FireArrows.Patches;

using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

public static class AnimationsLibAttachmentPatch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            // 1. Look for the string constant ".json"
            // 2. Look for the call to string.Concat(string, string)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldstr, ".json", "jsonmatch")
            )
            // If the sequence isn't found, this will throw an error to help debugging
            .ThrowIfInvalid("Could not find the '.json' concatenation in Attachment constructor.")
            .MatchStartForward(
                new CodeMatch(OpCodes.Ret)
            )
            .RemoveInstructions(1) 
            .InstructionEnumeration();
    }
}