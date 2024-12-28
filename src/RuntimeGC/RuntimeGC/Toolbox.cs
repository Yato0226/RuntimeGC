using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System;

namespace Toolbox
{
    internal class Toolbox
    {
        public static bool Cleaned { get; internal set; }

        internal static void CleanModMetaData()
        {
            throw new NotImplementedException();
        }
    }
    public static class ModMetaDataCleaner
    {
        private static int cacheMetaDataCount = -1;
        public static bool Cleaned => cacheMetaDataCount >= ModLister.AllInstalledMods.Count();

        public static void CleanModMetaData()
        {
            FieldInfo field = typeof(ModLister).GetField("mods", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            List<ModMetaData> value = (List<ModMetaData>)field.GetValue(null);
            int num = 0;
            int num2 = 0;
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            for (int i = value.Count - 1; i > -1; i--)
            {
                bool active = value[i].Active;
                bool flag2 = active;
                if (flag2)
                {
                    GenText.AppendWithComma(stringBuilder2, value[i].Name);
                    value[i].UnsetPreviewImage();
                    num2++;
                }
                else
                {
                    GenText.AppendWithComma(stringBuilder, value[i].Name);
                    value.RemoveAt(i);
                    num++;
                }
            }
            Log.Message(string.Concat(new string[]
    {
        "[ModMetaDataCleaner] Removed ",
        num.ToString(),
        " Metadata and cleaned ",
        num2.ToString(),
        " PreviewImage.\nRemoved: ",
        stringBuilder.ToString(),
        "\nCleaned: ",
        stringBuilder2.ToString()
    }));
            bool flag = Current.ProgramState == ProgramState.Playing;
            bool flag3 = flag;
            if (flag3)
            {
                Messages.Message(TranslatorFormattedStringExtensions.Translate("MsgModMetaDataCleaned", num, num2), MessageTypeDefOf.PositiveEvent, false);
            }
            field.SetValue(null, value);
            ModMetaDataCleaner.cacheMetaDataCount = value.Count;
            System.GC.Collect(2);
        }
    }

    public static class LanguageDataCleaner
    {
        private static int cacheLanguageDataCount = -1;
        public static bool Cleaned => cacheLanguageDataCount >= LanguageDatabase.AllLoadedLanguages.Count();

        public static void CleanLanguageData()
        {
            FieldInfo languages = typeof(LanguageDatabase).GetField("languages", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            List<LoadedLanguage> list = (List<LoadedLanguage>)languages.GetValue(null);
            int a = 0, b = 0;
            StringBuilder s1 = new StringBuilder();
            for (int i = list.Count - 1; i > -1; i--)
                if (list[i] != LanguageDatabase.activeLanguage && list[i] != LanguageDatabase.defaultLanguage)
                {
                    s1.AppendWithComma(list[i].FriendlyNameNative);
                    list.RemoveAt(i);
                    a++;
                }
                else
                {
                    b += list[i].defInjections.Count;
                    list[i].defInjections = new List<DefInjectionPackage>();
                }
            Verse.Log.Message("[LanguageDataCleaner] Removed " + a + " LoadedLanguages and cleaned " + b + " DefInjectionPackages.\nRemoved Languages: " + s1.ToString());
            if (Current.ProgramState == ProgramState.Playing)
                Messages.Message("MsgLanguageDataCleaned".Translate(a, b), MessageTypeDefOf.PositiveEvent, false);
            languages.SetValue(null, list);
            cacheLanguageDataCount = list.Count;
            System.GC.Collect(2);
        }
    }

    public static class DefPackageCleaner
    {
        private static ModContentPack coreMod = null;
        public static bool Cleaned => coreMod != null && coreMod.AllDefs.Count() == 0;

        public static void CleanDefPackage()
        {
            FieldInfo defPackages = typeof(ModContentPack).GetField("defPackages", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            int a = 0;
            foreach (ModContentPack pack in LoadedModManager.RunningMods)
            {
                if (pack.IsCoreMod) coreMod = pack;
                a += ((List<Def>)defPackages.GetValue(pack)).Count;
                defPackages.SetValue(pack, new List<Def>());
            }

            Verse.Log.Message("[DefPackageCleaner] Cleaned " + a + " DefPackages.");
            if (Current.ProgramState == ProgramState.Playing)
                Messages.Message("MsgDefPackageCleaned".Translate(a), MessageTypeDefOf.PositiveEvent, false);
            System.GC.Collect(2);
        }
    }

    public static class Launcher
    {
        public static void Launch(bool modMetaData,bool languageData,bool defPackage)
        {
            if (modMetaData)
                LongEventHandler.QueueLongEvent(ModMetaDataCleaner.CleanModMetaData, "Reclaiming Memory", false, null);
            if (languageData)
                LongEventHandler.QueueLongEvent(LanguageDataCleaner.CleanLanguageData, "Reclaiming Memory", false, null);
            if (defPackage)
                LongEventHandler.QueueLongEvent(DefPackageCleaner.CleanDefPackage, "Reclaiming Memory", false, null);
        }
    }
}
