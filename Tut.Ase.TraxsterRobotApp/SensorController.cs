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
    /// Controls sensor hardware.
    /// </summary>
    class SensorController
    {
        private readonly GertbotUartController gertController;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gert">Add-on card controller.</param>
        public SensorController(GertbotUartController gert)
        {
            this.gertController = gert;
        }
        
        /// <summary>
        /// Gets a raw sensor value.
        /// </summary>
        /// <param name="sensorId">Sensor ID.</param>
        /// <returns>Raw sensor value.</returns>
        public async Task<int> getSensorValue(int sensorId)
        {
            // Validating sensor ID
            switch (sensorId)
            {
                case DeviceConstants.FRONT_SENSOR_ID:
                case DeviceConstants.LEFT_SENSOR_ID:
                case DeviceConstants.RIGHT_SENSOR_ID:
                case DeviceConstants.REAR_SENSOR_ID:
                    break;

                default:
                    throw new ArgumentException("Unknown sensor ID " + sensorId);
            }
            
            // 4.13  Read ADC 
            // (Gertbot doc version 2.4 has an error here: claims that command ID is 0x0C)
            // 0xA0 0x0D <id> <MS> <LS> 0x50 0x50 0x50 0x50

            // The documentation does not tell what the ID should be.
            // In Gertbot C drivers, ADC ID is generated with:
            //
            // id = (board<<2) | adc;
            //
            // where "adc" is 0..3.
            // As board ID is presumably 0 here, just using the ADC ID.

            const byte COMMAND_ID = 0x0D;
            var sensorIdByte = (byte)sensorId;

            var input = new List<byte>()
            {
                COMMAND_ID,
                sensorIdByte,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE,
                GertbotUartController.END_BYTE,
            };

            // Return value according to Gertbot doc:
            // The first byte is the original id. 
            // The second byte is 0x0C
            // Bytes three and four are the MS & LS value of the ADC.  
            
            var adcReturnValue = await this.gertController.readItem(input);

            if (adcReturnValue.Count != 4)
            {
                throw new Exception("Unexpected sensor value output size from Gertbot");
            }
            if (adcReturnValue[0] != sensorIdByte)
            {
                throw new Exception("Unexpected sensor ID in sensor value output from Gertbot");
            }
            if (adcReturnValue[1] != COMMAND_ID)
            {
                throw new Exception("Unexpected command ID in sensor value output from Gertbot");
            }

            // Converting ADC output bytes to an integer
            var conversionInput = new List<byte> { adcReturnValue[2], adcReturnValue[3] };
            return ByteConverter.hexBytesToInt(conversionInput);
        }
    }
}
