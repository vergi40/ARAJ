namespace Tut.Ase.TraxsterRobotApp.Implementation
{
    public class Enums
    {
        public enum MainLogicStates
        {
            Run,
            Stopped,
            Emergency
        };

        /// <summary>
        /// States for robot movement
        /// </summary>
        public enum RobotRunMode
        {
            FindWall,
            FollowWall,
            Idle
        };

        /// <summary>
        /// Sensor referenced
        /// </summary>
        public enum Sensor
        {
            LeftSensor,
            FrontSensor,
            RightSensor,
            RearSensor
        };
    }
}
