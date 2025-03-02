using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MirageXR
{
    public class HyperlinkPositionData
    {
        public string DisplayText { get; set; }
        public Dictionary<string, Vector3> Positions { get; set; }

        private const string pattern = @"<color=[^>]+>\[([^\[\]]+)\]</color><pos=(-?\d+\.?\d*),(-?\d+\.?\d*),(-?\d+\.?\d*)>";

        public static HyperlinkPositionData SplitPositionsFromText(string fullText)
        {
            var data = new HyperlinkPositionData
            {
                DisplayText = fullText,
                Positions = new Dictionary<string, Vector3>()
            };
            var matches = Regex.Matches(fullText, pattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 5)
                {
                    var linkId = match.Groups[1].Value;
                    if (float.TryParse(match.Groups[2].Value, out var x) &&
                        float.TryParse(match.Groups[3].Value, out var y) &&
                        float.TryParse(match.Groups[4].Value, out var z))
                    {
                        data.Positions[linkId] = new Vector3(x, y, z);
                        data.DisplayText = data.DisplayText.Replace($"<pos={x:F2},{y:F2},{z:F2}>", "");
                    }
                }
            }
            
            return data;
        }
    }
}
