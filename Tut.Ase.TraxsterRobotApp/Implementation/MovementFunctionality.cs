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
        public const int LOOP_WAIT_TIME = 100; //ms

        public const int SENSOR_TOP_LIMIT = 80; // cm
        public const int SENSOR_BOTTOM_LIMIT = 10; // cm
        public const int IDEAL_RIGHT_SENSOR_DISTANCE = 40; //cm


        private Robot _robot;
        private bool _stopped;
        private Enums.RobotRunMode _runMode;

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
                        await Task.Delay(200);
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
                            // check if other sensors find walls
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

                            while (!_stopped && sensorValues[Enums.Sensor.RightSensor] > 50)
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
                        }

                        // Continues straight, till wall makes turn. Tries to follow it smoothly
                        else if (RunMode == Enums.RobotRunMode.FollowWall)
                        {
                            Debug.WriteLine("Run mode: Follow wall");

                            int margin = 8;
                            bool tightTurnDone = false;
                            
                            while (!_stopped)
                            {
                                var sensorValues = _mutualData.ReadFilteredData();
                                double rightSensor = sensorValues[Enums.Sensor.RightSensor];
                                
                                // Front sensor sees new wall
                                if (IsSensorValueInReach(sensorValues[Enums.Sensor.FrontSensor]))
                                {
                                    // Turn left until robot is parallel with new wall
                                    while (sensorValues[Enums.Sensor.FrontSensor] < 50 && ! _stopped)
                                    {
                                        ControlMotors(0, 100);
                                        await Task.Delay((int)(LOOP_WAIT_TIME));
                                        sensorValues = _mutualData.ReadFilteredData();
                                    }
                                }
                                // Continue straight if right sensor reading is within margin
                                if (rightSensor > IDEAL_RIGHT_SENSOR_DISTANCE - margin &&
                                    rightSensor < IDEAL_RIGHT_SENSOR_DISTANCE + margin)
                                {
                                    ControlMotors(100, 100);
                                    await Task.Delay(LOOP_WAIT_TIME);
                                }
                                
                                // If wall ends, continue for a while
                                else if (!IsSensorValueInReach(rightSensor) && !tightTurnDone)
                                {
                                    ControlMotors(100, 100);
                                    tightTurnDone = true;
                                    await Task.Delay(350);
                                }

                                // "P-controller", difference from right sensor value and setpoint.
                                // Controls with kind of a pulse-width modulation by stopping track
                                // Larger difference causes longer stopping sequence
                                else
                                {
                                    MakeRoughOrientationCorrection(rightSensor);
                                    double i = Math.Abs(rightSensor - IDEAL_RIGHT_SENSOR_DISTANCE)/4;
                                    i = Math.Min(i, 10);
                                    await Task.Delay((int)(LOOP_WAIT_TIME*i));

                                    // continue forward for a while to get new sensor data
                                    ControlMotors(100,100);
                                    await Task.Delay(LOOP_WAIT_TIME*3);

                                    tightTurnDone = false;
                                }
                            }
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
        /// Right sensor values is out of ideal distance (+ margin). Give turning command
        /// </summary>
        /// <param name="rightSensorValue"></param>
        /// <returns></returns>
        private void MakeRoughOrientationCorrection(double rightSensorValue)
        {
            // Turn counterclockwise
            if (rightSensorValue < IDEAL_RIGHT_SENSOR_DISTANCE)
            {
                ControlMotors(0, 100);
            }
            // Clockwise
            else if (rightSensorValue > IDEAL_RIGHT_SENSOR_DISTANCE)
            {
                ControlMotors(100, 0);
            }
            
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
