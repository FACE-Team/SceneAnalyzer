using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;

using System.IO;
using System.ComponentModel;
using System.Timers;
using System.Collections;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using Microsoft.VisualBasic;

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;


using ShoreNet;
using FACELibrary;
using ControllersLibrary;


using YarpManagerCS;


namespace SceneAnalyzer
{    
    public partial class MainWindow : Window
    {
             
        #region Saliency Variables

        [DllImport("VisualSaliency.dll")]
        private static extern void SaliencyMap(ref SalientPoint image, int numtemporal, int numspatial, float firsttau, int firstrad, int wFrameResized);

        [DllImport("VisualSaliency.dll", EntryPoint = "CreateVisualSaliency", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateVisualSaliency(int width, int height, int type, int numtemporal, int numspatial, float firsttau, int firstrad);

        [DllImport("VisualSaliency.dll", EntryPoint = "DestroyVisualSaliency", CallingConvention = CallingConvention.StdCall)]
        private static extern void DestroyVisualSaliency(IntPtr vs);

        [DllImport("VisualSaliency.dll", EntryPoint = "UpdateVisualSaliency", CallingConvention = CallingConvention.StdCall)]
        private static extern void UpdateVisualSaliency(IntPtr vs, IntPtr data);

        [DllImport("VisualSaliency.dll", EntryPoint = "GetSaliencyMap", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetSaliencyMap(IntPtr vs);

        [DllImport("VisualSaliency.dll", EntryPoint = "GetSaliencyMapRgb", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetSaliencyMapRgb(IntPtr vs);

        [DllImport("VisualSaliency.dll", EntryPoint = "GetSaliencyMapType", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetSaliencyMapType(IntPtr vs);

        [DllImport("VisualSaliency.dll", EntryPoint = "GetSalientPoint", CallingConvention = CallingConvention.StdCall)]
        private static extern SalientPoint GetSalientPoint(IntPtr vs);

        [StructLayout(LayoutKind.Sequential)]
        public struct SalientPoint
        {
            public UInt32 x;
            public UInt32 y;
            public UInt32 width;
            public UInt32 height;
        };

        private IntPtr vs;
        private IntPtr pinnedImageBuffer;
        private SalientPoint Spoint;
        private System.Threading.Thread saliencyThread = null;
        private System.Timers.Timer saliencySecondsTimer;        

        #endregion

        #region Shore engine parameters

        private static ShoreNetEngine CreateFaceEngine(float timeBase, bool updateTimeBase, uint threadCount, string model,
                                                        float imageScale, float minFaceSize, int minFaceScore, float idMemoryLength,
                                                        string idMemoryType, bool trackFaces, string phantomTrap,
                                                        bool searchEyes, bool searchNose, bool searchMouth,
                                                        bool analyzeEyes, bool analyzeMouth, bool analyzeGender, bool analyzeAge,
                                                        bool analyzeHappy, bool analyzeSad, bool analyzeSurprised, bool analyzeAngry)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string setupData = new StreamReader(executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + ".Resources.FaceSetupData.txt")).ReadToEnd();
            string setupCall = String.Format(CultureInfo.InvariantCulture, "CreateFaceEngine({0},{1},{2},'{3}',{4},{5},{6},{7},'{8}',{9},'{10}',{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21})",
                 timeBase, updateTimeBase.ToString().ToLower(), threadCount, model,
                 imageScale, minFaceSize, minFaceScore,
                 idMemoryLength, idMemoryType, trackFaces.ToString().ToLower(), phantomTrap,
                 searchEyes.ToString().ToLower(), searchNose.ToString().ToLower(), searchMouth.ToString().ToLower(),
                 analyzeEyes.ToString().ToLower(), analyzeMouth.ToString().ToLower(), analyzeGender.ToString().ToLower(), analyzeAge.ToString().ToLower(),
                 analyzeHappy.ToString().ToLower(), analyzeSad.ToString().ToLower(), analyzeSurprised.ToString().ToLower(), analyzeAngry.ToString().ToLower());

            return new ShoreNetEngine(setupData, setupCall);
        }

        private float timeBase = 0.03f;        // 0 = Use single image mode
        private bool updateTimeBase = true;    // Not used in video mode
        private uint threadCount = 2u;         // Let's take one thread only
        private string model = "Face.Front";   // Search frontal faces
        private float imageScale = 1.0f;       // Scale the images
        private float minFaceSize = 0.0f;      // Find small faces too
        private int minFaceScore = 9;          // That's the default value
        private float idMemoryLength = 90.0f;
        private string idMemoryType = "Spatial";
        private bool trackFaces = true;
        private string phantomTrap = "Off";
        private bool searchEyes = false;
        private bool searchNose = false;
        private bool searchMouth = false;
        private bool analyzeEyes = false;
        private bool analyzeMouth = false;
        private bool analyzeGender = true;
        private bool analyzeAge = true;
        private bool analyzeHappy = true;
        private bool analyzeSad = true;
        private bool analyzeSurprised = true;
        private bool analyzeAngry = true;

        private IntPtr imageIntPr;
        private UInt32 fWidth;
        private UInt32 fHeight;
        private Int32 lineFeed = 0;
        private ShoreNetContent content;
        private ShoreNetEngine engine;
        private System.Threading.Thread engineLoop; // Thread manages the shore engine work
        private delegate void UpdateGuiDelegate(ShoreNetObject sObj, int idx);
        private delegate void UpdateGUIDelegate(ShoreNetObject sObj);

        #endregion

        #region Kinect parameters

        private Kinect kinect =new Kinect();
      
        private int minTiltAngle = -27; //elevation angle
        private int maxTiltAngleVal = 27; //elevation angle

        private Skeleton[] frameSkeletons;
        private Brush[] skeletonBrushes;
        private bool drawSkeleton = false; //checkbox skeleton
        FaceTracker faceTracker = null;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private double _beamAngle;
        public double BeamAngle
        {
            get { return _beamAngle; }
            set
            {
                _beamAngle = value;
                OnPropertyChanged("BeamAngle");
            }
        }

        #endregion

        #region Delta per Cross-Ambient (Kinect-Shore)

        private double DeltaErrorX = 30; // 30px

        #endregion

        #region Info

        private Surroundings environment;
        private ObjectScene[] sceneObjects;
        private List<Subject> sceneSubjects;
        private List<Subject> sceneSubjectsCopy;
        private List<Subject> sceneSubjectsShore;
        private List<Subject> SubjectsCopy;
        private Winner winner;

        #endregion

        #region YARP variables

        private string sceneAnalyserOut = ConfigurationManager.AppSettings["YarpPortMetaSceneOPC_OUT"].ToString();//"/SceneAnalyserOPC/MetaSceneOPC:o";

        private string sceneAnalyserOutXML = ConfigurationManager.AppSettings["YarpPortMetaSceneXML_OUT"].ToString();// "/SceneAnalyzer/MetaSceneXML:o";
        private string sceneAnalyserInputSpeech = ConfigurationManager.AppSettings["YarpPortSpeech_INPUT"].ToString();// "/SceneAnalyzer/MetaSceneXML:o";
        private string OutputSpeech = ConfigurationManager.AppSettings["YarpPortSpeech_OUTPUT"].ToString();// "/SceneAnalyzer/MetaSceneXML:o";


        private string sceneAnalyserIn_lookAt = ConfigurationManager.AppSettings["YarpPortLookAt_INPUT"].ToString(); //"/SceneAnalyzer/LookAt:i";
        private string attentionModuleOut_lookAt = ConfigurationManager.AppSettings["YarpPortLookAt_OUTPUT"].ToString();//"/AttentionModule/LookAt:o";

     

        private YarpPort yarpPortScene;
        private YarpPort yarpPortSceneXML;
        private YarpPort yarpPortSpeech;

        private YarpPort yarpPortLookAt;

        private System.Threading.Thread senderThread = null;

        private Scene scene;

        private string receiveLookAtData = "";
        private string receiveSpeechData = "";


        private System.Timers.Timer checkYarpStatusTimer;
        private System.Timers.Timer yarpReceiver;
        private System.Timers.Timer yarpReceiverSpeech;


        private object lockObject = new object();
        private object lockSendImage = new object();
        private object lockSendScene = new object();

        UdpClient client;

        public delegate void EventHandler();
        public static event EventHandler _subrecognition;
     
        #endregion

        int j = 0;

        public MainWindow()
        {

            var dllDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);


            Console.WriteLine(Environment.GetEnvironmentVariable("PATH"));
            Console.WriteLine(dllDirectory);

            InitializeComponent();

           

            Init();
            InitKinect();

            if (!File.Exists(dllDirectory + "\\Shore140.dll"))
            {
                FacialExpCheckbox.IsEnabled = false;
            }
            else
            {
                FacialExpCheckbox.IsEnabled = true;
                InitShoreEngine();
            }

            InitSaliency();
            InitYarp();
            //InitUDP();
 
            _subrecognition += new EventHandler(checkNewSub);

            
        }



        private void checkNewSub()
        {
            System.Threading.Thread.Sleep(200);
            Subject newsub = sceneSubjects.Last(); 

            
          
                InputDialog.InputDialog dialog2 = new InputDialog.InputDialog("Assign ID and Name to " + newsub.idKinect.ToString(), newsub.idKinect.ToString(), "");

                
                if ((300 * j) + 300 < 1000)
                {
                    dialog2.Left = (300 * j) + 300;
                    dialog2.Top = 300;
                }
                else
                {
                    dialog2.Left = (300 * (j - 3)) + 300;
                    dialog2.Top = 600;
                }

               j++;

               if (dialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               {
                   string[] str = dialog2.ResultText.Split(';');

                   if (str.Length == 3)
                   {

                       if (sceneSubjects.Find(a => a.idKinect.ToString() == str[2]) != null)
                       {
                           sceneSubjects.Find(a => a.idKinect.ToString() == str[2]).id = Convert.ToInt32(str[0]);
                           sceneSubjects.Find(a => a.idKinect.ToString() == str[2]).name[0] = str[1];
                       }


                   }
                   j--;
               }
               else
                   j--;
                    

           
        }

        private void subrec(int j ) 
        {
            InputDialog.InputDialog dialog2 = new InputDialog.InputDialog("Assign ID and Name to " + sceneSubjects[j].idKinect.ToString(), sceneSubjects[j].idKinect.ToString(), "");
            if ((300 * j) + 300 < 1000)
            {
                dialog2.Left = (300 * j) + 300;
                dialog2.Top = 300;
            }
            else
            {
                dialog2.Left = (300 * (j - 3)) + 300;
                dialog2.Top = 600;
            }

            if (dialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] str = dialog2.ResultText.Split(';');

                if (str.Length == 3)
                {
                    lock(this)
                    {
                        sceneSubjects.Find(a => a.idKinect.ToString() == str[2]).id = Convert.ToInt32(str[0]);
                        sceneSubjects.Find(a => a.idKinect.ToString() == str[2]).name[0] = str[1];
                    }

                }
            }
                 
        }

        #region Initialization


        private void Init()
        {
            environment = new Surroundings();
            sceneObjects = new ObjectScene[] { new ObjectScene() };
            sceneSubjects = new List<Subject>();
            sceneSubjectsCopy = new List<Subject>();
            sceneSubjectsShore = new List<Subject>();
            SubjectsCopy = new List<Subject>();
            winner = new Winner();
        }

        private void InitKinect()
        {
       
            kinect.video = Video;
            kinect.InitKinect();
            if (kinect.sensor != null)
            {
                frameSkeletons = new Skeleton[kinect.sensor.SkeletonStream.FrameSkeletonArrayLength];
                kinect.sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(Sensor_AllFramesReady);
                kinect.sensor.Start();
               
               

                kinect.CreateAudioSource();
                kinect.soundReady = new EventWaitHandle(false, EventResetMode.AutoReset);
                kinect.audioSource.BeamAngleChanged += AudioSourceBeamAngleChanged;
                kinect.audioSource.Start();

                KinectTilt.SliderValueChanged += new RoutedEventHandler(SliderCtlr_SliderValueChanged);
                KinectTilt.SliderLabel.Content = "KinectTilt";
                KinectTilt.SliderLabel.Foreground = Brushes.Black;
            }

            skeletonBrushes = new Brush[] { Brushes.Red, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };
            RecognitionEnginePanel.IsEnabled = true;
        }

        private void InitShoreEngine()
        {

            engine = CreateFaceEngine(timeBase, updateTimeBase, threadCount, model,
                           imageScale, minFaceSize, minFaceScore,
                           idMemoryLength, idMemoryType, trackFaces, phantomTrap,
                           searchEyes, searchNose, searchMouth,
                           analyzeEyes, analyzeMouth, analyzeGender, analyzeAge,
                           analyzeHappy, analyzeSad, analyzeSurprised, analyzeAngry);
        }

        private void InitSaliency()
        {
            if (kinect.sensor != null)
            {
                int size = kinect.pixelFeed * kinect.colorImageBitmap.PixelWidth * kinect.colorImageBitmap.PixelHeight;
                pinnedImageBuffer = Marshal.AllocHGlobal(size);

                saliencySecondsTimer = new System.Timers.Timer();
                saliencySecondsTimer.Interval = 200;
                saliencySecondsTimer.Elapsed += new ElapsedEventHandler(saliencySecondsTimer_Tick);
            }
        }
        
        private void InitYarp()
        {
            #region define port

           
            yarpPortScene = new YarpPort();
            yarpPortScene.openSender(sceneAnalyserOut);

            yarpPortSceneXML = new YarpPort();
            yarpPortSceneXML.openSender(sceneAnalyserOutXML);

            yarpPortLookAt = new YarpPort();
            yarpPortLookAt.openReceiver(attentionModuleOut_lookAt, sceneAnalyserIn_lookAt);

            yarpPortSpeech = new YarpPort();
            yarpPortSpeech.openReceiver(OutputSpeech, sceneAnalyserInputSpeech);


         

            #endregion

            #region define timer or thread

            senderThread = new System.Threading.Thread(SendData);
            senderThread.Start();

        
            
            yarpReceiver = new System.Timers.Timer();
            yarpReceiver.Interval = 200;
            yarpReceiver.Elapsed += new ElapsedEventHandler(ReceiveDataLookAt);

      

            yarpReceiverSpeech = new System.Timers.Timer();
            yarpReceiverSpeech.Interval = 200;
            yarpReceiverSpeech.Elapsed += new ElapsedEventHandler(ReceiveDataSpeech);
            
            #endregion

            checkYarpStatusTimer = new System.Timers.Timer();
            checkYarpStatusTimer.Elapsed += new ElapsedEventHandler(CheckYarpConnections);
            checkYarpStatusTimer.Interval = (1000);
            checkYarpStatusTimer.Start();
          
            
        }

        private void InitUDP() 
        {
            string ip;
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = addr.ToString();
                    client = new UdpClient(ip, Convert.ToInt32(ConfigurationManager.AppSettings["UDPPortMetaScene"]));
                    break;
                }
            }

           

          
        }
        #endregion


        #region Kinect


        void AudioSourceBeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            BeamAngle = 1 * e.Angle;
            environment.soundAngle = (float)BeamAngle;
            environment.soundEstimatedX = (float)Math.Tan(Math.PI * BeamAngle / 180);
            kinect.soundReady.Set();
        }
        private void SliderCtlr_SliderValueChanged(object sender, RoutedEventArgs e)
        {
            int newVal = 0;
            Slider sliderObj = e.OriginalSource as Slider;
            int oldVal = kinect.sensor.ElevationAngle;

            newVal = (int)(((maxTiltAngleVal - minTiltAngle) * sliderObj.Value)) + minTiltAngle;
            if (newVal != oldVal)
            {
                if (newVal > minTiltAngle && newVal < maxTiltAngleVal)
                {
                    kinect.sensor.ElevationAngle = newVal;
                    // We must wait at least 1 second, and call no more frequently than 15 times every 20 seconds
                    // So, we wait at least 1350ms afterwards before we set backgroundUpdateInProgress to false.
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        private void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            byte[] pixelsColor;
            short[] pixelsDepth;

            #region ColorImageFrame
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                    return;

                pixelsColor = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixelsColor);
                
                fWidth = (UInt32)colorFrame.Width;
                fHeight = (UInt32)colorFrame.Height;
                uint stride = fWidth * (uint)kinect.pixelFeed;

                colorFrame.CopyPixelDataTo(pinnedImageBuffer, (int)(stride * fHeight)); //SALIENCY

                kinect.colorImageBitmap.WritePixels(kinect.colorImageBitmapRect, pixelsColor, (int)stride, 0);
                lineFeed = kinect.colorImageBitmap.BackBufferStride;
                imageIntPr = kinect.colorImageBitmap.BackBuffer;


                kinect.colorBitmap=kinect.ColorImageFrameToBitmap(colorFrame);
            
            }


            #endregion

            #region DepthImageFrame
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                    return;

                pixelsDepth = new short[depthFrame.PixelDataLength];
                depthFrame.CopyPixelDataTo(pixelsDepth);
                
             
                kinect.frameReady.Set();
            }


            #endregion

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;

                Canvas_Skeleton.Children.Clear();
                Canvas_Robot.Children.Clear();

                frame.CopySkeletonDataTo(frameSkeletons);


                #region Remove Subject from list
                foreach (Subject checkSub in sceneSubjects.ToList())
                {

                    bool remove = true;
                    foreach (Skeleton ske in frameSkeletons)
                    {
                        if (checkSub.idKinect == ske.TrackingId)
                        {
                            remove = false;
                            break;
                        }
                    }

                    if (remove)
                        sceneSubjects.Remove(checkSub);

                }
                #endregion

                #region Scan skeleton from Kinect
                for (int j = 0; j < frameSkeletons.Length; j++)
                {
                    Skeleton skeleton = frameSkeletons[j];

                    int i = 0;
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        #region check subject present
                        bool present = false;
                        foreach (Subject checkSub in sceneSubjects)
                        {
                            if (checkSub.idKinect == skeleton.TrackingId)
                            {
                                present = true;
                                break;
                            }
                            i++;
                        }

                        if (!present)
                        {

                            Subject newSub = new Subject();

                            newSub.idKinect = skeleton.TrackingId;
                            newSub.angle = 0;
                            sceneSubjects.Add(newSub);
                            i = sceneSubjects.Count - 1;

                            if (RecognitionCheckbox.IsChecked == true)
                                _subrecognition.Invoke();

                        }

                        #endregion


                        #region Tracking subject
                        System.Windows.Point spinInCanvas = new System.Windows.Point();
                        switch (skeleton.TrackingState)
                        {                            
                            case SkeletonTrackingState.Tracked:

                                sceneSubjects[i].trackedState = true;

                                if (drawSkeleton)
                                {
                                    var userBrush = skeletonBrushes[i];
                                    //Draw head and torso
                                    var figure = kinect.CreateFigure(skeleton, userBrush, new[] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine, JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter }, Canvas_Skeleton);
                                    Canvas_Skeleton.Children.Add(figure);
                                    //Draw left arm
                                    figure = kinect.CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft }, Canvas_Skeleton);
                                    Canvas_Skeleton.Children.Add(figure);
                                    //Draw right arm
                                    figure = kinect.CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight }, Canvas_Skeleton);
                                    Canvas_Skeleton.Children.Add(figure);
                                }

