using System.Collections.Generic;
using System.Text;
static class Parser
{
    public static Rule? Parse(string str)
    {
        try
        {
            var rule = new Rule(new BitArray(10), new BitArray(10), 0, 1);
            foreach (var token in StringsToTokens(str.Split('/')))
            {
                LoadToken(token, ref rule);
            }
            return rule;
        }
        catch
        {
            return null;
        }
    }
    static Token[] StringsToTokens(string[] input)
    {
        var result = new Token[input.Length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new Token(input[i], GetTokenType(input[i]));
        }
        return result;
    }
    public static TokenType GetTokenType(string token)
    {
        switch (token[0])
        {
            case 'B':
                return TokenType.Birth;
            case 'S':
                return TokenType.Survival;
            case 'p':
            case 'd':
                return TokenType.Density;
            default:
                return TokenType.Unknown;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case 'G':
                return TokenType.Generations;
        }
    }

    static void LoadToken(Token token, ref Rule rule)
    {
        switch (token.Type)
        {
            case TokenType.Birth:
                rule.Birth = GetValuesFromString(token.Content, false);
                break;
            case TokenType.Survival:
                rule.Survival = GetValuesFromString(token.Content, false);
                break;
            case TokenType.Density:
                if (double.TryParse(token.Content.Trim("pd= ".ToCharArray()), out double d))
                {
                    rule.StartDensity = d;
                }
                else
                {
                    throw new IncorrectRulestringException($"Density is not a number: {token.Content.Trim("pd= ".ToCharArray())}");
                }
                break;
            case TokenType.Generations:
                if (int.TryParse(token.Content.Trim('G'), out int g))
                {
                    rule.Generations = g;
                }
                else
                {
                    throw new IncorrectRulestringException($"Generations count is not a number: {token.Content.Trim('G')}");
                }
                break;
        }
    }
    static BitArray GetValuesFromString(string str, bool ignoreFirstSymbol)
    {
        var result = new BitArray(10);
        foreach (var symbol in str)
        {
            if (ignoreFirstSymbol)
            {
                ignoreFirstSymbol = false;
                continue;
            }
            switch (symbol)
            {
                case '0':
                    result[0] = true;
                    break;
                case '1':
                    result[1] = true;
                    break;
                case '2':
                    result[2] = true;
                    break;
                case '3':
                    result[3] = true;
                    break;
                case '4':
                    result[4] = true;
                    break;
                case '5':
                    result[5] = true;
                    break;
                case '6':
                    result[6] = true;
                    break;
                case '7':
                    result[7] = true;
                    break;
                case '8':
                    result[8] = true;
                    break;
                case '9':
                    result[9] = true;
                    break;
            }
        }
        return result;
    }
}
class Token
{
    public Token(string content, TokenType type)
    {
        Content = content;
        Type = type;
    }

    public string Content { get; }
    public TokenType Type { get; }
}
public enum TokenType
{
    Unknown,
    Birth,
    Survival,
    Generations,
    Density,
    DensityOf,
    FieldScale
}