using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class SensorReader
    {
        public const int LOOP_WAIT_TIME = 50;

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
                if (_stopped)
                {
                    continue;
                }
                else
                {
                    // Acquire raw data
                    Dictionary<Enums.Sensor, int> rawSensorValues = await _mutualData.ReadRawData();

                    // Filter raw data
                    // TODO

                    // Save filtered data back to mutual data
                    // TODO
                }
            }
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
