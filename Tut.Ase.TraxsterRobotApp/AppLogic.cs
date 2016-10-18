using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
