using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class MainLogic
    {
        public const int LOOP_WAIT_TIME = 100;

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

            ledControl();

            // Background loop for everything, always running
            while (true)
            {
                try
                {
                    // Emergency stop handling
                    if (observer.IsEmergencyStopEncountered && _state != Enums.MainLogicStates.Emergency)
                    {
                        _state = Enums.MainLogicStates.Emergency;

                        // Shut down / reset the threads
                        movementFunctions.Stop();
                        sensorReader.Stop();
                        observer.Stop();

                    }
                    // Delay each loop if robot is running normally
                    else
                    {
                        await Task.Delay(LOOP_WAIT_TIME);
                    }

                    // Waits for user to press run or stop button
                    if (_state == Enums.MainLogicStates.Stopped)
                    {
                        bool[] pinStates = await _robot.readPins();
                        // Start-button pressed
                        // When button is pressed, pin changes to false
                        if (!pinStates[DeviceConstants.BUTTON_MIDDLE_PIN])
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
                        bool[] pinStates = await _robot.readPins();
                        
                        // Emergency-stop is validated with right-button
                        if (!pinStates[DeviceConstants.BUTTON_RIGHT_PIN])
                        {
                            _state = Enums.MainLogicStates.Stopped;
                            Debug.WriteLine("EMERGENCYSTOP RESETED");

                        }

                        continue;
                    }
                }

                // Random exception handling
                catch (Exception e)
                {
                    //
                    Debug.WriteLine("Encountered exception: " + e);
                }
            }
        }

        private async Task ledControl()
        {
            bool leftLedState = false;
            bool rightLedState = false;

            while (true)
            {
                try
                {
                    bool[] pinStates = await _robot.readPins();

                    // Run -mode -> constant left led on
                    if (_state == Enums.MainLogicStates.Run)
                    {
                        leftLedState = true;
                    }

                    // Emergency -mode -> left & right led alternately blinking
                    else if (_state == Enums.MainLogicStates.Emergency)
                    {
                        if (leftLedState)
                        {
                            leftLedState = false;
                            rightLedState = true;
                        }
                        else
                        {
                            leftLedState = true;
                            rightLedState = false;
                        }
                    }
                    // Stopped -mode -> left led blinking, right off
                    else if (_state == Enums.MainLogicStates.Stopped)
                    {
                        if (leftLedState)
                        {
                            leftLedState = false;
                        }
                        else
                        {
                            leftLedState = true;
                        }
                        rightLedState = false;
                    }

                    pinStates[DeviceConstants.LED_LEFT_PIN] = leftLedState;
                    pinStates[DeviceConstants.LED_RIGHT_PIN] = rightLedState;

                    await _robot.writePins(pinStates);
                    await Task.Delay(400);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Encountered exception: " + e);
                }
            }
        }
    }
}
