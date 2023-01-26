using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.IO.Ports;//we are working with serial ports so we added this library

namespace TestCam
{
    public partial class Form1 : Form
    {
        private Capture capture; //Capture is the class name and capture is pointer to the camera(object name of the camera)(hoca sorabilir)
        private Image<Bgr, Byte> IMG; //Image is class type, IMG is containing the bgr image in bytes,
        private Image<Gray, Byte> GrayImg;//image list that includes gray image
        private Image<Gray, Byte> BWImg; //to create a black&white image
        

        private double myScaleX,myScaleY; //640=width of the image
        private int Xpx, Ypx, N; //center point in pixels
        private double Xcm, Ycm; //Px and Py;
        public double Zcm, Zcm1;
        public double d1 = 7.0;
        private double Th1, Th2;

        static SerialPort _serialPort; //object identifier

        

        public byte[] Buff = new byte[2];//buffer that contains the values to be used(from c# to arduino)
        //we use two values so its byte[2]
        //we have 2 variables(th1 and th2)

        public Form1() //this is the constructor, name of the class, no return datatype
        {
            InitializeComponent();

            _serialPort = new SerialPort();
            _serialPort.PortName = "COM3";//Set your board COM
            _serialPort.BaudRate = 9600; //means open the serial port
        }

        private void processFrame(object sender, EventArgs e)//the most important function is processFrame in this program
        {

            
            if (capture == null)//very important to handel exception, if its null, it is not connected to the camera yet
            {
                try //so we will try to connect it to the camera
                {
                    capture = new Capture(1); //now we created a new object and connected it to the camera
                    //parantezin içi boş veya 0 yazıyorsa default kamera bağlı demektir
                    //ikinci kamera için parantezin içine 1 yazmamız gerekir
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }

            IMG = capture.QueryFrame(); //QueryFrame means get an image from the camera, each secon it will take 15 pictures, when you run the program
            //and reach this line it will put -the last image that it gets* inside the object 'IMG'
            GrayImg = IMG.Convert<Gray, Byte>(); //Convert image from colored image to the gray image
            BWImg = GrayImg.ThresholdBinaryInv(new Gray(35), new Gray(255)); //part of the thing that i will do is changing first value to find the object

            Xpx = 0;//this helps how to specify the center of the object
            Ypx = 0;
            N = 0; //N is number of pixels
            for (int i = 0; i < BWImg.Width; i++) // for scaning the image
                for (int j = 0; j < BWImg.Height; j++)
                {
                    if (BWImg[j, i].Intensity > 128)//if intensity of the image is greater than 128
                    {
                        N++;
                        Xpx += i; //add its location to the px and py in pixels
                        Ypx += j;
                    }
                }

            if (N > 0)//if N>0 there is an object in the image
            {
                    Xpx = Xpx / N;
                    Ypx = Ypx / N;
                    textBox2.Text = Xpx.ToString();
                    textBox3.Text = Ypx.ToString();
                    textBox4.Text = N.ToString();
                    
                    
                    myScaleX = 37.0 / BWImg.Width;
                    myScaleY = 35.0 / BWImg.Height;

                    Xcm = (Xpx - 320) * myScaleX;
                    Ycm = (240 - Ypx) * myScaleX;

                    Console.WriteLine($"Xcm:{Xcm} Ycm {Ycm} {Xpx} {Ypx}");
                    //invers kinematics model
                    //Zcm = Ycm;
                    //Ycm1 = -Xcm;
                    //Xcm1 = -17;
                    double Px = 100;
                    double Py = -Xcm;
                    double Pz = Ycm + d1;

                    // Inverse K. model
                     Th1 = Math.Atan(Py / Px);
                     Th2 = Math.Atan((Math.Sin(Th1) * Pz) / Py);

                    Th1 = Th1 * (180 / Math.PI) + 90;

                    Th2 = -(Th2 * (180 / Math.PI)) + 70;
                    Console.Error.WriteLine($"Açılar {Th1} {Th2}");
                    textBox1.Text = Th1.ToString();
                    textBox5.Text = Th2.ToString(); 
                    
                
                    Buff[0] = (byte)Th1; //Th1
                    Buff[1] = (byte)Th2; //Th2
                    _serialPort.Write(Buff, 0, 2);
                

            }

            else
            {
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
            }

            try
            {

                imageBox1.Image = IMG;//color image
                imageBox2.Image = GrayImg; //gray image
                imageBox3.Image = BWImg;
                


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {

            _serialPort.Open();
            Application.Idle += processFrame; //means run this function in the background
            timer1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            _serialPort.Close();
            Application.Idle -= processFrame; //means stop running this function
            timer1.Enabled = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
          

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            processFrame(sender, e);
        }

    }
}