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

namespace ControllersLibrary
{
    /// <summary>
    /// Interaction logic for SliderController.xaml
    /// </summary>
    public partial class SliderController : UserControl
    {
        public Label SliderLabel
        {
            get { return sliderLabel; }
        }

        public Slider SliderControl
        {
            get { return sliderControl; }
        }

        public CheckBox SliderCheckbox
        {
            get { return sliderCheckbox; }
        }

        public DockPanel SliderDockpanel
        {
            get { return sliderDockpanel; }
        }

        /* Slider Changed Event */
        public static readonly RoutedEvent SliderValueChangedEvent = EventManager.RegisterRoutedEvent("SliderValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderController));

        public event RoutedEventHandler SliderValueChanged
        {
            add { AddHandler(SliderValueChangedEvent, value); }
            remove { RemoveHandler(SliderValueChangedEvent, value); }
        }

        /* Checkbox Checked Event */
        public static readonly RoutedEvent CheckboxCheckedEvent = EventManager.RegisterRoutedEvent("CheckboxChecked",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderController));

        public event RoutedEventHandler CheckboxChecked
        {
            add { AddHandler(CheckboxCheckedEvent, value); }
            remove { RemoveHandler(CheckboxCheckedEvent, value); }
        }

        /* Checkbox Unchecked Event */
        public static readonly RoutedEvent CheckboxUncheckedEvent = EventManager.RegisterRoutedEvent("CheckboxUnchecked",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SliderController));

        public event RoutedEventHandler CheckboxUnchecked
        {
            add { AddHandler(CheckboxUncheckedEvent, value); }
            remove { RemoveHandler(CheckboxUncheckedEvent, value); }
        }



        public SliderController()
        {
            InitializeComponent();
        }


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider sliderCtrl = e.OriginalSource as Slider;

            if (sliderTextbox != null)
                sliderTextbox.Text = String.Format(sliderCtrl.Value.ToString("0.000", CultureInfo.InvariantCulture));

            RaiseEvent(new RoutedEventArgs(SliderController.SliderValueChangedEvent, sliderCtrl));
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SliderController.CheckboxCheckedEvent, sliderCheckbox));
        }

        private void Checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SliderController.CheckboxUncheckedEvent, sliderCheckbox));
        }
    }
}
