using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class Settings : ModSettings
    {
        // ORES
        [SettingsCategory("RFS.CatGeneral")]
        [TweakValue("RimForge")]
        public static bool UseCustomTab = true;
        [TweakValue("RimForge")]
        public static bool GenerateTin = true;
        [TweakValue("RimForge")]
        public static bool GenerateCopper = true;

        // Power poles and similar
        [SettingsCategory("RFS.CatCables")]
        [TweakValue("RimForge", 5, 100)]
        public static float CableMaxDistance = 20;
        [TweakValue("RimForge", 0, 10)]
        public static int CableSegmentsPerCell = 6;

        // COILGUN
        [SettingsCategory("RFS.CatCoilgun")]
        [SettingsPercentage]
        [TweakValue("RimForge", 0.1f, 10)]
        public static float CoilgunBaseDamageMultiplier = 1f;
        [TweakValue("RimForge", 0, 1)]
        public static bool CoilgunSplatterBlood = true;
        [TweakValue("RimForge", 0, 1)]
        public static bool CoilgunDisplayDamageReport = true;
        [SettingsPercentage]
        [SettingsAdvanced]
        [TweakValue("RimForge", 0, 10)]
        public static float CoilgunBasePowerReq = 1;
        [SettingsPercentage]
        [SettingsAdvanced]
        [TweakValue("RimForge", 0, 1)]
        public static float CoilgunPenDamageMultiplier = 1f;
        [SettingsPercentage]
        [SettingsAdvanced]
        [TweakValue("RimForge", 0, 5)]
        public static float CoilgunBuildingDamageMulti = 1f;

        // Blessing
        [SettingsCategory("RFS.BlessingCat")]
        [TweakValue("RimForge", 0f, 1f)]
        [SettingsPercentage]
        public static float BlessingIncreaseChance = 0.5f;
        [TweakValue("RimForge")]
        public static bool BlessingCreateMasterwork = true;
        [TweakValue("RimForge")]
        public static bool BlessingCreateLegendary = true;

        // TESLA COILS
        [SettingsCategory("RFS.CatTesla")]
        [TweakValue("RimForge", 1, 250)]
        public static int TeslaMaxDistance = 25;
        [TweakValue("RimForge", 0, 10000)]
        public static int TeslaCooldown = 900;
        [TweakValue("RimForge", 0, 1000)]
        public static int TeslaStunDuration = 450;
        [TweakValue("RimForge", 1, 500)]
        [SettingsAdvanced]
        public static float TeslaDamage = 10;
        [SettingsAdvanced]
        [TweakValue("RimForge", 1, 30)]
        public static int TeslaTickInterval = 4;
        [SettingsPercentage]
        [SettingsAdvanced]
        [TweakValue("RimForge", 0, 50)]
        public static float TeslaMechanoidDamageMulti = 5;

        // RITUAL
        [SettingsCategory("RFS.CatRitual")]
        [SettingsPercentage]
        [TweakValue("RimForge", 0f, 1f)]
        public static float RitualFailCoefficient = 1f;
        [TweakValue("RimForge", 0f, 1f)]
        public static bool RitualMustBeAtNight = true;
        [SettingsPercentage]
        [SettingsAdvanced]
        [TweakValue("RimForge", 0f, 1f)]
        public static float RitualFailMajorChance = 1f / 3f;

        // GREENHOUSE
        [SettingsCategory("RFS.CatGreenhouse")]
        [TweakValue("RimForge", 0f, 2f)]
        [SettingsPercentage]
        public static float GreenhouseGrowthAccelerationFactor = 0.5f;
        [TweakValue("RimForge", 1, 4000)]
        public static int MaxGreenhouseSize = 300;
        [TweakValue("RimForge", 0.01f, 100f)]
        public static float GreenWattsPerCell = 25;

        // OTHER
        [SettingsCategory("RFS.CatRaids")]
        [SettingsPercentage]
        [TweakValue("RimForge", 0f, 5f)]
        public static float CursedRaidersNumberScale = 1f;
        [SettingsAdvanced]
        [SettingsPercentage]
        [TweakValue("RimForge", 0f, 1f)]
        public static float CursedRaidersNumberMultiplierMin = 0f;
        [SettingsAdvanced]
        [SettingsPercentage]
        [TweakValue("RimForge", 0f, 1f)]
        public static float CursedRaidersNumberMultiplierMax = 1f;
        [TweakValue("RimForge")]
        public static bool SendRaiderLetter = true;
        [TweakValue("RimForge", 0, 20)]
        public static float HateWaveRadius = 5.5f;
        [TweakValue("RimForge", 0, 1000)]
        [SettingsAdvanced]
        public static int HateWaveStunDuration = 400;

        private static bool showAdvanced;
        private static Vector2 scroll;
        private static Rect viewRect;

        public static List<SettingsEntry> GetAllEntries()
        {
            if (SettingsEntry.all != null)
                return SettingsEntry.all;

            var list = new List<SettingsEntry>();
            foreach (var field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var cat = field.GetCustomAttribute<SettingsCategoryAttribute>();
                if (cat != null)
                {
                    list.Add(new SettingsCategory(cat.GetLabel().Translate()));
                }
                var tv = field.GetCustomAttribute<TweakValue>();
                if (tv == null)
                    continue;

                var entry = new SettingsEntry(field);
                entry.TweakValue = tv;
                entry.DefaultValue = field.GetValue(null);
                entry.IsPercentage = field.GetCustomAttribute<SettingsPercentageAttribute>() != null;
                entry.IsAdvanced = field.GetCustomAttribute<SettingsAdvancedAttribute>() != null;

                string txt = $"RFS.{field.Name}".Translate();
                if (txt.IndexOf(':') < 0)
                    continue;

                string[] split = txt.Split(':');
                entry.Label = $"<b>{split[0]}</b>";
                entry.Description = split[1].Trim() + $"\n{"RFS.Default".Translate(entry.IsPercentage ? ((float)entry.DefaultValue * 100f).ToString("F0") + '%' : entry.DefaultValue.ToString())}";

                list.Add(entry);
            }

            SettingsEntry.all = list;
            return list;
        }

        public override void ExposeData()
        {
            foreach (var item in GetAllEntries())
            {
                if (item.Field == null)
                    continue;

                try
                {
                    item.ExposeData();
                }
                catch (Exception e)
                {
                    Core.Error($"Exception exposing settings data for '{item.Field.Name}':", e);
                }
            }
        }

        public static void DrawUI(Rect area)
        {
            var listingArea = area;
            listingArea.width = Mathf.Min(460, area.width);

            Listing_Standard listing = new Listing_Standard();
            Widgets.BeginScrollView(listingArea, ref scroll, viewRect);
            listing.Begin(new Rect(0, 0, listingArea.width - 24, 99999));
            listing.Gap();
            listing.Label("RFS.Header".Translate());
            listing.CheckboxLabeled("RFS.ShowAdvanced".Translate(), ref showAdvanced, "RFS.ShowAdvancedDesc".Translate());
            GUI.color = Color.Lerp(Color.red, Color.white, 0.4f);
            bool reset = listing.ButtonText("Reset All");
            GUI.color = Color.white;

            foreach (var item in GetAllEntries())
            {
                try
                {
                    if (reset)
                        item.Reset();
                    if (item.IsAdvanced && !showAdvanced)
                        continue;

                    item.Draw(listing);
                }
                catch (Exception e)
                {
                    Core.Error($"Exception drawing settings item '{item.Label}':", e);
                }
            }

            float h = listing.CurHeight;
            listing.End();
            Widgets.EndScrollView();
            viewRect = new Rect(0, 0, listingArea.width - 25, h);
        }
    }

    public class SettingsEntry
    {
        internal static List<SettingsEntry> all;

        public readonly FieldInfo Field;
        public TweakValue TweakValue;
        public string Label;
        public string Description;
        public object DefaultValue;
        public bool IsAdvanced;
        public bool IsPercentage;

        public SettingsEntry(FieldInfo fi)
        {
            this.Field = fi;
        }

        public T GetValue<T>()
        {
            return (T)Field.GetValue(null);
        }

        public T GetDefaultValue<T>()
        {
            return (T)DefaultValue;
        }

        public void SetValue(object obj)
        {
            Field.SetValue(null, obj);
        }

        public virtual void ExposeData()
        {
            if (Field == null)
                return;

            var type = Field.FieldType;
            string key = Field.Name;

            if (type == typeof(int))
            {
                var value = GetValue<int>();
                Scribe_Values.Look(ref value, key, (int)DefaultValue);
                SetValue(value);
                return;
            }
            if (type == typeof(float))
            {
                var value = GetValue<float>();
                Scribe_Values.Look(ref value, key, (float)DefaultValue);
                SetValue(value);
                return;
            }
            if (type == typeof(bool))
            {
                bool value = GetValue<bool>();
                Scribe_Values.Look(ref value, key, (bool)DefaultValue);
                SetValue(value);
                return;
            }

            Core.Error($"Unhandled setting type: {type.FullName}");
        }

        public virtual void Reset()
        {
            if (Field == null)
                return;

            SetValue(DefaultValue);
        }

        public virtual void Draw(Listing_Standard listing)
        {
            if (Field == null)
                return;

            var type = Field.FieldType;
            bool isChanged = !GetValue<object>().Equals(DefaultValue);
            Rect labelRect = default;

            if (type == typeof(int))
            {
                var value = GetValue<int>();
                var old = value;
                string label = isChanged ? Label + $": <color=yellow>{value}</color>" : Label + $": {value}";
                labelRect = listing.Label(label, tooltip: Description);
                float changed = listing.Slider(value, TweakValue.min, TweakValue.max);
                value = Mathf.RoundToInt(changed);
                if(old != value)
                    SetValue(value);
            }
            else if (type == typeof(float))
            {
                var value = GetValue<float>();
                var old = value;
                string label = isChanged ? Label + $": <color=yellow>{(IsPercentage ? (value * 100f).ToString("F0") + '%' : value.ToString("F1"))}</color>" : Label + $": {(IsPercentage ? (value * 100f).ToString("F0") + '%' : value.ToString("F1"))}";
                labelRect = listing.Label(label, tooltip: Description);
                value = listing.Slider(value, TweakValue.min, TweakValue.max);
                if (old != value)
                    SetValue(value);
            }
            else if (type == typeof(bool))
            {
                bool value = GetValue<bool>();
                var old = value;
                string label = isChanged ? Label + $": <color=yellow>{(value ? "RFS.Yes" : "RFS.No").Translate()}</color>" : Label + $": {(value ? "RFS.Yes" : "RFS.No").Translate()}";
                listing.CheckboxLabeled(label, ref value, Description);
                if(old != value)
                    SetValue(value);
            }

            if (labelRect != default && Mouse.IsOver(labelRect))
            {
                Widgets.DrawHighlight(labelRect);
                if (Input.GetMouseButtonDown(1))
                    Reset();
            }
        }
    }

    public class SettingsCategory : SettingsEntry
    {
        public SettingsCategory(string label)
            : base(null)
        {
            base.Label = label;
        }

        public override void Draw(Listing_Standard listing)
        {
            listing.GapLine();
            listing.Label($"<color=cyan>{Label}</color>");
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SettingsCategoryAttribute : Attribute
    {
        private readonly string label;

        public SettingsCategoryAttribute(string label)
        {
            this.label = label ?? throw new ArgumentNullException(nameof(label));
        }

        public string GetLabel() => label;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SettingsPercentageAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SettingsAdvancedAttribute : Attribute
    {
    }
}
