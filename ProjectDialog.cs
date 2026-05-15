using System;
using System.Drawing;
using System.Windows.Forms;

namespace GraffitAnimation
{
    public partial class ProjectDialog : Form
    {
        public string ProjectName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ProjectDialog()
        {
            InitializeProjectDialog();
        }

        private void InitializeProjectDialog()
        {
            this.Text = "Новый проект";
            this.Width = 350;
            this.Height = 250;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 20;

            Label nameLabel = new Label() { Text = "Название проекта:", Left = 20, Top = yPos, Width = 300 };
            this.Controls.Add(nameLabel);
            yPos += 25;

            TextBox nameBox = new TextBox() { Left = 20, Top = yPos, Width = 300, Text = "Новая анимация" };
            this.Controls.Add(nameBox);
            yPos += 35;

            Label widthLabel = new Label() { Text = "Ширина:", Left = 20, Top = yPos, Width = 140 };
            this.Controls.Add(widthLabel);
            Label heightLabel = new Label() { Text = "Высота:", Left = 180, Top = yPos, Width = 140 };
            this.Controls.Add(heightLabel);
            yPos += 25;

            NumericUpDown widthBox = new NumericUpDown() { Left = 20, Top = yPos, Width = 140, Minimum = 100, Maximum = 4000 };
            widthBox.Value = 800;
            this.Controls.Add(widthBox);
            NumericUpDown heightBox = new NumericUpDown() { Left = 180, Top = yPos, Width = 140, Minimum = 100, Maximum = 4000 };
            heightBox.Value = 600;
            this.Controls.Add(heightBox);
            yPos += 40;

            Button okBtn = new Button() { Text = "OK", Left = 150, Top = yPos, Width = 80, Height = 30, DialogResult = DialogResult.OK };
            okBtn.Click += (s, e) => { ProjectName = nameBox.Text; Width = (int)widthBox.Value; Height = (int)heightBox.Value; this.Close(); };
            this.Controls.Add(okBtn);

            Button cancelBtn = new Button() { Text = "Отмена", Left = 240, Top = yPos, Width = 80, Height = 30, DialogResult = DialogResult.Cancel };
            this.Controls.Add(cancelBtn);
        }
    }
}