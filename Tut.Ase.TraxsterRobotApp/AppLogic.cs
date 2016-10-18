using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO: poista valmiista, ei kuulu alkuperäiseen filuun
using System.Diagnostics;


namespace Tut.Ase.TraxsterRobotApp
{
    /// <summary>
    /// Implements application logic.
    /// </summary>
    class AppLogic
    {
        private Robot robot;
        

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="robot">Class that provides an interface to the physical robot.</param>
        public AppLogic(Robot robot)
        {
            this.robot = robot;
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <returns>Task.</returns>
        public async System.Threading.Tasks.Task run()
        {
            // TODO: Implement your app here!
            bool led1State = true;
            bool led2State = false;
            const int EPSILON = 10;


            try
            {
                while (true)
                {
                    // sensor values
                    double leftSensorValue = robot.getSensorValue(DeviceConstants.LEFT_SENSOR_ID);
                    double rightSensorValue = robot.getSensorValue(DeviceConstants.RIGHT_SENSOR_ID);
                    Debug.WriteLine("#DEBUG Sensor value for left sensor: " +
                                    leftSensorValue);
                    Debug.WriteLine("#DEBUG Sensor value for front sensor: " +
                                    robot.getSensorValue(DeviceConstants.FRONT_SENSOR_ID));
                    Debug.WriteLine("#DEBUG Sensor value for right sensor: " +
                                    rightSensorValue);
                    Debug.WriteLine("#DEBUG Sensor value for rear sensor: " +
                                    robot.getSensorValue(DeviceConstants.REAR_SENSOR_ID));

                    // button values
                    Debug.WriteLine("#DEBUG Button 1 state: " + robot.readPin(DeviceConstants.BUTTON1_PIN));
                    Debug.WriteLine("#DEBUG Button 2 state: " + robot.readPin(DeviceConstants.BUTTON2_PIN));

                    // example: change leds in every frame
                    led1State = !led1State;
                    led2State = !led2State;

                    robot.writePin(DeviceConstants.LED1_PIN, led1State);
                    robot.writePin(DeviceConstants.LED2_PIN, led2State);

                    // simple example: use motors depending of sensors
                    // drive straight
                    if (Math.Abs(leftSensorValue - rightSensorValue) < EPSILON)
                    {
                        robot.setMotorSpeed(10, 10);
                    }
                    // turn left
                    else if (leftSensorValue > rightSensorValue)
                    {
                        robot.setMotorSpeed(8, 10);
                    }
                    // turn right
                    else if (leftSensorValue < rightSensorValue)
                    {
                        robot.setMotorSpeed(10,8);
                    }

                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("#DEBUG Run mode encountered an exception and was stopped. " + ex.Message);
            }

            
        }
    }
}
