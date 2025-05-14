public abstract class Rule { }
class LLRule : Rule
{
    public LLRule(short birth, short survival)
    {
        Birth = birth;
        Survival = survival;
    }

    public short Birth { get; }
    public short Survival { get; }
}