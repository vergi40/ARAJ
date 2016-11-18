// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 10/2016
// Last modified: 11/2016

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
        // This is used to mutually exclude any threads calling this class. It is also supposed to force 
        // the environment not to cache locked variables as they are utilised from various threads.
        private object lockObject = new object();
        
        private string leftMotorSpeed = "0";
        private string rightMotorSpeed = "0";
        private string modeText = "...";
        private bool middleButtonPressed = false;
        private bool rightButtonPressed = false;
        //private string buzzerText = "Buzzer OFF";
        private string leftLedText = "Left LED OFF";
        private string rightLedText = "Right LED OFF";
        private int leftSensorValue = 80;
        private int frontSensorValue = 80;
        private int rightSensorValue = 80;
        private int rearSensorValue = 80;

        // Exceptions are thrown randomly as device is being controlled. This simulates
        // situations where device-related exceptions occur.
        private int requestCounter = 0;


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

        private async System.Threading.Tasks.Task runRobotApp()
        {
            using (var robot = new Robot(this))
            {
                await new AppLogic(robot).run();
            }
        }


        #region IUiDataSync AND RELATED

        int IUiDataSync.getSensorValue(int id)
        {
            throwHardwareExceptionIfAppropriate();

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

        private int convertSensorValueToRaw(int value)
        {
            // This formula was used as an example before the actual formula was resolved
            //return 32 * value - 1403;

            // This is to simulate a sensor seeing "nothing" and giving a very low output
            if (value >= 79)
            {
                return 30;
            }

            // To avoid division by zero (not expected though)
            if (value == 0)
            {
                value = 1;
            }

            return (int)((double)31000 / value + 100);
        }

        bool[] IUiDataSync.readPins()
        {
            throwHardwareExceptionIfAppropriate();

            // In hardware, buttons return true if *not* pressed.
            // Note that the state of non-plugged pins is whatever in hardware.
            bool[] returnValue = new bool[]
            {
                false, false, false, false,
                false, false, false, false // 8 items in total
            };

            returnValue[DeviceConstants.BUTTON_MIDDLE_PIN] = !this.MiddleButtonPressed.HasValue || !this.MiddleButtonPressed.Value;
            returnValue[DeviceConstants.BUTTON_RIGHT_PIN] = !this.RightButtonPressed.HasValue || !this.RightButtonPressed.Value;
            
            return returnValue;
        }

        void IUiDataSync.writePins(bool[] states)
        {
            const int ARRAY_LENGTH = 8;

            if (states.Length != ARRAY_LENGTH)
            {
                throw new ArgumentException("When writing pins, array length must be " + ARRAY_LENGTH);
            }

            throwHardwareExceptionIfAppropriate();

            string onOffLeft = states[DeviceConstants.LED_LEFT_PIN] ? "ON" : "OFF";
            string onOffRight = states[DeviceConstants.LED_RIGHT_PIN] ? "ON" : "OFF";

            this.LeftLedText = "Left LED " + onOffLeft;
            this.RightLedText = "Right LED " + onOffRight;
        }

        void IUiDataSync.setMotorSpeed(int left, int right)
        {
            throwHardwareExceptionIfAppropriate();

            // Restricting speeds
            if (left > 100) left = 100;
            if (left < -100) left = -100;
            if (right > 100) right = 100;
            if (right < -100) right = -100;

            this.LeftMotorSpeed = left.ToString();
            this.RightMotorSpeed = right.ToString();
        }
        
        void IUiDataSync.setSimulatorMode(bool enabled)
        {
            this.ModeText = enabled ? "Mode: simulator" : "Mode: device";
        }

        private void throwHardwareExceptionIfAppropriate()
        {
            lock (this.lockObject)
            {
                // Throwing an exception every 10th time to simulate hardware problems
                ++this.requestCounter;
                this.requestCounter = this.requestCounter % 10;
                
                if (this.requestCounter == 0)
                {
                    throw new Exception("Simulating hardware related errors");
                }
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
            get
            {
                lock (this.lockObject)
                {
                    return this.leftMotorSpeed;
                }
            }
            set
            {
                lock (this.lockObject)
                {
                    this.leftMotorSpeed = value;
                }
                OnPropertyChanged("LeftMotorSpeed");
            }
        }

        /// <summary>
        /// Right motor speed.
        /// </summary>
        public string RightMotorSpeed
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.rightMotorSpeed;
                }
            }
            set
            {
                lock (this.lockObject)
                {
                    this.rightMotorSpeed = value;
                }
                OnPropertyChanged("RightMotorSpeed");
            }
        }

        /// <summary>
        /// Application mode text.
        /// </summary>
        public string ModeText
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.modeText;
                }
            }
            set
            {
                lock (this.lockObject)
                {
                    this.modeText = value;
                }
                OnPropertyChanged("ModeText");
            }
        }

        /// <summary>
        /// Whether button 1 is being pressed.
        /// </summary>
        public bool? MiddleButtonPressed
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.middleButtonPressed;
                }
            }
            set
            {
                lock (this.lockObject)
                {
                    this.middleButtonPressed = value.HasValue && value.Value;
                }
            }
        }

        /// <summary>
        /// Whether button 2 is being pressed.
        /// </summary>
        public bool? RightButtonPressed
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.rightButtonPressed;
                }
            }
            set
            {
                lock (this.lockObject)
                {
                    this.rightButtonPressed = value.HasValue && value.Value;
                }
            }
        }

        /// <summary>
        /// Text that indicates led 1 status.
        /// </summary>
        public string LeftLedText
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.leftLedText;
                }
            }
            set
            {
                lock (this.lockObject)
                {
                    this.leftLedText = value;
                }
                OnPropertyChanged("LeftLedText");
            }
        }

        /// <summary>
        /// Text that indicates led 2 status.
        /// </summary>
        public string RightLedText
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.rightLedText;
                }
            }
            set
            {
                lock (this.lockObject)
                {
                    this.rightLedText = value;
                }
                OnPropertyChanged("RightLedText");
            }
        }

        /*/// <summary>
        /// Text that indicates buzzer status.
        /// </summary>
        public string BuzzerText
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.buzzerText;
                }
            }
            set
            {
                lock (this.lockObject)
                {
                    this.buzzerText = value;
                }
                OnPropertyChanged("BuzzerText");
            }
        }*/

        /// <summary>
        /// Left sensor value.
        /// </summary>
        public string LeftSensorValue
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.leftSensorValue.ToString();
                }
            }
            set
            {
                try
                {
                    lock (this.lockObject)
                    {
                        this.leftSensorValue = int.Parse(value);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Front sensor value.
        /// </summary>
        public string FrontSensorValue
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.frontSensorValue.ToString();
                }
            }
            set
            {
                try
                {
                    lock (this.lockObject)
                    {
                        this.frontSensorValue = int.Parse(value);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Right sensor value.
        /// </summary>
        public string RightSensorValue
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.rightSensorValue.ToString();
                }
            }
            set
            {
                try
                {
                    lock (this.lockObject)
                    {
                        this.rightSensorValue = int.Parse(value);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Rear sensor value.
        /// </summary>
        public string RearSensorValue
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.rearSensorValue.ToString();
                }
            }
            set
            {
                try
                {
                    lock (this.lockObject)
                    {
                        this.rearSensorValue = int.Parse(value);
                    }
                }
                catch { }
            }
        }

        #endregion
    }
}
