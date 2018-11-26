using System;

namespace RLBotAutoRunner
{
    public static class Extensions
    {
        public static void Shuffle<T>(this Random rand, T[] array)
        {
            int i = array.Length;
            while (i > 1)
            {
                int j = rand.Next(i--);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
}
