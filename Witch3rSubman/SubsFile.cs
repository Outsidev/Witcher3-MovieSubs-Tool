using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witch3rSubman
{
    class SubsFile:InnerFile
    {

        public SubsFile(InnerFile parent=null)
        {
            filPath = parent.filPath;
            ext = ".subs";
            rHexLoc = parent.rHexLoc;
        }

        public override void kendiniYazdir(BinaryReader binred, BinaryWriter binwr)
        {
            int newSize = lines[0].Length; binred.ReadInt32(); binred.ReadInt32();
            binwr.Write(BitConverter.GetBytes(newSize));//rsize
            binwr.Write(BitConverter.GetBytes(newSize));//zsize
            int newPos = binred.ReadInt32()+BundleFiles.sonraEklenecekByte;
            //sonraEklenecekByte = 0;
            binwr.Write(BitConverter.GetBytes(newPos));
            binwr.Write(binred.ReadBytes(32));
        }

        public override void textiniYazdir(BinaryReader binred, BinaryWriter binwr)
        {
                binwr.Write(lines[0]);
            binred.BaseStream.Position += lines[0].Length;
        }

        public override void getHexText(string line)
        {
            string t = line.Substring(1, line.IndexOf('>') - 1);
            hexes.Add(Convert.ToInt64(t));
            // int a = line.IndexOf("</>");
            string str = line.Substring(line.IndexOf('>') + 1, line.Length - line.IndexOf('>') - 1);
            byte[] encodedStr = (Encoding.Unicode.GetBytes(str));
            List<byte> nbi = new List<byte>(); //normalde newlineda 0d 0a gelmesi gerek, 
            //ama normal ecodingde sadece 0a geliyor. onu düzmek 0a gorulen yerden evvel 0d koydur.
            for (int i = 0; i < encodedStr.Length; i++)
            {
                if (encodedStr[i] == (byte)10)
                { 
                    nbi.Add((byte)13);
                    nbi.Add((byte)0);
                }
                nbi.Add(encodedStr[i]);
            }
            lines.Add(nbi.ToArray());

        }


    }
}
