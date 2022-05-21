using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.ComponentModel;
using System.Data;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            Stopwatch time = new Stopwatch();
            time.Start();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            txtDiscolor.Text = ImageOperations.FindDistinctColors(ImageMatrix).ToString();
           // ImageOperations.ListDistinctColor(ImageMatrix);
            List<int> adj = ImageOperations.MinimumSpanning();
            time.Stop();
            Min.Text = time.Elapsed.Minutes.ToString();
            Sec.Text = time.Elapsed.Seconds.ToString();
            MilSec.Text = time.Elapsed.Milliseconds.ToString();
           // adj = ImageOperations.ListDistinctColor(ImageMatrix);
           // SumOfTree.Text = ImageOperations.totalcost(adj).ToString();
           // txtGaussSigma.Text=ImageOperations.CalculateElcideanDistance(V1,V2).ToString();

        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);

            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void txtWidth_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void txtHeight_TextChanged(object sender, EventArgs e)
        {

        }

        private void SumOfTree_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void Min_TextChanged(object sender, EventArgs e)
        {

        }
    }
}