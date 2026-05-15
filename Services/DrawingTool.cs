using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GraffitAnimation.Services
{
    public class DrawingTool
    {
        public enum ToolType { Brush, Eraser, Fill }

        private ToolType currentTool = ToolType.Brush;
        private Color currentColor = Color.Black;
        public int BrushSize { get; set; } = 5;
        private Point lastPoint;
        private Bitmap currentBitmap;
        private bool isDrawing = false;

        public DrawingTool()
        {
            currentColor = Color.Black;
        }

        public void SelectTool(ToolType tool)
        {
            currentTool = tool;
        }

        public void SetColor(Color color)
        {
            currentColor = color;
        }

        public void StartDrawing(Point point, Bitmap bitmap)
        {
            if (bitmap == null) return;
            currentBitmap = bitmap;
            lastPoint = point;
            isDrawing = true;

            if (currentTool == ToolType.Brush)
            {
                DrawPoint(bitmap, point);
            }
        }

        public void Draw(Point point, Bitmap bitmap)
        {
            if (!isDrawing || bitmap == null) return;
            currentBitmap = bitmap;

            using (Graphics g = Graphics.FromImage(currentBitmap))
            {
                switch (currentTool)
                {
                    case ToolType.Brush:
                        DrawLine(g, lastPoint, point);
                        break;
                    case ToolType.Eraser:
                        EraseLine(g, lastPoint, point);
                        break;
                    case ToolType.Fill:
                        break;
                }
            }
            lastPoint = point;
        }

        public void EndDrawing()
        {
            if (currentTool == ToolType.Fill && isDrawing && currentBitmap != null && lastPoint.X >= 0 && lastPoint.X < currentBitmap.Width && lastPoint.Y >= 0 && lastPoint.Y < currentBitmap.Height)
            {
                FloodFill(currentBitmap, lastPoint, currentColor);
            }
            isDrawing = false;
        }

        private void DrawPoint(Bitmap bitmap, Point point)
        {
            if (point.X < 0 || point.X >= bitmap.Width || point.Y < 0 || point.Y >= bitmap.Height)
                return;

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (Brush brush = new SolidBrush(currentColor))
                {
                    g.FillEllipse(brush, point.X - BrushSize / 2, point.Y - BrushSize / 2, BrushSize, BrushSize);
                }
            }
        }

        private void DrawLine(Graphics g, Point from, Point to)
        {
            using (Pen pen = new Pen(currentColor, BrushSize))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                g.DrawLine(pen, from, to);
            }
        }

        private void EraseLine(Graphics g, Point from, Point to)
        {
            using (Pen pen = new Pen(Color.White, BrushSize))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                g.DrawLine(pen, from, to);
            }
        }

        private void FloodFill(Bitmap bitmap, Point point, Color fillColor)
        {
            if (point.X < 0 || point.X >= bitmap.Width || point.Y < 0 || point.Y >= bitmap.Height)
                return;

            Color targetColor = bitmap.GetPixel(point.X, point.Y);
            if (targetColor.ToArgb() == fillColor.ToArgb())
                return;

            var queue = new Queue<Point>();
            queue.Enqueue(point);
            var visited = new HashSet<Point>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (visited.Contains(current)) continue;
                visited.Add(current);

                if (current.X < 0 || current.X >= bitmap.Width || current.Y < 0 || current.Y >= bitmap.Height)
                    continue;

                Color pixelColor = bitmap.GetPixel(current.X, current.Y);
                if (pixelColor.ToArgb() != targetColor.ToArgb())
                    continue;

                bitmap.SetPixel(current.X, current.Y, fillColor);

                queue.Enqueue(new Point(current.X + 1, current.Y));
                queue.Enqueue(new Point(current.X - 1, current.Y));
                queue.Enqueue(new Point(current.X, current.Y + 1));
                queue.Enqueue(new Point(current.X, current.Y - 1));
            }
        }
    }
}