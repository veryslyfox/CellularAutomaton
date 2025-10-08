class Array512
{
    public Array512(BitArray array)
    {
        if (array.Length != 512)
        {
            throw new Not512Exception("");
        }
        Array = array;
    }
    public BitArray Array { get; }
}
// # 0 1 2
// # 3 • 4
// # 5 6 7