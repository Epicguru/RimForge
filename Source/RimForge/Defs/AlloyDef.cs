using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class AlloyDef : Def
    {
        public bool IsValid { get; protected set; }
        public RatioHolder LargestRatio
        {
            get
            {
                if (input == null || input.Count == 0)
                    return null;

                RatioHolder largest = null;
                foreach (var ratio in input)
                {
                    if (largest == null || ratio.count > largest.count)
                        largest = ratio;
                }
                return largest;
            }
        }

        /// <summary>
        /// Returns the minimum forge temperature required to create this alloy.
        /// This is set in the def (see <see cref="minTemperature"/>),
        /// but if a value is not specified, then the highest melting temperature of the input
        /// and output materials is used.
        /// </summary>
        public float MinTemperature
        {
            get
            {
                if (minTemperature != null)
                    return minTemperature.Value;

                float min = float.MinValue;

                if (input != null)
                {
                    foreach (var item in input)
                    {
                        if (item?.resource == null)
                            continue;
                        float point = item.resource.GetMeltingPoint();
                        if (point > min)
                            min = point;
                    }
                }
                if (output?.resource != null)
                {
                    if (output.resource.GetMeltingPoint() > min)
                        min = output.resource.GetMeltingPoint();
                }

                return min;
            }
        }

        public List<RatioHolder> input;
        public RatioHolder output;
        public bool allowBulk = true;
        public int bulkMultiplier = 10;
        public float baseWork = 500;

        private float? minTemperature = null;

        public Color? GetMoltenColor(int index)
        {
            if (!IsValid)
                return null;

            if (index > 3)
                return null;

            Color fallback = new Color32(255, 87, 20, 255);
            if (index == 3)
            {
                return output.resource.GetMoltenColor() ?? fallback;
            }

            int count = input.Count;
            var largest = LargestRatio;

            if (index == 0)
            {
                // If there are only 2 inputs, there should be nothing in the middle.

                if (count < 3)
                    return null;

                return largest.resource.GetMoltenColor() ?? fallback;
            }
            else
            {
                if (count < 3)
                    return input[index - 1].resource.GetMoltenColor() ?? fallback;

                if(index == 1)
                {
                    for (int i = 0; i < input.Count; i++)
                    {
                        if (input[i] == largest)
                            continue;
                        return input[i].resource.GetMoltenColor() ?? fallback;
                    }
                }
                else
                {
                    for (int i = input.Count - 1; i >= 0; i--)
                    {
                        if (input[i] == largest)
                            continue;
                        return input[i].resource.GetMoltenColor() ?? fallback;
                    }
                }

                Core.Error("???? FIXME");
                return null;
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            IsValid = true;

            // Child errors. Shouldn't be any.
            foreach (var thing in base.ConfigErrors())
                yield return thing;

            // Check that input is not null.
            if (input == null)
            {
                IsValid = false;
                yield return "No inputs defined! There must be at least 2 input materials.";
            }
            else
            {
                // Check that there are at least 2 input materials.
                // Specific errors within those materials are checked later.
                if (input.Count < 2)
                {
                    IsValid = false;
                    yield return "There must be at least 2 input materials for an alloy to be valid.";
                }
            }

            // Check that the output is not null.
            if (output == null)
            {
                IsValid = false;
                yield return "There is no output defined!";
            }

            // Process input errors.
            if (input != null)
            {
                foreach (var item in input)
                {
                    foreach (var error in item.ConfigErrors())
                    {
                        IsValid = false;
                        yield return $"[IN {item}] {error}";
                    }
                }
            }

            // Output errors.
            foreach (var error in output.ConfigErrors())
            {
                IsValid = false;
                yield return $"[OUT {output}] {error}";
            }

            if (allowBulk && bulkMultiplier <= 1)
            {
                yield return $"Bulk multiplier should be at least 2! Current: {bulkMultiplier}";
            }
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool full)
        {
            if (!full)
                return $"AlloyDef {defName} '{LabelCap}'";

            string inputs = "";
            if (input != null)
            {
                for (int i = 0; i < input.Count; i++)
                {
                    var item = input[i];
                    if (item == null)
                        continue;

                    inputs += "  -" + item;
                    if (i != input.Count - 1)
                        inputs += ",\n";
                }
            }
            return $"AlloyDef '{base.LabelCap}':\nInputs:\n{inputs}\nOutput:\n  -{output}\nAt {MinTemperature} degrees cel.";
        }
    }
}
