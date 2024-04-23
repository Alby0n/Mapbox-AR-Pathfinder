using System.Collections.Generic;
// ReSharper disable MemberCanBePrivate.Global

/// <summary>
/// Utility class to manage a list of symbol strings.
/// </summary>
public class DefineSymbols {
    private List<string> symbols;

    public DefineSymbols(string symbols)
    {
        Set(symbols);
    }

    public void Set(string sym)
    {
        symbols = new List<string>(sym.Split(new [] { ";" }, System.StringSplitOptions.None));
    }

    public bool Has(string symbol)
    {
        return (symbols.FindIndex(obj => obj == symbol) >= 0);
    }

    public void Add(string symbol)
    {
        if (!Has(symbol))
        {
            symbols.Add(symbol);
        }
    }

    public void Remove(string symbol)
    {
        symbols.Remove(symbol);
    }

    public string Get()
    {
        return string.Join(";", symbols.ToArray());
    }
}
