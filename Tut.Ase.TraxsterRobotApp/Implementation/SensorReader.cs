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

        private Robot _robot;
        private bool _stopped;
        public SensorReader(Robot robot)
        {
            _robot = robot;
            _stopped = true;
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
                    // Run logic
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
