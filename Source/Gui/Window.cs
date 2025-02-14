﻿using UnityEngine;
using Verse;
using InGameDefEditor.Gui.EditorWidgets;
using RimWorld;
using System.Collections.Generic;
using InGameDefEditor.Gui.EditorWidgets.Misc;
using System;
using System.Linq;

namespace InGameDefEditor
{
    class InGameDefEditorWindow : Window
	{
		private static IEditableDefType selectedDefType = null;
		private static IParentStatWidget selectedDef = null;
		private static string defFilter = "";

        private static Vector2 
			leftScroll = Vector2.zero,
			middleScroll = Vector2.zero,
			rightScroll = Vector2.zero;

        private float previousYMaxLeft = 0,
                      previousYMaxMiddle = 0,
                      previousYMaxRight = 0;

		private IEnumerable<IEditableDefType> defTypes;
		
		public override Vector2 InitialSize => new Vector2((float)UI.screenWidth - 20, (float)UI.screenHeight - 20);

        public InGameDefEditorWindow()
        {
            IOUtil.LoadData();
			this.defTypes = this.GetDefTypes(true);
        }

		public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Small;
            float outerY = 10;

			if (Widgets.ButtonText(new Rect(10, outerY, 200, 30), ((selectedDefType == null) ? "Def Type" : selectedDefType.Label)))
			{
				WindowUtil.DrawFloatingOptions(
                        new WindowUtil.FloatOptionsArgs<IEditableDefType>()
                        {
                            items = this.defTypes,
                            getDisplayName = dt => dt.Label,
							onSelect = dt =>
							{
								selectedDefType = dt;
								selectedDef = null;
								defFilter = "";
							}
						});
			}

			if (selectedDefType != null)
			{
				Widgets.Label(new Rect(550, outerY + 4, 100, 30), "InGameDefEditor.DefFilter".Translate());
				defFilter = Widgets.TextField(new Rect(625, outerY, 100, 30), defFilter).ToLower();
				if (Widgets.ButtonText(new Rect(220, outerY, 300, 30), ((selectedDef == null) ? selectedDefType.Label + " Def" : selectedDef.DisplayLabel)))
				{
					WindowUtil.DrawFloatingOptions(new WindowUtil.FloatOptionsArgs<object>()
					{
						items = selectedDefType.GetDefs().Where(def => (defFilter.Length > 0) ? this.GetDisplayLabel(def).ToLower().IndexOf(defFilter) != -1 : true),
						getDisplayName = i => this.GetDisplayLabel(i),
						onSelect = i =>
						{
							if (i is Def def)
								this.CreateSelected(def, selectedDefType.Type);
						}
					});
				}
			}

			if (Defs.DisabledDefs.Count > 0 &&
				Widgets.ButtonText(new Rect(rect.xMax - 550, outerY, 200, 30), "InGameDefEditor.DisabledDefs".Translate()))
			{
				WindowUtil.DrawFloatingOptions(
						new WindowUtil.FloatOptionsArgs<Pair<string, object>>()
						{
							items = Defs.DisabledDefs.All,
							getDisplayName = p => Util.GetLabel(p.Second),
							onSelect = p => GetSelectionFromPair(p),
						});
			}

			if (Defs.ApplyStatsAutoDefs.Count > 0 && 
				Widgets.ButtonText(new Rect(rect.xMax - 300, outerY, 200, 30), "InGameDefEditor.AutoLoaded".Translate()))
			{
				WindowUtil.DrawFloatingOptions(
						new WindowUtil.FloatOptionsArgs<Pair<string, object>>()
						{
							items = Defs.ApplyStatsAutoDefs.All,
							getDisplayName = p => Util.GetLabel(p.Second),
							onSelect = p => GetSelectionFromPair(p),
						});
			}

			outerY += 60;

            if (selectedDef != null)
            {
				float x = 0;
				float y = outerY;

				selectedDef.DrawStaticButtons(x, ref y, 370);

				if (!selectedDef.IsDisabled)
				{
					// Left column
					Widgets.BeginScrollView(
						new Rect(0, y, 370, rect.height - outerY - 120 - Math.Abs(outerY - y)),
						ref leftScroll,
						new Rect(0, 0, 354, this.previousYMaxLeft));
					y = 0;

					selectedDef.DrawLeft(x, ref y, 354);
					this.previousYMaxLeft = y;

					Widgets.EndScrollView();

					// Middle Column
					Widgets.BeginScrollView(
						new Rect(380, outerY, 370, rect.height - outerY - 120),
						ref middleScroll,
						new Rect(0, 0, 354, this.previousYMaxMiddle));
					y = 0;
					selectedDef.DrawMiddle(x, ref y, 354);
					this.previousYMaxMiddle = y;
					Widgets.EndScrollView();

					// Right Column
					Widgets.BeginScrollView(
						new Rect(760, outerY, 370, rect.height - outerY - 120),
						ref rightScroll,
						new Rect(0, 0, 354, this.previousYMaxRight));
					y = 0;
					selectedDef.DrawRight(x, ref y, 354);
					this.previousYMaxRight = y;

					Widgets.EndScrollView();
				}
			}

