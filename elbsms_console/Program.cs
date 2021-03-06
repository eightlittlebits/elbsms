﻿using System;
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
    }
}
