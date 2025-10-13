using System;
using Verse;

namespace RuntimeGC
{
    /// <summary>
    /// Represents the settings for the RuntimeGC mod.
    /// </summary>
    public class RuntimeGCSettings:ModSettings
    {
        private bool EnableMemoryUsageBar;
        private int MemoryUsageBarLowerBoundMb;
        private int MemoryUsageBarUpperBoundMb;
        private int MemoryUsageUpdateInterval;
        private bool EnableMemoryUsageTip;

        /// <summary>
        /// Whether to automatically clean mod metadata.
        /// </summary>
        public bool AutoCleanModMetaData;
        /// <summary>
        /// Whether to automatically clean language data.
        /// </summary>
        public bool AutoCleanLanguageData;
        /// <summary>
        /// Whether to automatically clean def packages.
        /// </summary>
        public bool AutoCleanDefPackage;

        /// <summary>
        /// Whether to mute WorldPawnGC.
        /// </summary>
        public bool DoMuteGC;
        /// <summary>
        /// Whether to mute BattleLog.
        /// </summary>
        public bool DoMuteBL;

        /// <summary>
        /// Whether to archive GC dialogs.
        /// </summary>
        public bool ArchiveGCDialog;
        /// <summary>
        /// Whether to archive general messages.
        /// </summary>
        public bool ArchiveMessageGeneral;

        /// <summary>
        /// Whether to display memory usage on screen for developers.
        /// </summary>
        public bool DevOnScreenMemoryUsage;