			if (Widgets.ButtonText(new Rect(100, rect.yMax - 32, 100, 32), "Close".Translate()))
			{
				Find.WindowStack.TryRemove(typeof(InGameDefEditorWindow), true);
			}
			if (selectedDefType != null && selectedDef != null && 
				Widgets.ButtonText(new Rect(rect.xMax - 340, rect.yMax - 32, 100, 32), "Reset".Translate() + " Def"))
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Reset " + selectedDef.DisplayLabel + "?", delegate { this.ResetSelected(); }));
			}

			if (selectedDefType != null &&
				Widgets.ButtonText(new Rect(rect.xMax - 230, rect.yMax - 32, 120, 32), "Reset".Translate() + " " + selectedDefType.Type))
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Reset".Translate() + " " + selectedDefType.Type + "?", () =>
				{
					selectedDefType.ResetTypeDefs();
					ResetSelected();
				}));
			}

			if (Widgets.ButtonText(new Rect(rect.xMax - 100, rect.yMax - 32, 100, 32), "InGameDefEditor.ResetAll".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "Reset everything to the original game's settings?",
                    delegate
					{
						Defs.DisabledDefs.Clear();
						Defs.ApplyStatsAutoDefs.Clear();
						foreach (var v in this.GetDefTypes(false))
							v.ResetTypeDefs();
						if (selectedDef != null)
							ResetSelected();
					}));
            }
        }

		private string GetDisplayLabel(object o)
		{
			if (o is Def def)
				return Util.GetLabel(def);
			return o.ToString();
		}

		private void GetSelectionFromPair(Pair<string, object> p)
		{
			if (DatabaseUtil.TryGetDefTypeFor(p.Second, out DefType dt))
			{
				foreach (var i in this.defTypes)
				{
					if (i.Type == dt)
					{
						selectedDefType = i;
						if (p.Second is Def def)
							this.CreateSelected(def, selectedDefType.Type);
						break;
					}
				}
			}
		}

		private void ResetSelected()
		{
			if (selectedDef != null)
			{
				Defs.DisabledDefs.Remove(selectedDef.BaseObject);
				Defs.ApplyStatsAutoDefs.Remove(selectedDef.BaseObject);

				selectedDef.ResetParent();
				selectedDef.Rebuild();
			}
		}

		public void ResetScrolls()
        {
            leftScroll = middleScroll = rightScroll = Vector2.zero;
        }

        public override void PostClose()
        {
            base.PostClose();
            IOUtil.SaveData();
		}

		private void CreateSelected(Def d, DefType type)
        {
            switch (type)
            {
				//case DefType.Animal:
				case DefType.Apparel:
				case DefType.Building:
				case DefType.Plant:
				case DefType.Resource:
				case DefType.Disabled:
				case DefType.Ingestible:
				case DefType.Mineable:
				case DefType.Weapon:
					selectedDef = new ThingDefWidget(d as ThingDef, type);
                    break;
				case DefType.Projectile:
					selectedDef = new ProjectileDefWidget(d as ThingDef, type);
					break;
				case DefType.Biome:
                    selectedDef = new BiomeWidget(d as BiomeDef, type);
                    break;
				case DefType.Recipe:
					selectedDef = new RecipeWidget(d as RecipeDef, type);
					break;
				case DefType.Trait:
					selectedDef = new TraitWidget(d as TraitDef, type);
					break;
				case DefType.Thought:
					selectedDef = new ThoughtDefWidget(d as ThoughtDef, type);
					break;
				case DefType.StoryTeller:
					selectedDef = new StoryTellerDefWidget(d as StorytellerDef, type);
					break;
				case DefType.Difficulty:
					selectedDef = new DifficultyDefWidget(d as DifficultyDef, type);
					break;
				case DefType.Hediff:
					selectedDef = new HediffDefWidget(d as HediffDef, type);
					break;
				case DefType.PawnKind:
					selectedDef = new PawnKindDefWidget(d as PawnKindDef, type);
					break;
			}
            this.ResetScrolls();
			IngredientCountWidget.ResetUniqueId();
		}

		/*private IEnumerable<DifficultyDef> SortDifficultyOptions(SortedDictionary<string, DifficultyDef>.ValueCollection values)
		{
			SortedDictionary<int, List<DifficultyDef>> d = new SortedDictionary<int, List<DifficultyDef>>();
			foreach (var v in values)
			{
				if (!d.TryGetValue(v.difficulty, out List<DifficultyDef> defs))
				{
					defs = new List<DifficultyDef>();
					d[v.difficulty] = defs;
				}
				defs.Add(v);
			}
			List<DifficultyDef> result = new List<DifficultyDef>(values.Count);
			foreach (List<DifficultyDef> defs in d.Values)
				foreach (DifficultyDef def in defs)
					result.Add(def);
			return result;
		}*/

		#region ButonWidget
		private interface IEditableDefType
		{
			DefType Type { get; }
			string Label { get; }

			IEnumerable<object> GetDefs();
			void ResetTypeDefs();
		}

		private struct EditableDefType<D> : IEditableDefType where D : Def, new()
		{
			private readonly string label;
			public readonly DefType type;
			public readonly IEnumerable<D> Defs;
			
			public string Label => this.label;
			public DefType Type => this.type;

			public EditableDefType(string label, DefType type, IEnumerable<D> defs)
			{
				this.label = label;
				this.type = type;
				this.Defs = defs;
			}

			public IEnumerable<object> GetDefs()
			{
				List<object> l = new List<object>(this.Defs.Count());
				foreach (Def d in this.Defs)
					l.Add(d);
				return l;
			}

			public void ResetTypeDefs()
			{
				foreach (var v in this.Defs)
				{
					InGameDefEditor.Defs.DisabledDefs.Remove(v);
					InGameDefEditor.Defs.ApplyStatsAutoDefs.Remove(v);
					Backup.ApplyStats(v);
				}
			}
		}

		private IEnumerable<IEditableDefType> GetDefTypes(bool includeDisabled)
		{
			List<IEditableDefType> defTypes = new List<IEditableDefType>()
			{
				//new EditableDefType<ThingDef>("Animals", DefType.Animal, Defs.AnimalDefs.Values),
				new EditableDefType<ThingDef>("Apparel", DefType.Apparel, Defs.ApparelDefs.Values),
				new EditableDefType<BackstoryDef>("Backstories", DefType.Backstory, Defs.BackstoryDefs.Values),
				new EditableDefType<BiomeDef>("Biomes", DefType.Biome, Defs.BiomeDefs.Values),
				new EditableDefType<ThingDef>("Buildings", DefType.Building, Defs.BuildingDefs.Values),
				new EditableDefType<DifficultyDef>("Difficulty", DefType.Difficulty, Defs.DifficultyDefs.Values),// this.SortDifficultyOptions(Defs.DifficultyDefs.Values)),
				new EditableDefType<HediffDef>("Hediffs", DefType.Hediff, Defs.HediffDefs.Values),
				new EditableDefType<ThingDef>("Ingestibles", DefType.Ingestible, Defs.IngestibleDefs.Values),
				new EditableDefType<ThingDef>("Mineables", DefType.Mineable, Defs.MineableDefs.Values),
				new EditableDefType<PawnKindDef>("Pawn Kind", DefType.PawnKind, Defs.PawnKindDefs.Values),
				new EditableDefType<ThingDef>("Plants", DefType.Plant, Defs.PlantDefs.Values),
				new EditableDefType<ThingDef>("Projectiles", DefType.Projectile, Defs.ProjectileDefs.Values),
				new EditableDefType<RecipeDef>("Recipes", DefType.Recipe, Defs.RecipeDefs.Values),
				new EditableDefType<ThingDef>("Resource", DefType.Resource, Defs.ResourceDefs.Values),
				new EditableDefType<StorytellerDef>("Story Tellers", DefType.StoryTeller, Defs.StoryTellerDefs.Values),
				new EditableDefType<ThoughtDef>("Thoughts", DefType.Thought, Defs.ThoughtDefs.Values),
				new EditableDefType<TraitDef>("Traits", DefType.Trait, Defs.TraitDefs.Values),
				new EditableDefType<ThingDef>("Weapons", DefType.Weapon, Defs.WeaponDefs.Values),
			};

			/*if (includeDisabled && Defs.DisabledDefs.Count > 0)
				defTypes.Add(new EditableDefType<ThingDef>("Disabled", DefType.Disabled, Defs.DisabledDefs.Values));*/
			return defTypes;
		}

		/*private interface IButtonWidget
        {
			DefType Type { get; }
            void Draw(float x, float y, float width, IParentStatWidget selected);
			void ResetTypeDefs();
        }

        private class ButtonWidget<D> : IButtonWidget where D : Def, new()
        {
            public delegate void OnSelect(Def d, DefType type);
			
			private readonly string label;
            private readonly DefType type;
            private readonly IEnumerable<D> possibleDefs;
            private readonly OnSelect onSelect;

			public DefType Type { get => this.type; }

			public ButtonWidget(string label, DefType type, IEnumerable<D> possibleDefs, OnSelect onSelect)
            {
                this.label = label;
                this.type = type;
                this.possibleDefs = possibleDefs;
                this.onSelect = onSelect;
			}

            public void Draw(float x, float y, float width, IParentStatWidget selected)
            {
                string label = this.label;
                if (selected != null && selected.Type == this.type)
                    label = selected.DisplayLabel;

                if (Widgets.ButtonText(new Rect(x, y, width, 30), label))
                {
                    WindowUtil.DrawFloatingOptions(
                        new WindowUtil.FloatOptionsArgs<D>()
                        {
                            items = possibleDefs,
                            getDisplayName = def => Util.GetLabel(def),
                            onSelect = def => this.onSelect(def, this.type)
                        });
                }
            }

			public void ResetTypeDefs()
			{
				foreach (var v in this.possibleDefs)
					Backup.ApplyStats(v);
			}
		}*/
		#endregion
	}
}