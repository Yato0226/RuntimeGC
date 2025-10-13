using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Mute
{
    /// <summary>
    /// Provides methods for detouring game functions to mute specific behaviors.
    /// </summary>
    public static class Launcher
    {
        #region Replacement
        /// <summary>
        /// Replacement method for WorldPawnGCTick to disable its functionality.
        /// </summary>
        public static void WorldPawnGCTick()
        {
        }

        /// <summary>
        /// Replacement method for BattleLog.Add to disable adding log entries.
        /// </summary>
        /// <param name="entry">The log entry to be added (ignored).</param>
        public static void Add(LogEntry entry)
        {
        }
        /// <summary>
        /// Replacement method for BattleLog.ExposeData to prevent battle log serialization.
        /// </summary>
        public static void ExposeData()
        {
            List<Battle> emptylist = new List<Battle>();
            Scribe_Collections.Look(ref emptylist, "battles", LookMode.Deep);
        }
        #endregion

        /// <summary>
        /// Launches the detouring process for WorldPawnGC and BattleLog.
        /// </summary>
        /// <param name="doGC">Whether to detour WorldPawnGC.</param>
        /// <param name="doBL">Whether to detour BattleLog.</param>
        public static void Launch(bool doGC,bool doBL)
        {
            BindingFlags bflag = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
            BindingFlags bflag2 = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
            if (doGC)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    DoDetour(typeof(RimWorld.Planet.WorldPawnGC).GetMethod("WorldPawnGCTick", bflag), typeof(Launcher).GetMethod("WorldPawnGCTick", bflag2));
                    Verse.Log.Message("[RuntimeGC] Detour completed: MuteGC");
                }, "Initializing", false, null);
            }
            if (doBL)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    DoDetour(typeof(Verse.BattleLog).GetMethod("Add", bflag), typeof(Launcher).GetMethod("Add", bflag2));
                    DoDetour(typeof(Verse.BattleLog).GetMethod("ExposeData", bflag), typeof(Launcher).GetMethod("ExposeData", bflag2));
                    Verse.Log.Message("[RuntimeGC] Detour completed: MuteBL");
                }, "Initializing", false, null);
            }
        }

        /// <summary>
        /// Performs a method detour (hook) from a source method to a destination method.
        /// </summary>
        /// <param name="source">The original method to detour.</param>
        /// <param name="destination">The new method to redirect to.</param>
        /// <returns>True if the detour was successful, false otherwise.</returns>
        public unsafe static bool DoDetour(MethodInfo source, MethodInfo destination)
        {
            if (IntPtr.Size == 8)
            {
                byte* arg_136_0 = (byte*)source.MethodHandle.GetFunctionPointer().ToInt64();
                long num = destination.MethodHandle.GetFunctionPointer().ToInt64();
                byte* ptr = arg_136_0;
                long* ptr2 = (long*)(ptr + 2);
                *ptr = 72;
                ptr[1] = 184;
                *ptr2 = num;
                ptr[10] = 255;
                ptr[11] = 224;
            }
            else
            {
                int num2 = source.MethodHandle.GetFunctionPointer().ToInt32();
                int arg_1A6_0 = destination.MethodHandle.GetFunctionPointer().ToInt32();
                byte* ptr3 = (byte*)num2;
                int* ptr4 = (int*)(ptr3 + 1);
                int num3 = arg_1A6_0 - num2 - 5;
                *ptr3 = 233;
                *ptr4 = num3;
            }
            return true;
        }
    }
}