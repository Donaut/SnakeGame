using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGameConsole
{
    internal static class ConsoleExtensions
    {
        /// <summary>
        /// Alternative to the <see cref="Console.Title"/> so its possible to avoid allocations.
        /// </summary>
        public static void SetConsoleTitle(ReadOnlySpan<char> title)
        {
            if (OperatingSystem.IsWindows())
            {
                var buffer = ArrayPool<char>.Shared.Rent(title.Length);
                title.CopyTo(buffer);
                Kernel32.SetConsoleTitle(buffer);
                ArrayPool<char>.Shared.Return(buffer);
                return;
            }
            
            // IsLinuxSupported() tries to set the console title and checks if its working if not fallback to the original
            //else if(OperatingSystem.IsLinux() && IsLinuxSupported())
            //{
            //}


            Console.Title = new string(title);
        }
    }

    internal partial class Kernel32
    {
        [LibraryImport("Kernel32.dll", EntryPoint = "SetConsoleTitleW", SetLastError = false, StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetConsoleTitle(char[] lpConsoleTitle);
    }
}
