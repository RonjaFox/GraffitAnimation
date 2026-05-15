using System;
using System.Drawing;
using System.Windows.Forms;
using GraffitAnimation.Models;
using GraffitAnimation.Services;

namespace GraffitAnimation
{
    public partial class MainForm : Form
    {
        private AnimationProject currentProject;
        private DrawingTool drawingTool;
        private ExportService exportService;
        private bool isPlaying = false;
        private int currentFrameIndex = 0;
        private System.Windows.Forms.Timer playbackTimer;
        private Bitmap canvasBitmap;
        private int selectedLayerIndex = 0;

        public MainForm()
        {
            InitializeComponent();
            drawingTool = new DrawingTool();
            exportService = new ExportService();
            InitializePlaybackTimer();
        }

        private void InitializePlaybackTimer()
        {
            playbackTimer = new System.Windows.Forms.Timer();
            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (isPlaying && currentProject != null)
            {
                currentFrameIndex++;
                if (currentFrameIndex >= currentProject.Frames.Count)
                {
                    currentFrameIndex = 0;
                }
                RefreshCanvas();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = "ГраффитАнимация";
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Меню
            MenuStrip menuStrip = new MenuStrip();
            
            // Файл
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Новый проект", null, NewProject_Click);
            fileMenu.DropDownItems.Add("Открыть", null, OpenProject_Click);
            fileMenu.DropDownItems.Add("Сохранить", null, SaveProject_Click);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Экспорт в GIF", null, ExportGIF_Click);
            fileMenu.DropDownItems.Add("Экспорт в видео", null, ExportVideo_Click);
            fileMenu.DropDownItems.Add("Выход", null, (s, e) => this.Close());
            menuStrip.Items.Add(fileMenu);

            // Правка
            ToolStripMenuItem editMenu = new ToolStripMenuItem("Правка");
            editMenu.DropDownItems.Add("Отменить", null, (s, e) => MessageBox.Show("Функция отмены в разработке"));
            editMenu.DropDownItems.Add("Повторить", null, (s, e) => MessageBox.Show("Функция повтора в разработке"));
            menuStrip.Items.Add(editMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Панель инструментов
            Panel toolPanel = new Panel() { Dock = DockStyle.Left, Width = 150, BackColor = Color.LightGray };
            
            int yPos = 10;
            
            // Выбор инструмента
            Label toolLabel = new Label() { Text = "Инструменты:", Left = 10, Top = yPos, Width = 130 };
            toolPanel.Controls.Add(toolLabel);
            yPos += 30;

            Button brushBtn = new Button() { Text = "✏️ Кисть", Left = 10, Top = yPos, Width = 130, Height = 30 };
            brushBtn.Click += (s, e) => { drawingTool.SelectTool(DrawingTool.ToolType.Brush); brushBtn.BackColor = Color.Yellow; };
            toolPanel.Controls.Add(brushBtn);
            yPos += 40;

            Button eraserBtn = new Button() { Text = "🧹 Ластик", Left = 10, Top = yPos, Width = 130, Height = 30 };
            eraserBtn.Click += (s, e) => { drawingTool.SelectTool(DrawingTool.ToolType.Eraser); eraserBtn.BackColor = Color.Yellow; };
            toolPanel.Controls.Add(eraserBtn);
            yPos += 40;

            Button fillBtn = new Button() { Text = "🪣 Заливка", Left = 10, Top = yPos, Width = 130, Height = 30 };
            fillBtn.Click += (s, e) => { drawingTool.SelectTool(DrawingTool.ToolType.Fill); fillBtn.BackColor = Color.Yellow; };
            toolPanel.Controls.Add(fillBtn);
            yPos += 40;

            Button colorBtn = new Button() { Text = "🎨 Цвет", Left = 10, Top = yPos, Width = 130, Height = 30 };
            colorBtn.Click += (s, e) => SelectColor();
            toolPanel.Controls.Add(colorBtn);
            yPos += 40;

            // Размер кисти
            Label sizeLabel = new Label() { Text = "Размер:", Left = 10, Top = yPos, Width = 130 };
            toolPanel.Controls.Add(sizeLabel);
            yPos += 25;

            TrackBar sizeTrack = new TrackBar() { Left = 10, Top = yPos, Width = 130, Minimum = 1, Maximum = 50, Value = 5 };
            sizeTrack.ValueChanged += (s, e) => drawingTool.BrushSize = sizeTrack.Value;
            toolPanel.Controls.Add(sizeTrack);
            yPos += 40;

            this.Controls.Add(toolPanel);

            // Основная область рисования
            Panel canvasPanel = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White };
            PictureBox canvas = new PictureBox() { Dock = DockStyle.Fill, BackColor = Color.White };
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
            canvas.Paint += Canvas_Paint;
            canvasPanel.Controls.Add(canvas);
            this.Controls.Add(canvasPanel);

            // Правая панель - управление кадрами и слоями
            Panel rightPanel = new Panel() { Dock = DockStyle.Right, Width = 250, BackColor = Color.LightGray };

            yPos = 10;

            // FPS
            Label fpsLabel = new Label() { Text = "FPS:", Left = 10, Top = yPos, Width = 230 };
            rightPanel.Controls.Add(fpsLabel);
            yPos += 25;

            NumericUpDown fpsSpinner = new NumericUpDown() { Left = 10, Top = yPos, Width = 230, Value = 10, Minimum = 1, Maximum = 120 };
            fpsSpinner.ValueChanged += (s, e) => UpdatePlaybackSpeed((int)fpsSpinner.Value);
            rightPanel.Controls.Add(fpsSpinner);
            yPos += 35;

            // Кнопки воспроизведения
            Button playBtn = new Button() { Text = "▶️ Воспроизведение", Left = 10, Top = yPos, Width = 110, Height = 30 };
            playBtn.Click += PlayButton_Click;
            rightPanel.Controls.Add(playBtn);

            Button stopBtn = new Button() { Text = "⏹️ Стоп", Left = 125, Top = yPos, Width = 110, Height = 30 };
            stopBtn.Click += StopButton_Click;
            rightPanel.Controls.Add(stopBtn);
            yPos += 40;

            // Информация о кадре
            Label frameLabel = new Label() { Text = "Кадры:", Left = 10, Top = yPos, Width = 230 };
            rightPanel.Controls.Add(frameLabel);
            yPos += 25;

            Label currentFrameLabel = new Label() { Text = "Кадр: 0/0", Left = 10, Top = yPos, Width = 230, Name = "CurrentFrameLabel" };
            rightPanel.Controls.Add(currentFrameLabel);
            yPos += 30;

            // Кнопки управления кадрами
            Button addFrameBtn = new Button() { Text = "+ Новый кадр", Left = 10, Top = yPos, Width = 230, Height = 30 };
            addFrameBtn.Click += AddFrame_Click;
            rightPanel.Controls.Add(addFrameBtn);
            yPos += 40;

            Button deleteFrameBtn = new Button() { Text = "- Удалить кадр", Left = 10, Top = yPos, Width = 230, Height = 30 };
            deleteFrameBtn.Click += DeleteFrame_Click;
            rightPanel.Controls.Add(deleteFrameBtn);
            yPos += 40;

            // ListBox для кадров
            ListBox framesList = new ListBox() { Left = 10, Top = yPos, Width = 230, Height = 150, Name = "FramesList" };
            framesList.SelectedIndexChanged += FramesList_SelectedIndexChanged;
            rightPanel.Controls.Add(framesList);
            yPos += 160;

            // Слои
            Label layersLabel = new Label() { Text = "Слои:", Left = 10, Top = yPos, Width = 230 };
            rightPanel.Controls.Add(layersLabel);
            yPos += 25;

            Button addLayerBtn = new Button() { Text = "+ Добавить слой", Left = 10, Top = yPos, Width = 230, Height = 30 };
            addLayerBtn.Click += AddLayer_Click;
            rightPanel.Controls.Add(addLayerBtn);
            yPos += 40;

            ListBox layersList = new ListBox() { Left = 10, Top = yPos, Width = 230, Height = 120, Name = "LayersList" };
            layersList.SelectedIndexChanged += LayersList_SelectedIndexChanged;
            rightPanel.Controls.Add(layersList);
            yPos += 130;

            // Музыка
            Button addMusicBtn = new Button() { Text = "🎵 Добавить музыку", Left = 10, Top = yPos, Width = 230, Height = 30 };
            addMusicBtn.Click += AddMusic_Click;
            rightPanel.Controls.Add(addMusicBtn);

            this.Controls.Add(rightPanel);
        }

        private void SelectColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                drawingTool.SetColor(colorDialog.Color);
            }
        }

