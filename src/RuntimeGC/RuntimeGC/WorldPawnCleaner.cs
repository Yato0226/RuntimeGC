using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace RuntimeGC
{
    public class WorldPawnCleaner
    {
        static string CopyrightStr = "RuntimeGC for 1.5,user19990313,Baidu Tieba&Ludeon forum";	

        ///Verbosity
        private Dictionary<Pawn, int> allPawnsCounter = new Dictionary<Pawn, int>();
        private Dictionary<Flags, int> allFlagsCounter = new Dictionary<Flags, int>();
        private bool verbose = false;
        ///Debug only.Well,useless.
        private bool debug = false;

        enum Flags
        {
            Colonist = 1,
            Prisoner = 2,
            FactionLeader = 8,
            KeptWorldPawn = 16,
            CorpseOwner = 4,
            RelationLvl0 = 32,
            RelationLvl1 = 64,
            RelationLvl2 = 128,
            TaleEntryOwner = 256,
            OnSale = 512,
            Animal = 1024,
            None = 0
        }
        private static int FlagsCountNotNull = Enum.GetNames(typeof(Flags)).Length - 1;

        List<Pawn> reference;
        Dictionary<Pawn, Flags> allFlags = new Dictionary<Pawn, Flags>();


        /// <summary>
        /// GC().The best way to shrink Rimworld saves,I think.
        /// </summary>
        /// <param name="verbose">Determine if GC() should log details very very verbosely</param>
        /// <returns>Count of disposed World pawns</returns>
        public int GC(bool verbose = false)
{
            bool flag = Current.ProgramState != ProgramState.Playing;
            bool flag19 = flag;
	int result;
	if (flag19)
	{
		Log.Error("You must be kidding me...GC a save without loading one?");
		result = 0;
	}
	else
	{
		Log.Message("[GC Log] Pre-Initializing GC...");
		this.reference = Find.WorldPawns.AllPawnsAliveOrDead.ToList<Pawn>();
		this.allFlags.Clear();
		if (verbose)
		{
			this.allFlagsCounter.Clear();
			this.allFlagsCounter.Add(WorldPawnCleaner.Flags.None, 0);
			for (int i = 0; i < WorldPawnCleaner.FlagsCountNotNull; i++)
			{
				this.allFlagsCounter.Add((WorldPawnCleaner.Flags)(1 << i), 0);
			}
		}
		Log.Message("[GC Log] Generating EntryPoints from Map Pawns...");
		List<Pawn> pawns;
		this.DiagnoseMapPawns(out pawns);
		if (verbose)
		{
			Log.Message("[GC Log][Verbose] " + this.allPawnsCounter.Count<KeyValuePair<Pawn, int>>().ToString() + " Map Pawns marked during diagnosis");
		}
		this.allPawnsCounter.Clear();
		if (verbose)
		{
			this.allFlagsCounter.Clear();
			this.allFlagsCounter.Add(WorldPawnCleaner.Flags.None, 0);
			for (int j = 0; j < WorldPawnCleaner.FlagsCountNotNull; j++)
			{
				this.allFlagsCounter.Add((WorldPawnCleaner.Flags)(1 << j), 0);
			}
		}
		Log.Message("[GC Log] Collecting Pawns concerned by Used Tales...");
		List<Pawn> pawns2;
		CleanserUtil.InitUsedTalePawns(out pawns2);
		Log.Message("[GC Log] Running diagnosis on WorldPawns...");
		foreach (Pawn pawn in this.reference)
		{
			bool flag2 = pawn == null;
			bool flag20 = flag2;
			if (flag20)
			{
				Log.Message("Encountered a null pawn.");
			}
			else
			{
				bool isColonist = pawn.IsColonist;
				bool flag21 = isColonist;
				if (flag21)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)129);
				}
				bool isPrisonerOfColony = pawn.IsPrisonerOfColony;
				bool flag22 = isPrisonerOfColony;
				if (flag22)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)130);
				}
				bool flag3 = PawnUtility.IsFactionLeader(pawn);
				bool flag23 = flag3;
				if (flag23)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)88);
				}
				bool flag4 = PawnUtility.IsKidnappedPawn(pawn);
				bool flag24 = flag4;
				if (flag24)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)144);
				}
				bool flag5 = pawn.Corpse != null;
				bool flag25 = flag5;
				if (flag25)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)68);
				}
				bool flag6 = pawns2.Contains(pawn);
				bool flag26 = flag6;
				if (flag26)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)288);
				}
				bool inContainerEnclosed = pawn.InContainerEnclosed;
				bool flag27 = inContainerEnclosed;
				if (flag27)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)48);
				}
				bool spawned = pawn.Spawned;
				bool flag28 = spawned;
				if (flag28)
				{
					this.addFlag(pawn, WorldPawnCleaner.Flags.RelationLvl0);
				}
				bool flag7 = CaravanUtility.IsPlayerControlledCaravanMember(pawn);
				bool flag29 = flag7;
				if (flag29)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)144);
				}
				bool flag8 = PawnUtility.IsTravelingInTransportPodWorldObject(pawn);
				bool flag30 = flag8;
				if (flag30)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)144);
				}
				bool flag9 = PawnUtility.ForSaleBySettlement(pawn);
				bool flag31 = flag9;
				if (flag31)
				{
					this.addFlag(pawn, (WorldPawnCleaner.Flags)544);
				}
				bool flag10 = !verbose;
				bool flag32 = !flag10;
				if (flag32)
				{
					Log.Message("[worldPawn] " + pawn.LabelShort + " [flag] " + this.markedFlagsString(pawn));
				}
			}
		}
		if (verbose)
		{
			Log.Message("[GC Log][Verbose] " + this.allPawnsCounter.Count<KeyValuePair<Pawn, int>>().ToString() + " World Pawns marked during diagnosis");
		}
		Log.Message("[GC Log] Expanding Relation networks through Map Pawn Entry Points...");
		for (int k = pawns.Count - 1; k > -1; k--)
		{
			bool flag11 = this.containsFlag(pawns[k], WorldPawnCleaner.Flags.RelationLvl2);
			bool flag33 = flag11;
			if (flag33)
			{
				this.expandRelation(pawns[k], WorldPawnCleaner.Flags.RelationLvl1);
				pawns.RemoveAt(k);
			}
		}
		for (int l = pawns.Count - 1; l > -1; l--)
		{
			bool flag12 = this.containsFlag(pawns[l], WorldPawnCleaner.Flags.RelationLvl1);
			bool flag34 = flag12;
			if (flag34)
			{
				this.expandRelation(pawns[l], WorldPawnCleaner.Flags.RelationLvl0);
				pawns.RemoveAt(l);
			}
		}
		Log.Message("[GC Log] Expanding Relation networks on marked World Pawns...");
		for (int m = this.reference.Count - 1; m > -1; m--)
		{
			bool flag13 = this.containsFlag(this.reference[m], WorldPawnCleaner.Flags.RelationLvl2);
			bool flag35 = flag13;
			if (flag35)
			{
				this.expandRelation(this.reference[m], WorldPawnCleaner.Flags.RelationLvl1);
				this.reference.RemoveAt(m);
			}
		}
		for (int n = this.reference.Count - 1; n > -1; n--)
		{
			bool flag14 = this.containsFlag(this.reference[n], WorldPawnCleaner.Flags.RelationLvl1);
			bool flag36 = flag14;
			if (flag36)
			{
				this.expandRelation(this.reference[n], WorldPawnCleaner.Flags.RelationLvl0);
				this.reference.RemoveAt(n);
			}
		}
		for (int k2 = this.reference.Count - 1; k2 > -1; k2--)
		{
			bool flag15 = this.containsFlag(this.reference[k2], WorldPawnCleaner.Flags.RelationLvl0);
			bool flag37 = flag15;
			if (flag37)
			{
				this.reference.RemoveAt(k2);
			}
		}
		int count = 0;
		if (verbose)
		{
			foreach (KeyValuePair<Pawn, int> keyValuePair in this.allPawnsCounter)
			{
				count += keyValuePair.Value;
			}
			Log.Message("[GC Log][Verbose] " + this.allPawnsCounter.Count<KeyValuePair<Pawn, int>>().ToString() + " World Pawns marked during Expanding");
			bool flag16 = this.debug;
			bool flag38 = flag16;
			if (flag38)
			{
				Log.Message("addFlag() called " + count.ToString() + " times");
			}
		}
		Log.Message("[GC Log] Excluding Pawns concerned by Used Tales...");
		foreach (Pawn pawn2 in pawns2)
		{
			this.reference.Remove(pawn2);
		}
		List<Thing> list = (from x in (from q in Find.QuestManager.QuestsListForReading
                                       where q.State == QuestState.Ongoing || q.State == QuestState.EndedSuccess
                                       select q).SelectMany((Quest q) => q.QuestLookTargets)
		where x.Thing != null
		select x.Thing).Distinct<Thing>().ToList<Thing>();
		Log.Message(string.Format("[GC Log] Excluding Quest Pawns: {0}", this.reference.RemoveAll((Pawn p) => list.Contains(p))));
		Log.Message("[GC Log] Disposing World Pawns...");
		count = this.reference.Count;
		for (int k3 = this.reference.Count - 1; k3 > -1; k3--)
		{
			Pawn item = this.reference[k3];
			Find.WorldPawns.RemovePawn(item);
			bool flag17 = !item.Destroyed;
			bool flag39 = flag17;
			if (flag39)
			{
				item.Destroy(0);
			}
			bool flag18 = !item.Discarded;
			bool flag40 = flag18;
			if (flag40)
			{
				item.Discard(true);
			}
		}
		if (verbose)
		{
			string str = "[GC Log][Verbose] Flag calls stat:";
			this.allFlagsCounter.Remove(WorldPawnCleaner.Flags.None);
			foreach (KeyValuePair<WorldPawnCleaner.Flags, int> keyValuePair2 in this.allFlagsCounter)
			{
				string[] array = new string[5];
				array[0] = str;
				array[1] = "\n  ";
				string[] strArrays = array;
				strArrays[2] = keyValuePair2.Key.ToString();
				strArrays[3] = " : ";
				strArrays[4] = keyValuePair2.Value.ToString();
				str = string.Concat(strArrays);
			}
			Log.Message(str);
		}
		Log.Message("[GC Log] GC() completed with " + count.ToString() + " World Pawns disposed");
		result = count;
	}
	return result;
}

        private Flags getFlag(Pawn pawn)
        {
            return allFlags.ContainsKey(pawn) ? allFlags[pawn] : Flags.None;
        }

        private bool containsFlag(Pawn pawn, Flags flag)
        {
            return containsFlag(getFlag(pawn), flag);
        }

        private bool containsFlag(Flags f1, Flags f2)
        {
            return (f1 & f2) != Flags.None;
        }

        private IEnumerable<Flags> splitFlag(Flags flag)
        {
            int f = 1;
            for (int j = 0; j < FlagsCountNotNull; j++)
            {
                if (containsFlag(flag, (Flags)(f = f << 1)))
                    yield return (Flags)f;
            }
            yield return Flags.None;
        }

        private string markedFlagsString(Pawn p)
        {
            List<Flags> flags = splitFlag(getFlag(p)).ToList();
            string str = "";
            int i = 0;
            for (; i < flags.Count-2; i++)
            {
                str += flags[i].ToString();
                str += "|";
            }
            str += flags[i].ToString();
            return str;
        }

        private void addFlag(Pawn pawn, Flags flag)
        {
            if (!allFlags.ContainsKey(pawn))
                allFlags.Add(pawn, flag);
            else allFlags[pawn] |= flag;

            if (verbose)
            {
                if (!allPawnsCounter.ContainsKey(pawn))
                    allPawnsCounter.Add(pawn, 1);
                else allPawnsCounter[pawn] += 1;

                foreach (Flags f in splitFlag(flag))
                    allFlagsCounter[f] += 1;
            }
        }

        private void expandRelation(Pawn p, Flags flag)
        {
            //Patch:null Relation_tracker for mechanoids & insects.
            if (p.relations == null) return;

            if (debug)
            {
                foreach (Pawn p0 in p.relations.FamilyByBlood)
                    foreach (PawnRelationDef d in p.GetRelations(p0))
                        Verse.Log.Message("(Family) " + p.LabelShort + " <" + d.label + "> " + p0.LabelShort);
            }

            foreach (DirectPawnRelation r in p.relations.DirectRelations)
                if (reference.Contains(r.otherPawn))
                {
                    addFlag(r.otherPawn, flag);
                    if (verbose) Verse.Log.Message("(Relation) " + p.LabelShort + " <" + r.def.label + "> " + r.otherPawn.LabelShort);
                }

            foreach (Pawn p2 in CleanserUtil.getPawnsWithDirectRelationsWithMe(p.relations))
            {
                if (reference.Contains(p2) && p2.GetRelations(p).Count<PawnRelationDef>() > 0)
                {
                    addFlag(p2, flag);
                    if (verbose) Verse.Log.Message("(Reflexed) <" + p2.LabelShort + "," + p.LabelShort + ">");
                }
            }
        }

        private void DiagnoseMapPawns(out List<Pawn> mapPawnEntryPoints)
        {
            List<Pawn> pawnlist = new List<Pawn>();
            foreach (Map map in Find.Maps)
                foreach (Pawn p in map.mapPawns.AllPawns)
                {
                    if (p.IsColonist) addFlag(p, Flags.Colonist | Flags.RelationLvl2);
                    if (p.IsPrisonerOfColony) addFlag(p, Flags.Prisoner | Flags.RelationLvl2);
                    if (PawnUtility.IsFactionLeader(p)) addFlag(p, Flags.FactionLeader | Flags.RelationLvl1);
                    if (PawnUtility.IsKidnappedPawn(p)) addFlag(p, Flags.KeptWorldPawn | Flags.RelationLvl2);
                    if (p.Corpse != null) addFlag(p, Flags.CorpseOwner | Flags.RelationLvl1);

                    /*Outsider caravan member patch*/
                    if (p.RaceProps.Humanlike) addFlag(p, Flags.RelationLvl1);

                    if (allFlags.ContainsKey(p))
                    {
                        pawnlist.Add(p);
                        if (verbose) Verse.Log.Message("[mapPawn] " + p.LabelShort + " [flag] " + markedFlagsString(p));
                    }

                    //if ((p.records.GetAsInt(RecordDefOf.TimeAsColonistOrColonyAnimal) > 0) && !(p.RaceProps.Humanlike)) addFlag(p, Flags.Animal | Flags.RelationLvl0);
                    //if (allUsedTaleOwner.Contains(p)) addFlag(p, Flags.TaleEntryOwner | Flags.RelationLvl0);
                    //if ((p.Name!=null) &&(!p.Name.Numerical)&&p.Name.ToStringFull.Contains("Serir")) Verse.Log.Message("Pawn:" + p.Name+",flag="+(allFlags.ContainsKey(p)? allFlags[p].ToString():"null"));
                    //if (p.InContainerEnclosed) addFlag(p, Flags.KeptWorldPawn | Flags.RelationLvl0);
                    //if (p.Spawned) addFlag(p, Flags.RelationLvl0);
                }
            mapPawnEntryPoints = pawnlist.ToList<Pawn>();
        }

        public void DisposeTmpForSystemGC()
        {
            this.reference = null;
            this.allPawnsCounter.Clear();
            this.allFlags.Clear();
            this.allFlagsCounter.Clear();
        }
    }
}