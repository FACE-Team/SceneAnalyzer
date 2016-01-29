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
using System.Globalization;
using System.Windows.Threading;
using System.Windows.Interop;
using SlimDX;
using SlimDX.DirectInput;

namespace ControllersLibrary
{
    /// <summary>
    /// Interaction logic for GamepadController.xaml
    /// </summary>
    public partial class GamepadController : UserControl
    {
        public enum ButtonType { Button1 = 1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, Button10, 
            XAxisLeft, YAxisDown, XAxisRight, YAxisUp }

        private Joystick joystick;
        private JoystickState state;
        private IntPtr windowHandle;
        private DispatcherTimer timer;
        private bool[] buttonsState;
        private Style styleButton = null;
        //private int numPOVs;
        //private int SliderCount;

        private int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private Dictionary<string, string> functionsAssociation;
        public Dictionary<string, string> FunctionsAssociation
        {
            get { return functionsAssociation; }
            set { functionsAssociation = value; }
        }

        private bool isActive = false;
        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public string JoyID
        {
            get { return JoyIDLabel.Content.ToString(); }
            set { JoyIDLabel.Content = value; }
        }
                

        public static readonly RoutedEvent ButtonClickedEvent = EventManager.RegisterRoutedEvent("ButtonClickedEventHandler",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GamepadController));

        public event RoutedEventHandler ButtonClickedEventHandler
        {
            add { AddHandler(ButtonClickedEvent, value); }
            remove { RemoveHandler(ButtonClickedEvent, value); }
        }

