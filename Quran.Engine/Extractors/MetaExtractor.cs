using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ProtoBuf;
using Quran.Model;

namespace Quran.Engine
{
    public static class MetaExtractor
    {
        public static void Extract()
        {
            XDocument xml = XDocument.Load("../../In/quran-data.xml");

            Meta meta = new Meta();

            var elements = xml.Root.Elements() as IEnumerable<XElement>;

            LoadSuras(xml, meta);

            meta.Juzs = LoadPartMetas(xml, "juzs");
            meta.Hizbs = LoadPartMetas(xml, "hizbs");
            meta.Manzils = LoadPartMetas(xml, "manzils");
            meta.Rukus = LoadPartMetas(xml, "rukus");
            meta.Pages = LoadPartMetas(xml, "pages");
            meta.Qaris = new Qari[] { 
                new Qari() { ID=0, Name = "بدون قرائت", EnglishName = "No Qari", Availability = new bool[114] },
                new Qari() { ID=1, Name = "ماهر المعيقلي", EnglishName = "Maher Almuaiqly", Availability = new bool[114] },
                new Qari() { ID=2, Name = "محمد صديق المنشاوي", EnglishName = "Muhammad Siddeeq al-Minshawi", Availability = new bool[114] },
                new Qari() { ID=3, Name = "عبدالباسط عبدالصمد", EnglishName = "AbdulBaset AbdulSamad", Availability = new bool[114] },
                new Qari() { ID=4, Name = "هاني الرفاعي", EnglishName = "Hani ar-Rifai", Availability = new bool[114] },
                new Qari() { ID=5, Name = "مشاري بن راشد العفاسي", EnglishName = "Mishaari Raashid al-Aafaasee", Availability = new bool[114] },
                new Qari() { ID=6, Name = "صلاح الهاشم", EnglishName = "Salah Al-Hashem", Availability = new bool[114] },
                new Qari() { ID=7, Name = "سعد الغامدي", EnglishName = "Saad al-Ghamdi", Availability = new bool[114] } };

            meta.Translators = new Translator[] { 
                new Translator() { ID = "fa.makarem", Name = "مکارم شیرازی" } };

            Engine.SaveMeta(meta);
        }


        private static void LoadSuras(XDocument xml, Meta meta)
        {
            List<SuraMeta> tempList = new List<SuraMeta>();

            foreach (XElement node in xml.Root.Elements("suras").DescendantNodes())
            {
                SuraMeta sura = new SuraMeta();

                sura.SuraNo = Int32.Parse(node.Attribute("index").Value);
                sura.TotalAyas = Int32.Parse(node.Attribute("ayas").Value);
                sura.NameArabic = node.Attribute("name").Value;
                sura.NameEnglish = node.Attribute("ename").Value;
                sura.IsMeccan = (node.Attribute("type").Value == "Meccan");
                sura.Order = Int32.Parse(node.Attribute("order").Value);
                sura.FullNameArabic = string.Format( "{0}   {1}", Utility.GetSuraNo(sura.SuraNo), sura.NameArabic ) ;
                tempList.Add(sura);
            }

            meta.Suras = tempList.ToArray();
        }

        private static PartMeta[] LoadPartMetas(XDocument xml, string name )
        {
            List<PartMeta> tempList = new List<PartMeta>();
            foreach (XElement node in xml.Root.Elements(name).DescendantNodes())
            {
                PartMeta partMeta = new PartMeta();

                partMeta.Index = Int32.Parse(node.Attribute("index").Value);
                partMeta.Sura = Int32.Parse(node.Attribute("sura").Value);
                partMeta.Aya = Int32.Parse(node.Attribute("aya").Value);

                tempList.Add(partMeta);
            }

            return tempList.ToArray();
        }

    }
}
