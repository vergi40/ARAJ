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
    /// Controls motor hardware.
    /// </summary>
    class MotorController
    {
        private const byte LEFT_MOTOR_ID = 0x00;
        private const byte RIGHT_MOTOR_ID = 0x01;

        // This motor does not exist but some related pins must be set up for GPIO
        private const byte MOTOR_ID_NON_EXISTING_MOTOR_2 = 0x02;

        private readonly GertbotUartController gertController;

        private bool initDone = false;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gert">Add-on card controller.</param>
        public MotorController(GertbotUartController gert)
        {
            this.gertController = gert;
        }

        /// <summary>
        /// Sets motor speeds (-100..100).
        /// </summary>
        /// <param name="leftMotorSpeed">Left motor speed.</param>
        /// <param name="rightMotorSpeed">Right motor speed.</param>
        public async Task setMotorSpeed(int leftMotorSpeed, int rightMotorSpeed)
        {
            if (!this.initDone)
            {
                await init();
                this.initDone = true;
            }

            // Applying motor speeds by giving start commands;
            // Setting motor speeds

            await setSpeed(LEFT_MOTOR_ID, processLeftMotorDutyCycle(leftMotorSpeed));
            await applySpeed(LEFT_MOTOR_ID, leftMotorSpeed);
            
            await setSpeed(RIGHT_MOTOR_ID, processRightMotorDutyCycle(rightMotorSpeed));
            await applySpeed(RIGHT_MOTOR_ID, rightMotorSpeed);

            // TODO-later: use sync command for motors to apply speeds them (almost) simultaneously
        }
        
        /// <summary>
        /// Disables end-stops and short-hot on some pins to enable their usage as GPIO.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task disableEndstopAndShortHot()
        {
            // Enable using EXT0 and EXT1 as GPIO pins
            var task1 = do_disableEndstopAndShortHot(LEFT_MOTOR_ID);
            
            // Enable using EXT4 and EXT5 as GPIO pins
            var task2 = do_disableEndstopAndShortHot(MOTOR_ID_NON_EXISTING_MOTOR_2);

            await task1;
            await task2;
        }

        private async Task do_disableEndstopAndShortHot(byte motorId)
        {
            // 4.3  End - stop & short / hot set up
            // 0xA0 0x02 <id> <end-stop & short/hot mode> 0x50

            var input = new List<byte>()
            {
                0x02,
                motorId,
                0x00, // Disable end-stop A and B
                0x00 // Not short/hot error propagation
            };

            await this.gertController.sendCommand(input);
        }

        private async Task init()
        {
            // Setting motor operation modes
            var leftTask = setOperationMode(LEFT_MOTOR_ID);
            var rightTask = setOperationMode(RIGHT_MOTOR_ID);
            await leftTask;
            await rightTask;

            // This is performed to enable some pins to be utilised as GPIO
            await disableEndstopAndShortHot();

            // Setting PWM frequency
            leftTask = setPwmFrequency(LEFT_MOTOR_ID);
            rightTask = setPwmFrequency(RIGHT_MOTOR_ID);
            await leftTask;
            await rightTask;
            
            // TODO-later: Enable sync mode
        }

        private async Task setOperationMode(byte motorId)
        {
            // 4.2 Operation mode
            // 0xA0 0x01 <id> <mode> 0x50
            // - id: motor ID
            // - mode: 0x01: brushed mode
            
            var input = new List<byte>()
            {
                0x01,
                motorId,
                0x01
            };
            
            await this.gertController.sendCommand(input);
        }
        
        private async Task setPwmFrequency(byte motorId)
        {
            // 4.4 DC/Brushed Pulse Width Modulation Motor Frequency
            // 0xA0 0x04 <id> <MS> <LS> 0x50
            // - id: motor ID
            // - MS: most significant
            // - LS: least significant
            
            //var frequencyBytes = ByteConverter.intToHexBytes(19000, 2); // 19 kHz
            var frequencyBytes = ByteConverter.intToHexBytes(10000, 2); // 10 kHz
            //var frequencyBytes = ByteConverter.intToHexBytes(5000, 2); // 5 kHz
            //var frequencyBytes = ByteConverter.intToHexBytes(1000, 2); // 1 kHz
            //var frequencyBytes = ByteConverter.intToHexBytes(100, 2); // 100 Hz

            var input = new List<byte>()
            {
                0x04,
                motorId,
                frequencyBytes[0],
                frequencyBytes[1]
            };
            
            await this.gertController.sendCommand(input);
        }

        private async Task setSpeed(byte motorId, int value)
        {
            // Setting motor speed by setting its duty cycle

            // 4.5 Brushed Motor Duty Cycle
            // 0xA0 0x05 <id> <MS> <LS> 0x50
            // - id: motor ID
            // - MS: most significant part of speed value
            // - LS: least significant part of speed value
            
            // Converting the duty cycle value to bytes
            var inputAsBytes = ByteConverter.intToHexBytes(value, 2);
            
            var input = new List<byte>()
            {
                0x05,
                motorId,
                inputAsBytes[0],
                inputAsBytes[1]
            };
            
            await this.gertController.sendCommand(input);
        }

        private async Task applySpeed(byte motorId, int speed)
        {
            // Applying motor speed by giving a start command

            // 4.6 Start/stop Brushed Motor
            // 0xA0 0x06 <id> <mode> 0x50
            // - id: motor ID
            // - mode: ramp + direction
            //
            // Start board 0 motor 1 in direction A, ramp-up in 1 second.
            // 0xA0 Start command
            // 0x06 Command ID
            // 0x01 Motor ID
            // 0x71 Direction+ramp
            // - 7: actually 1,5 sec?
            // - 1: direction A?
            // 0x50 End command

            // Taking motor direction from the value sign
            byte direction = 0x00;

            if (speed == 0)
            {
                // Stop
                direction = 0x00;
            }
            else if (speed > 0)
            {
                // Forward
                direction = 0x01;
            }
            else
            {
                // Back
                direction = 0x02;
            }

            // Combining direction and ramp values into a single byte
            //byte ramp = 0x30; // 0.5 second ramp expected
            byte ramp = 0x20; // 0.25 second ramp expected
            byte rampAndDirection = (byte)(ramp + direction);

            // Giving a start command to apply motor speed
            var input = new List<byte>()
            {
                0x06,
                motorId,
                rampAndDirection
            };

            await this.gertController.sendCommand(input);
        }

        private int processLeftMotorDutyCycle(int percentage)
        {
            // **********************************************************
            // The motors are asymmetric! As there is no speed feedback,
            // only voltage is used to control the motors.
            // To come along with asymmetric motors, both the motors have
            // their own speed scaling formula.
            // **********************************************************

            // Using absolute value
            var percentageAbs = Math.Abs(percentage);

            // Cannot be more than 100%
            percentageAbs = percentageAbs > 100 ?
                100 :
                percentageAbs;

            // Experimental reasonable limits were (11/2016):
            // min: 35% (if less, the motor might not start)
            // max: 45% (if more, very fast motor speed)
            // 
            // As the min and max values for the other motor were applied, the robot
            // went quite straight.
            //
            // -> d(s) = 0.1s + 35

            // This was originally good:
            //var actualPercentage = (int)(0.1 * percentageAbs + 35);

            // This is now tried as left did not start:
            var actualPercentage = (int)(0.1 * percentageAbs + 40);

            // Also, the duty cycle is expented per mille.
            return actualPercentage * 10;
        }

        private int processRightMotorDutyCycle(int percentage)
        {
            // **********************************************************
            // The motors are asymmetric! As there is no speed feedback,
            // only voltage is used to control the motors.
            // To come along with asymmetric motors, both the motors have
            // their own speed scaling formula.
            // **********************************************************

            // Using absolute value
            var percentageAbs = Math.Abs(percentage);

            // Cannot be more than 100%
            percentageAbs = percentageAbs > 100 ?
                100 :
                percentageAbs;

            // Experimental reasonable limits were (11/2016):
            // min: 25% (if less, the motor might not start)
            // max: 40% (if more, very fast motor speed)
            //
            // As the min and max values for the other motor were applied, the robot
            // went quite straight.
            //
            // -> d(s) = 0.15s + 25

            var actualPercentage = (int)(0.15 * percentageAbs + 25);

            // Also, the duty cycle is expented per mille.
            return actualPercentage * 10;
        }
    }
}
