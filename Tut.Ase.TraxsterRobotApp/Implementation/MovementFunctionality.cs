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

        public const int TRAVEL_DISTANCE_FROM_WALL = 45; // cm
        public const int SENSOR_TOP_LIMIT = 80; // cm
        public const int SENSOR_BOTTOM_LIMIT = 10; // cm
        public const int IDEAL_RIGHT_SENSOR_DISTANCE = 40; //cm

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
                //Debug.WriteLine("Robot run mode changed to: " + value);
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
               // await Task.Delay(LOOP_WAIT_TIME);

                try
                {
                    //
                    if (_stopped)
                    {
                        Debug.WriteLine("Stopped");
                        await Task.Delay(500);
                    }
                    // Run logic
                    else
                    {
                        Debug.WriteLine("Running");

                        // Robot started
                        if (RunMode == Enums.RobotRunMode.Idle)
                        {

                            Debug.WriteLine("Run mode: Idle");
                            // Wait a moment to acquire enough reliable sensor data
                            await Task.Delay(1500);

                            // Read filtered sensor values
                            var sensorValues = _mutualData.ReadFilteredData();

                            // Make decision based on the data
                            //RunMode = AnalyzeSensorsWhenIdle(sensorValues);
                            if (IsSensorValueInReach(sensorValues[Enums.Sensor.RightSensor]))
                            {
                                RunMode = Enums.RobotRunMode.FollowWall;
                            }
                            else
                            {
                                RunMode = Enums.RobotRunMode.FindWall;
                            }
                        }

                        // Robot doesn't see a wall, run forward till it does.
                        // If wall is seen on the side sensor closer, turn to that direction
                        else if (RunMode == Enums.RobotRunMode.FindWall)
                        {
                            Debug.WriteLine("Run mode: Find wall");

                            // Read filtered sensor values
                            var sensorValues = _mutualData.ReadFilteredData();
                            double margin = 5;
                            if (IsSensorValueInReach(sensorValues[Enums.Sensor.LeftSensor]))
                            {
                                ControlMotors(0, 100);
                                await Task.Delay(2000); // Turn 40
                            }
                            else if (IsSensorValueInReach(sensorValues[Enums.Sensor.RearSensor]))
                            {
                                ControlMotors(0, 100);
                                await Task.Delay(6000); // Turn 180
                            }

                            while (!_stopped && !IsSensorValueInReach(sensorValues[Enums.Sensor.RightSensor]))
                            {
                                sensorValues = _mutualData.ReadFilteredData();

                                if (sensorValues[Enums.Sensor.FrontSensor] < 70)
                                {
                                    ControlMotors(0, 100);
                                    await Task.Delay(1000);
                                }
                                else if (sensorValues[Enums.Sensor.LeftSensor] < 70)
                                {
                                    ControlMotors(0, 100);
                                    await Task.Delay(1500);
                                }
                                else
                                {
                                    ControlMotors(100, 100);
                                    await Task.Delay(100);
                                }
                            }
                            RunMode = Enums.RobotRunMode.FollowWall;
                            continue;


                            while (!_stopped)
                            {
                                ControlMotors(100, 100);
                                sensorValues = _mutualData.ReadFilteredData();

                                if (sensorValues[Enums.Sensor.FrontSensor] < TRAVEL_DISTANCE_FROM_WALL + margin)
                                {
                                    ControlMotors(0, 0);
                                    _turningDegrees = 270;
                                    _nextToTheWall = true;
                                    RunMode = Enums.RobotRunMode.Turn;
                                    break;
                                }

                                // Wall is seen on side sensors first
                                // Left side
                                if (IsSensorValueInReach(sensorValues[Enums.Sensor.LeftSensor]) &&
                                    IsSensorValueInReach(sensorValues[Enums.Sensor.FrontSensor]))
                                {
                                    if (sensorValues[Enums.Sensor.FrontSensor] > sensorValues[Enums.Sensor.LeftSensor])
                                    {
                                        _turningDegrees = 360 - SIDESENSOR_ANGLE;
                                        RunMode = Enums.RobotRunMode.Turn;
                                        break;
                                    }
                                }
                                // Right side
                                if (IsSensorValueInReach(sensorValues[Enums.Sensor.RightSensor]) &&
                                    IsSensorValueInReach(sensorValues[Enums.Sensor.FrontSensor]))
                                {
                                    if (sensorValues[Enums.Sensor.FrontSensor] > sensorValues[Enums.Sensor.RightSensor])
                                    {
                                        _turningDegrees = SIDESENSOR_ANGLE;
                                        RunMode = Enums.RobotRunMode.Turn;
                                        break;
                                    }
                                }

                                await Task.Delay(LOOP_WAIT_TIME);
                            }
                        }

                        // Continues straight, till wall makes turn. Tries to follow it smoothly
                        else if (RunMode == Enums.RobotRunMode.FollowWall)
                        {
                            Debug.WriteLine("Run mode: Follow wall");

                            int margin = 10;
                            bool tightTurnDone = false;
                            
                            while (!_stopped)
                            {
                                var sensorValues = _mutualData.ReadFilteredData();
                                double rightSensor = sensorValues[Enums.Sensor.RightSensor];

                                
                                // Wall turns 90 degrees or more
                                if (IsSensorValueInReach(sensorValues[Enums.Sensor.FrontSensor]))
                                {
                                    //if (sensorValues[Enums.Sensor.FrontSensor]-10 < sensorValues[Enums.Sensor.RightSensor])
                                    
                                    Debug.WriteLine("Wall in front, turning left.");
                                    //continue;
                                    while (sensorValues[Enums.Sensor.FrontSensor] < 50 && ! _stopped)
                                    {
                                        ControlMotors(0, 100);
                                        await Task.Delay((int)(LOOP_WAIT_TIME));

                                        //ControlMotors(100, 100);
                                        //await Task.Delay(LOOP_WAIT_TIME);
                                        sensorValues = _mutualData.ReadFilteredData();

                                    }

                                    Debug.WriteLine("Driving forward.");
                                    
                                }
                                // Continue straight if right sensor shows optimal reading
                                // and front sensor sees nothing
                                if (rightSensor > IDEAL_RIGHT_SENSOR_DISTANCE - margin &&
                                    rightSensor < IDEAL_RIGHT_SENSOR_DISTANCE + margin)
                                {
                                    ControlMotors(100, 100);
                                    await Task.Delay(LOOP_WAIT_TIME);
                                }

                                // Wall not seen on right anymore
                                //else if (!IsSensorValueInReach(sensorValues[Enums.Sensor.RightSensor]))
                                //{
                                //    RunMode = Enums.RobotRunMode.FindWall;
                                //}
                                else if (!IsSensorValueInReach(rightSensor) && !tightTurnDone)
                                {
                                    ControlMotors(100, 100);
                                    tightTurnDone = true;
                                    await Task.Delay(350);
                                }

                                // Turn "smoothly"
                                else
                                {
                                    MakeRoughOrientationCorrection(rightSensor, margin);
                                    double i = Math.Abs(rightSensor - IDEAL_RIGHT_SENSOR_DISTANCE)/4;
                                    i = Math.Min(i, 10);
                                    await Task.Delay((int)(LOOP_WAIT_TIME*i));

                                    ControlMotors(100,100);
                                    await Task.Delay(LOOP_WAIT_TIME*3);

                                    tightTurnDone = false;
                                }

                            }

                        }


                        else if (RunMode == Enums.RobotRunMode.Turn)
                        {
                            Debug.WriteLine("Run mode: Turn. Turning degrees: " + _turningDegrees);

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
                                _turningDegrees = 0;
                            }
                        }
                        //else if (RunMode == Enums.RobotRunMode.Turn90)
                        //{
                        //    //TODO
                        //}

                        await Task.Delay(LOOP_WAIT_TIME);
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
            ControlMotors(0, 0);
            double margin = 10;

            var sensorValues = _mutualData.ReadFilteredData();
            bool clockwise = false;
            if (degrees > 0)
            {
                clockwise = true;
            }

            // Start with rough turn
            // 13,33 seconds stands for 360 degrees
            //ControlMotors(0, -100, clockwise);
            //await Task.Delay((int)(degrees/360)*13333);

            // Rotate till only right sensor sees something
            // Optimal distance when right sensor reading is 30
            if (_nextToTheWall)
            {
                while (!_stopped)
                {
                    ControlMotors(0, -100, clockwise);
                    sensorValues = _mutualData.ReadFilteredData();

                    if (sensorValues[Enums.Sensor.RightSensor] > IDEAL_RIGHT_SENSOR_DISTANCE - margin
                        && sensorValues[Enums.Sensor.RightSensor] < IDEAL_RIGHT_SENSOR_DISTANCE + margin
                        && sensorValues[Enums.Sensor.FrontSensor] > sensorValues[Enums.Sensor.RightSensor])
                    {
                        break;
                    }
                    
                    await Task.Delay(LOOP_WAIT_TIME);
                }

                RunMode = Enums.RobotRunMode.FollowWall;
            }

            // Rotate till the front sensor has closer range than left or right
            else
            {
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

                    await Task.Delay(LOOP_WAIT_TIME);
                }

                RunMode = Enums.RobotRunMode.FindWall;
            }
        }


        private async Task Turn90Degrees(bool clockwise = true)
        {
            // Turning 360 degrees takes 13.33 seconds
            ControlMotors(100, 0, clockwise);
            await Task.Delay(3333);
        }

        /// <summary>
        /// Right sensor values is out of ideal distance (+ margin). Give turning command
        /// </summary>
        /// <param name="rightSensorValue"></param>
        /// <returns></returns>
        private void MakeRoughOrientationCorrection(double rightSensorValue, double margin)
        {
            // Turn counterclockwise
            if (rightSensorValue < IDEAL_RIGHT_SENSOR_DISTANCE - margin)
            {
                ControlMotors(0, 100);
            }
            // Clockwise
            else if (rightSensorValue > IDEAL_RIGHT_SENSOR_DISTANCE + margin)
            {
                ControlMotors(100, 0);
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

            int i = 0;
            while (i<5)
            {
                ControlMotors(0, 0);
                i++;
                Task.Delay(100);
            }
            
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
