﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace elbsms_core.Memory
{
    public enum RegionCode
    {
        [Description("SMS Japan")] SMSJapan = 0x03,
        [Description("SMS Export")] SMSExport = 0x04,
        [Description("GG Japan")] GGJapan = 0x05,
        [Description("GG Export")] GGExport = 0x06,
        [Description("GG International")] GGInternational = 0x07,
    }

    public class CartridgeHeader
    {
        private static readonly Dictionary<int, (string Description, uint Size)> RomSizes = new Dictionary<int, (string, uint)>
        {
            { 0x0A, ("8KB", 0x2000) },
            { 0x0B, ("16KB", 0x4000) },
            { 0x0C, ("32KB", 0x8000) },
            { 0x0D, ("48KB", 0xC000) },
            { 0x0E, ("64KB", 0x10000) },
            { 0x0F, ("128KB", 0x20000) },
            { 0x00, ("256KB", 0x40000) },
            { 0x01, ("512KB", 0x80000) },
            { 0x02, ("1MB", 0x100000) },
        };

        public string Header;
        public int Checksum;
        public int ProductCode;
        public int Version;
        public RegionCode Region;
        public int RomSize;

        public int ActualRomSize;
        public bool RomSizeValid => RomSizes[RomSize].Size == ActualRomSize;
        public string RomSizeDescription => RomSizes[RomSize].Description;

        public CartridgeHeader(byte[] romData)
        {
            Header = Encoding.ASCII.GetString(romData, 0x7FF0, 8);
            Checksum = BitConverter.ToUInt16(romData, 0x7FFA);
            ProductCode = ReadProductCode(romData, 0x7FFC);
            Version = romData[0x7FFE] & 0x0F;
            Region = (RegionCode)(romData[0x7FFF] >> 4);
            RomSize = romData[0x7FFF] & 0x0F;

            ActualRomSize = romData.Length;
        }

        private int ReadProductCode(byte[] romData, int index)
        {
            return DecodeBCD(new byte[] { romData[index], romData[index + 1], (byte)(romData[index + 2] >> 4) });
        }

        // https://stackoverflow.com/a/11701256/223708
        private int DecodeBCD(byte[] data)
        {
            int result = 0;

            for (int i = data.Length - 1; i >= 0; i--)
            {
                result *= 100;
                result += (10 * (data[i] >> 4));
                result += data[i] & 0x0F;
            }

            return result;
        }
    }
}