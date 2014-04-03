using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FaceDetection;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FaceDetection
{
    public partial class Form1 : Form
    {
        private FaceComputeAlgo newcontent;
        private FolderBrowserDialog folderBrowserDialog1;
        private OpenFileDialog openFolderDialog1;
        //private OpenFileDialog openFileDialog1 ;
        private bool fileOpened = false;
         
        public Form1()
        {
            InitializeComponent();
            newcontent = new FaceComputeAlgo();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
                MessageBox.Show("Please select images path");
            else if (!checkBox1.Checked && !radioButton1.Checked && !radioButton2.Checked && !radioButton3.Checked)
            {
                MessageBox.Show("Please select at least one search criteria");
            }
            else
            {
                String path = textBox1.Text;
                DirectoryInfo Folder;

                FileInfo[] Images;

                Folder = new DirectoryInfo(path);
                Images = Folder.GetFiles();
                
                //Image testImage = new Image(Images[0].FullName);
                //Image<Bgr, Byte> image = new Image<Bgr, byte>(Images[i].FullName);
                List<Bitmap> imagetest =  new List<Bitmap>();
                for (int h = 0; h < Images.Length; h++)
                {
                    Image<Bgr, Byte> imagesList = new Image<Bgr, Byte>(Images[h].FullName);
                    imagetest.Add(imagesList.ToBitmap());
                }

                List<Bitmap> faceimageList = new List<Bitmap>();
                if(checkBox1.Checked && !radioButton1.Checked && !radioButton2.Checked && !radioButton3.Checked)
                    faceimageList = newcontent.Search(imagetest);
                else if (checkBox1.Checked && (radioButton1.Checked || radioButton2.Checked || radioButton3.Checked))
                {
                    
                    faceimageList = newcontent.Search(imagetest);
                    if (radioButton1.Checked)
                        faceimageList = newcontent.dominantColorSearch(faceimageList, "Blue");
                    else if (radioButton2.Checked)
                        faceimageList = newcontent.dominantColorSearch(faceimageList, "Green");
                    else if (radioButton3.Checked)
                        faceimageList = newcontent.dominantColorSearch(faceimageList, "Red");
                }
                else if(!checkBox1.Checked)
                {
                    if(radioButton1.Checked)
                        faceimageList = newcontent.dominantColorSearch(imagetest, "Blue");
                    if(radioButton2.Checked)
                        faceimageList = newcontent.dominantColorSearch(imagetest, "Green");
                    if(radioButton3.Checked)
                        faceimageList = newcontent.dominantColorSearch(imagetest, "Red");

                }

               // List<Bitmap> faceColorImageList = newcontent.dominantColorSearch(faceimageList);

                if (faceimageList.Count > 0)
                {
                    flowLayoutPanel1.Controls.Clear();

                    PictureBox[] Shapes = new PictureBox[faceimageList.Count];

                    int xLocation = 33;
                    int yLocation = 100;

                    for (int i = 0; i < faceimageList.Count; i++)
                    {

                        Shapes[i] = new PictureBox();

                        Shapes[i].Name = "ItemNum_" + i.ToString();

                        Shapes[i].Location = new Point(xLocation + (i * 300), yLocation + ((i / 3) * 300));

                        Shapes[i].Size = new Size(300, 300);
                        Shapes[i].BackColor = Color.Black;

                        Shapes[i].Image = (Bitmap)(faceimageList[i]);

                        Shapes[i].Visible = true;

                        flowLayoutPanel1.Controls.Add(Shapes[i]);
                        //this.Controls.Add(Shapes[i]);

                    }
                }
                else
                {
                    MessageBox.Show("No images found based on your search criteria");
                }

            }
        }

        private void directory_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.folderBrowserDialog1.Description ="Select the image directory ";
            this.folderBrowserDialog1.ShowNewFolderButton = false;

            //openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            //this.openFileDialog1.DefaultExt = "rtf";
            //this.openFileDialog1.Filter = "rtf files (*.rtf)|*.rtf";


            // Default to the My Documents folder. 
            //this.folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Personal;
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
                //if (!fileOpened)
                //{
                //    // No file is opened, bring up openFileDialog in selected path.
                //    openFileDialog1.InitialDirectory = folderName;
                //    openFileDialog1.FileName = null;
                //    openMenuItem.PerformClick();
                //}
            } 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String path = textBox1.Text;
            String baseimage_path = textBox2.Text;

            FileInfo[] Images;
            DirectoryInfo Folder;

            Folder = new DirectoryInfo(path);
            Images = Folder.GetFiles();

            List<Bitmap> imagetest = new List<Bitmap>();
            for (int h = 0; h < Images.Length; h++)
            {
                Image<Bgr, Byte> imagesList = new Image<Bgr, Byte>(Images[h].FullName);
                imagetest.Add(imagesList.ToBitmap());
            }

           // var file = Directory.GetFiles(baseimage_path);
            Bitmap base_image = new Bitmap(baseimage_path);
            List<Bitmap> histoimageList = new List<Bitmap>();

            histoimageList = newcontent.histogramSearch(imagetest, base_image);

            if (histoimageList.Count > 1)
            {
                flowLayoutPanel1.Controls.Clear();

                PictureBox[] Shapes = new PictureBox[histoimageList.Count];

                int xLocation = 33;
                int yLocation = 100;

                for (int i = 0; i < histoimageList.Count; i++)
                {

                    Shapes[i] = new PictureBox();

                    Shapes[i].Name = "ItemNum_" + i.ToString();

                    Shapes[i].Location = new Point(xLocation + (i * 300), yLocation + ((i / 3) * 300));

                    Shapes[i].Size = new Size(300, 300);
                    Shapes[i].BackColor = Color.Black;

                    Shapes[i].Image = (Bitmap)(histoimageList[i]);

                    Shapes[i].Visible = true;

                    flowLayoutPanel1.Controls.Add(Shapes[i]);
                    //this.Controls.Add(Shapes[i]);

                }
            }
            else
            {
                MessageBox.Show("No matching images found");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.openFolderDialog1 = new OpenFileDialog();
            DialogResult result = openFolderDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox2.Text = openFolderDialog1.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
        }
    }
}
