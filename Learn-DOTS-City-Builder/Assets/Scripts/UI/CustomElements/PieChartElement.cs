using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.customElements
{
    /// <summary>
    /// Element to display a pie chart.
    /// </summary>
    [UxmlElement]
    public partial class PieChartElement : VisualElement
    {
        private static Color[] PIE_COLORS = new Color[]
        {
            new Color(26/255f, 188/255f, 156/255f),
            new Color(241/255f, 196/255f, 15/255f),
            new Color(52/255f, 152/255f, 219/255f),
            new Color(231/255f, 76/255f, 60/255f),
            //new Color(52/255f, 73/255f, 94/255f),
            //new Color(46/255f, 204/255f, 113/255f),
            //new Color(230/255f, 126/255f, 34/255f),
            new Color(155/255f, 89/255f, 182/255f),
            new Color(236/255f, 240/255f, 241/255f)
        };

        [UxmlAttribute]
        public int BorderWidth { get; set; } = 10;

        #region Data Fields

        private List<PieChartCategory> categories { get; set; } = new()
        {
            new() { title = "A", count = 22 },
            new() { title = "B", count = 89 },
            new() { title = "C", count = 65 },
            new() { title = "D", count = 42 },
        };

        private VisualElement chartElement;

        private VisualElement captionElement;

        /// <summary>
        /// Labels to display category sizes on <see cref="chartElement"/>.
        /// </summary>
        private List<Label> labels = new();

        #endregion

        #region Methods

        public PieChartElement()
        {
            this.style.flexDirection = FlexDirection.Row;

            this.chartElement = new() { name = "chart" };
            this.chartElement.style.flexGrow = 1;
            Add(this.chartElement);

            this.captionElement = new() { name = "caption" };
            this.captionElement.style.marginLeft = 20;
            Add(this.captionElement);

            this.chartElement.generateVisualContent += GenerateVisualContent;
            DisplayCaption();

            this.captionElement.RegisterCallback<GeometryChangedEvent>(DisplayChartLabels);
        }

        /// <summary>
        /// Set chart categories
        /// </summary>
        /// <param name="entries"></param>
        public void SetCategories(params PieChartCategory[] entries)
        {
            categories.Clear();
            categories.AddRange(entries);

            DisplayCaption();

            MarkDirtyRepaint();
        }

        private void GenerateVisualContent(MeshGenerationContext context)
        {
            float size = System.Math.Min(this.chartElement.contentRect.width, this.chartElement.contentRect.height);

            Painter2D painter = context.painter2D;
            painter.lineWidth = BorderWidth;
            if (painter.lineWidth == 0)
                painter.lineWidth = 1f;

            painter.fillColor = Color.red;
            painter.strokeColor = Color.blue;

            int max = categories.Sum(c => c.count);

            if (max == 0)
            {
                painter.strokeColor = Color.gray;
                painter.BeginPath();
                painter.Arc(Vector2.one * size * .5f, size * .5f - painter.lineWidth * .5f, 0, 360);
                painter.Stroke();
            }
            else
            {
                float previousAngle = 0;

                for (int i = 0; i < this.categories.Count; i++)
                {
                    float arcLength = ((float)this.categories[i].count) / ((float)max);
                    arcLength *= 360;
                    painter.strokeColor = PIE_COLORS[i];
                    painter.fillColor = PIE_COLORS[i];
                    painter.BeginPath();
                    painter.Arc(Vector2.one * size * .5f, size * .5f - painter.lineWidth * .5f, previousAngle, previousAngle + arcLength);
                    painter.Stroke();
                    previousAngle = previousAngle + arcLength;
                }
            }
        }

        /// <summary>
        /// Display pie chart section counts.
        /// </summary>
        /// <param name="_"></param>
        private void DisplayChartLabels(GeometryChangedEvent _)
        {
            float size = System.Math.Min(this.chartElement.contentRect.width, this.chartElement.contentRect.height);
            int max = categories.Sum(c => c.count);

            Label label;
            float previousAngle = 0;
            for (int i = 0; i < this.categories.Count; i++)
            {
                float rate = ((float)this.categories[i].count) / ((float)max);
                float arcLength = rate * 360;

                if (i >= this.labels.Count)
                {
                    label = new Label();
                    label.style.fontSize = 12;
                    label.style.color = Color.white;
                    label.style.position = Position.Absolute;
                    this.labels.Add(label);
                }

                rate *= 100;
                label = this.labels[i];
                this.chartElement.Add(label);
                label = this.labels[i];
                label.text = $" {System.Math.Round(rate, 0)}%\n ({this.categories[i].count})";

                float angle = Mathf.Deg2Rad * (previousAngle + arcLength / 2f);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                Vector2 pos = Vector2.one * size * .5f + direction * size * .5f;

                if (angle > Mathf.PI)
                {
                    pos += direction * 30;
                }

                label.style.translate = new Translate(pos.x, pos.y);

                previousAngle = previousAngle + arcLength;
            }

            for (int i = this.categories.Count; i < this.labels.Count; i++)
            {
                this.labels[i].RemoveFromHierarchy();
            }
        }

        /// <summary>
        /// Display caption next to pie chart.
        /// </summary>
        private void DisplayCaption()
        {
            this.captionElement.Clear();

            for (int i = 0; i < categories.Count; i++)
            {
                VisualElement captionEntry = new() { name = "caption__entry" };
                captionEntry.style.flexDirection = FlexDirection.Row;
                captionEntry.style.alignItems = Align.Center;
                captionEntry.style.marginBottom = 5;

                VisualElement captionColor = new() { name = "caption__entry__color" };
                captionColor.style.height = 15;
                captionColor.style.width = 30;
                captionColor.style.backgroundColor = PIE_COLORS[i];
                captionEntry.Add(captionColor);

                Label captionLabel = new() { name = "caption__entry__label", text = categories[i].title };
                captionLabel.style.color = Color.white;
                captionLabel.style.fontSize = 12;
                captionLabel.style.marginTop = captionLabel.style.marginBottom = captionLabel.style.paddingTop = captionLabel.style.paddingBottom = 0;
                captionLabel.style.marginLeft = 5;
                captionEntry.Add(captionLabel);

                this.captionElement.Add(captionEntry);
            }
        }

        #endregion

        public class PieChartCategory
        {
            public string title;

            public int count;
        }
    }
}