using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace GraffitAnimation.Models
{
    public class AnimationProject
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<AnimationFrame> Frames { get; set; } = new List<AnimationFrame>();
        public string MusicFile { get; set; }

        public void SaveToFile(string filePath)
        {
            using (var zipArchive = ZipFile.Open(filePath, ZipArchiveMode.Create))
            {
                // Сохраняем метаданные проекта
                var metaEntry = zipArchive.CreateEntry("project.meta");
                using (var stream = metaEntry.Open())
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine($"Name={Name}");
                    writer.WriteLine($"Width={Width}");
                    writer.WriteLine($"Height={Height}");
                    writer.WriteLine($"FrameCount={Frames.Count}");
                    if (!string.IsNullOrEmpty(MusicFile) && File.Exists(MusicFile))
                        writer.WriteLine($"MusicFile={Path.GetFileName(MusicFile)}");
                }

                // Сохраняем каждый кадр
                for (int i = 0; i < Frames.Count; i++)
                {
                    var frame = Frames[i];
                    for (int j = 0; j < frame.Layers.Count; j++)
                    {
                        var layer = frame.Layers[j];
                        var layerEntry = zipArchive.CreateEntry($"frames/frame_{i}/layer_{j}.png");
                        using (var stream = layerEntry.Open())
                        {
                            if (layer.Bitmap != null)
                                layer.Bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }

                // Сохраняем музыку если есть
                if (!string.IsNullOrEmpty(MusicFile) && File.Exists(MusicFile))
                {
                    var musicEntry = zipArchive.CreateEntry($"music/{Path.GetFileName(MusicFile)}");
                    using (var stream = musicEntry.Open())
                    using (var fileStream = File.OpenRead(MusicFile))
                    {
                        fileStream.CopyTo(stream);
                    }
                }
            }
        }

        public static AnimationProject LoadFromFile(string filePath)
        {
            var project = new AnimationProject();
            using (var zipArchive = ZipFile.OpenRead(filePath))
            {
                // Загружаем метаданные
                var metaEntry = zipArchive.GetEntry("project.meta");
                if (metaEntry != null)
                {
                    using (var stream = metaEntry.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("Name=")) project.Name = line.Substring(5);
                            if (line.StartsWith("Width=")) project.Width = int.Parse(line.Substring(6));
                            if (line.StartsWith("Height=")) project.Height = int.Parse(line.Substring(7));
                            if (line.StartsWith("FrameCount=")) { /* frameCount = int.Parse(line.Substring(11)); */ }
                        }
                    }
                }

                // Загружаем кадры
                int frameIndex = 0;
                while (true)
                {
                    var layerIndex = 0;
                    var frame = new AnimationFrame();
                    bool frameExists = false;

                    while (true)
                    {
                        var layerEntry = zipArchive.GetEntry($"frames/frame_{frameIndex}/layer_{layerIndex}.png");
                        if (layerEntry == null) break;
                        frameExists = true;

                        using (var stream = layerEntry.Open())
                        {
                            var bitmap = new Bitmap(stream);
                            var layer = new AnimationFrame.Layer()
                            {
                                Name = $"Layer {layerIndex + 1}",
                                Bitmap = bitmap,
                                ZIndex = layerIndex
                            };
                            frame.Layers.Add(layer);
                        }
                        layerIndex++;
                    }

                    if (!frameExists) break;
                    project.Frames.Add(frame);
                    frameIndex++;
                }
            }
            return project;
        }
    }
}