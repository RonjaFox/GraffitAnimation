using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GraffitAnimation.Models
{
    public class AnimationFrame
    {
        public class Layer
        {
            public string Name { get; set; }
            public Bitmap Bitmap { get; set; }
            public bool IsVisible { get; set; } = true;
            public float Opacity { get; set; } = 1.0f;
            public int ZIndex { get; set; } = 0;
        }

        public List<Layer> Layers { get; set; } = new List<Layer>();
        public int Duration { get; set; } = 100; // в миллисекундах

        public Bitmap RenderToImage()
        {
            if (Layers.Count == 0)
                return new Bitmap(800, 600);

            var sortedLayers = Layers.OrderBy(l => l.ZIndex).ToList();
            var result = new Bitmap(sortedLayers[0].Bitmap.Width, sortedLayers[0].Bitmap.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.White);
                foreach (var layer in sortedLayers)
                {
                    if (layer.IsVisible && layer.Bitmap != null)
                    {
                        // Простое рендеринг с учетом видимости
                        g.DrawImage(layer.Bitmap, 0, 0);
                    }
                }
            }
            return result;
        }
    }
}