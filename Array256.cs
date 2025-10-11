public class Array256
{
    public Array256(BitArray array)
    {
        if (array.Length != 256)
        {
            throw new Not256Exception("");
        }
        Array = array;
    }
    public bool this[int index] => Array[index];
    public BitArray Array { get; }
}
// # 0 1 2
// # 3 • 4
// # 5 6 7
//Лытдыбр