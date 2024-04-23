using System.Collections.Generic;
using UnityEditor;


/// <summary>
/// Utility class that manages Define Symbols for a given set of build targets.
/// </summary>
public class DefineSymbolsManager  {
    private Dictionary<BuildTargetGroup, DefineSymbols> defineSymbols  = new Dictionary<BuildTargetGroup, DefineSymbols>();

    public DefineSymbolsManager(BuildTargetGroup[] groups)
    {
        foreach (var group in groups)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            defineSymbols.Add(group, new DefineSymbols(symbols));
        }
    }

    public void UpdateFromBuildSettings()
    {
        var groups = defineSymbols.Keys;
        defineSymbols = new Dictionary<BuildTargetGroup, DefineSymbols>();

        foreach (var group in groups)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            defineSymbols.Add(group, new DefineSymbols(symbols));
        }

    }

    public void ApplyToBuildSettings()
    {
        foreach (var e in defineSymbols)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(e.Key, e.Value.Get());
        }
    }

    public void Add(string symbol)
    {
        foreach (var item in defineSymbols)
        {
            item.Value.Add(symbol);
        }
    }

    public void Remove(string symbol)
    {
        foreach (var item in defineSymbols)
        {
            item.Value.Remove(symbol);
        }
    }

    public bool Has(string symbol)
    {
        var has = true;

        foreach (var item in defineSymbols)
        {
            has = has && item.Value.Has(symbol);
        }

        return has;
    }

    public override string ToString()
    {
        var str = "DefineSymbolsManager {\n";

        foreach (var item in defineSymbols)
        {
            str += item.Key + ": " + item.Value.Get() + "\n";
        }

        str += "}";

        return str;
    }
}
