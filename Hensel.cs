class HenselRule
{
    public HenselRule(Array512 birth, Array512 survival, double startDensity, int generations)
    {
        Birth = birth;
        Survival = survival;
        StartDensity = startDensity;
        Generations = generations;
    }

    public Array512 Birth { get; }
    public Array512 Survival { get; }
    public double StartDensity { get; }
    public int Generations { get; }
}