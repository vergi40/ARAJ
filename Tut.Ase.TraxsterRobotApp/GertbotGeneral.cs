// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 11/2016
// Last modified: 11/2016

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tut.Ase.TraxsterRobotApp
{
    /// <summary>
    /// Provides general Gertbot operations.
    /// </summary>
    class GertbotGeneral
    {
        private const byte COMMAND__READ_ERROR_STATUS = 0x07;
        private const byte COMMAND__READ_VERSION = 0x13;
        private const byte BOARD_ID = 0x00;

        private readonly GertbotUartController gertController;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gert">Gert controller.</param>
        public GertbotGeneral(GertbotUartController gert)
        {
            this.gertController = gert;
        }

        /// <summary>
        /// Gets version information.
        /// </summary>
        /// <returns>Version info.</returns>
        public async Task<string> getVersion()
        {
            // 4.1 Read version
            // 0xA0 0x13 <ID> 0x50 0x50 0x50 0x50

            var request = new List<byte>()
            {
                COMMAND__READ_VERSION,
                BOARD_ID,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE
            };
            var versionRaw = await this.gertController.readItem(request);

            var stringBuilder = new System.Text.StringBuilder();

            foreach (var item in versionRaw)
            {
                stringBuilder.Append(item);
                stringBuilder.Append(";");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Reads error status.
        /// </summary>
        /// <returns>Error status.</returns>
        public async Task<string> readErrorStatus()
        {
            // 4.7 Read error status
            // 0xA0 0x07 <id> 0x50 0x50 0x50 0x50

            var request = new List<byte>()
            {
                COMMAND__READ_ERROR_STATUS,
                BOARD_ID,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE
            };
            var versionRaw = await this.gertController.readItem(request);

            var stringBuilder = new System.Text.StringBuilder();

            foreach (var item in versionRaw)
            {
                stringBuilder.Append(item);
                stringBuilder.Append(";");
            }

            return stringBuilder.ToString();
        }
    }
}
