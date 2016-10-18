// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 10/2016
// Last modified: 10/2016

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Tut.Ase.TraxsterRobotApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, System.ComponentModel.INotifyPropertyChanged, IUiDataSync
    {
        private object lockObject = new object();
        
        private string leftMotorSpeed = "0";
        private string rightMotorSpeed = "0";
        private string modeText = "...";
        private bool button1Pressed = false;
        private bool button2Pressed = false;
        private string buzzerText = "Buzzer OFF";
        private string led1Text = "LED 1 OFF";
        private string led2Text = "LED 2 OFF";
        private int leftSensorValue = 80;
        private int frontSensorValue = 80;
        private int rightSensorValue = 80;
        private int rearSensorValue = 80;


        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            // Required for binding properties to controls
            DataContext = this;

            // Starting application execution
            System.Threading.Tasks.Task.Run(() => runRobotApp());
        }

        private async void runRobotApp()
        {
            using (var robot = new Robot(this))
            {
                await new AppLogic(robot).run();
            }
        }


        #region IUiDataSync

        double IUiDataSync.getSensorValue(int id)
        {
            lock (this.lockObject)
            {
                switch (id)
                {
                    case DeviceConstants.LEFT_SENSOR_ID:
                        return convertSensorValueToRaw(this.leftSensorValue);

                    case DeviceConstants.FRONT_SENSOR_ID:
                        return convertSensorValueToRaw(this.frontSensorValue);

                    case DeviceConstants.RIGHT_SENSOR_ID:
                        return convertSensorValueToRaw(this.rightSensorValue);

                    case DeviceConstants.REAR_SENSOR_ID:
                        return convertSensorValueToRaw(this.rearSensorValue);

                    default:
                        throw new ArgumentException("Unknown sensor " + id);
                }
            }
        }

        private double convertSensorValueToRaw(int value)
        {
            return 32 * value - 1403;
        }

        bool IUiDataSync.readPin(int id)
        {
            lock (this.lockObject)
            {
                switch (id)
                {
                    // TODO: Invert bool values according to pin setup

                    case DeviceConstants.BUTTON1_PIN:
                        return this.Button1Pressed.HasValue && this.Button1Pressed.Value;
                      
                    case DeviceConstants.BUTTON2_PIN:
                        return this.Button2Pressed.HasValue && this.Button2Pressed.Value;

                    default:
                        throw new ArgumentException("Unknown input pin " + id);
                }
            }
        }

        void IUiDataSync.writePin(int id, bool state)
        {
            // TODO: invert value if required due to pin setup
            string onOff = state ? "ON" : "OFF";

            lock (this.lockObject)
            {
                switch (id)
                {
                    case DeviceConstants.BUZZER_PIN:
                        this.BuzzerText = "Buzzer " + onOff;
                        break;

                    case DeviceConstants.LED1_PIN:
                        this.Led1Text = "LED 1 " + onOff;
                        break;

                    case DeviceConstants.LED2_PIN:
                        this.Led2Text = "LED 2 " + onOff;
                        break;

                    default:
                        throw new ArgumentException("Unknown output pin " + id);
                }
            }
        }

        void IUiDataSync.setMotorSpeed(int left, int right)
        {
            lock (this.lockObject)
            {
                this.LeftMotorSpeed = left.ToString();
                this.RightMotorSpeed = right.ToString();
            }
        }
        
        void IUiDataSync.setSimulatorMode(bool enabled)
        {
            lock (this.lockObject)
            {
                this.ModeText = enabled ? "Mode: simulator" : "Mode: device";
            }
        }

        #endregion


        #region UI BOUND PROPERTIES AND RELATED

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private async void OnPropertyChanged(string name)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            });
        }

        /// <summary>
        /// Left motor speed.
        /// </summary>
        public string LeftMotorSpeed
        {
            get { return this.leftMotorSpeed; }
            set
            {
                this.leftMotorSpeed = value;
                OnPropertyChanged("LeftMotorSpeed");
            }
        }

        /// <summary>
        /// Right motor speed.
        /// </summary>
        public string RightMotorSpeed
        {
            get { return this.rightMotorSpeed; }
            set
            {
                this.rightMotorSpeed = value;
                OnPropertyChanged("RightMotorSpeed");
            }
        }

        /// <summary>
        /// Application mode text.
        /// </summary>
        public string ModeText
        {
            get { return this.modeText; }
            set
            {
                this.modeText = value;
                OnPropertyChanged("ModeText");
            }
        }

        /// <summary>
        /// Whether button 1 is being pressed.
        /// </summary>
        public bool? Button1Pressed
        {
            get { return this.button1Pressed; }
            set {
                this.button1Pressed = value.HasValue && value.Value;
            }
        }

        /// <summary>
        /// Whether button 2 is being pressed.
        /// </summary>
        public bool? Button2Pressed
        {
            get { return this.button2Pressed; }
            set
            {
                this.button2Pressed = value.HasValue && value.Value;
            }
        }

        /// <summary>
        /// Text that indicates led 1 status.
        /// </summary>
        public string Led1Text
        {
            get { return this.led1Text; }
            set
            {
                this.led1Text = value;
                OnPropertyChanged("Led1Text");
            }
        }

        /// <summary>
        /// Text that indicates led 2 status.
        /// </summary>
        public string Led2Text
        {
            get { return this.led2Text; }
            set
            {
                this.led2Text = value;
                OnPropertyChanged("Led2Text");
            }
        }

        /// <summary>
        /// Text that indicates buzzer status.
        /// </summary>
        public string BuzzerText
        {
            get { return this.buzzerText; }
            set
            {
                this.buzzerText = value;
                OnPropertyChanged("BuzzerText");
            }
        }

        /// <summary>
        /// Left sensor value.
        /// </summary>
        public string LeftSensorValue
        {
            get { return this.leftSensorValue.ToString(); }
            set
            {
                try
                {
                    this.leftSensorValue = int.Parse(value);
                }
                catch { }
            }
        }

        /// <summary>
        /// Front sensor value.
        /// </summary>
        public string FrontSensorValue
        {
            get { return this.frontSensorValue.ToString(); }
            set
            {
                try
                {
                    this.frontSensorValue = int.Parse(value);
                }
                catch { }
            }
        }

        /// <summary>
        /// Right sensor value.
        /// </summary>
        public string RightSensorValue
        {
            get { return this.rightSensorValue.ToString(); }
            set
            {
                try
                {
                    this.rightSensorValue = int.Parse(value);
                }
                catch { }
            }
        }

        /// <summary>
        /// Rear sensor value.
        /// </summary>
        public string RearSensorValue
        {
            get { return this.rearSensorValue.ToString(); }
            set
            {
                try
                {
                    this.rearSensorValue = int.Parse(value);
                }
                catch { }
            }
        }

        #endregion
    }
}
