using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.customElements
{
    [UxmlElement]
    public partial class PieChartElement : VisualElement
    {
        private static Color[] PIE_COLORS = new Color[]
        {
            new Color(26/255f, 188/255f, 156/255f),
            new Color(241/255f, 196/255f, 15/255f),
            new Color(52/255f, 152/255f, 219/255f),
            new Color(231/255f, 76/255f, 60/255f),
            new Color(52/255f, 73/255f, 94/255f),
            new Color(46/255f, 204/255f, 113/255f),
            new Color(230/255f, 126/255f, 34/255f),
            new Color(155/255f, 89/255f, 182/255f),
            new Color(236/255f, 240/255f, 241/255f)
        };

        private List<PieChartEntry> categories { get; set; } = new()
        {
            new() { title = "A", count = 22 },
            new() { title = "B", count = 89 },
            new() { title = "C", count = 65 },
            new() { title = "D", count = 42 },
        };

        public PieChartElement()
        {
            generateVisualContent += GenerateVisualContent;
        }

        public void SetEntries(IEnumerable<PieChartEntry> entries)
        {
            categories.Clear();
            categories.AddRange(entries);

            MarkDirtyRepaint();
        }

        private void GenerateVisualContent(MeshGenerationContext context)
        {
            float width = contentRect.width;
            float height = contentRect.height;

            Painter2D painter = context.painter2D;
            painter.lineWidth = resolvedStyle.borderTopWidth;
            painter.fillColor = Color.red;
            painter.strokeColor = Color.blue;

            int max = categories.Sum(c => c.count);
            float previousAngle = 0;

            Debug.Log(this.categories.Count);

            for (int i = 0; i < this.categories.Count; i++)
            {
                float arcLength = ((float)this.categories[i].count) / ((float)max);
                arcLength *= 360;
                painter.strokeColor = PIE_COLORS[i];
                painter.fillColor = PIE_COLORS[i];
                Debug.Log(PIE_COLORS[i]);
                painter.BeginPath();
                painter.Arc(new Vector2(width * 0.5f, height * 0.5f + painter.lineWidth * .5f), width * .5f - painter.lineWidth * .5f, previousAngle, previousAngle + arcLength);
                painter.Stroke();

                previousAngle = previousAngle + arcLength;
            }
        }

        public class PieChartEntry
        {
            public string title;

            public int count;
        }
    }
}