        public static readonly RoutedEvent AxisChangedEvent = EventManager.RegisterRoutedEvent("AxisChangedEventHandler",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GamepadController));

        public event RoutedEventHandler AxisChangedEventHandler
        {
            add { AddHandler(AxisChangedEvent, value); }
            remove { RemoveHandler(AxisChangedEvent, value); }
        }

        public GamepadController(int idGamepad)
        {
            InitializeComponent();
        
            functionsAssociation = new Dictionary<string, string>();
            state = new JoystickState();
            timer = new DispatcherTimer();
            windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            id = idGamepad;

            CreateDevice(id);
            JoyIDLabel.Content = "Controller " + Convert.ToString(id);
                        
            // set the timer to go off 12 times a second to read input. NOTE: Normally applications would read this much faster.
            timer.Interval = new TimeSpan(10000 / 12);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }


        #region Init

        private void CreateDevice(int idGamepad)
        {
            // make sure that DirectInput has been initialized
            DirectInput dinput = new DirectInput();

            foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                try
                {
                    joystick = new Joystick(dinput, device.InstanceGuid);
                    //joystick.SetCooperativeLevel(windowHandle, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);
                    if (joystick.Properties.JoystickId == idGamepad)
                        break;
                }
                catch (DirectInputException)
                {
                }
            }

            if (joystick == null)
            {
                //throw new Exception("There are no joysticks attached to the system.");
                MessageBox.Show("There are no joysticks attached to the system.");                
                return;
            }

            foreach (DeviceObjectInstance deviceObject in joystick.GetObjects())
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);

                //UpdateControl(deviceObject);
            }

            joystick.Acquire();
        }

        private void ReleaseDevice()
        {
            timer.Stop();

            if (joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }
            joystick = null;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (joystick != null)
            {
                if (joystick.Acquire().IsFailure)
                    return;

                if (joystick.Poll().IsFailure)
                    return;

                state = joystick.GetCurrentState();
                if (Result.Last.IsFailure)
                    return;

                UpdateUI();
            }
        }

        public void SetButtonLabel(ButtonType t, string label)
        {
            switch (t)
            {
                case ButtonType.Button1:
                    Button1Label.Content = label;
                    break;
                case ButtonType.Button2:
                    Button2Label.Content = label;
                    break;
                case ButtonType.Button3:
                    Button3Label.Content = label;
                    break;
                case ButtonType.Button4:
                    Button4Label.Content = label;
                    break;
                case ButtonType.Button5:
                    Button5Label.Content = label;
                    break;
                case ButtonType.Button6:
                    Button6Label.Content = label;
                    break;
                case ButtonType.Button7:
                    Button7Label.Content = label;
                    break;
                case ButtonType.Button8:
                    Button8Label.Content = label;
                    break;
                case ButtonType.Button9:
                    Button9Label.Content = label;
                    break;
                case ButtonType.Button10:
                    Button10Label.Content = label;
                    break;
                case ButtonType.XAxisLeft:
                    XAxisLabelLeft.Content = label;
                    break;
                case ButtonType.YAxisDown:
                    YAxisLabelDown.Content = label;
                    break;
                case ButtonType.XAxisRight:
                    XAxisLabelRight.Content = label;
                    break;
                case ButtonType.YAxisUp:
                    YAxisLabelUp.Content = label;
                    break;
            }
        }

        #endregion


        #region Updates

        private void UpdateUI()
        {
            if (isActive == true)
            {
                XaxisValue.Content = state.X.ToString(CultureInfo.CurrentCulture);
                YaxisValue.Content = state.Y.ToString(CultureInfo.CurrentCulture);
                SliderX.Value = state.X;
                SliderY.Value = state.Y;

                if ((state.X > 10 || state.X < -10 || state.Y > 10 || state.Y < -10))
                {
                    RaiseEvent(new RoutedEventArgs(GamepadController.AxisChangedEvent, new Point(state.X, state.Y)));
                }

                buttonsState = state.GetButtons();

                for (int i = 0; i < buttonsState.Length - 114; i++)
                {
                    ButtonClicked(i + 1, buttonsState[i]);
                    if (buttonsState[i])
                    {
                        ButtonListLabel.Content = (i + 1).ToString("0", CultureInfo.CurrentCulture);                        
                    }                    
                }
            }
        }

        private void UpdateControl(DeviceObjectInstance d)
        {
            if (ObjectGuid.XAxis == d.ObjectTypeGuid)
            {
                XaxisValueLabel.IsEnabled = true;
                XaxisValue.IsEnabled = true;
            }
            else if (ObjectGuid.YAxis == d.ObjectTypeGuid)
            {
                YaxisValueLabel.IsEnabled = true;
                YaxisValue.IsEnabled = true;
            }

            //if (ObjectGuid.ZAxis == d.ObjectTypeGuid)
            //{
            //    label_ZAxis.Enabled = true;
            //    label_Z.Enabled = true;
            //}
            //if (ObjectGuid.RotationalXAxis == d.ObjectTypeGuid)
            //{
            //    label_XRotation.Enabled = true;
            //    label_XRot.Enabled = true;
            //}
            //if (ObjectGuid.RotationalYAxis == d.ObjectTypeGuid)
            //{
            //    label_YRotation.Enabled = true;
            //    label_YRot.Enabled = true;
            //}
            //if (ObjectGuid.RotationalZAxis == d.ObjectTypeGuid)
            //{
            //    label_ZRotation.Enabled = true;
            //    label_ZRot.Enabled = true;
            //}

            //if (ObjectGuid.Slider == d.ObjectTypeGuid)
            //{
            //    switch (SliderCount++)
            //    {
            //        case 0:
            //            label_Slider0.Enabled = true;
            //            label_S0.Enabled = true;
            //            break;

            //        case 1:
            //            label_Slider1.Enabled = true;
            //            label_S1.Enabled = true;
            //            break;
            //    }
            //}

            //if (ObjectGuid.PovController == d.ObjectTypeGuid)
            //{
            //    switch (numPOVs++)
            //    {
            //        case 0:
            //            label_POV0.Enabled = true;
            //            label_P0.Enabled = true;
            //            break;

            //        case 1:
            //            label_POV1.Enabled = true;
            //            label_P1.Enabled = true;
            //            break;

            //        case 2:
            //            label_POV2.Enabled = true;
            //            label_P2.Enabled = true;
            //            break;

            //        case 3:
            //            label_POV3.Enabled = true;
            //            label_P3.Enabled = true;
            //            break;
            //    }
            //}
        }

        #endregion


        #region Gamepad buttons

        /// <summary>
        /// Gamepad button click
        /// </summary>
        /// <param name="buttonID"></param>
        /// <param name="value"></param>
        private void ButtonClicked(int buttonID, bool value)
        {
            if (value)
            {
                styleButton = (Style)this.FindResource("PressedRoundButtonStyle");
                RaiseEvent(new RoutedEventArgs(GamepadController.ButtonClickedEvent, buttonID));
            }
            else
            {
                styleButton = (Style)this.FindResource("RoundButtonStyle");
            }

            switch (buttonID)
            {
                case 1:
                    Button1.Style = styleButton;
                    break;
                case 2:
                    Button2.Style = styleButton;
                    break;
                case 3:
                    Button3.Style = styleButton;
                    break;
                case 4:
                    Button4.Style = styleButton;
                    break;
                case 5:
                    Button5.Style = styleButton;
                    break;
                case 6:
                    Button6.Style = styleButton;
                    break;
                case 7:
                    Button7.Style = styleButton;
                    break;
                case 8:
                    Button8.Style = styleButton;
                    break;
                case 9:
                    Button9.Style = styleButton;
                    break;
                case 10:
                    Button10.Style = styleButton;
                    break;
                default:
                    //
                    break;
            }
        }


        /// <summary>
        /// Mouse button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(GamepadController.ButtonClickedEvent, Int32.Parse(((Button)sender).Uid)));
        }

        #endregion



        //private void SetStyle(RadialGradientBrush styleBrush)
        //{
        //    Style style = (Style)this.FindResource("RoundButtonStyle");
        //    Setter setter = (Setter)style.Setters[6];
        //    ControlTemplate ct = (ControlTemplate)setter.Value;
        //    Grid g = (Grid)ct.LoadContent();
        //    //Ellipse ell = (Ellipse)g.Children[1];
        //    foreach (object ctrl in g.Children)
        //    {
        //        if (ctrl.GetType() == typeof(Ellipse))
        //            ((Ellipse)ctrl).Fill = styleBrush; ///non funziona
        //    }
        //}
    
    }
}