        private void UpdatePlaybackSpeed(int fps)
        {
            playbackTimer.Interval = fps > 0 ? 1000 / fps : 100;
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (currentProject != null && currentProject.Frames.Count > 0)
            {
                isPlaying = true;
                currentFrameIndex = 0;
                playbackTimer.Start();
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            isPlaying = false;
            playbackTimer.Stop();
        }

        private void AddFrame_Click(object sender, EventArgs e)
        {
            if (currentProject != null)
            {
                var newFrame = new AnimationFrame();
                var layer = new AnimationFrame.Layer() { Name = "Layer 1", Bitmap = new Bitmap(800, 600) };
                newFrame.Layers.Add(layer);
                currentProject.Frames.Add(newFrame);
                currentFrameIndex = currentProject.Frames.Count - 1;
                RefreshFramesList();
                RefreshLayersList();
                RefreshCanvas();
            }
        }

        private void DeleteFrame_Click(object sender, EventArgs e)
        {
            if (currentProject != null && currentFrameIndex >= 0 && currentFrameIndex < currentProject.Frames.Count)
            {
                currentProject.Frames.RemoveAt(currentFrameIndex);
                if (currentFrameIndex >= currentProject.Frames.Count && currentFrameIndex > 0)
                    currentFrameIndex--;
                RefreshFramesList();
                RefreshLayersList();
                RefreshCanvas();
            }
        }

        private void AddLayer_Click(object sender, EventArgs e)
        {
            if (currentProject != null && currentFrameIndex >= 0 && currentFrameIndex < currentProject.Frames.Count)
            {
                var frame = currentProject.Frames[currentFrameIndex];
                var layer = new AnimationFrame.Layer() 
                { 
                    Name = $"Layer {frame.Layers.Count + 1}", 
                    Bitmap = new Bitmap(800, 600) 
                };
                frame.Layers.Add(layer);
                RefreshLayersList();
                RefreshCanvas();
            }
        }

        private void FramesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox framesList = this.Controls["RightPanel"]?.Controls["FramesList"] as ListBox;
            if (framesList != null && framesList.SelectedIndex >= 0)
            {
                currentFrameIndex = framesList.SelectedIndex;
                RefreshLayersList();
                RefreshCanvas();
            }
        }

