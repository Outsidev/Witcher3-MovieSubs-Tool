using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witch3rSubman
{
    class BundleFiles
    {
        public static int sonraEklenecekByte = 0;

        public string TARGET_LANG = "en";
        public int TARGET_LANG_ID = 0;
        public string path;
        public string fileOriginalLoc;
        public List<InnerFile> innerFiles;
        BinaryReader binred;
        BinaryWriter binwr;
        BinaryWriter binwrSubs;
        byte[] bb8;
        int maxBufsize = 3000000;

        string[] lines;
        int index = 0;

        public BundleFiles(string _path)
        {
            path = _path;
        }

        public void ExtractVideos(string modLoc, string xmlLoc)
        {
            //string modLoc = @"C:\GOG Games\The Witcher 3 Wild Hunt\mods\modTRMovies\content\";
            trXmlleriOku(xmlLoc);//xmli oku ve türkçe satırları al.
            List<MovieUsm> usmMovies = new List<MovieUsm>();
            List<MovieSubs> subsMovies = new List<MovieSubs>();

            using (binred = new BinaryReader(File.Open(path, FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite)))
            {
                using (binwr = new BinaryWriter(File.Open(modLoc, FileMode.Create)))
                {
                    binwr.Write(binred.ReadBytes(8));//"POTATO70"
                    uint fileSize = binred.ReadUInt32();
                    binred.ReadUInt32();
                    uint headerSz = binred.ReadUInt32();
                    binred.ReadUInt32();
                    binred.BaseStream.Position += 8;
                    uint fileCount = headerSz / 320;

                    binwr.Write(new byte[24]);

                    for (int i = 0; i < fileCount; i++)
                    {
                        long headPos = binred.BaseStream.Position;

                        string fileName = returnStrman(binred);
                        string ext = Path.GetExtension(fileName);

                        if (ext == ".usm")
                        {
                            MovieUsm usmMov = new MovieUsm();
                            usmMov.path = fileName;
                            usmMov.headerLoc = Convert.ToUInt32(binwr.BaseStream.Position);

                            binred.BaseStream.Position = headPos + 280;
                            usmMov.rsize = binred.ReadUInt32();
                            usmMov.usmLoc = binred.ReadUInt32();

                            binred.BaseStream.Position = usmMov.usmLoc+4;
                            long temppos = binred.BaseStream.Position;
                            binred.BaseStream.Position += 32;
                            uint utfuzunlugu = readBig32end(binred);
                            binred.BaseStream.Position += utfuzunlugu;
                            if (binred.ReadByte() == (byte)0x0)
                            {
                                binred.BaseStream.Position -= 2;
                                while (binred.ReadByte() == (byte)0x0)
                                {
                                    binred.BaseStream.Position -= 2;
                                }
                                binred.BaseStream.Position -= 3;
                                string uzanti = new string(binred.ReadChars(3));
                                if (uzanti != "txt")
                                {
                                    //eger bundle dosyasında altyazı yoksa atla
                                    binred.BaseStream.Position = headPos + 320;
                                    continue;
                                }
                            }

                            binred.BaseStream.Position = headPos;
                            usmMov.headerBytes = binred.ReadBytes(320);
                            binwr.Write(usmMov.headerBytes);
                            usmMovies.Add(usmMov);
                        }
                        else if (ext == ".subs" && fileName.Substring(fileName.Length - 7, 2) == TARGET_LANG)
                        {
                            MovieSubs subsMov = new MovieSubs();
                            subsMov.path = fileName;
                            subsMov.headerLoc = Convert.ToUInt32(binwr.BaseStream.Position);

                            binred.BaseStream.Position = headPos + 280;
                            subsMov.rsize = binred.ReadUInt32();
                            subsMov.subsLoc = binred.ReadUInt32();

                             binred.BaseStream.Position = headPos;
                             subsMov.headerBytes = binred.ReadBytes(320);
                            
                            binwr.Write(subsMov.headerBytes);
                            subsMovies.Add(subsMov);
                        }

                        binred.BaseStream.Position = headPos + 320;

                    }

                    for (int i = 0; i < usmMovies.Count; i++)
                    {
                        MovieUsm movie = usmMovies[i];
                        int temp = Convert.ToInt32(binwr.BaseStream.Position);
                        binwr.BaseStream.Position = movie.headerLoc + 284;
                        binwr.Write(temp);//yeni usmKonumu
                        binwr.BaseStream.Position = temp; //yeni usmKonumuna git
                        binred.BaseStream.Position = movie.usmLoc; //orjinal usmKonumu
                        InnerFile trXml = new InnerFile();
                        for (int k = 0; k < innerFiles.Count; k++)
                        {
                            if( Path.GetFileName(innerFiles[k].filPath) == Path.GetFileName(movie.path))
                            {
                                trXml = innerFiles[k];
                                long newSize = yeniTRUsmleriYaz(movie.rsize, trXml);
                                long pos = binwr.BaseStream.Position;
                                binwr.BaseStream.Position = movie.headerLoc + 276;
                                binwr.Write(Convert.ToUInt32(newSize));
                                binwr.Write(Convert.ToUInt32(newSize));
                                binwr.BaseStream.Position = pos;
                                break;
                            }
                        }

                    }

                    for (int i = 0; i < subsMovies.Count; i++)
                    {
                        MovieSubs movie = subsMovies[i];
                        int temp = Convert.ToInt32(binwr.BaseStream.Position);
                        binwr.BaseStream.Position = movie.headerLoc + 284;
                        binwr.Write(temp);//yeni subsKonumu
                        binwr.BaseStream.Position = temp; //yeni usmKonumuna git
                        binred.BaseStream.Position = movie.subsLoc; //orjinal usmKonumu
                        InnerFile trXml = new InnerFile();
                        for (int k = 0; k < innerFiles.Count; k++)
                        {
                            if (innerFiles[k].filPath == movie.path)
                            {
                                trXml = innerFiles[k];
                                long newSize = yeniTRSubslariYaz(trXml);
                                long pos = binwr.BaseStream.Position;
                                binwr.BaseStream.Position = movie.headerLoc + 276;
                                binwr.Write(Convert.ToUInt32(newSize));
                                binwr.Write(Convert.ToUInt32(newSize));
                                binwr.BaseStream.Position = pos;
                                break;
                            }
                        }

                    }


                    int finSize = Convert.ToInt32(binwr.BaseStream.Position);
                    binwr.BaseStream.Position = 8;
                    binwr.Write(finSize); binwr.BaseStream.Position = 16;
                    binwr.Write(( (usmMovies.Count+subsMovies.Count) * 320)); binred.BaseStream.Position = 20;
                    binwr.Write(binred.ReadBytes(12));
                }
            }
        }

        public int yeniTRSubslariYaz(InnerFile xmlFile)
        {
            binwr.Write(xmlFile.lines[0]);
            return xmlFile.lines[0].Length;
        }

        public long yeniTRUsmleriYaz(uint rSize, InnerFile trXml)
        {
            int replikNo = 0;
            long boyutBaslangici = binwr.BaseStream.Position;  
            long usmLoc = binred.BaseStream.Position;//binwr.BaseStream.Position;
            binwr.Write(binred.ReadBytes(2048));//CRID vsleri yaz
            while (binred.BaseStream.Position - usmLoc < rSize)
            {
                //4 4 yaz, sbtyi(altyazıları) bulana kadar
                long baseLoc = binred.BaseStream.Position;
                string dortOku = new string(binred.ReadChars(4)); //blogun uzunlugu
                uint offset = readBig32end(binred);
                if (dortOku != "@SBT")
                {
                    binred.BaseStream.Position = baseLoc;
                    tasirmadanYaz(offset + 8);
                }
                else
                {
                    binred.BaseStream.Position = baseLoc + 32;
                    char aa = binred.ReadChar();
                    if (aa.Equals('#') || aa.Equals('@'))
                    {
                        binred.BaseStream.Position = baseLoc;
                        tasirmadanYaz(offset + 8);
                    }else
                    {
                        //altyazı ise
                        binred.BaseStream.Position = baseLoc+32;
                        int dilno = binred.ReadInt32();
                        binred.BaseStream.Position = baseLoc;
                        if (dilno==TARGET_LANG_ID)
                        {//ingilizceyse degistir //ing dil no=0
                            binwr.Write(binred.ReadBytes(4));//@sbt yaz
                            int yeniOffset = 52 + trXml.lines[replikNo].Length - 8;
                            binwr.Write(toBigEndianBytes(yeniOffset));//yeni offseti yaz   
                            binred.BaseStream.Position += 4;                         
                            binwr.Write(binred.ReadBytes(24));

                            binwr.Write(binred.ReadBytes(4));//dili yaz                            
                            binwr.Write(binred.ReadBytes(12));//geç                            
                            binwr.Write(trXml.lines[replikNo].Length);//cümle uzunlugunu yaz
                            binwr.Write(trXml.lines[replikNo]);//türkçe cümleyi yazdır
                            binred.BaseStream.Position = baseLoc + offset + 8;
                            replikNo++;
                        }
                        else
                        {
                            tasirmadanYaz(offset + 8);//baska dilse atla
                        }
                        
                    }
                }
            }
            long movieYeniBoyutu = binwr.BaseStream.Position-boyutBaslangici;
           // Console.WriteLine("Position-usmLoc:"+ (binred.BaseStream.Position - usmLoc)+" /Rsize:" + rSize);
            return movieYeniBoyutu;
        }


        public void trXmlleriOku(string xml)
        {
            lines = File.ReadAllLines(xml);
            fileOriginalLoc = readLine(); readLine();

            innerFiles = new List<InnerFile>();
            while (index < lines.Length)
            {
                InnerFile infile = new InnerFile();
                infile.getFileExt(readLine());
                if (infile.ext == ".usm") infile = new UsmFile(infile);
                else if (infile.ext == ".subs") infile = new SubsFile(infile);

                while (!lines[index].Contains("</dosyaAdi>"))
                {
                    if (infile.ext == ".usm")
                        infile.getHexText(readLine());
                    else
                        infile.getHexText(readLines());
                }

                innerFiles.Add(infile);
                readLine();
                while (index < lines.Length && lines[index] == "")
                    readLine();
            }
        }


        string readLine()
        {
            string s = lines[index];
            index++;
            return s;
        }

        string readLines()
        {
            string s = "";
            while (!lines[index].Contains("</>"))
            {
                s += lines[index] + "\n";
                index++;
            }
            s += lines[index]; index++;
            return s.Remove(s.Length - 3, 3);
        }


        void tasirmadanYaz(uint redSiz)
        {//maxbufdan kucukse sorun yok, buyukse duzgun okuyup bellegi tasirmadan yazsın
            if (redSiz > maxBufsize)
                duzgunOkuyupYaz(maxBufsize, redSiz);
            else
            {
                bb8 = binred.ReadBytes(Convert.ToInt32(redSiz));
                binwr.Write(bb8);
            }
        }

        void duzgunOkuyupYaz(int maxBufSize,uint redSize)
        {   //bellegi tasirmadan ve System.OutOfMemoryException verdirmeden oku
            int bytesRead = 0;
            bb8 = new byte[maxBufSize];
            while ( (bytesRead+=binred.Read(bb8,0,bb8.Length))>0)
            {
                binwr.Write(bb8, 0, maxBufSize);
                if (bytesRead + maxBufSize > redSize)
                    break;
            }
            int kalan = Convert.ToInt32(redSize) - bytesRead;
            bb8 = binred.ReadBytes(kalan);
            binwr.Write(bb8);
            

        }

        string returnStrman(BinaryReader br)
        {
            string str = "";
            while (true)
            {
                byte k = br.ReadByte();
                if (k == 0x00)
                    return str;
                else
                    str += (char)k;
            }
        }


        uint readBig32end(BinaryReader br)
        {
            byte[] bb = br.ReadBytes(4); Array.Reverse(bb);
            uint a = BitConverter.ToUInt32(bb, 0);
            return a;
        }

        byte[] toBigEndianBytes(int i)
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
