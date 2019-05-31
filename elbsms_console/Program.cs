using System;
using System.Diagnostics;
using System.IO;
using elbsms_core;

namespace elbsms_console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: elbsms_console <path to rom>");
                return;
            }

            string romPath = args[0];

            var masterSystem = new MasterSystem();

            var cartridge = GameMedia.LoadFromFile(romPath, GameMediaType.Cartridge);
            
            masterSystem.Initialise();
            masterSystem.LoadGameMedia(cartridge);

            Console.WriteLine($"Starting: {DateTime.Now}");
            Console.WriteLine();

            ulong instructionCount = 0;

            var sw = Stopwatch.StartNew();

            try
            {
                while (true)
                {
                    instructionCount++;
                    masterSystem.SingleStep();
                }
            }
            catch (InfiniteLoopException)
            {

            }
            catch (NotImplementedException ex)
            {
                Console.WriteLine(ex.Message);
            }

            sw.Stop();

            masterSystem.Shutdown();

            var cyclesExecuted = masterSystem.Clock.Timestamp;
            var effectiveClock = cyclesExecuted / (sw.ElapsedMilliseconds / 1000.0);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Finished: {DateTime.Now}");
            Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms Instructions: {instructionCount:N0}, Instructions/ms: {instructionCount / (double)sw.ElapsedMilliseconds}, Effective Clock: {FormatFrequency(effectiveClock)}");
        }

        static string FormatFrequency(double frequency)
        {
            string[] frequencyUnit = { "Hz", "KHz", "MHz", "GHz" };

            int currentFreqUnit = 0;

            while (frequency >= 1000.0)
            {
                frequency /= 1000.0;
                currentFreqUnit++;
            }

            return $"{frequency:.##} {frequencyUnit[currentFreqUnit]}";
        }

        private static byte[] LoadRomFromFile(string filename)
        {
            byte[] fileData;

            using (FileStream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // some roms have a 512 (0x200) byte header left by the dumper/copier. A normal ROM
                // will have a length a multiple of 0x4000. If we mod with 0x4000 and have a 512 byte 
                // remainder then skip the first 512 bytes of the rom.
                if ((file.Length % 0x4000) == 0x200)
                {
                    fileData = new byte[file.Length - 0x200];
                    file.Seek(0x200, SeekOrigin.Begin);
                }
                else
                {
                    /* Normal ROM */
                    fileData = new byte[file.Length];
                }

                file.Read(fileData, 0, fileData.Length);
            }

            return fileData;
        }
    }
}
