﻿using InGameDefEditor.Stats.Misc;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System;

namespace InGameDefEditor
{
    static class Util
    {
        public static bool IsNullEmpty<T>(List<T> l)
        {
            return l == null || l.Count == 0;
        }

        public static bool ListsRoughlyEqual<T>(IEnumerable<T> l, IEnumerable<T> r)
        {
            //Log.Error("L: " + ((l == null) ? "null" : l.Count().ToString()) + " R: " + ((r == null) ? "null" : r.Count().ToString()));
            if ((l == null || (l != null && l.Count() == 0)) &&
                (r == null || (r != null && r.Count() == 0)))
            {
                return true;
            }
            if (l != null && r != null &&
                l.Count() == r.Count())
            {
                return true;
            }
            return false;
        }

        public static bool AreEqual(IEnumerable<IDefStat> l, IEnumerable<IDefStat> r)
        {
            if (!ListsRoughlyEqual(l, r))
                return false;

            if (l == null || r == null)
                return true;

            Dictionary<Def, IDefStat> lookup = new Dictionary<Def, IDefStat>();
            foreach (var v in l)
                lookup[v.BaseDef] = v;

            try
            {
                foreach (var v in r)
                {
                    if (lookup.TryGetValue(v.BaseDef, out var found))
                    {
                        if (!v.Equals(found))
                        {
                            //Log.Error("Not Equal");
                            return false;
                        }
                    }
                    else
                    {
                        //Log.Error("Not Found");
                        return false;
                    }
                }
            }
            finally
            {
                lookup.Clear();
                lookup = null;
            }
            return true;
        }

        public static bool AreEqual<D>(IEnumerable<DefStat<D>> l, IEnumerable<DefStat<D>> r) where D : Def
        {
            if (!ListsRoughlyEqual(l, r))
                return false;

            if (l == null || r == null)
                return true;
            
                Dictionary<Def, IDefStat> lookup = new Dictionary<Def, IDefStat>();
                foreach (var v in l)
                    lookup[v.BaseDef] = v;

            try
            {
                foreach (var v in r)
                {
                    if (lookup.TryGetValue(v.BaseDef, out var found))
                    {
                        if (!v.Equals(found))
                        {
                            //Log.Error("Not Equal");
                            return false;
                        }
                    }
                    else
                    {
                        //Log.Error("Not Found");
                        return false;
                    }
                }
            }
            finally
            {
                lookup.Clear();
                lookup = null;
            }
            return true;
        }

        public static bool AreEqual<D>(IEnumerable<FloatValueStat<D>> l, IEnumerable<FloatValueStat<D>> r) where D : Def
        {
            if (!ListsRoughlyEqual(l, r))
                return false;

            if (l == null || r == null)
                return true;

            Dictionary<Def, IDefStat> lookup = new Dictionary<Def, IDefStat>();
            foreach (var v in l)
                lookup[v.BaseDef] = v;

            try
            {
                foreach (var v in r)
                {
                    if (lookup.TryGetValue(v.BaseDef, out var found))
                    {
                        if (!v.Equals(found))
                        {
                            //Log.Error("Not Equal");
                            return false;
                        }
                    }
                    else
                    {
                        //Log.Error("Not Found");
                        return false;
                    }
                }
            }
            finally
            {
                lookup.Clear();
                lookup = null;
            }
            return true;
        }

        public static bool AreEqual<D1, D2>(List<FloatValueDoubleDefStat<D1, D2>> l, IEnumerable<FloatValueDoubleDefStat<D1, D2>> r) where D1 : Def where D2 : Def
        {
            if (!ListsRoughlyEqual(l, r))
                return false;

            if (l == null || r == null)
                return true;
            
                Dictionary<Def, IDefStat> lookup = new Dictionary<Def, IDefStat>();
                foreach (var v in l)
                    lookup[v.BaseDef] = v;

            try
            {
                foreach (var v in r)
                {
                    if (lookup.TryGetValue(v.BaseDef, out var found))
                    {
                        if (!v.Equals(found))
                        {
                            //Log.Error("Not Equal");
                            return false;
                        }
                    }
                    else
                    {
                        //Log.Error("Not Found");
                        return false;
                    }
                }
            }
            finally
            {
                lookup.Clear();
                lookup = null;
            }
            return true;
        }

