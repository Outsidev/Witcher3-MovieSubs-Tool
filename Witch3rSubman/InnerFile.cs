using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witch3rSubman
{
    class InnerFile
    {
        public string filPath;
        public string ext;

        public List<long> hexes = new List<long>();
        public long rHexLoc;//rsize loc, (yani data posa goturecek hexin yeri)
        public List<byte[]> lines = new List<byte[]>();

        public InnerFile()
        {

        }

        public virtual void getHexText(string line)
        {
        }
        public virtual void kendiniYazdir(BinaryReader binred, BinaryWriter binwr)
        {
        }
        public virtual void textiniYazdir(BinaryReader binred, BinaryWriter binwr)
        { }

        public List<byte[]> getData()
        {
            return lines;
        }

        public void getFileExt(string line)
        {
            rHexLoc = Convert.ToInt64(line.Substring(12, line.IndexOf('>')-12));
            filPath = line.Remove(0,line.IndexOf('>')+1);
            int n = line.IndexOf('.');          
            ext = line.Substring(n, line.Length - n);
        }

    }
}
