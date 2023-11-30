using System.Diagnostics;

namespace SnakeGameConsole;

internal class Program
{
    static void Main(string[] args)
    {
        float t = 0.0f;
        const float dt = 0.01f; // 60 FPS (1 / 60 = 0.01666666)

        float currentTime = Stopwatch.GetTimestamp() / Stopwatch.Frequency;
        float accumulator = 0.0f;

        while (true)
        {
            float newTime = Stopwatch.GetTimestamp() / Stopwatch.Frequency;
            float frameTime = newTime - currentTime;
            currentTime = newTime;

            accumulator += frameTime;

            while (accumulator >= dt)
            {
                //integrate(state, t, dt);
                accumulator -= dt;
                t += dt;
            }

            //render(state);
        }
        Console.WriteLine("Hello, World!");
    }
}
