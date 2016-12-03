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
        public const int LOOP_WAIT_TIME = 200;

        private Robot _robot;
        private bool _stopped;
        private Enums.RobotRunMode _runMode;
        public MovementFunctionality(Robot robot, MutualData mutualData)
        {
            _robot = robot;
            _stopped = true;
            _runMode = Enums.RobotRunMode.Idle;
        }

        public async Task StartLogic()
        {
            while (true)
            {
                await Task.Delay(LOOP_WAIT_TIME);

                //
                if (_stopped)
                {
                    Debug.WriteLine("Stopped");
                }
                else
                {
                    Debug.WriteLine("Running");

                    // Run logic
                    if (_runMode == Enums.RobotRunMode.Idle)
                    {
                        //TODO
                    }
                    else if (_runMode == Enums.RobotRunMode.FindWall)
                    {
                        //TODO
                        // Test example
                        //...
                        //...
                        _runMode = Enums.RobotRunMode.FollowWall;
                    }
                    else if (_runMode == Enums.RobotRunMode.FollowWall)
                    {
                        //TODO
                        // Continue straight forward
                        ControlMotors(100, 100);

                    }
                    else if (_runMode == Enums.RobotRunMode.Turn)
                    {
                        //TODO
                    }
                    else if (_runMode == Enums.RobotRunMode.Turn90)
                    {
                        //TODO
                    }

                }


            }

        }

        public void Run()
        {
            _stopped = false;
            if (_runMode == Enums.RobotRunMode.Idle)
            {
                _runMode = Enums.RobotRunMode.FindWall;
            }
        }

        public void Stop()
        {
            _stopped = true;
            ControlMotors(0, 0);
            _runMode = Enums.RobotRunMode.Idle;

        }

        private void ControlMotors(int leftMotor, int rightMotor)
        {
            //TODO
            _robot.setMotorSpeed(leftMotor, rightMotor);
        }
    }
}
