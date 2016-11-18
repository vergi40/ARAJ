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
    /// Performs byte value conversion.
    /// </summary>
    static class ByteConverter
    {
        /// <summary>
        /// Converts an integer to a hex byte sequence.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="size">Expected output item count. If the given integer does not
        /// require that many bytes to be utilised, the first bytes will only contain zeros.</param>
        /// <returns>Hex byte value. Most significant bytes will be in the beginning.</returns>
        public static List<byte> intToHexBytes(int input, int size)
        {
            const int EXPECTED_BITCONVERTER_OUTPUT = 4;

            if (input < 0)
            {
                throw new ArgumentException("intToHexBytes: Only non-negative input supported");
            }
            if (size < 1)
            {
                throw new ArgumentException("intToHexBytes: Size must be positive");
            }
            if (size > EXPECTED_BITCONVERTER_OUTPUT)
            {
                throw new ArgumentException("intToHexBytes: Size cannot exceed " + EXPECTED_BITCONVERTER_OUTPUT);
            }

            // Getting bytes; this will return 4 bytes due to a 32 bit number
            var bytes = BitConverter.GetBytes(input);

            if (bytes.Length != EXPECTED_BITCONVERTER_OUTPUT)
            {
                throw new Exception("intToHexBytes: unexpected output length from BitConverter: " + bytes.Length);
            }

            // Converting to a list and adding zeros to start if required
            var retval = new List<byte>();

            // This is how many leading zeros will be skipped
            var mostSignificantBytesToSkip = EXPECTED_BITCONVERTER_OUTPUT - size;

            // Taking each byte from the conversion output
            for (int a = 0; a < bytes.Length; ++a)
            {
                // For little endian processing, reversing the byte order as Gert wants most significant bytes first
                int indexForGet = BitConverter.IsLittleEndian ?
                    bytes.Length - a - 1 : // Little endian: start from array end
                    a; // Big endian: start from array beginning

                if (a < mostSignificantBytesToSkip)
                {
                    if (bytes[indexForGet] != 0x00)
                    {
                        throw new ArgumentException("intToHexBytes: Count of significant bytes exceeds given size");
                    }
                    else
                    {
                        // Skip zero byte
                        continue;
                    }
                }

                retval.Add(bytes[indexForGet]);
            }

            return retval;
        }

        /// <summary>
        /// Converts bytes to the corresponding integer.
        /// </summary>
        /// <param name="bytes">Input; most significant bytes first.</param>
        /// <returns>Bytes as int.</returns>
        public static int hexBytesToInt(List<byte> bytes)
        {
            const int EXPECTED_BITCONVERTER_INPUT = 4;

            if (bytes.Count < 1)
            {
                throw new ArgumentException("hexBytesToInt: Invalid empty input");
            }
            if (bytes.Count > EXPECTED_BITCONVERTER_INPUT)
            {
                throw new ArgumentException("hexBytesToInt: Count of bytes exceeds maximum for Int32");
            }

            var conversionInput = new byte[EXPECTED_BITCONVERTER_INPUT];

            var leadingZerosToBeAdded = EXPECTED_BITCONVERTER_INPUT - bytes.Count;

            // The loop will start from the most significant byte of "bytes" parameter
            for (int a = 0; a < EXPECTED_BITCONVERTER_INPUT; ++a)
            {
                // In little endian, the input expects least significant bytes first
                var indexForAssignment = BitConverter.IsLittleEndian ?
                    EXPECTED_BITCONVERTER_INPUT - a - 1 :
                    a;

                // Adding leading zeros if required
                if (a < leadingZerosToBeAdded)
                {
                    conversionInput[indexForAssignment] = 0x00;
                    continue;
                }

                // Expecting most significant bytes first
                var indexForGet = a - leadingZerosToBeAdded;

                conversionInput[indexForAssignment] = bytes[indexForGet];
            }

            return BitConverter.ToInt32(conversionInput, 0);
        }
    }
}
