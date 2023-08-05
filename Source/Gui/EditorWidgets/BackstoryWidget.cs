using InGameDefEditor.Gui.EditorWidgets.Misc;
using InGameDefEditor.Stats.Misc;
using RimWorld;
using System.Collections.Generic;
using Verse;
using InGameDefEditor.Gui.EditorWidgets.Dialog;
using static InGameDefEditor.WindowUtil;
using System;
using System.Linq;
using InGameDefEditor.Stats;
using System.Text;
using UnityEngine;

namespace InGameDefEditor.Gui.EditorWidgets
{
	class BackstoryWidget : AParentDefStatWidget<BackstoryDef>
	{
		private readonly DefType type;
		private readonly List<IInputWidget> inputWidgets;

		private List<WorkTags> workTags;
		private PlusMinusArgs<WorkTags> workDisables;
		private PlusMinusArgs<WorkTags> requiredWorkTags;

		private List<IntInputWidget<Dictionary<SkillDef, int>>> skillGains;
		private PlusMinusArgs<SkillDef> skillGainsPlusMinus;

		private List<IntInputWidget<BackstoryTrait>> forcedTraits;
		private PlusMinusArgs<TraitDef> forcedTraitsPlusMinus;

		private List<IntInputWidget<BackstoryTrait>> disallowedTraits;
		private PlusMinusArgs<TraitDef> disallowedTraitsPlusMinus;

		private List<IntInputWidget<BackstoryThingDefCountClass>> possessions;
		private PlusMinusArgs<TraitDef> possessionsPlusMinus;

		//public List<string> spawnCategories = new List<string>();

		public override string DisplayLabel => base.Def.title;

		public BackstoryWidget(BackstoryDef backstory, DefType type) : base(backstory, type)
		{
			if (backstory.skillGains == null)
				backstory.skillGains = new Dictionary<SkillDef, int>();
			if (backstory.forcedTraits == null)
				backstory.forcedTraits = new List<BackstoryTrait>();
			if (backstory.disallowedTraits == null)
				backstory.disallowedTraits = new List<BackstoryTrait>();

			this.type = type;
			this.inputWidgets = new List<IInputWidget>()
			{
				new BoolInputWidget<BackstoryDef>(base.Def, "Shuffleable", b => b.shuffleable, (b, v) => b.shuffleable = v),
				new EnumInputWidget<BackstoryDef, BackstorySlot>(base.Def, "Slot", 200, b => b.slot, (b, v) => b.slot = v),
				new DefInputWidget<BackstoryDef, BodyTypeDef>(base.Def, "Body Type Global", 200, b => b.bodyTypeGlobal, (b, v) => b.bodyTypeGlobal = v, true),
				new DefInputWidget<BackstoryDef, BodyTypeDef>(base.Def, "Body Type Male", 200, b =>  b.bodyTypeMale, (b, v) => b.bodyTypeMale = v, true, d => d == BodyTypeDefOf.Female),
				new DefInputWidget<BackstoryDef, BodyTypeDef>(base.Def, "Body Type Female", 200, b =>  b.bodyTypeFemale, (b, v) => b.bodyTypeFemale = v, true, d => d == BodyTypeDefOf.Male),
				new DefInputWidget<BackstoryDef, RulePackDef>(base.Def, "Name Maker", 200, b => b.nameMaker, (b, v) => b.nameMaker = v, true),
			};

			var dic = new SortedDictionary<string, WorkTags>();
			foreach (var v in Enum.GetValues(typeof(WorkTags)).Cast<WorkTags>())
				dic.Add(v.ToString(), v);
			this.workTags = new List<WorkTags>(dic.Values);
			dic.Clear();
			dic = null;

			this.workDisables = new PlusMinusArgs<WorkTags>()
			{
				allItems = this.workTags,
				isBeingUsed = v => (base.Def.workDisables & v) == v,
				onAdd = v => base.Def.workDisables |= v,
				onRemove = v => base.Def.workDisables &= ~v,
				getDisplayName = v => v.ToString()
			};

			this.requiredWorkTags = new PlusMinusArgs<WorkTags>()
			{
				allItems = this.workTags,
				isBeingUsed = v => (base.Def.requiredWorkTags & v) == v,
				onAdd = v => base.Def.requiredWorkTags |= v,
				onRemove = v => base.Def.requiredWorkTags &= ~v,
				getDisplayName = v => v.ToString()
			};

			this.skillGainsPlusMinus = new PlusMinusArgs<SkillDef>()
			{
				allItems = Util.SortedDefList<SkillDef>(),
				beingUsed = () => base.Def.skillGains.Keys,
				onAdd = v =>
				{
					base.Def.skillGains[v] = 0;
					this.skillGains.Add(this.CreateSkillGainsInput(v));
				},
				onRemove = v =>
				{
					base.Def.skillGains.Remove(v);
					for (int i = 0; i < this.skillGains.Count; ++i)
						if (this.skillGains[i].DisplayLabel == Util.GetLabel(v))
						{
							this.skillGains.RemoveAt(i);
							break;
						}
				},
				getDisplayName = v => Util.GetLabel(v),
			};
			
			this.forcedTraitsPlusMinus = new PlusMinusArgs<TraitDef>()
			{
				allItems = Util.SortedDefList<TraitDef>(),
				beingUsed = () => Util.ConvertItems(base.Def.forcedTraits, v => v.def),
				onAdd = v =>
				{
					BackstoryTrait te = new BackstoryTrait { def = v, degree = 0 };
					base.Def.forcedTraits.Add(te);
					this.forcedTraits.Add(this.CreateBackstoryTraitInput(te));
					this.RemoveDisallowedTraits(v);
				},
				onRemove = v =>
				{
					this.RemoveForcedTraits(v);
				},
				getDisplayName = v => Util.GetLabel(v),
			};

			this.disallowedTraitsPlusMinus = new PlusMinusArgs<TraitDef>()
			{
				allItems = Util.SortedDefList<TraitDef>(),
				beingUsed = () => Util.ConvertItems(base.Def.disallowedTraits, v => v.def),
				onAdd = v =>
				{
					BackstoryTrait te = new BackstoryTrait { def = v, degree = 0 };
					base.Def.disallowedTraits.Add(te);
					this.disallowedTraits.Add(this.CreateBackstoryTraitInput(te));
					this.RemoveForcedTraits(v);
				},
				onRemove = v =>
				{
					this.RemoveDisallowedTraits(v);
				},
				getDisplayName = v => Util.GetLabel(v),
			};

			this.Rebuild();
		}

