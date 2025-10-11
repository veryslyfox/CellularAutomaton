class HenselRule
{
    public HenselRule(Array256 birth, Array256 survival, double startDensity, int generations)
    {
        Birth = birth;
        Survival = survival;
        StartDensity = startDensity;
        Generations = generations;
    }
    public Array256 Birth { get; }
    public Array256 Survival { get; }
    public double StartDensity { get; }
    public int Generations { get; }
}