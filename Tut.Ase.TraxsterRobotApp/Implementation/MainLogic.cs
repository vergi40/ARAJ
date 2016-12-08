using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class MainLogic
    {
        public const int LOOP_WAIT_TIME = 500;

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
        public async Task StartLogic()
        {
            // Initialize shared data
            MutualData mutualData = new MutualData(_robot);

            // Initialize concurrent objects
            MovementFunctionality movementFunctions = new MovementFunctionality(_robot, mutualData);
            SensorReader sensorReader = new SensorReader(mutualData);
            EmergencyStopObserver observer = new EmergencyStopObserver(_robot, mutualData);

            Task.Run(() => movementFunctions.StartLogic());
            Task.Run(() => sensorReader.StartLogic());
            Task.Run(() => observer.StartLogic());

            // Background loop for everything, always running
            while (true)
            {
                try
                {
                    await Task.Delay(LOOP_WAIT_TIME);

                    // Waits for user to press run or stop button
                    if (_state == Enums.MainLogicStates.Stopped)
                    {
                        bool[] pinStates = await _robot.readPins();
                        // Start-button pressed
                        // When button is pressed, pin changes to false
                        if (! pinStates[DeviceConstants.BUTTON_MIDDLE_PIN])
                        {
                            _state = Enums.MainLogicStates.Run;

                            // Start up the threads etc
                            movementFunctions.Run();
                            sensorReader.Run();
                            observer.Run();
                            
                            continue;
                        }

                    }
                    if (_state == Enums.MainLogicStates.Run)
                    {
                        bool[] pinStates = await _robot.readPins();
                        // Stop-button pressed
                        if (!pinStates[DeviceConstants.BUTTON_RIGHT_PIN])
                        {
                            _state = Enums.MainLogicStates.Stopped;

                            // Shut down / reset the threads
                            movementFunctions.Stop();
                            sensorReader.Stop();
                            observer.Stop();

                            continue;
                        }
                    }
                    if (_state == Enums.MainLogicStates.Emergency)
                    {
                        //TODO
                        // Emergency-stop is validated with right-button
                        break;
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
                    Debug.WriteLine("Encountered exception: " + e);
                }
            }
            //await Task.WhenAll(task1, task2, task3);
        }
    }
}