        public static bool AreEqual<D>(List<MinMaxStat<D>> l, IEnumerable<MinMaxStat<D>> r) where D : Def
        {
            if (!ListsRoughlyEqual(l, r))
                return false;

            if (l == null || r == null)
                return true;

            Dictionary<Def, IDefStat> lookup = new Dictionary<Def, IDefStat>();
            foreach (var v in l)
                lookup[v.BaseDef] = v;

            try
            {
                foreach (var v in r)
                {
                    if (lookup.TryGetValue(v.BaseDef, out var found))
                    {
                        if (!v.Equals(found))
                        {
                            //Log.Error("Not Equal");
                            return false;
                        }
                    }
                    else
                    {
                        //Log.Error("Not Found");
                        return false;
                    }
                }
            }
            finally
            {
                lookup.Clear();
                lookup = null;
            }
            return true;
        }

        public delegate bool LREqual<T>(T l, T r);
        public static bool AreEqual<T>(IEnumerable<T> l, IEnumerable<T> r, LREqual<T> areEqual)
        {
            if (!ListsRoughlyEqual(l, r))
                return false;

            if (l == null || r == null)
                return true;

            LinkedList<T> ll = new LinkedList<T>(l);
            LinkedListNode<T> n = ll.First;
            while (n != null)
            {
                var next = n.Next;
                foreach(var v in r)
                {
                    if ((areEqual != null && areEqual(n.Value, v)) || 
                        (areEqual == null && object.Equals(n.Value, v)))
                    {
                        ll.Remove(n);
                        break;
                    }
                }
                n = next;
            }

            return ll.Count == 0;
        }

        public static bool AreEqual<T>(DefStat<T> l, DefStat<T> r) where T : Def
        {
            return
                l == null && r == null ||
                (l != null && r != null && l.Equals(r));

        }

        public delegate T CreateItem<T, U>(U u);
        public static void Populate<T, U>(List<T> to, List<U> from, CreateItem<T, U> createItem)
        {
            if (to != null)
                to.Clear();

            if (!IsNullEmpty(from))
            {
                foreach (U u in from)
                    to.Add(createItem(u));
            }
        }

        private static Dictionary<string, Def> defDic = null;
        public static void ClearDefDic()
        {
            if (defDic != null)
            {
                defDic.Clear();
                defDic = null;
            }
        }
        public static bool TryGetDef<D>(string defName, out D def) where D : Def
        {
            if (defDic == null || defDic.Count == 0)
            {
                defDic = new Dictionary<string, Def>();
                AddToDefDic(DefDatabase<ThingDef>.AllDefsListForReading);
                AddToDefDic(DefDatabase<StatDef>.AllDefsListForReading);
                AddToDefDic(DefDatabase<SoundDef>.AllDefsListForReading);
                AddToDefDic(DefDatabase<WeatherDef>.AllDefsListForReading);
                AddToDefDic(DefDatabase<TerrainDef>.AllDefsListForReading);
                AddToDefDic(DefDatabase<PawnKindDef>.AllDefsListForReading);
                AddToDefDic(DefDatabase<BiomeDef>.AllDefsListForReading);
                AddToDefDic(DefDatabase<ToolCapacityDef>.AllDefsListForReading);
                AddToDefDic(DefDatabase<IncidentDef>.AllDefsListForReading);
            }
            if (defDic.TryGetValue(defName, out Def d))
            {
                def = d as D;
                return true;
            }
            def = null;
            return false;
        }
        private static void AddToDefDic<D>(IEnumerable<D> defs) where D : Def
        {
            foreach (Def d in defs)
            {
                defDic[d.defName] = d;
            }
        }

        public delegate U Convert<T, U>(T t);
        public static IEnumerable<U> ConvertItems<T, U> (IEnumerable<T> t, Convert<T, U> convert)
        {
            List<U> u = new List<U>();
            foreach (var v in t)
                u.Add(convert(v));
            return u;
        }

        public static string IsNull<T> (string label, T t)
        {
            return label + " is " + ((t == null) ? "null" : "not null");
        }

        public static bool TryAssignStatDef<D>(D def, out DefStat<D> stat) where D : Def
        {
            if (def == null)
            {
                stat = null;
                return false;
            }
            stat = new DefStat<D>(def);
            return true;
        }

        public static void TryAssignDef<D>(DefStat<D> from, out D def) where D : Def
        {
            if (from == null)
                def = null;
            else
                def = from.Def;
        }

        public static void AssignStat<D>(DefStat<D> from, DefStat<D> to) where D : Def
        {
            if (from != null && to != null)
                from.AssignStats(to);
        }

        public static bool InitializeDefStat<D>(DefStat<D> s) where D : Def
        {
            if (s != null)
                if (!s.Initialize())
                {
                    Log.Warning("Failed to initialize DefStat " + s.defName);
                    return false;
                }
            return true;
        }
    }
}
