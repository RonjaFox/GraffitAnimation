using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GraffitAnimation.Models;

namespace GraffitAnimation.Services
{
    public class ExportService
    {
        private string ffmpegPath = "ffmpeg"; // Предполагается, что ffmpeg в PATH

        public void ExportToGIF(AnimationProject project, string outputPath, int fps)
        {
            try
            {
                // Создаем временную папку для кадров
                string tempDir = Path.Combine(Path.GetTempPath(), "graffiti_export_" + Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                // Сохраняем каждый кадр как PNG
                for (int i = 0; i < project.Frames.Count; i++)
                {
                    var frame = project.Frames[i];
                    var image = frame.RenderToImage();
                    image.Save(Path.Combine(tempDir, $"frame_{i:D4}.png"));
                }

                // Используем ffmpeg для создания GIF
                string inputPattern = Path.Combine(tempDir, "frame_%04d.png");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-framerate {fps} -i \"{inputPattern}\" -loop 0 \"{outputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                }

                // Очищаем временные файлы
                Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при экспорте в GIF: {ex.Message}");
            }
        }

        public void ExportToVideo(AnimationProject project, string outputPath, int fps, string musicFile = null)
        {
            try
            {
                // Создаем временную папку для кадров
                string tempDir = Path.Combine(Path.GetTempPath(), "graffiti_video_" + Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                // Сохраняем каждый кадр как PNG
                for (int i = 0; i < project.Frames.Count; i++)
                {
                    var frame = project.Frames[i];
                    var image = frame.RenderToImage();
                    image.Save(Path.Combine(tempDir, $"frame_{i:D4}.png"));
                }

                // Используем ffmpeg для создания видео
                string inputPattern = Path.Combine(tempDir, "frame_%04d.png");
                string ffmpegArgs;

                if (!string.IsNullOrEmpty(musicFile) && File.Exists(musicFile))
                {
                    ffmpegArgs = $"-framerate {fps} -i \"{inputPattern}\" -i \"{musicFile}\" " +
                                 $"-c:v libx264 -pix_fmt yuv420p -c:a aac \"{outputPath}\"";
                }
                else
                {
                    ffmpegArgs = $"-framerate {fps} -i \"{inputPattern}\" " +
                                 $"-c:v libx264 -pix_fmt yuv420p \"{outputPath}\"";
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = ffmpegArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                }

                // Очищаем временные файлы
                Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при экспорте в видео: {ex.Message}");
            }
        }
    }
}