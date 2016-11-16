﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class MainLogic
    {
        public const int LOOP_WAIT_TIME = 50;

        private Robot _robot;

        private Enums.MainLogicStates _state;

        public MainLogic(Robot robot)
        {
            _robot = robot;
            _state = Enums.MainLogicStates.Stopped;

        }

        /// <summary>
        /// Starts and runs the main logic
        /// </summary>
        /// <returns></returns>
        public async Task RunLogic()
        {
            // Initialize concurrent objects
            RobotFunctionality robotFunctions = new RobotFunctionality(_robot);
            SensorReader sensorReader = new SensorReader(_robot);
            EmergencyStopObserver observer = new EmergencyStopObserver(_robot);

            Task task1 = robotFunctions.Logic();
            Task task2 = sensorReader.Logic();
            Task task3 = observer.Logic();

            // Background loop for everything, always running
            while (true)
            {
                try
                {
                    await Task.Delay(LOOP_WAIT_TIME);

                    // Waits for user to press "Run" button
                    if (_state == Enums.MainLogicStates.Stopped)
                    {
                        // Start-button pressed
                        if (_robot.readPin(DeviceConstants.BUTTON1_PIN))
                        {
                            _state = Enums.MainLogicStates.Run;

                            // Start up the threads etc
                            robotFunctions.Run();
                            sensorReader.Run();
                            observer.Run();

                            continue;
                        }

                    }
                    if (_state == Enums.MainLogicStates.Run)
                    {
                        // Stop-button pressed
                        if (_robot.readPin(DeviceConstants.BUTTON1_PIN))
                        {
                            _state = Enums.MainLogicStates.Stopped;

                            // Shut down / reset the threads

                            continue;
                        }
                    }
                    if (_state == Enums.MainLogicStates.Emergency)
                    {
                        //TODO
                    }
                }

                    // All exception handling / state changing here
                catch (EmergencyStopException)
                {
                    _state = Enums.MainLogicStates.Emergency;
                }
                catch (Exception e)
                {
                    //
                }
            }

            await Task.WhenAll(task1, task2, task3);

        }
    }
}
