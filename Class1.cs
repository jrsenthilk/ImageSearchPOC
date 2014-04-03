using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Structure;
using ExifLib;
using Emgu.CV.GPU;
using System.IO;

namespace FaceDetection
{
    class FaceComputeAlgo
    {
        private List<Bitmap> imageList;
        private List<Bitmap> colorImageList;
        private List<Bitmap> histoImageList;

        public FaceComputeAlgo()
        {
            imageList = new List<Bitmap>();
            colorImageList = new List<Bitmap>();
            histoImageList = new List<Bitmap>();
        }

        public List<Bitmap> dominantColorSearch(List<Bitmap> images, String color)
        {
            //if(colorImageList.)
            colorImageList.Clear();

            for (int i = 0; i < images.Count; i++)
            {
                Image<Bgr, byte> img = new Image<Bgr, byte>(images[i]);
                int blue=0;
                int red=0;
                int green=0;
                for (int k = 0; k < img.Height; k++)
                {
                    for (int m = 0; m < img.Width; m++)
                    {
                        if (img.Data[k, m, 0] >= img.Data[k, m, 1])
                        {
                            if (img.Data[k, m, 0] >= img.Data[k, m, 2])
                            {
                                blue++;
                            }
                            else //if (img.Data[k, m, 2] >= img.Data[k, m, 1])
                            {
                                red++;
                            }
                        }
                        else if (img.Data[k, m, 1] >= img.Data[k, m, 2])
                        {
                            green++;
                        }
                        else //if (img.Data[k, m, 2] >= img.Data[k, m, 0])
                        {
                            red++;
                        }
                    }
                }

                Image<Bgr, byte> resizedImage = img.Resize(300, 300, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

                int diffBlueGreen = Math.Abs(blue - green);
                int diffBlueRed = Math.Abs(blue - red);
                int diffGreenRed = Math.Abs(green - red);

                if (blue > red)
                {
                    if (blue > green)
                    {
                        if (color == "Blue" | (((color == "Green") && (diffBlueGreen < 10000)) || ((color == "Red") && (diffBlueRed < 10000))))
                        {
                            colorImageList.Add(resizedImage.ToBitmap());
                        }
                    }
                    else if (color == "Green" | (((color == "Blue" ) && (diffBlueGreen < 10000)) || ((color == "Red") && (diffGreenRed < 10000))))
                    {
                        colorImageList.Add(resizedImage.ToBitmap());
                    }
                }
                else
                {
                    if (red > green )
                    {
                        if (color == "Red" | (((color == "Blue" ) && (diffBlueRed < 10000)) || ((color == "Green") && (diffGreenRed < 10000))))
                        {
                            colorImageList.Add(resizedImage.ToBitmap());
                        }
                    }
                    else if (color == "Green" | (((color == "Blue") && (diffBlueGreen < 10000)) || ((color == "Red") && (diffGreenRed < 10000))))
                    {
                        colorImageList.Add(resizedImage.ToBitmap());
                    }


                }
            }

            return colorImageList;
        }

        public List<Bitmap> histogramSearch(List<Bitmap> images, Bitmap ori_image)
        {
            float[] BlueHist;
            float[] GreenHist;
            float[] RedHist;

            float[] base_BlueHist;
            float[] base_GreenHist;
            float[] base_RedHist;

            histoImageList.Clear();

            Image<Bgr, byte> base_img = new Image<Bgr, byte>(ori_image);
            DenseHistogram blue_Histo = new DenseHistogram(255, new RangeF(0, 255));
            DenseHistogram green_Histo = new DenseHistogram(255, new RangeF(0, 255));
            DenseHistogram red_Histo = new DenseHistogram(255, new RangeF(0, 255));

            Image<Gray, Byte> base_img2Blue = base_img[0];
            Image<Gray, Byte> base_img2Green = base_img[1];
            Image<Gray, Byte> base_img2Red = base_img[2];

           

            blue_Histo.Calculate(new Image<Gray, Byte>[] { base_img2Blue }, true, null);
          

            green_Histo.Calculate(new Image<Gray, Byte>[] { base_img2Green }, true, null);
           

            red_Histo.Calculate(new Image<Gray, Byte>[] { base_img2Red }, true, null);
          


            for (int i = 0; i < images.Count; i++)
            {
                Image<Bgr, byte> img = new Image<Bgr, byte>(images[i]);
                
                DenseHistogram blue_CompareHisto = new DenseHistogram(255, new RangeF(0, 255));
                DenseHistogram green_CompareHisto = new DenseHistogram(255, new RangeF(0, 255));
                DenseHistogram red_CompareHisto = new DenseHistogram(255, new RangeF(0, 255));

                Image<Gray, Byte> img2Blue = img[0];
                Image<Gray, Byte> img2Green = img[1];
                Image<Gray, Byte> img2Red = img[2];

                blue_CompareHisto.Calculate(new Image<Gray, Byte>[] { img2Blue }, true, null);           
               

              
                BlueHist = new float[256];
                blue_CompareHisto.MatND.ManagedArray.CopyTo(BlueHist, 0);

                green_CompareHisto.Calculate(new Image<Gray, Byte>[] { img2Green }, true, null);
              

                GreenHist = new float[256];
                green_CompareHisto.MatND.ManagedArray.CopyTo(GreenHist, 0);

                red_CompareHisto.Calculate(new Image<Gray, Byte>[] { img2Red }, true, null);

                RedHist = new float[256];
                red_CompareHisto.MatND.ManagedArray.CopyTo(RedHist, 0);

                double cBlue = CvInvoke.cvCompareHist(blue_Histo, blue_CompareHisto, Emgu.CV.CvEnum.HISTOGRAM_COMP_METHOD.CV_COMP_CORREL);
                double cGreen = CvInvoke.cvCompareHist(green_Histo, green_CompareHisto, Emgu.CV.CvEnum.HISTOGRAM_COMP_METHOD.CV_COMP_CORREL);
                double cRed = CvInvoke.cvCompareHist(red_Histo, red_CompareHisto, Emgu.CV.CvEnum.HISTOGRAM_COMP_METHOD.CV_COMP_CORREL);

                double cBlue_ratio = (cBlue / 1.00) * 100;
                double cRed_ratio = (cRed / 1.00) * 100;
                double cGreen_ratio = (cGreen / 1.00) * 100;

                if (cBlue_ratio > 55 || cRed_ratio > 55 || cGreen_ratio > 55)
                {
                    Image<Bgr, byte> resizedImage = img.Resize(300, 300, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                    histoImageList.Add(resizedImage.ToBitmap());
                }

            }

            Image<Bgr, byte> base_resizedImage = base_img.Resize(300, 300, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            histoImageList.Add(base_resizedImage.ToBitmap());
            return histoImageList;
        }
        

        public List<Bitmap> Search(List<Bitmap> images)
        {
            imageList.Clear();
            for (int i = 0; i < images.Count; i++)
            {
                Image<Bgr, Byte> image = new Image<Bgr, byte>(images[i]);
                long detectionTime;
                List<Rectangle> faces = new List<Rectangle>();
                List<Rectangle> eyes = new List<Rectangle>();

                DetectFace.Detect(image, "haarcascade_frontalface_default.xml", "haarcascade_eye.xml", faces, eyes, out detectionTime);

                if (faces.Count > 0)
                {
                    //foreach (Rectangle face in faces)
                    //    image.Draw(face, new Bgr(Color.Red), 2);

                   // foreach (Rectangle eye in eyes)
                   //     image.Draw(eye, new Bgr(Color.Blue), 2);

                    Image<Bgr, byte> resizedImage = image.Resize(300, 300, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

                    imageList.Add(resizedImage.ToBitmap());
                }
                //pictureboxview.Image = resizedImage.ToBitmap();
            }
             //Read the files as an 8-bit Bgr image
            //Graphics g = Graphics.FromImage(image);
            return imageList;
            

            

            //if (faces.Count > 0)
            //{
            //    using (ExifReader reader = new ExifReader(@"C:\test\test4face.jpg"))
            //    {
            //        // Extract the tag data using the ExifTags enumeration
            //        DateTime datePictureTaken;

            //        if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized,
            //                                        out datePictureTaken))
            //        {
            //            // Do whatever is required with the extracted information
            //            String imageCommnets = string.Format("The picture was taken on {0}", datePictureTaken);
            //            MCvFont f = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_PLAIN, 3.0, 3.0);
            //            image.Draw(imageCommnets, ref f, new Point(150, 150), new Emgu.CV.Structure.Bgr(255.0, 0, 0));
            //        }


            //    }
            //}

            //display the image 
            //Emgu.CV.UI.ImageViewer.Show(image, String.Format(
            //"Completed face and eye detection using {0} in {1} milliseconds",
            //GpuInvoke.HasCuda ? "GPU" : "CPU",
            //detectionTime));
            
        }

    }
}
