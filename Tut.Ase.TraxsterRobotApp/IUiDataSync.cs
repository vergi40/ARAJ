// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 10/2016
// Last modified: 10/2016

using System;

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
        double getSensorValue(int id);

        /// <summary>
        /// Reads a pin state.
        /// </summary>
        /// <param name="id">Pin ID.</param>
        /// <returns>State.</returns>
        bool readPin(int id);

        /// <summary>
        /// Writes a pin state.
        /// </summary>
        /// <param name="id">Pin ID.</param>
        /// <param name="state">State.</param>
        void writePin(int id, bool state);

        /// <summary>
        /// Sets simulator mode.
        /// </summary>
        /// <param name="enabled">True if simulator mode is enabled. Otherwise, false.</param>
        void setSimulatorMode(bool enabled);
    }
}
