using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class MovementFunctionality
    {
        public const int LOOP_WAIT_TIME = 100;
        public const int TRAVEL_DISTANCE_FROM_WALL = 20; // cm
        public const int SIDESENSOR_ANGLE = 40;


        private Robot _robot;
        private bool _stopped;
        private double _turningDegrees;
        private Enums.RobotRunMode _runMode;

        public Enums.RobotRunMode RunMode
        {
            get
            {
                return _runMode;
            }
            set
            {
                Debug.WriteLine("Robot run mode changed to: " + value);
                _runMode = value;
            }
        }

        private MutualData _mutualData;
        public MovementFunctionality(Robot robot, MutualData mutualData)
        {
            _robot = robot;
            _stopped = true;
            _turningDegrees = 0;
            RunMode = Enums.RobotRunMode.Idle;
            _mutualData = mutualData;
        }

        public async Task StartLogic()
        {
            while (true)
            {
                await Task.Delay(LOOP_WAIT_TIME);

                try
                {
                    //
                    if (_stopped)
                    {
                        Debug.WriteLine("Stopped");
                    }
                    // Run logic
                    else
                    {
                        Debug.WriteLine("Running");

                        // Robot started
                        if (RunMode == Enums.RobotRunMode.Idle)
                        {
                            // Wait a moment to acquire enough reliable sensor data
                            await Task.Delay(1500);
                        
                            // Read filtered sensor values
                            var sensorValues = _mutualData.ReadFilteredData();

                            // Make decision based on the data
                            RunMode = AnalyzeSensorsWhenIdle(sensorValues);
                        }
                        // Robot doesn't see a wall, run forward till it does.
                        // If wall is seen on the side sensor closer, turn to that direction
                        else if (RunMode == Enums.RobotRunMode.FindWall)
                        {
                            // Read filtered sensor values
                            var sensorValues = _mutualData.ReadFilteredData();
                            while (sensorValues[Enums.Sensor.FrontSensor] > TRAVEL_DISTANCE_FROM_WALL)
                            {
                                ControlMotors(100, 100);
                                sensorValues = _mutualData.ReadFilteredData();
                                await Task.Delay(LOOP_WAIT_TIME);

                                // Wall is seen on side sensors first
                                // Left side
                                if (IsSensorValueInReach(sensorValues[Enums.Sensor.LeftSensor]))
                                {
                                    if (sensorValues[Enums.Sensor.FrontSensor] > sensorValues[Enums.Sensor.LeftSensor])
                                    {
                                        _turningDegrees = 360 - SIDESENSOR_ANGLE;
                                        RunMode = Enums.RobotRunMode.Turn;
                                        break;
                                    }
                                }
                                // Right side
                                if (IsSensorValueInReach(sensorValues[Enums.Sensor.RightSensor]))
                                {
                                    if (sensorValues[Enums.Sensor.FrontSensor] > sensorValues[Enums.Sensor.RightSensor])
                                    {
                                        _turningDegrees = SIDESENSOR_ANGLE;
                                        RunMode = Enums.RobotRunMode.Turn;
                                        break;
                                    }
                                }
                            }

                            // Turn along the wall
                            _turningDegrees = 270;
                            RunMode = Enums.RobotRunMode.Turn;
                        }
                        else if (RunMode == Enums.RobotRunMode.FollowWall)
                        {
                            //TODO
                            // Continue straight forward
                            ControlMotors(100, 100);

                        }
                        else if (RunMode == Enums.RobotRunMode.Turn)
                        {
                            //TODO
                        }
                        else if (RunMode == Enums.RobotRunMode.Turn90)
                        {
                            //TODO
                        }
                    } 
                }
                catch (Exception)
                {
                    // Random exception
                }
            }
        }

        private Enums.RobotRunMode AnalyzeSensorsWhenIdle(Dictionary<Enums.Sensor, double> sensorValues)
        {
            // Check sensor values and calculate turning value based on that
            // Wall behind
            if (IsSensorValueInReach(sensorValues[Enums.Sensor.RearSensor]))
            {
                _turningDegrees = 180;
                return Enums.RobotRunMode.Turn;
            }
            // Wall on front sector
            else if (IsSensorValueInReach(sensorValues[Enums.Sensor.FrontSensor]))
            {
                // Straight forward
                if (! IsSensorValueInReach(sensorValues[Enums.Sensor.LeftSensor]) &&
                    ! IsSensorValueInReach(sensorValues[Enums.Sensor.RightSensor]))
                {
                    return Enums.RobotRunMode.FindWall;
                }
                // Straight forward
                else if (sensorValues[Enums.Sensor.FrontSensor] < sensorValues[Enums.Sensor.LeftSensor] &&
                    sensorValues[Enums.Sensor.FrontSensor] < sensorValues[Enums.Sensor.RightSensor])
                {
                    return Enums.RobotRunMode.FindWall;
                }
                // Left side
                else if (sensorValues[Enums.Sensor.FrontSensor] > sensorValues[Enums.Sensor.LeftSensor])
                {
                    _turningDegrees = 360 - SIDESENSOR_ANGLE;
                    return Enums.RobotRunMode.Turn;
                }
                //Right side
                else if (sensorValues[Enums.Sensor.FrontSensor] > sensorValues[Enums.Sensor.RightSensor])
                {
                    _turningDegrees = SIDESENSOR_ANGLE;
                    return Enums.RobotRunMode.Turn;
                }
            }
            // Wall on left side
            else if (IsSensorValueInReach(sensorValues[Enums.Sensor.LeftSensor]))
            {
                _turningDegrees = 360 - SIDESENSOR_ANGLE;
                return Enums.RobotRunMode.Turn;
            }
            // Wall on right side
            else if (IsSensorValueInReach(sensorValues[Enums.Sensor.RightSensor]))
            {
                _turningDegrees = SIDESENSOR_ANGLE;
                return Enums.RobotRunMode.Turn;
            }

            // Wall not seen
            return Enums.RobotRunMode.FindWall;
        }

        /// <summary>
        /// Checks if sensor value (wall) is in reach.
        /// </summary>
        /// <param name="sensorValue"></param>
        /// <returns></returns>
        private bool IsSensorValueInReach(double sensorValue)
        {
            //TODO: need limits for sensor values
            if (sensorValue > 10 && sensorValue < 80)
            {
                return true;
            }
            return false;
        }

        public void Run()
        {
            _stopped = false;
        }

        public void Stop()
        {
            _stopped = true;
            ControlMotors(0, 0);
            RunMode = Enums.RobotRunMode.Idle;

        }

        private void ControlMotors(int leftMotor, int rightMotor)
        {
            //TODO
            _robot.setMotorSpeed(leftMotor, rightMotor);
        }
    }
}
