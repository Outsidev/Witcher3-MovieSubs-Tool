using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witch3rSubman
{

    class Headerler
    {
        public string adi;
        public int size;
        public int zsize;
        public int gotoPos;
        public int sira;

        public Headerler()
        {

        }
    }

    class Metadata
    {
        Headerler[] headerler;
        BinaryReader binredMeta;
        BinaryReader binredBund;
        BinaryWriter binwrit;

        public Metadata()
        {
            
        }

        public void doIT(string bund, string meta)
        {
            bundleHeaderleriOku(bund);
            metadataOkuYaz(meta);
        }

        void bundleHeaderleriOku(string bund)
        {
            using(binredBund = new BinaryReader(File.Open(bund, FileMode.Open)) )
            {
                binredBund.BaseStream.Position += 16;
                int headerCount = (binredBund.ReadInt32()/320);
                binredBund.BaseStream.Position += 12;

                headerler = new Headerler[headerCount];
                for (int i = 0; i < headerCount; i++)
                {
                    long origPos = binredBund.BaseStream.Position;

                    Headerler hed = new Headerler();
                    string adi = noktayaDekOku(binredBund);
                    hed.adi = adi.Remove(adi.Length-1);
                    binredBund.BaseStream.Position = origPos+276;
                    hed.zsize = binredBund.ReadInt32();
                    hed.size = binredBund.ReadInt32();
                    hed.gotoPos = binredBund.ReadInt32();
                    binredBund.BaseStream.Position += 32;
                    headerler[i] = hed;
                }

            }

        }

        public void metadataOkuYaz(string meta)
        {

            using (binredMeta = new BinaryReader(File.Open(meta, FileMode.Open,FileAccess.Read,FileShare.ReadWrite)))
            {
                using (binwrit = new BinaryWriter(File.Open(meta, FileMode.Open, FileAccess.Write,FileShare.Read)))
                { 
                    binredMeta.BaseStream.Position = 20;//ilk 19geç, yazılara gel
                    int slashliSay = 0, tsay=0;
                    string kaka = "", leman = "";
                    while( (kaka = noktayaDekOku(binredMeta)).Length > 1 )
                    {
                        leman = kaka;
                        slashliSay++;
                    }
                    tsay = 0;
                    while ((kaka = noktayaDekOku(binredMeta)).Length > 1)
                    {
                        leman = kaka;
                        tsay++;
                    }
                    //yazıları geçti
                    int sonlandirmaByteUzunlugu = leman.Length - 1;
                    binredMeta.BaseStream.Position -= 1+leman.Length;
                    int sonlanmaNo = binredMeta.ReadInt32();
                    binredMeta.BaseStream.Position -= 4 - sonlandirmaByteUzunlugu;
                    binredMeta.BaseStream.Position += 32;
                    //32bytelıklar slashlılar
                    //buradan noları keşfet
                    int say = 0;
                    while(binredMeta.ReadInt32() != sonlanmaNo)
                    {
                        binredMeta.BaseStream.Position -= 4;
                        int adBasPos = binredMeta.ReadInt32();
                        string myName = getNameFromStart(binredMeta,adBasPos);
                        int selected = -1;
                        for (int jj = 0; jj < headerler.Length; jj++)
                        {
                            if(headerler[jj].adi == myName)
                            {
                                selected = jj;
                                break;
                            }
                        }
                        binredMeta.ReadInt32();//?

                        binwrit.BaseStream.Position = binredMeta.BaseStream.Position;
                        binwrit.Write(headerler[selected].size);//yenisizeYaz
                        binwrit.Write(headerler[selected].size);//yenisizeYaz
                        int size = binredMeta.ReadInt32(); binredMeta.ReadInt32();//gene size
                        int siraNo = binredMeta.ReadInt32();
                        headerler[selected].sira = siraNo;
                        binredMeta.BaseStream.Position += 12;
                        say++;
                    }

                    headerler = headerler.OrderBy(p => p.sira).ToArray();
                    //20bytelıklar gotoPosların yerleri
                    //binredMeta.BaseStream.Position -= 4 - sonlandirmaByteUzunlugu;
                    binredMeta.BaseStream.Position += 17;//+4byte
                    int i = 0;
                    int sizeToplami = 0;
                    while (i < say)
                    {
                        int siraNo = binredMeta.ReadInt32(); binredMeta.ReadInt32();
                        binwrit.BaseStream.Position = binredMeta.BaseStream.Position;
                        binwrit.Write(headerler[i].gotoPos);//yeniusm                        
                        binwrit.Write(headerler[i].size);//yenisize
                        sizeToplami += headerler[i].size;                  
                        int usmLoc = binredMeta.ReadInt32();                        
                        int size = binredMeta.ReadInt32();
                        binredMeta.ReadInt32();
                        i++;
                    }

                    binwrit.BaseStream.Position = binredMeta.BaseStream.Position + 25 + 4*3;
                    binwrit.Write(sizeToplami);
                                        
                }
            }
        }

        public string getNameFromStart(BinaryReader binred,int startPos)
        {
            long orjPos = binred.BaseStream.Position;
            binred.BaseStream.Position = startPos+18;
            string aa = noktayaDekOku(binred);
            binred.BaseStream.Position = orjPos;
            return aa.Remove(aa.Length - 1);
        }

        public string noktayaDekOku(BinaryReader binred)
        {
            byte b;
            string ss="";
            do
            { 
                b = binred.ReadByte();
                ss += (char)b;
            } while (b != 0x00) ;

            return ss;
        }
    }
}
