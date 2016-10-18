// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 10/2016
// Last modified: 10/2016

using System;

namespace Tut.Ase.TraxsterRobotApp
{
    /// <summary>
    /// Provides access to the physical devices of the robot.
    /// </summary>
    public class Robot : IDisposable
    {
        private readonly IUiDataSync uiDataSync;
        private readonly bool runOnSimulator;
        
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="uiSync">User interface synchronizer object.</param>
        public Robot(IUiDataSync uiSync)
        {
            this.uiDataSync = uiSync;

            // Recognizing if running on device or on PC. If run on desktop, considering the app is being run on a desktop simulator.
            var analyticsInfo = Windows.System.Profile.AnalyticsInfo.VersionInfo;
            this.runOnSimulator = (analyticsInfo.DeviceFamily.Trim().ToLower() == "windows.desktop");

            uiSync.setSimulatorMode(this.runOnSimulator);
        }
        
        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            // TODO: impl dispose (probably required at least for serial port classes)
        }

        /// <summary>
        /// Gets a raw sensor value.
        /// </summary>
        /// <param name="id">Pin ID.</param>
        /// <returns>Sensor value.</returns>
        public double getSensorValue(int id)
        {
            if (this.runOnSimulator)
            {
                return this.uiDataSync.getSensorValue(id);
            }
            else
            {
                // TODO: Get from physical device
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Sets motor speeds.
        /// </summary>
        /// <param name="leftMotorSpeed">Left motor speed.</param>
        /// <param name="rightMotorSpeed">Right motor speed.</param>
        public void setMotorSpeed(int leftMotorSpeed, int rightMotorSpeed)
        {
            if (this.runOnSimulator)
            {
                this.uiDataSync.setMotorSpeed(leftMotorSpeed, rightMotorSpeed);
            }
            else
            {
                // TODO: Get from physical device
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Reads the state of a pin.
        /// </summary>
        /// <param name="id">Pin ID.</param>
        /// <returns>Pin state.</returns>
        public bool readPin(int id)
        {
            if (this.runOnSimulator)
            {
                return this.uiDataSync.readPin(id);
            }
            else
            {
                // TODO: Get from physical device
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Writes the state of a pin.
        /// </summary>
        /// <param name="id">Pin ID.</param>
        /// <param name="state">Pin state.</param>
        public void writePin(int id, bool state)
        {
            if (this.runOnSimulator)
            {
                this.uiDataSync.writePin(id, state);
            }
            else
            {
                // TODO: Get from physical device
                throw new NotImplementedException();
            }
        }
    }
}
