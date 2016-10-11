using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witch3rSubman
{
    class UsmFile:InnerFile
    {
        public List<int> maxCh = new List<int>();

        public UsmFile(InnerFile parent=null)
        {
            filPath = parent.filPath;
            ext = ".usm";
            rHexLoc = parent.rHexLoc;
        }

        public override void getHexText(string line)
        {
            base.getHexText(line);
            string t = line.Substring(1, line.IndexOf('*') - 1);
            hexes.Add(Convert.ToInt64(t));
            t = line.Substring(line.IndexOf('*') + 1, line.IndexOf('>') - (line.IndexOf('*') + 1));
            maxCh.Add(Convert.ToInt32(t));
            string te = line.Substring(line.IndexOf('>') + 1, line.IndexOf("</>")-line.IndexOf('>')-1);
            lines.Add(Encoding.UTF8.GetBytes(te));
        }

        public override void kendiniYazdir(BinaryReader binred, BinaryWriter binwr)
        {
            int ekleByte = eklenecekByte();
            int newSize = binred.ReadInt32() + ekleByte; binred.ReadInt32();
            binwr.Write(BitConverter.GetBytes(newSize));//rsize
            binwr.Write(BitConverter.GetBytes(newSize));//zsize
            int newPos = binred.ReadInt32() + BundleFiles.sonraEklenecekByte;
            BundleFiles.sonraEklenecekByte += ekleByte;
            binwr.Write(BitConverter.GetBytes(newPos));
            binwr.Write(binred.ReadBytes(32));
        }

        public override void textiniYazdir(BinaryReader binred, BinaryWriter binwr)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                long orgPos = binred.BaseStream.Position;

                long sbtbasPos = binred.BaseStream.Position-48;
                binred.BaseStream.Position = sbtbasPos;
                int sbtSize = bigEndianOku(binred.ReadBytes(4));
                binred.BaseStream.Position = orgPos;

                binred.BaseStream.Position -= 4;
                int orgMaxCh = binred.ReadInt32();//ingilizce cumlenin nulla kadar uzunlugu
                if (maxCh[i] > lines[i].Length)
                {//eger cumlenin boyutu orgBoyuttan kucukse
                    binwr.BaseStream.Position -= 4;
                    binwr.Write(lines[i].Length);
                    binwr.Write(lines[i]);
                    for (int a = 0; a < (maxCh[i] - lines[i].Length); a++)
                        binwr.Write((byte)0x0);
                }else
                {
                    binwr.BaseStream.Position -= 48;//sbtnin uzunlugunu da degistirmek gerek, sbt+4+sbtuzunlugu diger dosya demek.
                    int yeniSbtSize = sbtSize+(lines[i].Length - maxCh[i]);
                    binwr.Write(toBigEndianBytes(yeniSbtSize)); //getbytes'ın big endian versiyonu

                    binwr.BaseStream.Position += 40;//charSizea git.
                    binwr.Write(BitConverter.GetBytes(lines[i].Length));
                    binwr.Write(lines[i]);
                }
                binred.BaseStream.Position = orgPos + maxCh[i];
                if(i+1 < lines.Count)
                binwr.Write(binred.ReadBytes((int)(hexes[i + 1]-binred.BaseStream.Position)));      //
            }
        }

        public int eklenecekByte()
        {
            int sum = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                int s = lines[i].Length - maxCh[i];
                sum += s < 0 ? 0: s;
            }
            if (sum < 0) sum = 0;
            return sum;
        }

        public int bigEndianOku(byte[] bb)
        {
            Array.Reverse(bb);
            int a = BitConverter.ToInt32(bb, 0);
            return a;
        }

        public byte[] toBigEndianBytes(int i)
        {
            byte[] result = new byte[4];

            result[0] = (byte)(i >> 24);
            result[1] = (byte)(i >> 16);
            result[2] = (byte)(i >> 8);
            result[3] = (byte)(i /*>> 0*/);

            return result;
        }

    }
}
