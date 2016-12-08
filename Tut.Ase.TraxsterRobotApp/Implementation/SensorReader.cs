using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class SensorReader
    {
        public const int LOOP_WAIT_TIME = 100;

        private bool _stopped;
        private MutualData _mutualData;

        public SensorReader(MutualData mutualData)
        {
            _stopped = true;
            _mutualData = mutualData;
        }

        public async Task StartLogic()
        {
            while (true)
            {
                await Task.Delay(LOOP_WAIT_TIME);

                //
                try
                {
                    if (_stopped)
                    {
                        continue;
                    }
                    else
                    {
                        // Acquire raw data
                        Queue<Dictionary<Enums.Sensor, int>> rawSensorValuesQueue = await _mutualData.ReadRawData();

                        // Filter raw data
                        Dictionary<Enums.Sensor, double> filteredSensorValues = Filter(rawSensorValuesQueue);

                        // Save filtered data back to mutual data
                        _mutualData.WriteFilteredData(filteredSensorValues);

                    }
                }
                catch (Exception e)
                {
                    // Catch random generated exceptions
                }
            }
        }

        /// <summary>
        /// AD-converter equationg values demonstrated below
        ///     filtered value
        ///        ^
        ///    30- |  |
        ///        |   \
        ///        |     -----
        /// -------|----------- ->  raw value
        /// --     |        5000
        ///    \   |
        ///     |  |
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private Dictionary<Enums.Sensor, double> Filter(Queue<Dictionary<Enums.Sensor, int>> values)
        {
            //TODO
            // do stuff here
            // Equation
            // d = 31000 / (r - 100)
            var calculatedValuesContainer = new List<List<double>>();
            foreach (var value in values)
            {
                var calculatedValuesForOneTick = new List<double>();

                foreach (var sensor in value)
                {
                    double result = 0;

                    if (sensor.Value - 100 != 0)
                    {
                        result = (double)31000/(sensor.Value - 100);
                        if (result < 0)
                        {
                            result = -result;
                        }
                    }

                    calculatedValuesForOneTick.Add(result);
                }
                calculatedValuesContainer.Add(calculatedValuesForOneTick);
            }

            // circle buffer etc?
            // ...

            // Quick test version with simple mean value
            double leftSensorMeanValue = 0;
            double frontSensorMeanValue = 0;
            double rightSensorMeanValue = 0;
            double rearSensorMeanValue = 0;

            foreach (var oneTick in calculatedValuesContainer)
            {
                leftSensorMeanValue += oneTick[0];
                frontSensorMeanValue += oneTick[1];
                rightSensorMeanValue += oneTick[2];
                rearSensorMeanValue += oneTick[3];
            }

            var valueCount = values.Count;
            if (valueCount < 1)
                valueCount = 1;
            var filteredValues = new Dictionary<Enums.Sensor, double>();

            filteredValues[Enums.Sensor.LeftSensor] = leftSensorMeanValue / valueCount;
            filteredValues[Enums.Sensor.FrontSensor] = frontSensorMeanValue / valueCount;
            filteredValues[Enums.Sensor.RightSensor] = rightSensorMeanValue / valueCount;
            filteredValues[Enums.Sensor.RearSensor] = rearSensorMeanValue / valueCount;

            return filteredValues;
        }

        public void Run()
        {
            _stopped = false;
        }

        public void Stop()
        {
            _stopped = true;
        }
    }
}
