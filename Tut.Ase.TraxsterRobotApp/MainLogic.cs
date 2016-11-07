﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp
{
    
    class MainLogic
    {
        public const int LOOP_WAIT_TIME = 50;

        private Robot _robot;
        private bool _emergencyStop;

        public MainLogic(Robot robot)
        {
            _robot = robot;
            _emergencyStop = false;

        }

        /// <summary>
        /// Initialize different threads
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            // Thread: Sensor observing for emergency stop
            var task1 = ObserveEmergencyStop();

            // Thread: Sensor reading
            var task2 = ReadSensors();

            // Thread: Controlling the robot
            var task3 = RunRobot();
            
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
