global using System.Collections;
public class Rule
{
    public Rule(Array256 birth, Array256 survival, double startDensity, int generations)
    {
        Birth = birth;
        Survival = survival;
        StartDensity = startDensity;
        Generations = generations;
    }

    public Array256 Birth { get; set; }
    public Array256 Survival { get; set; }
    public double StartDensity { get; set; }
    public int Generations { get; set; }
}
