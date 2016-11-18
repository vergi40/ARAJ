// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 10/2016
// Last modified: 11/2016

using System;
using System.Collections.Generic;

namespace Tut.Ase.TraxsterRobotApp
{
    /// <summary>
    /// Interface to sync data with a UI.
    /// </summary>
    public interface IUiDataSync
    {
        /// <summary>
        /// Sets motor speed.
        /// </summary>
        /// <param name="left">Left motor speed.</param>
        /// <param name="right">Right motor speed.</param>
        void setMotorSpeed(int left, int right);

        /// <summary>
        /// Gets a sensor value.
        /// </summary>
        /// <param name="id">Sensor ID.</param>
        /// <returns>Sensor value.</returns>
        int getSensorValue(int id);

        /// <summary>
        /// Reads pin states.
        /// </summary>
        /// <returns>Pin states ordered after pin ID.</returns>
        bool[] readPins();

        /// <summary>
        /// Writes the states of pins.
        /// </summary>
        /// <param name="states">Pin states. Array length must be 8!</param>
        void writePins(bool[] states);

        /// <summary>
        /// Sets simulator mode.
        /// </summary>
        /// <param name="enabled">True if simulator mode is enabled. Otherwise, false.</param>
        void setSimulatorMode(bool enabled);
    }
}
