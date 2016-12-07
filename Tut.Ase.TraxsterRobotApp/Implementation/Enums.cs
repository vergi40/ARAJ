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

        public enum RobotRunMode
        {
            FindWall,
            Turn,
            FollowWall,
            Turn90,
            Idle
        };

        public enum Sensor
        {
            LeftSensor,
            FrontSensor,
            RightSensor,
            RearSensor
        };
    }
}
