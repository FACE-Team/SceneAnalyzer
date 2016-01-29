using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;


//using ControllersLibrary;
using Microsoft.Kinect;

namespace SceneAnalyzer
{
    public class Kinect
    {
        private KinectSensor Sensor;
        public KinectSensor sensor 
        {
            get { return Sensor; }
            set { Sensor = value; }
        }
        //for Saliency
        private int PixelFeed;
        public int pixelFeed
        {
            get { return PixelFeed; }
            set { PixelFeed = value; }
        }
     
        private WriteableBitmap ColorImageBitmap;
        public WriteableBitmap colorImageBitmap 
        {
            get { return ColorImageBitmap; }
            set { ColorImageBitmap = value; }
        }

        private System.Drawing.Bitmap ColorBitmap;
        public System.Drawing.Bitmap colorBitmap
        {
            get { return ColorBitmap; }
            set { ColorBitmap = value; }
        }
        private Int32Rect ColorImageBitmapRect;
        public Int32Rect colorImageBitmapRect
        {
            get { return ColorImageBitmapRect; }
            set { ColorImageBitmapRect = value; }
        }

        private int ColorImageStride;
        public int colorImageStride 
        {
            get { return ColorImageStride; }
            set { ColorImageStride = value; }
        }
        private ColorImageFrame ColorFrameout;
        public ColorImageFrame colorFrameout
        {
            get { return ColorFrameout; }
            set { ColorFrameout = value; }
        }

        private System.Windows.Controls.Image Video;
        public System.Windows.Controls.Image video
        {
            get { return Video; }
            set { Video = value; }
        }
       
        private EventWaitHandle SoundReady = null;
        public EventWaitHandle soundReady
        {
            get { return SoundReady; }
            set { SoundReady = value; }
        }
        private EventWaitHandle subjectReady = null;

        private bool RGBMode = true; // RGB or black/white video display
        public bool rgbMode 
        {
            get { return RGBMode; }
            set { RGBMode = value; }
        }
        private EventWaitHandle FrameReady = null; // An event for synchronizing threads
        public EventWaitHandle frameReady 
        {
            get { return FrameReady; }
            set { FrameReady = value; }
        }

        private CoordinateMapper Mapper;
        public CoordinateMapper mapper
        {
            get { return Mapper; }
            set { Mapper = value; }
        }
        private KinectAudioSource AudioSource = null;
        public KinectAudioSource audioSource
        {
            get { return AudioSource; }
            set { AudioSource = value; }
        }

      

        public void InitKinect()
        {
            foreach (KinectSensor kinectSensor in KinectSensor.KinectSensors)
            {
                if (kinectSensor.Status == KinectStatus.Connected)
                {
                    sensor = kinectSensor;
                   
                    break;
                }
            }

            if (sensor != null)
            {


                ColorImageStream colorStream;

                if (RGBMode)
                {
                    sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    colorStream = sensor.ColorStream;
                    pixelFeed = 4; // Because the image is RGBA32 we must add 4 to the leftTop pointer to access the next pixel in the same line.
                    colorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                }
                else
                {
                    sensor.ColorStream.Enable(ColorImageFormat.RawYuvResolution640x480Fps15);
                    colorStream = sensor.ColorStream;
                    pixelFeed = 2; // Because the image is interleaved YUV, we must add 2 to the leftTop pointer to access the next pixel in the same line.
                    colorImageBitmap = new WriteableBitmap(colorStream.FrameWidth, colorStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                }

                sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                Video.Source = colorImageBitmap;
                colorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth, colorStream.FrameHeight);
                colorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                frameReady = new EventWaitHandle(false, EventResetMode.ManualReset);

                mapper = new CoordinateMapper(sensor);
                sensor.SkeletonStream.Enable();
            }
        }

       
        public Polyline CreateFigure(Skeleton skeleton, Brush brush, IEnumerable<JointType> joints, Canvas Canv)
        {
            var figure = new Polyline { StrokeThickness = 5, Stroke = brush };
            foreach (var t in joints)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[t], Canv));
            }

            return figure;
        }

      
        //Canvas_Skeleton
        public System.Windows.Point GetPassivePoint(Skeleton sk, Canvas Canv)
        {
            var point = sensor.MapSkeletonPointToColor(sk.Position, sensor.ColorStream.Format);
            point.X *= (int)Canv.ActualWidth / sensor.ColorStream.FrameWidth;
            point.Y *= (int)Canv.ActualHeight / sensor.ColorStream.FrameHeight;

            return new System.Windows.Point(point.X, point.Y);
        }

        public System.Windows.Point GetJointPoint(Joint joint, Canvas Canv)
        {
            //Because depth image data and color image data come from separate sensors, pixels in the two images may not always line 
            //up exactly. The two sensors may have different fields of
            //view, or may not be aimed precisely in the same direction. This means that a point near the edge of the depth image may 
            //correspond to a pixel just beyond the edge of the color image, or vice versa.
            DepthImagePoint point = mapper.MapSkeletonPointToDepthPoint(joint.Position, sensor.DepthStream.Format); 
            point.X *= (int)Canv.ActualWidth / sensor.DepthStream.FrameWidth;
            point.Y *= (int)Canv.ActualHeight / sensor.DepthStream.FrameHeight;

            return new System.Windows.Point(point.X, point.Y);
        }

        public  KinectAudioSource CreateAudioSource()
        {
 
            if (KinectSensor.KinectSensors.Count > 0)
            {
                audioSource = KinectSensor.KinectSensors[0].AudioSource;
                audioSource.NoiseSuppression = true;
                audioSource.AutomaticGainControlEnabled = true;
                audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
                audioSource.EchoCancellationMode = EchoCancellationMode.CancellationAndSuppression;
               
            }
            return audioSource;
        }

        public System.Drawing.Bitmap ColorImageFrameToBitmap(ColorImageFrame colorFrame)
        {
            byte[] pixelBuffer = new byte[colorFrame.PixelDataLength];
            colorFrame.CopyPixelDataTo(pixelBuffer);


            System.Drawing.Bitmap bitmapFrame = new System.Drawing.Bitmap(colorFrame.Width, colorFrame.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);


            BitmapData bitmapData = bitmapFrame.LockBits(new System.Drawing.Rectangle(0, 0, colorFrame.Width, colorFrame.Height), ImageLockMode.WriteOnly, bitmapFrame.PixelFormat);


            IntPtr intPointer = bitmapData.Scan0;
            //Marshal.Copy(pixelBuffer, 0, intPointer, bitmapData.Width * bitmapData.Height);
            Marshal.Copy(pixelBuffer, 0, intPointer, colorFrame.PixelDataLength);

            bitmapFrame.UnlockBits(bitmapData);

            return bitmapFrame;
           


        }

        public System.Drawing.Bitmap MakeGrayscale3(System.Drawing.Bitmap original)
        {
            //create a blank bitmap the same size as original
            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
              {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
              });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new System.Drawing.Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, System.Drawing.GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }
      
        
    }
}
