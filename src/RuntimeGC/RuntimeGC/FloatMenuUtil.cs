using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using Toolbox;
using LudeonTK;

namespace RuntimeGC
{
    /// <summary>
    /// Utility class for managing float menus in RuntimeGC.
    /// </summary>
    public static class FloatMenuUtil
    {
        /// <summary>
        /// Stores groups of float menu item labels.
        /// </summary>
        private static readonly Dictionary<string, List<string>> groups;
        /// <summary>
        /// Indicates if a float menu item is for developer mode only.
        /// </summary>
        private static readonly Dictionary<string, bool> devOnly;
        /// <summary>
        /// Stores actions associated with float menu item labels.
        /// </summary>
        private static readonly Dictionary<string, Action> items;

        /// <summary>
        /// Group identifier for general tools.
        /// </summary>
        public static readonly string GroupTools = "tools";
        /// <summary>
        /// Group identifier for fix-related tools.
        /// </summary>
        public static readonly string GroupFix = "fix";
        /// <summary>
        /// Group identifier for quickbar actions.
        /// </summary>
        public static readonly string GroupQuickbar = "quickbar";

        /// <summary>
        /// Group identifier for memory monitor update modes.
        /// </summary>
        public static readonly string GroupMMUpdateMode = "mmupdate";

        static FloatMenuUtil()
        {
            groups = new Dictionary<string, List<string>>();
            items = new Dictionary<string, Action>();
            devOnly = new Dictionary<string, bool>();

            string group;

            group = GroupFix;
            Add(group, "FloatDebugLog".Translate(), delegate {
            {
                bool flag = !Find.WindowStack.TryRemove(typeof(EditWindow_Log), true);
                bool flag2 = flag;
                if (flag2)
                {
                    Find.WindowStack.Add(new EditWindow_Log());
                }
            }});
            Add(group, "FloatAGRegen".Translate(), delegate {
                foreach (Map map in Find.Maps)
                {
                    map.avoidGrid.Regenerate();
                }
                Message("MsgTextAGR".Translate(), MessageTypeDefOf.PositiveEvent);
            });
            Add(group, "FloatFactionFixItems1".Translate(), delegate {
                CleanserUtil.FixFactionRelationships();
                Message("MsgTextFFR".Translate(), MessageTypeDefOf.PositiveEvent);
                });
            Add(group, "FloatFactionFixItems2".Translate(), delegate {
                Message("MsgTextFFL".Translate(CleanserUtil.FixFactionLeader_Wrapped()), MessageTypeDefOf.PositiveEvent);
                });

            group = GroupTools;
            Add(group, "FloatToolsItems1".Translate(), delegate {
                int a = CleanserUtil.DeconstructAnimalFamily();
                Verse.Log.Message("CleanserUtil.DeconstructAnimalFamily():Round 1 completed.");
                CleanserUtil.DeconstructAnimalFamily();
                Verse.Log.Message("CleanserUtil.DeconstructAnimalFamily():Round 2 completed.");
                Message("MsgTextAFT".Translate(a), MessageTypeDefOf.PositiveEvent);
                });

            Add(group, "FloatToolsItems2".Translate(), delegate {
                Message("MsgTextRFH".Translate(CleanserUtil.RemoveFilth(Find.CurrentMap, true)), MessageTypeDefOf.PositiveEvent);
                });
            Add(group, "FloatToolsItems2Dev1".Translate(), delegate {
                Message("MsgTextRFM".Translate(CleanserUtil.RemoveFilth(Find.CurrentMap, false)), MessageTypeDefOf.PositiveEvent);
            }, true);
            Add(group, "FloatToolsItems2Dev2".Translate(), delegate {
                int i = 0;
                foreach (Map m in Find.Maps)
                    i += CleanserUtil.RemoveFilth(m, false);
                Message("MsgTextRFW".Translate(i), MessageTypeDefOf.PositiveEvent);
            }, true);

            Add(group, "FloatToolsItems2Snow".Translate(), delegate {
                CleanserUtil.RemoveSnow(Find.CurrentMap, true);
                Message("MsgTextRSH".Translate(), MessageTypeDefOf.PositiveEvent);
            });
            //Add(group, "FloatToolsItems2SnowDev1".Translate(), delegate {
            //    CleanserUtil.RemoveSnow(Find.CurrentMap, false);
            //    Message("MsgTextRSM".Translate(), MessageTypeDefOf.PositiveEvent);
            //}, true);
            //Add(group, "FloatToolsItems2SnowDev2".Translate(), delegate {
            //    foreach (Map m in Find.Maps)
            //        CleanserUtil.RemoveSnow(m, false);
            //    Message("MsgTextRSW".Translate(), MessageTypeDefOf.PositiveEvent);
            //}, true);

            Add(group, "FloatToolsItems3".Translate(), delegate {
                Message("MsgTextRCM".Translate(CleanserUtil.RemoveCorpses()), MessageTypeDefOf.NeutralEvent);
                });
            Add(group, "FloatToolsItems4".Translate(), delegate {
                Message("MsgTextRBL".Translate(CleanserUtil.RemoveAllBattleLogEntries()), MessageTypeDefOf.PositiveEvent);
                });
            
            Add(group, "FloatToolsItems5".Translate(), delegate {
                Message("MsgTextRAM".Translate(CleanserUtil.RemoveIArchivable(false)), MessageTypeDefOf.PositiveEvent);
            });
            Add(group, "FloatToolsItems5Dev".Translate(), delegate {
                Message("MsgTextRAM".Translate(CleanserUtil.RemoveIArchivable(true)), MessageTypeDefOf.PositiveEvent);
            },true);


            

            group = GroupQuickbar;
            Add(group, "QuickCloseLetStack".Translate(), delegate {
                LetterStack ls = Find.LetterStack;
                if (ls == null) return;
                for (int i = ls.LettersListForReading.Count - 1; i > -1; i--)
                    ls.RemoveLetter(ls.LettersListForReading[i]);
            });
            Add(group, "QuickUnlockSpeedLimit".Translate(), delegate {
                CleanserUtil.UnlockNormalSpeedLimit();
                Find.WindowStack.TryRemove(typeof(UserInterface));
            });
            Add(group, "QuickOpenSettings".Translate(), delegate {
                CleanserUtil.OpenModSettingsPage();
            });


            group = GroupMMUpdateMode;
            foreach (MemoryMonitorUpdateMode m in Enum.GetValues(typeof(MemoryMonitorUpdateMode)))
                Add(group, ("MMUpdate_" + m.ToString()).Translate(), delegate
                {
                    RuntimeGC.Settings.MemoryMonitorUpdateInterval = (int)m;
                    UIUtil.Notify_MMBtnLabelChanged();
                }, m.ToString().Contains("Debug_"));
        }

