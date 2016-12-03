// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 11/2016
// Last modified: 11/2016

using System;
using System.Collections.Generic;

namespace Tut.Ase.TraxsterRobotApp
{
    /// <summary>
    /// Stub class for testing.
    /// </summary>
    class GertbotUartController
    {
        public const byte END_BYTE = 0xFF;

        private readonly byte dataToReturnInRead;
        private byte latestPinBitsSent = 0x00;


        public GertbotUartController(byte dataToReturn)
        {
            this.dataToReturnInRead = dataToReturn;
        }


        public async System.Threading.Tasks.Task sendCommand(List<byte> input)
        {
            // Method must be async as the "real" method async in the application

            if (input.Count > 4)
            {
                // This is the payload byte when writing pins
                this.latestPinBitsSent = input[4];
            }
        }

        public async System.Threading.Tasks.Task<List<byte>> readItem(object a)
        {
            // Method must be async as the "real" method async in the application

            return new List<byte>()
            {
                0x00,
                0x0E, // Command ID
                0x00,
                0x00,
                this.dataToReturnInRead
            };
        }

        public byte LatestPinBitsSent // This property exists just for unit testing
        {
            get { return this.latestPinBitsSent; }
        }
    }

    /// <summary>
    /// Stub class for testing.
    /// </summary>
    class MotorController
    {
        public async System.Threading.Tasks.Task disableEndstopAndShortHot()
        {
            // Stub method; must be async as the "real" method async in the application
        }
    }
}
