// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 11/2016
// Last modified: 11/2016

using System;
using Tut.Ase.TraxsterRobotApp.Implementation;

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
            await new MainLogic(robot).StartLogic();
        }
    }
}
