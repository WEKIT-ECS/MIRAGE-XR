using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace ReadyPlayerMe.Core.Editor
{
    public static class DefineSymbolHelper
    {
        private const string READY_PLAYER_ME_SYMBOL = "READY_PLAYER_ME";

        private static void ModifyScriptingDefineSymbolInAllBuildTargetGroups(string defineSymbol, bool addSymbol)
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                List<string> defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

                if (addSymbol && !defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Add(defineSymbol);
                }
                else if (!addSymbol && defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Remove(defineSymbol);
                }
                else
                {
                    continue;
                }

                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
                }
                catch (Exception e)
                {
                    var actionWord = addSymbol ? "set" : "remove";
                    Debug.LogWarning($"Could not {actionWord} {defineSymbol} defines for build target: {target} group: {group} {e}");
                }
            }

            if (addSymbol)
            {
                CompilationPipeline.RequestScriptCompilation();
            }
        }

        public static void AddSymbols()
        {
            ModifyScriptingDefineSymbolInAllBuildTargetGroups(READY_PLAYER_ME_SYMBOL, true);
            CompilationPipeline.RequestScriptCompilation();
        }

        public static void RemoveSymbols()
        {
            ModifyScriptingDefineSymbolInAllBuildTargetGroups(READY_PLAYER_ME_SYMBOL, false);
        }

    }
}
