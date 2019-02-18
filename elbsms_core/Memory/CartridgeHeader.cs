using System;
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
        public string Header;
        public int Checksum;
        public int ProductCode;
        public int Version;
        public RegionCode Region;
        public int HeaderRomSize;

        public int ActualRomSize;

        public CartridgeHeader(byte[] romData)
        {
            Header = Encoding.ASCII.GetString(romData, 0x7FF0, 8);
            Checksum = BitConverter.ToUInt16(romData, 0x7FFA);
            ProductCode = ReadProductCode(romData, 0x7FFC);
            Version = romData[0x7FFE] & 0x0F;
            Region = (RegionCode)(romData[0x7FFF] >> 4);
            // TODO(david): Read size from the cartridge header

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