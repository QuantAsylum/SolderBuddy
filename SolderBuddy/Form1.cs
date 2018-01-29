using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SolderBuddy
{
    public partial class Form1 : Form
    {
        float ViewWidthMM = 10;
        GerberViewer GvTop = new GerberViewer();


        /// we have a list of lists. These can best be thought of as rows and columns. The columns are ragged...that
        /// is, col0 might have 10 parts, and col1 might have 2. The rows hold different parts (eg 0.1, 10K, etc). The columns
        /// hold different refdes of the same part 
        List<List<OneLineBOM.Part>> PartList;

        /// <summary>
        ///  RowIndex iterates among the different values (eg 10K, 0.1uF)
        /// </summary>
        int RowIndex;

        /// <summary>
        /// ColIndex iterates among the parts of the same value (eg, C23, C65, etc, all 0.1uF)
        /// </summary>
        int ColIndex;

        string GerberSilkPath;
        string PlacementsPath;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "...";
            label5.Text = "...";
            textBox1.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            textBox2.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Form1_Resize(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBox1.Text) == false)
                return;

            if (File.Exists(textBox2.Text) == false)
                return;

            GerberSilkPath = textBox1.Text;
            PlacementsPath = textBox2.Text;

            // Load the silk layer
            GvTop.LoadLayer(GerberSilkPath);

            // Load the bom
            OneLineBOM bom = new OneLineBOM();
            // Return a list of lists
            PartList = bom.LoadFile(PlacementsPath, "top");

            label2.Text = string.Format("Total Parts: {0} Total Placements: {1}", PartList.Count, CountTotalPlacements());

            RowIndex = 0;
            ColIndex = 0;
            HighlightPart();
        }

        private void HighlightPart()
        {
            var part = PartList[RowIndex][ColIndex];

            Bitmap b1 = GvTop.Draw(Color.DarkGray, true, new Bitmap(pictureBox3.Width, pictureBox3.Height), part.Location, ViewWidthMM);
            b1.RotateFlip(RotateFlipType.RotateNoneFlipY);
            pictureBox3.Image = b1;

            Bitmap b2 = GvTop.Draw(Color.DarkGray, true, new Bitmap(pictureBox2.Width, pictureBox2.Height), part.Location, ViewWidthMM*3);
            b2.RotateFlip(RotateFlipType.RotateNoneFlipY);
            pictureBox2.Image = b2;

            Bitmap b3 = GvTop.Draw(Color.DarkGray, true, new Bitmap(pictureBox1.Width, pictureBox1.Height), part.Location, ViewWidthMM * 25);
            b3.RotateFlip(RotateFlipType.RotateNoneFlipY);
            pictureBox1.Image = b3;

            label1.Text = string.Format("{0} {1} [{2} of {3}] [{4} of {5}]", part.RefDes, part.Value, ColIndex+1, PartList[RowIndex].Count, RowIndex + 1, PartList.Count);
        }

        int CountTotalPlacements()
        {
            int count = 0;

            for (int i = 0; i < PartList.Count; i++)
            {
                for (int j = 0; j < PartList[i].Count; j++)
                {
                    ++count;
                }
            }

            return count;
        }

        // Next part
        private void button2_Click(object sender, EventArgs e)
        {
            if (ColIndex == PartList[RowIndex].Count - 1)
            {
                // Last part on this row. Move to the next row
                ColIndex = 0;
                
                if (RowIndex == PartList.Count -1)
                {
                    // Last row. Wrap to first
                    RowIndex = 0;
                }
                else
                {
                    ++RowIndex;
                }
            }
            else
            {
                ++ColIndex;
            }

            string nextPart;
            try
            {
                nextPart = PartList[RowIndex + 1][0].Value;
                label5.Text = "Next: " + nextPart;
            }
            catch(Exception ex)
            {
                label5.Text = "???";
            }

            HighlightPart();
            
        }

        // Previous part
        private void button3_Click(object sender, EventArgs e)
        {
            if (ColIndex == 0)
            {
                if (RowIndex == 0)
                {
                    // Need to wrap
                    RowIndex = PartList.Count - 1;
                }
                else
                {
                    --RowIndex;
                }
            }
            else
            {
                --ColIndex;
            }

            HighlightPart();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        // Load gerber silk file
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        // Placements
        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (ofd.ShowDialog() == DialogResult.OK) 
            {
                textBox2.Text = ofd.FileName;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Location = new Point(0, 0);
            pictureBox2.Location = new Point(this.Width * 1/3, 0);
            pictureBox3.Location = new Point(this.Width * 2/3, 0);

            pictureBox1.Width = panel3.Width / 3;
            pictureBox1.Height = panel3.Height;

            pictureBox2.Width = panel3.Width / 3;
            pictureBox2.Height = panel3.Height;

            pictureBox3.Width = panel3.Width / 3;
            pictureBox3.Height = panel3.Height;
        }
    }
}