        /// <summary>
        /// Adds a new item to a float menu group.
        /// </summary>
        /// <param name="group">The group the item belongs to.</param>
        /// <param name="label">The label of the float menu item.</param>
        /// <param name="action">The action to perform when the item is selected.</param>
        /// <param name="devonly">Whether the item is only visible in developer mode.</param>
        public static void Add(string group,string label,Action action,bool devonly = false)
        {
            if (!groups.ContainsKey(group))
                groups.Add(group, new List<string>());
            groups[group].Add(label);
            items.Add(label, action);
            devOnly.Add(label, devonly);
        }

        /// <summary>
        /// Generates and displays a float menu for a specific group.
        /// </summary>
        /// <param name="group">The identifier of the group to generate the menu for.</param>
        public static void GenerateFloatMenuGroup(string group)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach(string label in groups[group])
            {
                if (devOnly[label] && (!Prefs.DevMode)) continue;
                list.Add(new FloatMenuOption(label, items[label]));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        /// <summary>
        /// Generates and displays float menu options related to memory reclamation.
        /// </summary>
        public static void GenerateMemoryReclaimOptions()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            FloatMenuOption option;
            option = new FloatMenuOption("FloatACModMetaData".Translate(), Toolbox.Toolbox.CleanModMetaData);
            if (Toolbox.Toolbox.Cleaned)
            {
                option.Label = "FloatACModMetaDataCleared".Translate();
                option.Disabled = true;
            }
            list.Add(option);
            option = new FloatMenuOption("FloatACLanguageData".Translate(), LanguageDataCleaner.CleanLanguageData);
            if (LanguageDataCleaner.Cleaned)
            {
                option.Label = "FloatACLanguageDataCleared".Translate();
                option.Disabled = true;
            }
            list.Add(option);
            option = new FloatMenuOption("FloatACDefPackage".Translate(), DefPackageCleaner.CleanDefPackage);
            if (DefPackageCleaner.Cleaned)
            {
                option.Label = "FloatACDefPackageCleared".Translate();
                option.Disabled = true;
            }
            list.Add(option);
            Find.WindowStack.Add(new FloatMenu(list));
        }

        /// <summary>
        /// Displays a message to the user.
        /// </summary>
        /// <param name="str">The message string.</param>
        /// <param name="type">The type of message (e.g., PositiveEvent, NegativeEvent).</param>
        public static void Message(string str,MessageTypeDef type)
        {
            Messages.Message(str,type,RuntimeGC.Settings.ArchiveMessageGeneral);
        }
    }
}