        private void LayersList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox layersList = this.Controls["RightPanel"]?.Controls["LayersList"] as ListBox;
            if (layersList != null && layersList.SelectedIndex >= 0)
            {
                selectedLayerIndex = layersList.SelectedIndex;
                RefreshCanvas();
            }
        }

        private void RefreshFramesList()
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel && ctrl.Dock == DockStyle.Right)
                {
                    ListBox framesList = ctrl.Controls["FramesList"] as ListBox;
                    if (framesList != null)
                    {
                        framesList.Items.Clear();
                        for (int i = 0; i < currentProject.Frames.Count; i++)
                        {
                            framesList.Items.Add($"Кадр {i + 1}");
                        }
                        framesList.SelectedIndex = currentFrameIndex;
                    }
                }
            }
        }

        private void RefreshLayersList()
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel && ctrl.Dock == DockStyle.Right)
                {
                    ListBox layersList = ctrl.Controls["LayersList"] as ListBox;
                    if (layersList != null && currentProject != null && currentFrameIndex >= 0 && currentFrameIndex < currentProject.Frames.Count)
                    {
                        layersList.Items.Clear();
                        var frame = currentProject.Frames[currentFrameIndex];
                        for (int i = 0; i < frame.Layers.Count; i++)
                        {
                            layersList.Items.Add(frame.Layers[i].Name);
                        }
                        if (frame.Layers.Count > 0)
                            layersList.SelectedIndex = Math.Min(selectedLayerIndex, frame.Layers.Count - 1);
                    }
                }
            }
        }

        private void RefreshCanvas()
        {
            if (currentProject != null && currentFrameIndex >= 0 && currentFrameIndex < currentProject.Frames.Count)
            {
                var frame = currentProject.Frames[currentFrameIndex];
                canvasBitmap = frame.RenderToImage();
                
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Panel && ctrl.Dock == DockStyle.Fill)
                    {
                        PictureBox canvas = ctrl.Controls[0] as PictureBox;
                        if (canvas != null)
                        {
                            canvas.Image = canvasBitmap;
                        }
                    }
                }

                // Обновить информацию о кадре
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Panel && ctrl.Dock == DockStyle.Right)
                    {
                        Label frameLabel = ctrl.Controls["CurrentFrameLabel"] as Label;
                        if (frameLabel != null)
                        {
                            frameLabel.Text = $"Кадр: {currentFrameIndex + 1}/{currentProject.Frames.Count}";
                        }
                    }
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentProject != null && currentFrameIndex >= 0 && currentFrameIndex < currentProject.Frames.Count)
            {
                var frame = currentProject.Frames[currentFrameIndex];
                if (selectedLayerIndex >= 0 && selectedLayerIndex < frame.Layers.Count)
                {
                    var layer = frame.Layers[selectedLayerIndex];
                    drawingTool.StartDrawing(e.Location, layer.Bitmap);
                    RefreshCanvas();
                }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentProject != null && currentFrameIndex >= 0 && currentFrameIndex < currentProject.Frames.Count && e.Button == MouseButtons.Left)
            {
                var frame = currentProject.Frames[currentFrameIndex];
                if (selectedLayerIndex >= 0 && selectedLayerIndex < frame.Layers.Count)
                {
                    var layer = frame.Layers[selectedLayerIndex];
                    drawingTool.Draw(e.Location, layer.Bitmap);
                    RefreshCanvas();
                }
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            drawingTool.EndDrawing();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            if (canvasBitmap != null)
            {
                e.Graphics.DrawImage(canvasBitmap, 0, 0);
            }
        }

        private void NewProject_Click(object sender, EventArgs e)
        {
            ProjectDialog dialog = new ProjectDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                currentProject = new AnimationProject() 
                { 
                    Name = dialog.ProjectName,
                    Width = dialog.Width,
                    Height = dialog.Height
                };
                var firstFrame = new AnimationFrame();
                var layer = new AnimationFrame.Layer() { Name = "Layer 1", Bitmap = new Bitmap(dialog.Width, dialog.Height) };
                firstFrame.Layers.Add(layer);
                currentProject.Frames.Add(firstFrame);
                currentFrameIndex = 0;
                RefreshFramesList();
                RefreshLayersList();
                RefreshCanvas();
                this.Text = $"ГраффитАнимация - {currentProject.Name}";
            }
        }

        private void SaveProject_Click(object sender, EventArgs e)
        {
            if (currentProject != null)
            {
                SaveFileDialog saveDialog = new SaveFileDialog()
                {
                    Filter = "GraffitAnimation Files (*.grfan)|*.grfan",
                    FileName = currentProject.Name
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    currentProject.SaveToFile(saveDialog.FileName);
                    MessageBox.Show("Проект сохранен!");
                }
            }
        }

        private void OpenProject_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Filter = "GraffitAnimation Files (*.grfan)|*.grfan"
            };
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                currentProject = AnimationProject.LoadFromFile(openDialog.FileName);
                currentFrameIndex = 0;
                RefreshFramesList();
                RefreshLayersList();
                RefreshCanvas();
                this.Text = $"ГраффитАнимация - {currentProject.Name}";
            }
        }

        private void ExportGIF_Click(object sender, EventArgs e)
        {
            if (currentProject != null)
            {
                SaveFileDialog saveDialog = new SaveFileDialog()
                {
                    Filter = "GIF Files (*.gif)|*.gif",
                    FileName = currentProject.Name + ".gif"
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    int fps = 10;
                    foreach (Control ctrl in this.Controls)
                    {
                        if (ctrl is Panel && ctrl.Dock == DockStyle.Right)
                        {
                            NumericUpDown fpsSpinner = ctrl.Controls.OfType<NumericUpDown>().FirstOrDefault();
                            if (fpsSpinner != null)
                                fps = (int)fpsSpinner.Value;
                        }
                    }
                    exportService.ExportToGIF(currentProject, saveDialog.FileName, fps);
                    MessageBox.Show("Анимация экспортирована в GIF!");
                }
            }
        }

        private void ExportVideo_Click(object sender, EventArgs e)
        {
            if (currentProject != null)
            {
                SaveFileDialog saveDialog = new SaveFileDialog()
                {
                    Filter = "Video Files (*.mp4)|*.mp4",
                    FileName = currentProject.Name + ".mp4"
                };
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    int fps = 10;
                    foreach (Control ctrl in this.Controls)
                    {
                        if (ctrl is Panel && ctrl.Dock == DockStyle.Right)
                        {
                            NumericUpDown fpsSpinner = ctrl.Controls.OfType<NumericUpDown>().FirstOrDefault();
                            if (fpsSpinner != null)
                                fps = (int)fpsSpinner.Value;
                        }
                    }
                    exportService.ExportToVideo(currentProject, saveDialog.FileName, fps, currentProject.MusicFile);
                    MessageBox.Show("Анимация экспортирована в видео!");
                }
            }
        }

        private void AddMusic_Click(object sender, EventArgs e)
        {
            if (currentProject != null)
            {
                OpenFileDialog openDialog = new OpenFileDialog()
                {
                    Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav"
                };
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    currentProject.MusicFile = openDialog.FileName;
                    MessageBox.Show("Музыка добавлена к проекту!");
                }
            }
        }
    }
}