		public void DisableAutoDeploy()
		{
			Defs.ApplyStatsAutoDefs.Remove(base.Def.identifier);
		}

		public override void DrawLeft(float x, ref float y, float width)
		{
			foreach (var v in this.inputWidgets)
				v.Draw(x, ref y, width);
		}

		public override void DrawMiddle(float x, ref float y, float width)
		{
			PlusMinusLabel(x, ref y, width, "Skill Gains", this.skillGainsPlusMinus);
			foreach (var v in this.skillGains)
				v.Draw(x, ref y, width);

			PlusMinusLabel(x, ref y, width, "Required Work Tags", this.requiredWorkTags);
			WindowUtil.DrawFlagList(x, ref y, width, workTags, (int)base.Def.requiredWorkTags, v => v == WorkTags.None);

			y += 10;
			PlusMinusLabel(x, ref y, width, "Disabled Work Tags", this.workDisables);
			WindowUtil.DrawFlagList(x, ref y, width, workTags, (int)base.Def.workDisables, v => v == WorkTags.None);

			y += 10;
			PlusMinusLabel(x, ref y, width, "Forced Traits", this.forcedTraitsPlusMinus);
			foreach (var v in this.forcedTraits)
				v.Draw(x + 10, ref y, width - 10);

			y += 10;
			PlusMinusLabel(x, ref y, width, "Disallowed Traits", this.disallowedTraitsPlusMinus);
			foreach (var v in this.disallowedTraits)
				v.Draw(x + 10, ref y, width - 10);
		}

		public override void DrawRight(float x, ref float y, float width)
		{
			DrawLabel(x, ref y, width, "Spawn Categories", 30f, true);
			foreach (var v in base.Def.spawnCategories)
				DrawLabel(10, ref y, width - 10, "- " + v);
		}

		public override void Rebuild()
		{
			this.ResetBuffers();
		}

		public override void ResetBuffers()
		{
			this.inputWidgets?.ForEach(v => v.ResetBuffers());
			this.skillGains?.ForEach(v => v.ResetBuffers());
			this.forcedTraits?.ForEach(v => v.ResetBuffers());
			this.disallowedTraits?.ForEach(v => v.ResetBuffers());

			if (this.skillGains == null)
				this.skillGains = new List<IntInputWidget<Dictionary<SkillDef, int>>>();
			this.skillGains.Clear();
			foreach (KeyValuePair<SkillDef, int> kv in base.Def.skillGains)
				this.skillGains.Add(this.CreateSkillGainsInput(kv.Key));

			if (this.forcedTraits != null)
				this.forcedTraits.Clear();
			Util.Populate(out this.forcedTraits, base.Def.forcedTraits, te => this.CreateBackstoryTraitInput(te));

			if (this.disallowedTraits != null)
				this.disallowedTraits.Clear();
			Util.Populate(out this.disallowedTraits, base.Def.disallowedTraits, te => this.CreateBackstoryTraitInput(te));
		}

		public new void ResetParent()
		{
			base.ResetParent();
			Backup.ApplyStats(base.Def);
		}

		private void RemoveForcedTraits(TraitDef td)
		{
			base.Def.forcedTraits.RemoveAll(d => d.def == td);
			this.forcedTraits.RemoveAll(d => d.Parent.def == td);
		}

		private void RemoveDisallowedTraits(TraitDef td)
		{
			base.Def.disallowedTraits.RemoveAll(d => d.def == td);
			this.disallowedTraits.RemoveAll(d => d.Parent.def == td);
		}

		private IntInputWidget<Dictionary<SkillDef, int>> CreateSkillGainsInput(SkillDef sd)
		{
			return new IntInputWidget<Dictionary<SkillDef, int>>(base.Def.skillGains, Util.GetLabel(sd), d => d[sd], (d, i) => d[sd] = i);
		}

		private IntInputWidget<BackstoryTrait> CreateBackstoryTraitInput(BackstoryTrait te)
		{
			var input = new IntInputWidget<BackstoryTrait>(te, Util.GetLabel(te.def) + " (Degree)", d => d.degree, (d, i) => d.degree = i);
			StringBuilder sb = new StringBuilder(Util.GetLabel(te.def));
			sb.AppendLine();
			sb.AppendLine("Degrees:");
			foreach (var degree in te.def.degreeDatas)
			{
				sb.AppendLine(degree.degree + " = " + degree.label);
			}
			input.ToolTip = sb.ToString();
			input.IsValid = v =>
			{
				foreach (var degree in te.def.degreeDatas)
					if (degree.degree == v)
						return true;
				return false;
			};
			return input;
		}

		protected override void AddDefsToAutoApply(bool isAutoApply)
		{

		}
	}
}
