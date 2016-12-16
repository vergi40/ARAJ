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
        public const int SENSOR_TOP_LIMIT = 80; // cm
        public const int SENSOR_BOTTOM_LIMIT = 10; // cm
        public const int IDEAL_RIGHT_SENSOR_DISTANCE = 50;

        public const int SIDESENSOR_ANGLE = 40;



        private Robot _robot;
        private bool _stopped;
        private double _turningDegrees;
        private Enums.RobotRunMode _runMode;
        private bool _nextToTheWall;

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
            _turningDegrees = -1;
            _nextToTheWall = false;
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
                            while (sensorValues[Enums.Sensor.FrontSensor] > TRAVEL_DISTANCE_FROM_WALL && !_stopped)
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
                            _nextToTheWall = true;
                            RunMode = Enums.RobotRunMode.Turn;
                        }

                        // Continues straight, till wall makes turn. Tries to follow it smoothly
                        else if (RunMode == Enums.RobotRunMode.FollowWall)
                        {
                            int margin = 5;

                            while (!_stopped)
                            {
                                var sensorValues = _mutualData.ReadFilteredData();
                                double rightSensor = sensorValues[Enums.Sensor.RightSensor];

                                // Continue straight if right sensor shows optimal reading
                                // and front sensor sees nothing
                                if (! IsSensorValueInReach(sensorValues[Enums.Sensor.FrontSensor]) &&
                                    rightSensor > IDEAL_RIGHT_SENSOR_DISTANCE - margin &&
                                    rightSensor < IDEAL_RIGHT_SENSOR_DISTANCE + margin)
                                {
                                    ControlMotors(100, 100);
                                }
                                // Wall turns 90 degrees or more
                                // TODO
                                // Wall not seen on right anymore
                                // TODO

                                // Turn smoothly
                                else
                                {
                                    MakeSmoothOrientationCorrection(rightSensor, margin);
                                }

                                await Task.Delay(LOOP_WAIT_TIME);
                            }

                        }


                        else if (RunMode == Enums.RobotRunMode.Turn)
                        {
                            // Error in code logic
                            if (_turningDegrees < 0)
                            {
                                RunMode = Enums.RobotRunMode.FindWall;
                            }
                            else
                            {
                                double degrees = _turningDegrees;
                                if (degrees > 180)
                                    degrees -= 360;

                                RotateRobot(degrees).Wait();
                            }
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

        /// <summary>
        /// Rotates the robot either facing straight to the wall or leaving the wall on the right side.
        /// Assumes that robot is already situated close to wall in some direction.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        private async Task RotateRobot(double degrees)
        {
            var sensorValues = _mutualData.ReadFilteredData();
            bool clockwise = false;
            if (degrees > 0)
            {
                clockwise = true;
            }

            // Rotate till only right sensor sees something
            if (_nextToTheWall)
            {
                // TODO: needs testing how fast robot rotates
                while (! IsSensorValueInReach(sensorValues[Enums.Sensor.RightSensor]) && !_stopped)
                {
                    ControlMotors(0, -100, clockwise);
                    sensorValues = _mutualData.ReadFilteredData();
                    await Task.Delay(500);
                }

                RunMode = Enums.RobotRunMode.FollowWall;
            }

            // Rotate till the front sensor has closer range than left or right
            else
            {
                // TODO: needs testing how fast robot rotates
                while (! _stopped) // just in case
                {
                    ControlMotors(0, -100, clockwise);
                    sensorValues = _mutualData.ReadFilteredData();

                    if (IsSensorValueInReach(sensorValues[Enums.Sensor.FrontSensor]))
                    {
                        if (sensorValues[Enums.Sensor.FrontSensor] < sensorValues[Enums.Sensor.LeftSensor] && 
                            sensorValues[Enums.Sensor.FrontSensor] < sensorValues[Enums.Sensor.RightSensor])
                        {
                            break;
                        }
                    }

                    await Task.Delay(500);
                }

                RunMode = Enums.RobotRunMode.FindWall;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rightSensorValue"></param>
        /// <returns></returns>
        private void MakeSmoothOrientationCorrection(double rightSensorValue, double margin)
        {
            // Turn right
            if (rightSensorValue > IDEAL_RIGHT_SENSOR_DISTANCE + margin*4)
            {
                ControlMotors(100, 40);
            }
            else if (rightSensorValue > IDEAL_RIGHT_SENSOR_DISTANCE + margin*2)
            {
                ControlMotors(100, 70);
            }
            else if (rightSensorValue > IDEAL_RIGHT_SENSOR_DISTANCE + margin)
            {
                ControlMotors(100, 80);
            }
            // Turn left
            else if (rightSensorValue < IDEAL_RIGHT_SENSOR_DISTANCE - margin*8)
            {
                ControlMotors(0, 100);
            }
            else if (rightSensorValue < IDEAL_RIGHT_SENSOR_DISTANCE - margin*4)
            {
                ControlMotors(40, 100);
            }
            else if (rightSensorValue < IDEAL_RIGHT_SENSOR_DISTANCE - margin*2)
            {
                ControlMotors(70, 100);
            }
            else if (rightSensorValue < IDEAL_RIGHT_SENSOR_DISTANCE - margin)
            {
                ControlMotors(80, 100);
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
            if (sensorValue > SENSOR_BOTTOM_LIMIT && sensorValue < SENSOR_TOP_LIMIT)
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

        /// <summary>
        /// Motor parameters are given as if turning clockwise direction everytime.
        /// </summary>
        /// <param name="leftMotor"></param>
        /// <param name="rightMotor"></param>
        /// <param name="clockwise"></param>
        private void ControlMotors(int leftMotor, int rightMotor, bool clockwise = true)
        {
            for (int i=0;i<4;i++)
            {
                try
                {
                    // Turn counter-clockwise - reverse motors
                    if (!clockwise)
                    {
                        int tempLeftMotor = leftMotor;
                        leftMotor = rightMotor;
                        rightMotor = tempLeftMotor;
                    }
                    _robot.setMotorSpeed(leftMotor, rightMotor);
                    break;
                }
                catch (Exception e)
                {
                    // random exception
                }
            }
        }
    }
}
