using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace RuntimeGC
{
    /// <summary>
    /// Represents the main user interface window for RuntimeGC.
    /// </summary>
    public class UserInterface : MainTabWindow
    {
        private int pawnsAliveCount;
        private int pawnsDeadCount;
        private bool pawnsCountDirty = true;

        /// <summary>
        /// Gets the requested size of the tab.
        /// </summary>
        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(350f, 500f);
            }
        }

        /// <summary>
        /// Gets the count of all alive world pawns.
        /// </summary>
        public int PawnsAliveCount
        {
            get
            {
                if (pawnsCountDirty)
                {
                    pawnsCountDirty = false;
                    pawnsAliveCount = Find.WorldPawns.AllPawnsAlive.Count();
                    pawnsDeadCount = Find.WorldPawns.AllPawnsDead.Count();
                }
                return pawnsAliveCount;
            }
        }

        /// <summary>
        /// Gets the count of all dead world pawns.
        /// </summary>
        public int PawnsDeadCount
        {
            get
            {
                if (pawnsCountDirty)
                {
                    pawnsCountDirty = false;
                    pawnsAliveCount = Find.WorldPawns.AllPawnsAlive.Count();
                    pawnsDeadCount = Find.WorldPawns.AllPawnsDead.Count();
                }
                return pawnsDeadCount;
            }
        }

        /// <summary>
        /// Draws the contents of the main window.
        /// </summary>
        /// <param name="canvas">The rectangle defining the window's area.</param>
        public override void DoWindowContents(Rect canvas)
        {
            UnityEngine.GUI.BeginGroup(canvas);
            Listing_Standard std = new Listing_Standard();
            std.Begin(canvas);
            Text.Font = GameFont.Medium;
            std.Label("RuntimeGCTitle".Translate());
            Text.Font = GameFont.Small;
            std.Label("RuntimeGCVer".Translate("1.5"));
            std.Label("By user19990313");
            std.Gap();
            std.Label(string.Concat(new object[]{"pawnsAlive:",
                                                PawnsAliveCount,
                                                " pawnsDead:",
                                                PawnsDeadCount
                                                }));
            std.Gap();
            float f = std.CurHeight;
            std.End();

            this.DoComponents(new Rect(canvas.x+35f, canvas.y + f, canvas.width, canvas.height - f));
            this.DoHelpContents(new Rect(canvas.x, canvas.y + f, canvas.width, canvas.height - f));

            UnityEngine.GUI.EndGroup();
        }

        /// <summary>
        /// Draws the interactive components of the user interface.
        /// </summary>
        /// <param name="rect">The rectangle defining the area for components.</param>
        public void DoComponents(Rect rect)
        {
            UnityEngine.GUI.BeginGroup(rect);
            Rect rect2 = new Rect(0f, 0f, 170f, rect.height);
            Text.Font = GameFont.Small;
            List<ListableOption> list = new List<ListableOption>();

            list.Add(new ListableOption("BtnTextGCWP".Translate(), delegate {
                GC_wrapped();
            }, null));

            list.Add(new ListableOption("BtnTextGCV".Translate(), delegate {
                GC_wrapped(true);
            }, null));

            list.Add(new ListableOption("BtnTextAddOn".Translate(), delegate {
                FloatMenuUtil.GenerateFloatMenuGroup(FloatMenuUtil.GroupTools);
            }, null));

            list.Add(new ListableOption("BtnTextFix".Translate(), delegate {
                FloatMenuUtil.GenerateFloatMenuGroup(FloatMenuUtil.GroupFix);
            }, null));

            list.Add(new ListableOption("BtnTextQB".Translate(), delegate {
                FloatMenuUtil.GenerateFloatMenuGroup(FloatMenuUtil.GroupQuickbar);
            }, null));

            list.Add(new ListableOption("BtnTextSysGC".Translate(), delegate {
                if (Event.current.shift)
                    FloatMenuUtil.GenerateMemoryReclaimOptions();
                CleanserUtil.GCObject.DisposeTmpForSystemGC();
                System.GC.Collect();
                Messages.Message("MsgTextSysGC".Translate(), MessageTypeDefOf.PositiveEvent);
            }, null));

            OptionListingUtility.DrawOptionListing(rect2, list);
            UnityEngine.GUI.EndGroup();
        }

        /// <summary>
        /// Draws the help content section of the user interface.
        /// </summary>
        /// <param name="rect">The rectangle defining the area for help contents.</param>
        public void DoHelpContents(Rect rect)
        {
            UnityEngine.GUI.BeginGroup(rect);
            Rect rect2 = new Rect(0f, 0f, 35f, rect.height);
            Text.Font = GameFont.Small;
            List<ListableOption> list = new List<ListableOption>();

            list.Add(new ListableOption("?", delegate {
                //GC_wrapped();
                Find.WindowStack.Add(new Dialog_MessageBox("HelpTextGCWP".Translate()));
            }, null));

            list.Add(new ListableOption("?", delegate {
                //GC_wrapped(true);
                Find.WindowStack.Add(new Dialog_MessageBox("HelpTextGCV".Translate()));
            }, null));
            
            list.Add(new ListableOption("?", delegate {
                //FloatMenuUtil.GenerateFloatMenuGroup(FloatMenuUtil.GroupTools);
                Find.WindowStack.Add(new Dialog_MessageBox("HelpTextAddOn".Translate()));
            }, null));

            list.Add(new ListableOption("?", delegate {
                //FloatMenuUtil.GenerateFloatMenuGroup(FloatMenuUtil.GroupFix);
                Find.WindowStack.Add(new Dialog_MessageBox("HelpTextFix".Translate()));
            }, null));

            list.Add(new ListableOption("?", delegate {
                //FloatMenuUtil.GenerateFloatMenuGroup(FloatMenuUtil.GroupQuickAccess);
                Find.WindowStack.Add(new Dialog_MessageBox("HelpTextQB".Translate()));
            }, null));

            list.Add(new ListableOption("?", delegate {
                //System.GC.Collect();
                Find.WindowStack.Add(new Dialog_MessageBox("HelpTextSysGC".Translate()));
            }, null));

            OptionListingUtility.DrawOptionListing(rect2, list);
            UnityEngine.GUI.EndGroup();
        }

        /// <summary>
        /// Performs a garbage collection operation and displays a summary.
        /// </summary>
        /// <param name="verbose">If set to <c>true</c>, provides verbose output.</param>
        public void GC_wrapped(bool verbose=false)
        {
            int a = PawnsAliveCount;
            int b = PawnsDeadCount;
            int i = CleanserUtil.GCObject.GC(verbose);
            Notify_PawnsCountDirty();
            int j = a + b - PawnsAliveCount - PawnsDeadCount;

            string str = "DlgTextGC".Translate(a, PawnsAliveCount,
                                               b, PawnsDeadCount,
                                               j);
            Find.WindowStack.Add(new Dialog_MessageBox($"{str}\n\n" + (i == j ?
                                                      ((string)"DlgTextGCAdvice1".Translate()) : ((string)"DlgTextGCAdvice2".Translate(i - j))
                                                     + (!verbose ? "" : "\n\n" + ((string)"DlgTextGCV".Translate()))

            )));
            if(RuntimeGC.Settings.ArchiveGCDialog)
                Find.Archive.Add(new ArchivedDialog(str, "DlgArchiveTitle".Translate(), null));
        }

        /// <summary>
        /// Notifies the UI that the pawns count is dirty and needs to be re-calculated.
        /// </summary>
        public void Notify_PawnsCountDirty()
        {
            this.pawnsCountDirty = true;
        }

        /// <summary>
        /// Called before the window is opened. Initializes state.
        /// </summary>
        public override void PreOpen()
        {
            base.PreOpen();
            this.forcePause = true;
            this.Notify_PawnsCountDirty();
        }
    }
}
