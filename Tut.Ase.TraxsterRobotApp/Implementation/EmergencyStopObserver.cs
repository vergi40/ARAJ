using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class EmergencyStopObserver
    {
        public const int LOOP_WAIT_TIME = 100;
        public const int SAFETY_DISTANCE = 17;

        private Robot _robot;
        private bool _stopped;
        private bool _isEmergencyStopEncountered;
        private MutualData _mutualData;


        /// <summary>
        /// Tells publicly if program should change to emergency state
        /// </summary>
        public bool IsEmergencyStopEncountered
        {
            get { return _isEmergencyStopEncountered; }
            set { _isEmergencyStopEncountered = value; }
        }

        public EmergencyStopObserver(Robot robot, MutualData mutualData)
        {
            _robot = robot;
            _stopped = true;
            _mutualData = mutualData;
            _isEmergencyStopEncountered = false;
        }

        public async Task StartLogic()
        {
            while (true)
            {
                await Task.Delay(LOOP_WAIT_TIME);

                //
                if (_stopped)
                {
                    Debug.WriteLine("Observer_stopped");
                    continue;

                }
                else
                {

                    try
                    {
                        // Read filtered sensor values
                        foreach (var sensor in _mutualData.ReadFilteredData())
                        {
                            Debug.WriteLine(sensor.Key);
                            Debug.WriteLine(sensor.Value);
                            // Check if sensor value (distance from wall) is too small
                            if (sensor.Value < SAFETY_DISTANCE)
                            {
                                Debug.WriteLine("EMERGENCY STATE");
                                // MainLogic will see this
                                IsEmergencyStopEncountered = true;

                                for (int i = 0; i < 10; i++)
                                {
                                    try
                                    {
                                        // fast motor stop
                                        await _robot.setMotorSpeed(0, 0);
                                    }
                                                                        
                                    catch (Exception)
                                    {
                                        // Catch random exceptions
                                    }
                                    await Task.Delay(100);
                                }

                                break;
                            }
                        }
                    }

                    // Handle possible empty mutualData values
                    catch (NullReferenceException)
                    {
                        Debug.WriteLine("Encountered NullReferenceException at observer");
                    }

                }

            }

        }


        public void Run()
        {
            _stopped = false;
            IsEmergencyStopEncountered = false;
        }

        public void Stop()
        {
            IsEmergencyStopEncountered = false;
            Task.Delay(200);
            _stopped = true;
            
        }
    }
}