using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class MutualData
    {
        public const int FILTER_BUFFER_SIZE = 2;
        private readonly Robot _robot;
        private Queue<Dictionary<Enums.Sensor, int>> _rawSensorValuesQueue;
        private Dictionary<Enums.Sensor, double> _filteredSensorValues;

        // Prevents access to critical areas
        private object _filteredDataLock = new object();

        public MutualData(Robot robot)
        {
            this._robot = robot;
            _rawSensorValuesQueue = new Queue<Dictionary<Enums.Sensor, int>>();
        }

        public void WriteFilteredData(Dictionary<Enums.Sensor, double> values)
        {
            //TODO
            lock (_filteredDataLock)
            {
                _filteredSensorValues = values;
            }
        }

        /// <summary>
        /// Returns last successfully filtered data to be used for navigation. 
        /// </summary>
        /// <returns></returns>
        public Dictionary<Enums.Sensor, double> ReadFilteredData()
        {
            //TODO
            return _filteredSensorValues;
        }

        /// <summary>
        /// Reads raw data from sensors and saves it to 
        /// </summary>
        /// <returns></returns>
        public async Task<Queue<Dictionary<Enums.Sensor, int>>> ReadRawData()
        {
            int leftSensor = 0;
            int frontSensor = 0;
            int rightSensor = 0;
            int rearSensor = 0;
            int tryTimes = 4;

            // Try to read each sensor separately
            for (int i=0;i<tryTimes;i++)
            {
                // Very eager to throw exceptions
                try
                {
                    // Read raw values and save them to mutual data
                    leftSensor = await _robot.getSensorValue(DeviceConstants.LEFT_SENSOR_ID);
                    break;
                }
                catch (Exception e)
                {
                    // Catch random generated exceptions
                }
            }

            for (int i = 0; i < tryTimes; i++)
            {
                // Very eager to throw exceptions
                try
                {
                    // Read raw values and save them to mutual data
                    frontSensor = await _robot.getSensorValue(DeviceConstants.FRONT_SENSOR_ID);
                    break;
                }
                catch (Exception e)
                {
                    // Catch random generated exceptions
                }
            }

            for (int i = 0; i < tryTimes; i++)
            {
                // Very eager to throw exceptions
                try
                {
                    // Read raw values and save them to mutual data
                    rightSensor = await _robot.getSensorValue(DeviceConstants.RIGHT_SENSOR_ID);
                    break;
                }
                catch (Exception e)
                {
                    // Catch random generated exceptions
                }
            }

            for (int i = 0; i < 3; i++)
            {
                // Very eager to throw exceptions
                try
                {
                    // Read raw values and save them to mutual data
                    rearSensor = await _robot.getSensorValue(DeviceConstants.REAR_SENSOR_ID);
                    break;
                }
                catch (Exception e)
                {
                    // Catch random generated exceptions
                }
            }

            Dictionary<Enums.Sensor, int> rawSensorValues = new Dictionary<Enums.Sensor, int>();
            rawSensorValues[Enums.Sensor.LeftSensor] = leftSensor;
            rawSensorValues[Enums.Sensor.FrontSensor] = frontSensor;
            rawSensorValues[Enums.Sensor.RightSensor] = rightSensor;
            rawSensorValues[Enums.Sensor.RearSensor] = rearSensor;
            //rawSensorValues[Enums.Sensor.RearSensor] = 200;

            _rawSensorValuesQueue.Enqueue(rawSensorValues);

            // For example 10 units means 10*50ms = 1000ms
            
            if (_rawSensorValuesQueue.Count > FILTER_BUFFER_SIZE)
            {
                _rawSensorValuesQueue.Dequeue();
            }
            return _rawSensorValuesQueue;
        }
    }
}
