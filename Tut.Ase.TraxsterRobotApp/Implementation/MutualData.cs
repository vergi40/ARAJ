using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class MutualData
    {
        public const int LOOP_WAIT_TIME = 500;

        private readonly Robot _robot;
        private Dictionary<Enums.Sensor, int> _rawSensorValues;
        private List<int> _filteredSensorValue;

        // Prevents access to critical areas
        private object _rawDataLock;
        private object _filteredDataLock;

        public MutualData(Robot _robot)
        {
            this._robot = _robot;
        }

        public void WriteFilteredData()
        {

        }

        public void ReadFilteredData()
        {

        }

        public async Task<Dictionary<Enums.Sensor, int>> ReadRawData()
        {
            // Read raw values and save them to mutual data
            int leftSensor = await _robot.getSensorValue(DeviceConstants.LEFT_SENSOR_ID);
            int frontSensor = await _robot.getSensorValue(DeviceConstants.FRONT_SENSOR_ID);
            int rightSensor = await _robot.getSensorValue(DeviceConstants.RIGHT_SENSOR_ID);
            int rearSensor = await _robot.getSensorValue(DeviceConstants.REAR_SENSOR_ID);

            Dictionary<Enums.Sensor, int> rawSensorValues = new Dictionary<Enums.Sensor, int>();
            rawSensorValues[Enums.Sensor.LeftSensor] = leftSensor;
            rawSensorValues[Enums.Sensor.FrontSensor] = frontSensor;
            rawSensorValues[Enums.Sensor.RightSensor] = rightSensor;
            rawSensorValues[Enums.Sensor.RearSensor] = rearSensor;

            return rawSensorValues;
        }
    }
}
