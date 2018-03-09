using System;
using System.Diagnostics;
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

            Cartridge cartridge = Cartridge.LoadFromFile(romPath);

            MasterSystem masterSystem = new MasterSystem(cartridge);

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

            var cyclesExecuted = masterSystem.Clock.Timestamp;
            var effectiveClock = cyclesExecuted / (sw.ElapsedMilliseconds / 1000.0);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Finished: {DateTime.Now}");
            Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms Instructions: {instructionCount:N0}, Instructions/ms: {instructionCount/(double)sw.ElapsedMilliseconds}, Effective Clock: {FormatFrequency(effectiveClock)}");
        }

        private enum FrequencyUnit
        {
            Hz,
            KHz,
            MHz,
            GHz
        }

        static string FormatFrequency(double frequency)
        {
            int freqUnit = 0;

            while (frequency >= 1000.0)
            {
                frequency /= 1000.0;
                freqUnit++;
            }

            return $"{frequency:.##} {(FrequencyUnit)freqUnit}";
        }
    }
}
