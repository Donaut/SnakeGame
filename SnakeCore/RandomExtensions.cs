namespace SnakeCore
{
    internal static class RandomExtensions
    {
        public static float NextSingle(this Random random, float min, float max)
        {
            if (min >= max) throw new ArgumentException("min must be less than max.");

            var value = random.NextSingle(); // Generate a random double between 0.0 (inclusive) and 1.0 (exclusive)
            var result = (value * (max - min) + min); // Scale and shift to the desired range

            return result;
        }
    }
}
