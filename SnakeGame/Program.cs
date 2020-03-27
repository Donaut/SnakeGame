using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace SnakeGame
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Nyomj egy ENTERT a játékhoz.");
            Console.ReadLine();
            while (true)
            {
                var game = new SnakeGame(24, 40);
                game.Run(8);

                System.Threading.Thread.Sleep(5000);
                Console.Clear();

                Console.WriteLine("Újra akarod kezdeni? y/n");
                var input = Console.ReadKey(true);
                if (input.Key == ConsoleKey.Y)
                {
                    continue;
                }
                if (input.Key == ConsoleKey.N)
                {
                    break;
                }
            }

        }

    }

}
