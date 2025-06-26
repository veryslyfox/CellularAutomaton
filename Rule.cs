global using System.Collections;
public class Rule
{
    public Rule(BitArray birth, BitArray survival)
    {
        Birth = birth;
        Survival = survival;
    }

    public BitArray Birth { get; set; }
    public BitArray Survival { get; set; }
}
