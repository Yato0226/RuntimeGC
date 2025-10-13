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
    /// <summary>
    /// Provides utility methods for various tasks.
    /// </summary>
    internal class Toolbox
    {
        /// <summary>
        /// Gets or sets a value indicating whether something has been cleaned.
        /// </summary>
        public static bool Cleaned { get; internal set; }

        /// <summary>
        /// Cleans mod metadata. (Not yet implemented)
        /// </summary>
        internal static void CleanModMetaData()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Provides methods for cleaning up mod metadata.
    /// </summary>
    public static class ModMetaDataCleaner
    {
        private static int cacheMetaDataCount = -1;
        /// <summary>
        /// Gets a value indicating whether mod metadata has been cleaned.
        /// </summary>
        public static bool Cleaned => cacheMetaDataCount >= ModLister.AllInstalledMods.Count();

        /// <summary>
        /// Cleans up inactive mod metadata and associated preview images.
        /// </summary>
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

    /// <summary>
    /// Provides methods for cleaning up language data.
    /// </summary>
    public static class LanguageDataCleaner
    {
        private static int cacheLanguageDataCount = -1;
        /// <summary>
        /// Gets a value indicating whether language data has been cleaned.
        /// </summary>
        public static bool Cleaned => cacheLanguageDataCount >= LanguageDatabase.AllLoadedLanguages.Count();

        /// <summary>
        /// Cleans up unused language data and def injection packages.
        /// </summary>
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

    /// <summary>
    /// Provides methods for cleaning up def packages.
    /// </summary>
    public static class DefPackageCleaner
    {
        private static ModContentPack coreMod = null;
        /// <summary>
        /// Gets a value indicating whether def packages have been cleaned.
        /// </summary>
        public static bool Cleaned => coreMod != null && coreMod.AllDefs.Count() == 0;

        /// <summary>
        /// Cleans up def packages from all running mods.
        /// </summary>
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

    /// <summary>
    /// Provides methods to launch cleaning operations.
    /// </summary>
    public static class Launcher
    {
        /// <summary>
        /// Queues long events to clean mod metadata, language data, and def packages.
        /// </summary>
        /// <param name="modMetaData">Whether to clean mod metadata.</param>
        /// <param name="languageData">Whether to clean language data.</param>
        /// <param name="defPackage">Whether to clean def packages.</param>
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