                                foreach (Joint joint in skeleton.Joints)
                                {
                                    switch (joint.JointType)
                                    {
                                        case JointType.HandRight:
                                            sceneSubjects[i].righthand_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            break;

                                        case JointType.HandLeft:
                                            sceneSubjects[i].lefthand_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            break;

                                        case JointType.Spine:
                                            sceneSubjects[i].spincenter_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };

                                            //SDK del kinect prende come(0,0) il punto in alto a destra della depth e il punto (640,480) in basso a sinistra
                                            //il mondo di FACE invece va da 0 a 1 in entrambi gli assi (X,Y) per questo si normalizza in base alle info del SDK
                                            //spinPoints.Insert(i, GetJointPoint(skeleton.Joints[JointType.Spine], Canvas_Skeleton));

                                            spinInCanvas = kinect.GetJointPoint(skeleton.Joints[JointType.Spine], Canvas_Skeleton);
                                            //System.Diagnostics.Debug.WriteLine("Tracked [" + sceneSubjects[i].id + "] SpinCanvas (" + spinInCanvas.X + "," + spinInCanvas.Y + ")");
                                            sceneSubjects[i].normalizedspincenter_xy = new List<float> { 
                                                (float)Math.Round((decimal)(spinInCanvas.X) / ((kinect.sensor.DepthStream.FrameWidth)), 2, MidpointRounding.ToEven), 
                                                (float)Math.Round((decimal)((kinect.sensor.DepthStream.FrameHeight - spinInCanvas.Y) / (kinect.sensor.DepthStream.FrameHeight)), 2, MidpointRounding.ToEven)
                                            };
                                            //System.Diagnostics.Debug.WriteLine("TrackeNormd [" + sceneSubjects[i].id + "] SpinCanvas (" + sceneSubjects[i].normalizedspincenter_xy[0] + "," + sceneSubjects[i].normalizedspincenter_xy [1] + ")");
                                            
                                            if (joint.Position.Z > 0)
                                                sceneSubjects[i].angle = (float)Math.Round((Math.Atan((joint.Position.X / joint.Position.Z)) * (180 / (Math.PI))), 2);
                                            sceneSubjects[i].speak_prob = Math.Abs(Math.Abs(sceneSubjects[i].angle - environment.soundAngle) - 57) / 57;
                                            break;

                                        case JointType.Head:
                                            sceneSubjects[i].head_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            
                                            //write the id over head
                                            System.Windows.Point xyhead0 = kinect.GetJointPoint(skeleton.Joints[JointType.Head], Canvas_Skeleton);
                                            if (sceneSubjects[i].id != 0)
                                            {
                                                DrawPointString(Convert.ToString(sceneSubjects[i].id), xyhead0.X, (float)(xyhead0.Y - 50), Brushes.Green);
                                                DrawPointString(sceneSubjects[i].name[0], xyhead0.X, (float)(xyhead0.Y - 65), Brushes.Red);
                                            }
                                            else
                                            {

                                                DrawPointString(Convert.ToString(sceneSubjects[i].idKinect), xyhead0.X, (float)(xyhead0.Y - 50), Brushes.Blue);
                                                // DrawPointString(Convert.ToString(sceneSubjects[i].gesture), xyhead0.X, (float)(xyhead0.Y - 65), Brushes.Green);
                                            }
                                            break;

                                        case JointType.WristLeft:
                                            sceneSubjects[i].leftwrist_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            break;

                                        case JointType.WristRight:
                                            sceneSubjects[i].rightwrist_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            break;

                                        case JointType.ElbowLeft:
                                            sceneSubjects[i].leftelbow_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            break;
                                        case JointType.ElbowRight:
                                            sceneSubjects[i].rightelbow_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            break;
                                        case JointType.ShoulderLeft:
                                            sceneSubjects[i].leftshoulder_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            break;
                                        case JointType.ShoulderRight:
                                            sceneSubjects[i].rightshoulder_xyz = new List<float> { joint.Position.X, joint.Position.Y, joint.Position.Z };
                                            break;

                                    }
                                }

                                #region Gesture
                                if (sceneSubjects[i].head_xyz != null && sceneSubjects[i].spincenter_xyz != null)
                                {
                                    sceneSubjects[i].gesture = 0;

                                    // TRUE if the right or left hand is over the treshold
                                    // FALSE if the subject is not trackable (i.e. the head joint is equals to the spin joint), See Tracked Passive Subjects
                                    double treshold = sceneSubjects[i].head_xyz[1] - ((sceneSubjects[i].head_xyz[1] - sceneSubjects[i].spincenter_xyz[1]) / 3);
                                    if (((sceneSubjects[i].righthand_xyz[1] > treshold) || (sceneSubjects[i].lefthand_xyz[1] > treshold)) &&
                                        (sceneSubjects[i].head_xyz[1] - sceneSubjects[i].spincenter_xyz[1]) > 0)
                                    {
                                        sceneSubjects[i].gesture = 1;
                                    }
                                }

                                #endregion
                                break;

                            case SkeletonTrackingState.PositionOnly:

                                sceneSubjects[i].trackedState = false;
                                //The skeleton position in the case of PositionOnly tracking state is the HipPoint, NOT the SpinPoint
                                sceneSubjects[i].spincenter_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                
                                spinInCanvas = kinect.GetPassivePoint(skeleton, Canvas_Skeleton);
                                //System.Diagnostics.Debug.WriteLine("NoTracked [" + sceneSubjects[i].id + "] SpinCanvas (" + spinInCanvas.X + "," + spinInCanvas.Y + ")");
                                sceneSubjects[i].normalizedspincenter_xy = new List<float> { 
                                    (float)Math.Round((decimal)(spinInCanvas.X / kinect.sensor.ColorStream.FrameWidth), 2, MidpointRounding.ToEven),
                                    (float)Math.Round((decimal)((kinect.sensor.ColorStream.FrameHeight - spinInCanvas.Y) / kinect.sensor.ColorStream.FrameHeight), 2, MidpointRounding.ToEven)
                                };
                                //System.Diagnostics.Debug.WriteLine("NoTrackeNormd [" + sceneSubjects[i].id + "] SpinCanvas (" + sceneSubjects[i].normalizedspincenter_xy[0] + "," + sceneSubjects[i].normalizedspincenter_xy[1] + ")");

                                sceneSubjects[i].lefthand_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                sceneSubjects[i].righthand_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                sceneSubjects[i].head_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                sceneSubjects[i].leftwrist_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                sceneSubjects[i].rightwrist_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                sceneSubjects[i].leftelbow_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                sceneSubjects[i].rightelbow_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                sceneSubjects[i].leftshoulder_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };
                                sceneSubjects[i].rightshoulder_xyz = new List<float> { skeleton.Position.X, skeleton.Position.Y, skeleton.Position.Z };

                                if (skeleton.Position.Z > 0)
                                    sceneSubjects[i].angle = (float)Math.Round((Math.Atan((skeleton.Position.X / skeleton.Position.Z)) * (180 / (Math.PI))), 2);

                                sceneSubjects[i].speak_prob = Math.Abs(Math.Abs(sceneSubjects[i].angle - environment.soundAngle) - 57) / 57;

                                if(sceneSubjects[i].id!=0){
                                    DrawPointString(Convert.ToString(sceneSubjects[i].id), spinInCanvas.X, (float)(spinInCanvas.Y - 110),Brushes.Red);
                                    DrawPointString(sceneSubjects[i].name[0], spinInCanvas.X, (float)(spinInCanvas.Y - 138),Brushes.Red);
                                }
                                else
                                    DrawPointString(Convert.ToString(sceneSubjects[i].idKinect), spinInCanvas.X, (float)(spinInCanvas.Y - 110),Brushes.Blue);

                                DrawPoint(spinInCanvas.X, (float)(spinInCanvas.Y));
                                break;
                        }
                        #endregion

                      


                        #region FaceTracker
                        if (FacetrackingCheckbox.IsChecked == true)
                        {
                            if (faceTracker == null)
                            {
                                try
                                {
                                    faceTracker = new FaceTracker(kinect.sensor);
                                }
                                catch (InvalidOperationException)
                                {
                                    // During some shutdown scenarios the FaceTracker
                                    // is unable to be instantiated.  Catch that exception
                                    // and don't track a face.
                                    Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                                    faceTracker = null;
                                }
                            }

                            if (faceTracker != null)
                            {
                                FaceTrackFrame frameFace = faceTracker.Track(kinect.sensor.ColorStream.Format, pixelsColor, kinect.sensor.DepthStream.Format, pixelsDepth, skeleton);

                                if (frameFace.TrackSuccessful)
                                {
                                    //txtTracked.Content = "TRACKED";
                                    //txtRoll.Content = frameFace.Rotation.Z;
                                    //txtPitch.Content = frameFace.Rotation.X;
                                    //txtYaw.Content = frameFace.Rotation.Y;
                                    //Debug.WriteLine("TRACKED " + frameFace.Rotation.Z + frameFace.Rotation.X + frameFace.Rotation.Y);

                                    sceneSubjects[i].headorient_rpy = new List<float> { frameFace.Rotation.Z, frameFace.Rotation.X, frameFace.Rotation.Y };
                                }

                                else if (sceneSubjects[i].headorient_rpy != null)
                                {
                                    sceneSubjects[i].headorient_rpy.Clear();
                                }
                            }
                        }
                        #endregion

                     
                    
                    }

                    environment.numberSubject = sceneSubjects.Count;

                    WriteStack();


                }
                #endregion
            }
        }

        private void WriteStack()
        {
            #region View subject

            int idx = 0;
            StringBuilder[] sbGeneric = new StringBuilder[6];

                    
            
            foreach (Subject subj in sceneSubjects.ToList())
            {
                sbGeneric[idx] = new StringBuilder("");
                foreach (System.Reflection.PropertyInfo prop in typeof(Subject).GetProperties())
                {
                    object val = typeof(Subject).GetProperty(prop.Name).GetValue(subj, null);
                    if (val != null)
                    {
                        if (prop.PropertyType.IsGenericType)
                        {
                            System.Collections.IList l = (System.Collections.IList)val;
                            sbGeneric[idx].AppendFormat(" {0}:( ", prop.Name);
                            foreach (object elem in l)
                            {
                                if (elem.ToString() != null)
                                {
                                    if (elem.ToString().Length > 5)
                                        sbGeneric[idx].AppendFormat(" {0},", elem.ToString().Substring(0, 5));
                                    else
                                        sbGeneric[idx].AppendFormat(" {0},", elem.ToString());
                                }
                            }
                            sbGeneric[idx].AppendFormat(")\n");
                        }
                        else
                        {
                            sbGeneric[idx].AppendFormat(" {0} : {1} \n", prop.Name, val.ToString());
                        }
                    }
                }
                idx++;
            }

            List<Subject> copyshore = sceneSubjectsShore;
            foreach (Subject subj2 in copyshore.ToList())
            {
                if (idx >= 6)
                    break;

                sbGeneric[idx] = new StringBuilder("");
                foreach (System.Reflection.PropertyInfo prop in typeof(Subject).GetProperties())
                {
                    object val = typeof(Subject).GetProperty(prop.Name).GetValue(subj2, null);
                    if (val != null)
                    {
                        if (prop.PropertyType.IsGenericType)
                        {
                            System.Collections.IList l = (System.Collections.IList)val;
                            sbGeneric[idx].AppendFormat(" {0}:( ", prop.Name);
                            foreach (object elem in l)
                            {
                                if (elem.ToString() != null)
                                {
                                    if (elem.ToString().Length > 5)
                                        sbGeneric[idx].AppendFormat(" {0},", elem.ToString().Substring(0, 5));
                                    else
                                        sbGeneric[idx].AppendFormat(" {0},", elem.ToString());
                                }
                            }
                            sbGeneric[idx].AppendFormat(")\n");
                        }
                        else
                        {
                            sbGeneric[idx].AppendFormat(" {0} : {1} \n", prop.Name, val.ToString());
                        }
                    }
                }
                idx++;
            }
            
            #endregion

            SubjParamsPanel.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(delegate()
                {
                    Subj1.Content = sbGeneric[0];
                    Subj2.Content = sbGeneric[1];
                    Subj3.Content = sbGeneric[2];
                    Subj4.Content = sbGeneric[3];
                    Subj5.Content = sbGeneric[4];
                    Subj6.Content = sbGeneric[5];
                }
            ));

            #region View enviroment

            StringBuilder sbGen = new StringBuilder();
            foreach (System.Reflection.PropertyInfo prop in typeof(Surroundings).GetProperties())
            {
                object val = typeof(Surroundings).GetProperty(prop.Name).GetValue(environment, null);
                if (val != null)
                {
                    if (prop.PropertyType.IsGenericType)
                    {
                        System.Collections.IList l = (System.Collections.IList)val;
                        sbGen.AppendFormat(" {0}:( ", prop.Name);
                        foreach (object elem in l)
                        {
                            if (elem.ToString() != null)
                            {
                                if (elem.ToString().Length > 4)
                                    sbGen.AppendFormat(" {0},", elem.ToString().Substring(0, 4));
                                else
                                    sbGen.AppendFormat(" {0},", elem.ToString());
                            }
                        }
                        sbGen.AppendFormat(")\n");
                    }
                    else
                    {
                        sbGen.AppendFormat(" {0} : {1} \n", prop.Name, val.ToString());
                    }
                }
            }

            EnvirParamsPanel.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(delegate()
                {
                    Envir0.Content = "";
                    Envir0.Content = sbGen;
                }
            ));

            #endregion
        }

        #endregion


        #region DrawPoint


        public void DrawPoint(double offX, double offY)
        {
            var dr = new Label
            {
                Foreground = Brushes.Red,
                FontSize = 25,
                Content = ("•"),
                Opacity = 1,
                Margin = new Thickness(offX, offY, 0, 0)
            };
            Canvas_Skeleton.Children.Add(dr);
        }

        public void DrawPoint2(double offX, double offY, Brush br)
        {
            var dr = new Label
            {
                Foreground = br,
                FontSize = 5,
                Content = ("°"),
                Opacity = 1,
                Margin = new Thickness(offX, offY, 0, 0)
            };
            Canvas_Skeleton.Children.Add(dr);
        }

        public void DrawPointString(string str, double offX, double offY)
        {
            var dr = new Label
            {
                Foreground = Brushes.Red,
                FontSize = 20,
                Content = str,
                Opacity = 1,
                Margin = new Thickness(offX, offY, 0, 0)
            };

            Canvas_Robot.Children.Add(dr);
        }

        public void DrawPointString(string str, double offX, double offY, Brush br)
        {
            var dr = new Label
            {
                Foreground = br,
                FontSize = 20,
                Content = str,
                Opacity = 1,
                Margin = new Thickness(offX, offY, 0, 0)
            };

            Canvas_Robot.Children.Add(dr);
        }

        #endregion


        #region Shore
        
        // The engineLoop thread executes RunEngine() function. This thread waits until a signal is received
        // (frameReady.WaitOne()). In this case, the signal means that the copy of frame bytes is ready.
        // Since the engineLoop thread is not the UI thread (the thread which manages the interface), 
        // it cannot modify objects belonging to the interface. A Dispatcher object (the last line of the function) 
        // is necessary for updating the interface.
        private void RunEngine()
        {
            while (kinect.frameReady.WaitOne())
            {
                content = engine.Process(imageIntPr + 1, fWidth, fHeight, 1U, kinect.pixelFeed, lineFeed, 0, "GRAYSCALE");

                if (content == null) return;

                Canvas_Shore.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(delegate()
                {
                    Canvas_Shore.Children.Clear();
                }));


                List<Subject> sh = new List<Subject>() { };

                for (uint i = 0; i < content.GetObjectCount(); i++)
                {
                    try
                    {
                        ShoreNetObject sObj = content.GetObject(i);
                        if (sObj.GetShoreType() == "Face")
                        {
                            bool present = false;
                            System.Windows.Point middleEyes = new System.Windows.Point((sObj.GetMarkerOf("LeftEye").GetX() + sObj.GetMarkerOf("RightEye").GetX()) / 2, sObj.GetMarkerOf("LeftEye").GetY());

                            sceneSubjectsCopy = sceneSubjects.ToList();

                            for (int j = 0; j < sceneSubjectsCopy.Count; j++)
                            {
                                if (sceneSubjectsCopy[j].normalizedspincenter_xy != null)
                                {
                                    //calcolo lo spine
                                    double zeta = sceneSubjectsCopy[j].spincenter_xyz[2];

                                    System.Windows.Point spinInCanvas = new System.Windows.Point();
                                    if (sceneSubjectsCopy[j].trackedState)
                                        spinInCanvas = new System.Windows.Point((int)(sceneSubjectsCopy[j].normalizedspincenter_xy[0] * kinect.sensor.DepthStream.FrameWidth),
                                            (int)(kinect.sensor.DepthStream.FrameHeight - (sceneSubjectsCopy[j].normalizedspincenter_xy[1] * kinect.sensor.DepthStream.FrameHeight)));
                                    else
                                        spinInCanvas = new System.Windows.Point((int)(sceneSubjectsCopy[j].normalizedspincenter_xy[0] * kinect.sensor.ColorStream.FrameWidth),
                                            (int)(kinect.sensor.ColorStream.FrameHeight - (sceneSubjectsCopy[j].normalizedspincenter_xy[1] * kinect.sensor.ColorStream.FrameHeight)));




                                    double ErrorX = spinInCanvas.X - middleEyes.X; 

                                    if (Math.Abs(ErrorX) < DeltaErrorX)
                                    {
                                        try
                                        {
                                            sceneSubjects[j].gender = (sObj.GetAttributeOf("Gender") == "Female") ?  "Female" :"Male";
                                            sceneSubjects[j].age = (int)sObj.GetRatingOf("Age"); 
                                            sceneSubjects[j].happiness_ratio = sObj.GetRatingOf("Happy"); 
                                            sceneSubjects[j].surprise_ratio = sObj.GetRatingOf("Surprised"); 
                                            sceneSubjects[j].anger_ratio = sObj.GetRatingOf("Angry"); 
                                            sceneSubjects[j].sadness_ratio =sObj.GetRatingOf("Sad");  
                                            sceneSubjects[j].uptime = sObj.GetRatingOf("Uptime");
                                            sceneSubjects[j].middleeyes_xy = new List<float>() { (float)middleEyes.X, (float)middleEyes.Y };

                                            present = true;
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Shore Error" + e.Message);
                                        }
                                        
                                        break;
                                    }
                                }
                            }

                            if (!present)
                            {
                            
                                Subject newSub = new Subject();
                                newSub.idKinect = 0;
                                newSub.angle = 0;
                                newSub.gender = (sObj.GetAttributeOf("Gender") == "Female") ? "Female" : "Male";
                                newSub.age = (int)sObj.GetRatingOf("Age");
                                newSub.happiness_ratio = sObj.GetRatingOf("Happy");
                                newSub.surprise_ratio = sObj.GetRatingOf("Surprised");
                                newSub.anger_ratio = sObj.GetRatingOf("Angry");
                                newSub.sadness_ratio = sObj.GetRatingOf("Sad");
                                newSub.uptime = sObj.GetRatingOf("Uptime");
                                newSub.middleeyes_xy = new List<float>() { (float)middleEyes.X, (float)middleEyes.Y };

                                newSub.normalizedspincenter_xy = new List<float>() { };
                                newSub.spincenter_xyz = new List<float>() { };
                                newSub.lefthand_xyz = new List<float>(){ };
                                newSub.righthand_xyz = new List<float>() { };
                                newSub.head_xyz = new List<float>() { };
                                newSub.leftwrist_xyz = new List<float>(){ };
                                newSub.rightwrist_xyz = new List<float>() { };
                                newSub.leftelbow_xyz = new List<float>() { };
                                newSub.rightelbow_xyz = new List<float>() { };
                                newSub.leftshoulder_xyz = new List<float>() { };
                                newSub.rightshoulder_xyz = new List<float>() { };

                                sh.Add(newSub);


                            }

                            #region draw in canvas

                            Dictionary<string, float> expRatio = new Dictionary<string, float>() 
                            {
                                { "Angry", sObj.GetRatingOf("Angry") },
                                { "Happy", sObj.GetRatingOf("Happy") },
                                { "Sad", sObj.GetRatingOf("Sad") },
                                { "Surprised", sObj.GetRatingOf("Surprised") }
                            };
                            float ratioVal = (float)Math.Round((decimal)expRatio.Values.Max(), 1);
                            string ratioName = expRatio.OrderByDescending(kvp => kvp.Value).First().Key;

                            // Draw subject information: Gender, age +/- deviation, expression, expression rate
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine(sObj.GetAttributeOf("Gender"));
                            sb.AppendLine("Age: " + sObj.GetRatingOf("Age") + " +/- " + sObj.GetRatingOf("AgeDeviation"));
                            sb.AppendLine(ratioName + ": " + ratioVal + "%");
                            
                          
                           

                           
                            double width = Math.Abs(sObj.GetRegion().GetLeft() - sObj.GetRegion().GetRight());
                            double height = Math.Abs(sObj.GetRegion().GetTop() - sObj.GetRegion().GetBottom());
                            double left = sObj.GetRegion().GetLeft();
                            double top = sObj.GetRegion().GetTop();
                            double bottom =  sObj.GetRegion().GetBottom();
                            string gender = sObj.GetAttributeOf("Gender");

                            Canvas_Shore.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                            new Action(delegate()
                            {
                                Rectangle rect = new Rectangle();
                                rect.Width = width;
                                rect.Height = height;
                                rect.StrokeThickness = 2;
                                rect.Stroke = (gender == "Female") ? Brushes.Fuchsia : Brushes.Cyan;
                                rect.Margin = new Thickness(left, top, 0, 0); //draw the rectangle

                                Label lab = new Label
                                {
                                    Foreground = (gender == "Female") ? Brushes.Fuchsia : Brushes.Cyan,
                                    FontSize = 12,
                                    FontWeight = FontWeights.Bold,
                                    Content = sb.ToString(),
                                    Opacity = 1,
                                    Margin = new Thickness(left,bottom, 0, 0) //draw under the rectangle
                                };


                                    Canvas_Shore.Children.Add(rect);
                                    Canvas_Shore.Children.Add(lab);

                              
                            }));
                            #endregion
                        }
                    }
                    catch(Exception e) 
                    {
                        Console.WriteLine("error shore" + e.Message);

                    }
                }

                sceneSubjectsShore = sh;
               
            }
        }

        #endregion



        #region Saliency activation

        private bool saliencyIsActive = false;
        private SalientPoint[] saliency = new SalientPoint[30];
        private int currentId = 0;

        public void saliencyEngine()
        {
            while (kinect.frameReady.WaitOne())
            {
                if (saliencyIsActive == true)
                {
                    try
                    {
                        UpdateVisualSaliency(vs, pinnedImageBuffer);
                        Spoint = GetSalientPoint(vs);

                        saliency[currentId % 30] = Spoint;
                        currentId++;
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine("Error Saliency" + ex.Message);
                    }
                      
                }
            }
        }

        void saliencySecondsTimer_Tick(object sender, EventArgs e)
        {

            if (saliencyIsActive == true)
            {
                SalientPoint avgPoint = new SalientPoint();
                uint sumX = 0, sumY = 0;
                for (int i = 0; i < saliency.ToArray().Length; i++)
                {
                    sumX += saliency[i].x;
                    sumY += saliency[i].y;
                }
                avgPoint.x = (uint)(sumX / saliency.Length);
                avgPoint.y = (uint)(sumY / saliency.Length);

                Dispatcher.BeginInvoke((System.Threading.ThreadStart)delegate
                {
                    Canvas.SetLeft(salientPoint, avgPoint.x);
                    Canvas.SetTop(salientPoint, avgPoint.y);
                });
                environment.virtualsaliency_xy = new List<float> { avgPoint.x, avgPoint.y };
            }
        }
        #endregion

        
        #region Yarp

        private void SendData()
        {
            List<ObjectScene> objects = new List<ObjectScene> { sceneObjects[0] };
            while (true)
            {
             
                scene = new Scene();
                lock (lockSendScene)
                {
                    scene.Subjects = new List<Subject>(sceneSubjects.Count + sceneSubjectsShore.Count) { };

                    scene.Subjects.AddRange(sceneSubjects);
                    scene.Subjects.AddRange(sceneSubjectsShore);

                    scene.Objects = objects;
                    scene.Environment = environment;
                }

                string sceneData = ComUtils.XmlUtils.Serialize<Scene>(scene);               

               
               Console.WriteLine(sceneData);

                yarpPortSceneXML.sendData(sceneData);

             
                yarpPortScene.sendComandOPC("add" ,scene.getstringOPC());
                if (client != null)
                {
                    byte[] data = Encoding.UTF8.GetBytes(sceneData);
                    client.Send(data, data.Length);
                }
            }
        }



        void ReceiveDataSpeech(object sender, ElapsedEventArgs e)
        {
            yarpPortSpeech.receivedData(out receiveSpeechData);

            if (receiveSpeechData != null && receiveSpeechData != "")
            {
                try
                {
                    environment.recognizedWord = receiveSpeechData;
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error Speech: " + exc.Message);
                }

              
            }
        }
     


        void ReceiveDataLookAt(object sender, ElapsedEventArgs e)
        {
            yarpPortLookAt.receivedData(out receiveLookAtData);

            if (receiveLookAtData != null && receiveLookAtData != "")
            {
                try
                {
                    winner = ComUtils.XmlUtils.Deserialize<Winner>(receiveLookAtData);  //check winner data
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error XML Winner: " + exc.Message);
                }

                System.Threading.Thread t1 = new System.Threading.Thread(delegate()
                {
                  
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate()
                        {
                            Canvas.SetLeft(ViewPoint, winner.spinX * 640);
                            Canvas.SetTop(ViewPoint, (480 - winner.spinY * 480));
                        }
                    ));

                });
                t1.Priority = ThreadPriority.Lowest;
                t1.Start();
            }
        }
  
        
        

        void CheckYarpConnections(object source, ElapsedEventArgs e)
        {
            #region PortExists-> attentionModuleOut_lookAt
            if (yarpPortScene != null && yarpPortScene.PortExists(attentionModuleOut_lookAt))
            {

                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate()
                {
                    if (AttentionModStatus.Fill == Brushes.Red)
                    {
                        AttentionModStatus.Fill = Brushes.Green;
                        ViewPoint.Visibility = Visibility.Visible;
                    }
                }));
            }
            else if (yarpPortScene != null && !yarpPortScene.PortExists(attentionModuleOut_lookAt))
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate()
                        {
                            if (AttentionModStatus.Fill == Brushes.Green)
                            {
                                AttentionModStatus.Fill = Brushes.Red;
                                ViewPoint.Visibility = Visibility.Hidden;
                            }
                        }));
            }
            #endregion

            #region PortExists-> OutputSpeechStatus
            if (yarpPortScene != null && yarpPortScene.PortExists(OutputSpeech))
            {

                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate()
                    {
                        if (OutputSpeechStatus.Fill == Brushes.Red)
                        {
                            OutputSpeechStatus.Fill = Brushes.Green;
                        }
                    }));
            }
            else if (yarpPortScene != null && !yarpPortScene.PortExists(OutputSpeech))
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate()
                    {
                        if (OutputSpeechStatus.Fill == Brushes.Green)
                        {
                            OutputSpeechStatus.Fill = Brushes.Red;
                        }
                    }));
            }
            #endregion

           

            #region NetworkExists
            if (yarpPortScene != null && yarpPortScene.NetworkExists())
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate() 
                        { 
                            if(YarpServerStatus.Fill == Brushes.Red)
                                YarpServerStatus.Fill = Brushes.Green; 
                        }));
            }
            else if (yarpPortScene != null && !yarpPortScene.NetworkExists())
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(delegate() 
                        {
                            if (YarpServerStatus.Fill == Brushes.Green)
                                YarpServerStatus.Fill = Brushes.Red; 
                        }));
            }
            #endregion
        }


        #endregion

        #region Texboxes Delta



        private void ErrorX_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(delegate()
            {
                DeltaErrorX = (ErrorX.Text.Trim().Length != 0) ? double.Parse(ErrorX.Text) : 30;
            }));
        }

    

        #endregion


        #region Checkboxes

        private void CheckboxFacialexp_Checked(object sender, RoutedEventArgs e)
        {
            Canvas_Shore.Children.Clear();

            InitShoreEngine();
            if (engineLoop != null)
                StopShore();
            engineLoop = new System.Threading.Thread(RunEngine);
            engineLoop.Start();
            PanelShore.IsEnabled = true;
           
        }

        private void CheckboxFacialexp_UnChecked(object sender, RoutedEventArgs e)
        {
            Canvas_Shore.Children.Clear();

            if (engineLoop != null)
                StopShore();
            
            PanelShore.IsEnabled = false;
         
        }

        private void CheckboxSkeleton_Checked(object sender, RoutedEventArgs e)
        {
            drawSkeleton = true;
            Canvas_Shore.Children.Clear();
        }

        private void CheckboxSkeleton_Unchecked(object sender, RoutedEventArgs e)
        {
            drawSkeleton = false;
            Canvas_Shore.Children.Clear();
        }

        private void Saliency_Checked(object sender, RoutedEventArgs e)
        {
            saliencyIsActive = true;
            saliencyThread = new System.Threading.Thread(saliencyEngine);
            saliencyThread.Start();
            salientPoint.Visibility = Visibility.Visible;
            saliencySecondsTimer.Start();
        }

        private void Saliency_Unchecked(object sender, RoutedEventArgs e)
        {
            saliencyIsActive = false;
            StopSaliency();
            salientPoint.Visibility = Visibility.Hidden;
            environment.virtualsaliency_xy.Clear();
           
        }
        private void CheckboxSpeech_Checked(object sender, RoutedEventArgs e)
        {
            yarpReceiverSpeech.Start();
        }

        private void CheckboxSpeech_Unchecked(object sender, RoutedEventArgs e)
        {
            yarpReceiverSpeech.Stop();
        }


        #endregion


        #region Close app

        public void StopKinect()
        {
            if (kinect.sensor != null)
            {
                kinect.sensor.AudioSource.Stop();
                kinect.sensor.Stop();
            }
        }

        public void StopShore()
        {
            if (engineLoop != null)
                engineLoop.Abort();
        }

        public void StopSaliency()
        {
            if (saliencyThread != null)
                saliencyThread.Abort();
            if (saliencySecondsTimer != null)
                saliencySecondsTimer.Stop();
        }

        public void StopYarp()
        {
            #region Thred or timer  
            if (senderThread != null)
                senderThread.Abort();
          
            if(checkYarpStatusTimer != null)
                checkYarpStatusTimer.Stop();

          
            if (yarpReceiver != null)
                yarpReceiver.Elapsed -= new ElapsedEventHandler(ReceiveDataLookAt);
                yarpReceiver.Stop();


          
            #endregion

            #region Port


            

            if (yarpPortScene != null)
                yarpPortScene.Close();

            if (yarpPortLookAt != null)
                yarpPortLookAt.Close();

           

            #endregion


        }

        private void Window_Closing(object sender, EventArgs e)
        {
            StopYarp();
            StopShore();
            StopSaliency();
            StopKinect();
           


        }

        #endregion

        private void btnudp_Click(object sender, RoutedEventArgs e)
        {
            client = new UdpClient(UDPhost.Text, 5000);

        }

    
     
            
        
    }
}