        private bool excludeAnomalyCorpses;
        /// <summary>
        /// Gets or sets a value indicating whether to exclude unnatural or anomaly DLC corpses from cleaning.
        /// </summary>
        public bool ExcludeAnomalyCorpses
        {
            get => excludeAnomalyCorpses;
            set => excludeAnomalyCorpses = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the memory monitor bar is enabled.
        /// </summary>
        public bool EnableMemoryMonitorBar
        {
            get
            {
                return this.EnableMemoryUsageBar;
            }
            set
            {
                if (this.EnableMemoryUsageBar != value)
                {
                    this.EnableMemoryUsageBar = value;
                    this.UpdateCache();
                }
            }
        }
        /// <summary>
        /// Gets or sets the lower bound in MB for the memory monitor bar.
        /// </summary>
        public int MemoryMonitorBarLowerBoundMb
        {
            get
            {
                return this.MemoryUsageBarLowerBoundMb;
            }
            set
            {
                if (this.MemoryUsageBarLowerBoundMb != value)
                {
                    this.MemoryUsageBarLowerBoundMb = value;
                    this.UpdateCache();
                }
            }
        }
        /// <summary>
        /// Gets or sets the upper bound in MB for the memory monitor bar.
        /// </summary>
        public int MemoryMonitorBarUpperBoundMb
        {
            get
            {
                return this.MemoryUsageBarUpperBoundMb;
            }
            set
            {
                if (this.MemoryUsageBarUpperBoundMb != value)
                {
                    this.MemoryUsageBarUpperBoundMb = value;
                    this.UpdateCache();
                }
            }
        }
        /// <summary>
        /// Gets or sets the update interval for the memory monitor.
        /// </summary>
        public int MemoryMonitorUpdateInterval
        {
            get
            {
                return this.MemoryUsageUpdateInterval;
            }
            set
            {
                if (this.MemoryUsageUpdateInterval != value)
                {
                    this.MemoryUsageUpdateInterval = value;
                    MainButtonWorker_RuntimeGC.Notify_UpdateIntervalChanged(value);
                }
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the memory monitor tip is enabled.
        /// </summary>
        public bool EnableMemoryMonitorTip
        {
            get
            {
                return this.EnableMemoryUsageTip;
            }
            set
            {
                if (this.EnableMemoryUsageTip != value)
                {
                    this.EnableMemoryUsageTip = value;
                    this.UpdateCache();
                }
            }
        }

        internal int restartFlags = 0;

        /// <summary>
        /// Gets the current mod instance.
        /// </summary>
        public new Mod Mod
        {
            get
            {
                Verse.Log.Warning("RuntimeGCSettings.get_Mod() is called!");
                return LoadedModManager.GetMod<RuntimeGC>();
            }
            set { }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeGCSettings"/> class.
        /// </summary>
        public RuntimeGCSettings()
        {
            this.Init();
            this.UpdateCache();
        }

        /// <summary>
        /// Initializes the settings to their default values.
        /// </summary>
        public void Init()
        {
            this.InitMemoryMonitor();

            this.AutoCleanModMetaData = true;
            this.AutoCleanLanguageData = true;
            this.AutoCleanDefPackage = false;

            this.DoMuteGC = true;
            this.DoMuteBL = false;

            this.ArchiveGCDialog = true;
            this.ArchiveMessageGeneral = false;

            this.DevOnScreenMemoryUsage = false;
            this.ExcludeAnomalyCorpses = false;
        }

        /// <summary>
        /// Initializes memory monitor related settings to their default values.
        /// </summary>
        public void InitMemoryMonitor()
        {
            this.EnableMemoryUsageBar = true;
            this.MemoryUsageBarLowerBoundMb = 0;
            this.MemoryUsageBarUpperBoundMb = 1024 * (IntPtr.Size == 4 ? 1 : 2);
            this.MemoryUsageUpdateInterval = (int)MemoryMonitorUpdateMode.Moderate;
            this.EnableMemoryUsageTip = true;
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        public void ResetToDefault()
        {
            if (this.AutoCleanModMetaData != true)
                this.restartFlags ^= 1 << 0;
            if (this.AutoCleanLanguageData != true)
                this.restartFlags ^= 1 << 1;
            if (this.AutoCleanDefPackage != false)
                this.restartFlags ^= 1 << 2;

            if (this.DoMuteGC != true)
                this.restartFlags ^= 1 << 3;
            if (this.DoMuteBL != false)
                this.restartFlags ^= 1 << 4;

            this.Init();
            this.UpdateCache();
            UIUtil.Notify_MMBtnLabelChanged();
        }

        /// <summary>
        /// Updates the cached settings in MainButtonWorker_RuntimeGC.
        /// </summary>
        public void UpdateCache()
        {
            MainButtonWorker_RuntimeGC.UpdateSettings(this);
        }

        /// <summary>
        /// Exposes the settings data for saving and loading.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref EnableMemoryUsageBar, "EnableMemoryUsageBar", true);
            Scribe_Values.Look<int>(ref MemoryUsageBarLowerBoundMb, "MemoryUsageBarLowerBoundMb", 0);
            Scribe_Values.Look<int>(ref MemoryUsageBarUpperBoundMb, "MemoryUsageBarUpperBoundMb", 1024 * (IntPtr.Size == 4 ? 1 : 2));
            Scribe_Values.Look<int>(ref MemoryUsageUpdateInterval, "MemoryUsageUpdateInterval", (int)MemoryMonitorUpdateMode.Moderate);
            Scribe_Values.Look<bool>(ref EnableMemoryUsageTip, "EnableMemoryUsageTip", true);

            Scribe_Values.Look<bool>(ref AutoCleanModMetaData, "AutoCleanModMetaData", true);
            Scribe_Values.Look<bool>(ref AutoCleanLanguageData, "AutoCleanLanguageData", true);
            Scribe_Values.Look<bool>(ref AutoCleanDefPackage, "AutoCleanDefPackage", false);

            Scribe_Values.Look<bool>(ref DoMuteGC, "DoMuteGC", true);
            Scribe_Values.Look<bool>(ref DoMuteBL, "DoMuteBL", false);

            Scribe_Values.Look<bool>(ref ArchiveGCDialog, "ArchiveGCDialog", true);
            Scribe_Values.Look<bool>(ref ArchiveMessageGeneral, "ArchiveMessageGeneral", false);

            Scribe_Values.Look<bool>(ref DevOnScreenMemoryUsage, "DevOnScreenMemoryUsage", false);
            Scribe_Values.Look<bool>(ref excludeAnomalyCorpses, "ExcludeAnomalyCorpses", false);
            
            if(Scribe.mode== LoadSaveMode.LoadingVars)
            {
                if (MemoryUsageBarLowerBoundMb < 0)
                    MemoryUsageBarLowerBoundMb = 0;
                if (MemoryUsageBarUpperBoundMb > 1024 * (IntPtr.Size == 4 ? 4 : 128))
                    MemoryUsageBarUpperBoundMb = 1024 * (IntPtr.Size == 4 ? 4 : 128);
                if (MemoryUsageBarUpperBoundMb <= MemoryUsageBarLowerBoundMb)
                {
                    MemoryUsageBarLowerBoundMb = 0;
                    MemoryUsageBarUpperBoundMb = 1024 * (IntPtr.Size == 4 ? 1 : 2);
                }
                this.UpdateCache();
            }
        }

        /// <summary>
        /// Checks if a restart is required due to changed settings.
        /// </summary>
        /// <returns>True if a restart is required, false otherwise.</returns>
        public bool RequiresRestart()
        {
            return this.restartFlags != 0;
        }
    }
}
