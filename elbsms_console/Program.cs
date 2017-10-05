using System;
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

            while (true)
            {
                masterSystem.SingleStep();
            }
        }
    }
}
