using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    class EmergencyStopObserver
    {
        public const int LOOP_WAIT_TIME = 100;

        private Robot _robot;
        private bool _stopped;
        public EmergencyStopObserver(Robot robot, MutualData mutualData)
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

                    // Read filtered data 
                    // TODO

                    // Analyze
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
