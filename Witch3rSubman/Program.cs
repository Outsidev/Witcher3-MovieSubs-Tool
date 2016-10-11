using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witch3rSubman
{
    class Program
    {
        static void Main(string[] args)
        {
            string witchLoc = witchLoc = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try{
                witchLoc = witchLoc.Substring(0, witchLoc.IndexOf("\\content"));
            }
            catch{
                Console.WriteLine("Witcher konumunu yanlış mı verdiniz? Konumu kontrol edin.");
                Console.WriteLine("İptal Edildi.");
                Console.Read();
                return;
            }
            
            Console.WriteLine("Witch3r Konumu=" + witchLoc);

            #region Parametreler

            //-xml,-witchloc, -tek;
            foreach (string arg in args)
            {
                if (arg == "-yapan")
                {
                    Console.WriteLine("Burak Sey(Grimm)");
                }else if(arg == "-ver")
                {
                    Console.WriteLine("Ver:noSubs 1.6 -bawhos -en-to-tr");                    
                    //update(1.09.16): en to tr
                }
            }

            #endregion

            //patch0 duruyorsa sil, oyunda probleme yol açıyor 
            string patch0Loc = Path.Combine(witchLoc, @"content\patch0");
            string substrLoc = Path.Combine(witchLoc, @"DLC\substr");
            if (Directory.Exists(patch0Loc))
            {
                Directory.Delete(patch0Loc, true);
            }
            if(Directory.Exists(substrLoc))
            {                
                Directory.Delete(substrLoc, true);
            }
            


            //orj bundlelar
            string c0bundleLoc = Path.Combine(witchLoc,@"content\content0\bundles\movies.bundle");
            string c4bundleLoc = Path.Combine(witchLoc, @"content\content4\bundles\movies.bundle");
            string hosblobbundleLoc = Path.Combine(witchLoc, @"DLC\ep1\content\bundles\blob.bundle");
            string bawblobbundleLoc = Path.Combine(witchLoc, @"DLC\bob\content\bundles\blob.bundle");

            //modlanacak bundlelar
            string c0mod = Path.Combine(witchLoc,@"mods\modTRMoviesc0\content\movies.bundle");
            //string c0subsDLC = Path.Combine(witchLoc, @"DLC\substr\content\movies.bundle"); //DLC TR subslari için
            string c4mod = Path.Combine(witchLoc, @"mods\modTRMoviesc4\content\movies.bundle");
            string hosmod = Path.Combine(witchLoc, @"mods\modTRMoviesHOS\content\blob.bundle");
            string bawmod = Path.Combine(witchLoc, @"mods\modTRMoviesBAW\content\blob.bundle");

            //turkce xmller altyazıları
            string c0Xml = Path.Combine(witchLoc, @"content\content12\movies.bundle-Altyazilar.xml");
            string c4Xml = Path.Combine(witchLoc, @"content\content12\movies.bundle2-Altyazilar.xml");
            string hosxml = Path.Combine(witchLoc, @"content\content12\blob.bundle-Altyazilar.xml");
            string bawxml = Path.Combine(witchLoc, @"content\content12\blobBAW.bundle-Altyazilar.xml");

            //modlanan bundle metadatalari
            string c0metadataLoc = Path.Combine(witchLoc, @"mods\modTRMoviesc0\content\metadata.store");
            string c4metadataLoc = Path.Combine(witchLoc, @"mods\modTRMoviesc4\content\metadata.store");
            string hosmetadataLoc = Path.Combine(witchLoc, @"mods\modTRMoviesHOS\content\metadata.store");
            string bawmetadataLoc = Path.Combine(witchLoc, @"mods\modTRMoviesBAW\content\metadata.store");

            Metadata meth = new Metadata();

            BundleFiles c0bund = new BundleFiles(c0bundleLoc);
            BundleFiles c4bund = new BundleFiles(c4bundleLoc);
            BundleFiles hosblobbund = new BundleFiles(hosblobbundleLoc);
            BundleFiles bawblobbund = new BundleFiles(bawblobbundleLoc);

            
            Console.WriteLine("C0-Movies.bundle Çıkarılıyor...");
            c0bund.ExtractVideos(c0mod, c0Xml);

            Console.WriteLine("C0-Mod Metadata*");
            meth.doIT(c0mod, c0metadataLoc);


            Console.WriteLine("C4-Movies.bundle Çıkarılıyor...");
            c4bund.ExtractVideos(c4mod, c4Xml);

            Console.WriteLine("C4-Mod Metadata*");
            meth.doIT(c4mod, c4metadataLoc);

            if (!File.Exists(hosblobbundleLoc))
            {
                Console.WriteLine("Hearts of Stone DLC'si bulunamadı, geçildi.");
                DirectoryInfo di = new DirectoryInfo(Path.Combine(witchLoc,@"mods\modTRMoviesHOS"));
                foreach (FileInfo fil in di.GetFiles())
                {
                    fil.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                di.Delete();
            }
            else if(File.Exists(hosblobbundleLoc))
            { 
                Console.WriteLine("Hos-Blob.bundle Çıkarılıyor...");
                hosblobbund.ExtractVideos(hosmod, hosxml);

                Console.WriteLine("Hos-Mod Metadata*");
                meth.doIT(hosmod, hosmetadataLoc);
            }
            
            if (!File.Exists(bawblobbundleLoc))
            {
                Console.WriteLine("Blood and Wine DLC'si bulunamadı, geçildi.");
                DirectoryInfo di = new DirectoryInfo(Path.Combine(witchLoc, @"mods\modTRMoviesBAW"));
                foreach (FileInfo fil in di.GetFiles())
                {
                    fil.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                di.Delete();
            }
            else if (File.Exists(bawblobbundleLoc))
            {
                Console.WriteLine("Baw-Blob.bundle Çıkarılıyor...");
                bawblobbund.ExtractVideos(bawmod, bawxml);

                Console.WriteLine("Baw-Mod Metadata*");
                meth.doIT(bawmod, bawmetadataLoc);
            }
            

            Console.WriteLine("Bitti! Bir tuşa basın.");
            Console.ReadKey();
        }
    }
}
