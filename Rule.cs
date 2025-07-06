global using System.Collections;
public class Rule
{
    public Rule(BitArray birth, BitArray survival, double startDensity, int generations)
    {
        Birth = birth;
        Survival = survival;
        StartDensity = startDensity;
        Generations = generations;
    }

    public BitArray Birth { get; set; }
    public BitArray Survival { get; set; }
    public double StartDensity { get; set; }
    public int Generations { get; set; }
}
