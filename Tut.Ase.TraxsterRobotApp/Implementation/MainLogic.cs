using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class MainLogic
    {
        public const int LOOP_WAIT_TIME = 50;

        private Robot _robot;
        private bool _emergencyStop;
        private bool _runButtonPressed;

        private Enums.MainLogicStates _state;

        public MainLogic(Robot robot)
        {
            _robot = robot;
            _emergencyStop = false;
            _runButtonPressed = false;

            _state = Enums.MainLogicStates.Stopped;

        }

        /// <summary>
        /// Starts the main logic
        /// </summary>
        /// <returns></returns>
        public async Task RunLogic()
        {
            // Background loop, waits for user to press "Run" button
            while(true)
            {
                if (_runButtonPressed)
                {
                    while (!_emergencyStop)
                    {
                        
                    }
                }
            }

            bool notRunning = true;
            // Thread: Sensor observing for emergency stop
            var task1 = ObserveEmergencyStop();

            // Thread: Sensor reading
            var task2 = ReadSensors();

            // Thread: Controlling the robot
            var task3 = RunRobot();

            while (true)
            {
                // Jos logiikka ei vielä käynnissä
                if (notRunning)
                {
                    // Thread: Sensor observing for emergency stop
                    task1.Initialize();

                    // Thread: Sensor reading
                    task2.Initialize();

                    // Thread: Controlling the robot
                    task3.Initialize();

                    notRunning = false;
                }

                
                try
                {
                    // Tee jotain staten mukaan

                    // Run

                    // Stopped

                    // Emergency





                }
                catch (EmergencyStopException)
                {
                    // lopeta runmode
                    task1.Stop();

                    // Mene hätätilaan
                    throw;
                }
            }

            
            
            await Task.WhenAll(task1, task2, task3);
        }

        private async Task ObserveEmergencyStop()
        {
            while (true)
            {
                return;
            }
        }

        private async Task ReadSensors()
        {
            while (true)
            {
                return;
            }
        }

        private async Task RunRobot()
        {
            try
            {
                while (true)
                {
                    // Test
                    Debug.WriteLine("#DEBUG Button 1 state: " + _robot.readPin(DeviceConstants.BUTTON1_PIN));
                    await Task.Delay(LOOP_WAIT_TIME);
                }
            }
            catch (EmergencyStopException)
            {
                _emergencyStop = true;
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _emergencyStop = true;
                return;
            }
            
        }
    }
}
