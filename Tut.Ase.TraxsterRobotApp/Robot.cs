// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 10/2016
// Last modified: 11/2016

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp
{
    /// <summary>
    /// Provides access to the physical devices of the robot. This class is thread-safe: it is guaranteed that any 
    /// concurrent method calls do not mess things up.
    /// </summary>
    public sealed class Robot : IDisposable
    {
        private readonly IUiDataSync uiDataSync;
        private readonly bool runOnSimulator;

        private MotorController motorController = null;
        private SensorController sensorController = null;
        private GertbotUartController gertController = null;
        private GertbotGeneral gertbotGeneral = null;
        private GpioController gpioController = null;


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

            // Initialize hardware related objects if not run on simulator
            if (!this.runOnSimulator)
            {
                this.gertController = new GertbotUartController();
                this.sensorController = new SensorController(this.gertController);
                this.motorController = new MotorController(this.gertController);
                this.gertbotGeneral = new GertbotGeneral(this.gertController);
                this.gpioController = new GpioController(this.gertController, this.motorController);
            }
        }
        
        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            if (this.gertController != null)
            {
                try
                {
                    this.gertController.Dispose();
                    this.gertController = null;
                }
                catch { }
            }
        }

        /// <summary>
        /// Gets a raw sensor value.
        /// </summary>
        /// <param name="id">Pin ID.</param>
        /// <returns>Sensor value.</returns>
        public async Task<int> getSensorValue(int id)
        {
            if (this.runOnSimulator)
            {
                return this.uiDataSync.getSensorValue(id);
            }
            else
            {
                return await this.sensorController.getSensorValue(id);
            }
        }

        /// <summary>
        /// Sets motor speeds (-100..100).
        /// </summary>
        /// <param name="leftMotorSpeed">Left motor speed.</param>
        /// <param name="rightMotorSpeed">Right motor speed.</param>
        public async Task setMotorSpeed(int leftMotorSpeed, int rightMotorSpeed)
        {
            if (this.runOnSimulator)
            {
                this.uiDataSync.setMotorSpeed(leftMotorSpeed, rightMotorSpeed);
            }
            else
            {
                await this.motorController.setMotorSpeed(leftMotorSpeed, rightMotorSpeed);
            }
        }
        
        /// <summary>
        /// Reads the states of pins.
        /// </summary>
        /// <returns>Pin states ordered after pin ID.</returns>
        public async Task<bool[]> readPins()
        {
            if (this.runOnSimulator)
            {
                return this.uiDataSync.readPins();
            }
            else
            {
                return await this.gpioController.readPins();
            }
        }

        /// <summary>
        /// Writes the states of pins.
        /// </summary>
        /// <param name="states">Pin states. Array length must be 8!</param>
        public async Task writePins(bool[] states)
        {
            if (this.runOnSimulator)
            {
                this.uiDataSync.writePins(states);
            }
            else
            {
                await this.gpioController.writePins(states);
            }
        }

        /// <summary>
        /// Use this method to test whether communication with the add-on card works.
        /// </summary>
        /// <returns>Some information retrieved from the add-on card.</returns>
        public async Task<string> testAddOnCardCommunication()
        {
            if (this.runOnSimulator)
            {
                throw new InvalidOperationException();
            }
            else
            {
                return await this.gertbotGeneral.getVersion();
            }
        }

        /// <summary>
        /// Gets error status information.
        /// </summary>
        /// <returns>Error status.</returns>
        public async Task<string> readAddOnCardErrorStatus()
        {
            if (this.runOnSimulator)
            {
                throw new InvalidOperationException();
            }
            else
            {
                return await this.gertbotGeneral.readErrorStatus();
            }
        }
    }
}
