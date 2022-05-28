using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        //List<RGBPixel> ListOfDC;
        //List<NodeOfcolors> ListOfMST;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
           
            txtWidth.Text =ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            Stopwatch time = new Stopwatch();
            time.Start();

            int NumOfK = Convert.ToInt32(txtNumOfK.Text.ToString());
            txtDiscolor.Text = ImageOperations.FindDistinctColorsANDList(ImageMatrix).ToString();
            ImageOperations.MST();
            SumOfTree.Text = ImageOperations.SumOfCost.ToString("0.00");
            List<HashSet<int>> l = ImageOperations.FindTheClustersForDistictColor(ImageOperations.ListOfMST, NumOfK);
            Dictionary<int, int> d = ImageOperations.FindTherepresentiveColorForeachcluster(l, ImageOperations.ListOfMST.Count);
            // RGBPixel[,] i =ImageOperations.QuantizationTheImage(ImageMatrix,d);
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, NumOfK, ImageOperations.SumOfCost);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);

            time.Stop();
            Min.Text = time.Elapsed.Minutes.ToString();
            Sec.Text = time.Elapsed.Seconds.ToString();
            MilSec.Text = time.Elapsed.Milliseconds.ToString();
            
            //double sigma = double.Parse(txtGaussSigma.Text);
            //int maskSize = (int)nudMaskSize.Value;
            //ImageOperations.QuantizationTheImage(Matrixforimagepath, colorandreprsentivecolor);
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

        private void txtDiscolor_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtNumOfK_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}