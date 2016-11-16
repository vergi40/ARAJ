using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp
{
    public class Enums
    {
        public enum MainLogicStates
        {
            Run,
            Stopped,
            Emergency
        };

        public enum RobotRunMode
        {
            SearchWall,
            FollowWall,
            WallApproach,
            WallEnding
        };
    }
}
