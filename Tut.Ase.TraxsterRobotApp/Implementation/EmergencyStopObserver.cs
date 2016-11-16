using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class EmergencyStopObserver
    {
        public const int LOOP_WAIT_TIME = 50;

        private Robot _robot;
        private bool _stopped;
        public EmergencyStopObserver(Robot robot)
        {
            _robot = robot;
            _stopped = true;
        }

        public async Task Logic()
        {
            while (true)
            {
                //
                if (_stopped)
                {
                    continue;
                }
                else
                {
                    // Run logic
                }

                await Task.Delay(LOOP_WAIT_TIME);

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
