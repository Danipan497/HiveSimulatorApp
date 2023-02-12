﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HiveSimulator
{
    public partial class FieldForm : Form
    {
        public FieldForm()
        {
            InitializeComponent();
            BackgroundImage = Renderer.ResizeImage(Properties.Resources.Hive__inside_,ClientRectangle.Width, ClientRectangle.Height);
        }

        public Renderer renderer;

        private void FieldForm_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show(e.Location.ToString());
        }

        private void FieldForm_Load(object sender, EventArgs e)
        {

        }

        private void FieldForm_Paint(object sender, PaintEventArgs e)
        {
            renderer.PaintField(e.Graphics);
        }
    }
}
