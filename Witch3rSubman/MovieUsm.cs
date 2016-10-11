using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witch3rSubman
{
    class MovieUsm
    {
        public string path;

        public byte[] headerBytes;
        public byte[] contentBytes;

        public uint rsize;
        public uint headerLoc;
        public uint usmLoc;

        public MovieUsm()
        {

        }

    }
}
