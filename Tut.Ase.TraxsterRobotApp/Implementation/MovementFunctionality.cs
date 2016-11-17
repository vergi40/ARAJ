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
        public MovementFunctionality(Robot robot)
        {
            _robot = robot;
            _stopped = true;
        }

        public async Task Logic()
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
