using InGameDefEditor.Stats.DefStat;
using InGameDefEditor.Stats.Misc;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using Verse;

namespace InGameDefEditor.Stats
{
	[Serializable]
	public class BackstoryDefStats : DefStat<BackstoryDef>, IParentStat {
		[XmlElement(IsNullable = false)]
		public string identifier;
		public bool shuffleable;

		public BackstorySlot slot;
		public WorkTags workDisables;
		public WorkTags requiredWorkTags;

		public DefStat<BodyTypeDef> bodyTypeGlobal;
		public DefStat<BodyTypeDef> bodyTypeMale;
		public DefStat<BodyTypeDef> bodyTypeFemale;
		public DefStat<RulePackDef> nameMaker;

		public List<IntValueDefStat<SkillDef>> skillGains;
		public List<IntValueDefStat<TraitDef>> forcedTraits;
		public List<IntValueDefStat<TraitDef>> disallowedTraits;
		public List<IntValueDefStat<ThingDef>> possessions;

		public List<string> spawnCategories = new List<string>();

		public BackstoryDefStats() { }
		public BackstoryDefStats(BackstoryDef d) : base(d) {
			this.identifier = d.identifier;
			this.shuffleable = d.shuffleable;
			this.slot = d.slot;
			this.workDisables = d.workDisables;
			this.requiredWorkTags = d.requiredWorkTags;
			Util.AssignDefStat(d.bodyTypeGlobal, out this.bodyTypeGlobal);
			Util.AssignDefStat(d.bodyTypeMale, out this.bodyTypeMale);
			Util.AssignDefStat(d.bodyTypeFemale, out this.bodyTypeFemale);
			Util.AssignDefStat(d.nameMaker, out this.nameMaker);
			Util.Populate(out this.skillGains, d.skillGains, v => new IntValueDefStat<SkillDef>(v.Key, v.Value));
			Util.Populate(out this.forcedTraits, d.forcedTraits, v => new IntValueDefStat<TraitDef>(v.def, v.degree));
			Util.Populate(out this.disallowedTraits, d.disallowedTraits, v => new IntValueDefStat<TraitDef>(v.def, v.degree));
			Util.Populate(out this.possessions, d.possessions, v => new IntValueDefStat<ThingDef>(v.key, v.value));
			Util.Populate(out this.spawnCategories, d.spawnCategories);
		}

		public override bool Initialize()
		{
			Util.InitializeDefStat(this.bodyTypeGlobal);
			Util.InitializeDefStat(this.bodyTypeFemale);
			Util.InitializeDefStat(this.bodyTypeMale);
			Util.InitializeDefStat(this.nameMaker);

			this.skillGains?.ForEach(v => v.Initialize());
			this.forcedTraits?.ForEach(v => v.Initialize());
			this.disallowedTraits?.ForEach(v => v.Initialize());
			this.possessions?.ForEach(v => v.Initialize());
			return true;
		}

		public void ApplyStats(object t)
		{
			if (t is BackstoryDef to)
			{
				to.identifier = this.identifier;
				to.shuffleable = this.shuffleable;
				to.slot = this.slot;
				to.workDisables = this.workDisables;
				to.requiredWorkTags = this.requiredWorkTags;
				to.bodyTypeGlobal = Util.AssignDef(this.bodyTypeGlobal);
				to.bodyTypeMale = Util.AssignDef(this.bodyTypeMale);
				to.bodyTypeFemale = Util.AssignDef(this.bodyTypeFemale);
				to.nameMaker = Util.AssignDef(this.nameMaker);
				if (this.skillGains != null)
				{
					to.skillGains.Clear();
					foreach (var v in this.skillGains)
						to.skillGains.Add(v.Def, v.value);
				}
				Util.Populate(out to.forcedTraits, this.forcedTraits, v => new BackstoryTrait { def = v.Def, degree = v.value });
				Util.Populate(out to.disallowedTraits, this.disallowedTraits, v => new BackstoryTrait { def = v.Def, degree = v.value });
				Util.Populate(out to.possessions, this.possessions, v => new BackstoryThingDefCountClass { key = v.Def, value = v.value });
				Util.Populate(out to.spawnCategories, this.spawnCategories);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj != null &&
				obj is BackstoryDefStats b)
			{
				return
					object.Equals(this.identifier, b.identifier) &&
					this.shuffleable == b.shuffleable &&
					this.slot == b.slot &&
					this.workDisables == b.workDisables &&
					this.requiredWorkTags == b.requiredWorkTags &&
					object.Equals(this.bodyTypeGlobal, b.bodyTypeGlobal) &&
					object.Equals(this.bodyTypeMale, b.bodyTypeMale) &&
					object.Equals(this.bodyTypeFemale, b.bodyTypeFemale) &&
					object.Equals(this.nameMaker, b.nameMaker) &&
					Util.AreEqual(this.skillGains, b.skillGains, v => v.defName.GetHashCode()) &&
					Util.AreEqual(this.forcedTraits, b.forcedTraits, v => v.defName.GetHashCode()) &&
					Util.AreEqual(this.disallowedTraits, b.disallowedTraits, v => v.defName.GetHashCode()) &&
					Util.AreEqual(this.possessions, b.possessions, v => v.defName.GetHashCode()) &&
					Util.AreEqual(this.spawnCategories, b.spawnCategories, v => v.GetHashCode());
			}
			return false;
		}

		public override string ToString()
		{
			return
				this.GetType().Name + Environment.NewLine +
				"    identifier: " + identifier;
		}

		public override int GetHashCode()
		{
			return this.identifier.GetHashCode();
		}
	}
}