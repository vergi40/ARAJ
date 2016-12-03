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
    /// Uses GPIO pins.
    /// </summary>
    class GpioController
    {
        private const byte BOARD_ID = 0x00;

        private readonly GertbotUartController gertController;
        private readonly MotorController motorController;

        // To keep the program structure easy to understand, semaphores are used
        // in each public method (excluding Dispose) and not anywhere else.
        private System.Threading.SemaphoreSlim semaphore;

        private bool initDone = false;
        private byte currentOutputPinStatesByte = 0x00;

        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gert">Add-on card controller.</param>
        /// <param name="mc">Motor controller.</param>
        public GpioController(GertbotUartController gert, MotorController mc)
        {
            this.gertController = gert;
            this.motorController = mc;

            this.semaphore = new System.Threading.SemaphoreSlim(
                1, // Initial count > 0: the semaphore is initially free
                1  // Max count: only one thread can enter at a time
                );
        }

        /// <summary>
        /// Writes the states of pins.
        /// </summary>
        /// <param name="states">Pin states. Array length must be 8!</param>
        /// <returns>Task.</returns>
        public async Task writePins(bool[] states)
        {
            // Getting the byte to change pin states
            var newOutputPinStatesByte = buildPinStateSetterByte(states);

            // Mutual exclusion is here to prevent concurrency problems
            await this.semaphore.WaitAsync();

            try
            {
                if (!initDone)
                {
                    await init();
                    this.initDone = true;
                }

                // 4.15  Write I/O
                // 0xA0 0x0F <id> <MS> <MM> <LS> 0x50

                var input = new List<byte>()
                {
                    0x0F,
                    BOARD_ID,
                    0x00, // MS; unused in current Gertbot
                    0x00, // MM; not important here
                    newOutputPinStatesByte // LS
                };

                await this.gertController.sendCommand(input);

                this.currentOutputPinStatesByte = newOutputPinStatesByte;
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        /// <summary>
        /// Reads the states of pins.
        /// </summary>
        /// <returns>Pin states ordered after pin ID.</returns>
        public async Task<bool[]> readPins()
        {
            const byte COMMAND_ID = 0x0E;
            
            // Mutual exclusion is here to prevent concurrency problems
            await this.semaphore.WaitAsync();

            try
            {
                if (!this.initDone)
                {
                    await init();
                    this.initDone = true;
                }
            }
            finally
            {
                this.semaphore.Release();
            }

            // Read operations are stateless in this class so no semaphore utilised after this point

            // 4.14  Read I/O
            // 0xA0 0x0E <id> 0x50 0x50 0x50 0x50 0x50

            var input = new List<byte>()
            {
                COMMAND_ID,
                BOARD_ID,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE
            };

            // Gert doc for the return value:
            // The first byte is the original id. 
            // The second byte is 0x0E
            // Bytes three, four and five are the MS, MM & LS values read.
            var pinStates = await this.gertController.readItem(input);

            if (pinStates.Count != 5)
            {
                throw new Exception("Unexpected pin state output size from Gertbot");
            }
            if (pinStates[1] != COMMAND_ID)
            {
                throw new Exception("Invalid pin state output from Gertbot");
            }
            
            var byteOfInterest = pinStates[4];

            var returnValue = new bool[]
            {
                false, false, false, false,
                false, false, false, false // 8 items in total
            };

            // Getting pin states
            returnValue[0] = ((byteOfInterest & 0x01) == 0x01); // EXT0: bit 0 of 0..7 -> 0x01 = 0000 0001
            returnValue[1] = ((byteOfInterest & 0x02) == 0x02); // EXT1: bit 1 of 0..7 -> 0x02 = 0000 0010
            returnValue[2] = ((byteOfInterest & 0x04) == 0x04); // EXT2: bit 2 of 0..7 -> 0x04 = 0000 0100
            returnValue[3] = ((byteOfInterest & 0x08) == 0x08); // EXT3: bit 3 of 0..7 -> 0x08 = 0000 1000
            returnValue[4] = ((byteOfInterest & 0x10) == 0x10); // EXT4: bit 4 of 0..7 -> 0x10 = 0001 0000
            returnValue[5] = ((byteOfInterest & 0x20) == 0x20); // EXT5: bit 5 of 0..7 -> 0x20 = 0010 0000
            returnValue[6] = ((byteOfInterest & 0x40) == 0x40); // EXT6: bit 6 of 0..7 -> 0x40 = 0100 0000
            returnValue[7] = ((byteOfInterest & 0x80) == 0x80); // EXT7: bit 7 of 0..7 -> 0x80 = 1000 0000

            return returnValue;
        }
        
        private byte buildPinStateSetterByte(bool[] states)
        {
            const int ARRAY_LENGTH = 8;

            if (states.Length != ARRAY_LENGTH)
            {
                throw new ArgumentException("When writing pins, array length must be " + ARRAY_LENGTH);
            }

            // Summing byte components together
            var totalByte =
                  (states[0] ? 0x01 : 0x00)  // 0000 0001
                + (states[1] ? 0x02 : 0x00)  // 0000 0010
                + (states[2] ? 0x04 : 0x00)  // 0000 0100
                + (states[3] ? 0x08 : 0x00)  // 0000 1000
                + (states[4] ? 0x10 : 0x00)  // 0001 0000
                + (states[5] ? 0x20 : 0x00)  // 0010 0000
                + (states[6] ? 0x40 : 0x00)  // 0100 0000
                + (states[7] ? 0x80 : 0x00); // 1000 0000

            return (byte)totalByte;
        }
        
        private async Task init()
        {
            // To use Gert J3 header pins as GPIO, the special functions
            // on those pins must be disabled!
            // See disabling end-stop in MotorController class.
            await this.motorController.disableEndstopAndShortHot();

            // Setting GPIO pins as inputs or outputs
            //
            // 4.16  Set I/O
            // 0xA0 0x10 <id> <MS> <MM> <LS> 0x50
            
            var input = new List<byte>()
            {
                0x10,
                BOARD_ID,
                0x00, // MS; unused in current Gertbot
                0x00, // MM; not important
                0x30 // LS; 0011 0000: 0 means output, 1 means input
                // The last 8 bits are mapped as follows:
                // EXT7 EXT6 EXT5 EXT4; EXT3 EXT2 EXT1 EXT0
            };

            await this.gertController.sendCommand(input);
        }
    }
}
