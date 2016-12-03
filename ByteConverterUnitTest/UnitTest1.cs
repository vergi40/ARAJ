// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 11/2016
// Last modified: 11/2016

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tut.Ase.TraxsterRobotApp;

namespace ByteConverterUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void test_intToHexBytes()
        {
            var typicalCaseExpected = new List<byte>() { 0x02, 0x0A };
            var largeIntExpected1 = new List<byte>() { 0x01, 0x86, 0xA0 };
            var largeIntExpected2 = new List<byte>() { 0x27, 0x10 };
            var addZerosExpected = new List<byte>() { 0x00, 0x00, 0x0C };
            var zeroValueExpected = new List<byte>() { 0x00 };
            
            // Running positive tests:
            // - typical case
            // - adding zeros to front
            // - zero value
            assertByteList(typicalCaseExpected, ByteConverter.intToHexBytes(522, 2));
            assertByteList(largeIntExpected1, ByteConverter.intToHexBytes(100000, 3));
            assertByteList(largeIntExpected2, ByteConverter.intToHexBytes(10000, 2));
            assertByteList(addZerosExpected, ByteConverter.intToHexBytes(12, 3));
            assertByteList(zeroValueExpected, ByteConverter.intToHexBytes(0, 1));

            // Running error tests:
            // - negative input value
            // - non-positive byte count given
            // - input does not fit to given byte count
            bool exThrownNegativeValue = false;
            bool exThrownNonPositiveCount = false;
            bool exThrownInputDoesNotFit = false;

            try
            {
                ByteConverter.intToHexBytes(-1, 2);
            }
            catch
            {
                exThrownNegativeValue = true;
            }

            try
            {
                ByteConverter.intToHexBytes(2, 0);
            }
            catch
            {
                exThrownNonPositiveCount = true;
            }

            try
            {
                ByteConverter.intToHexBytes(256, 1);
            }
            catch
            {
                exThrownInputDoesNotFit = true;
            }

            Assert.IsTrue(exThrownNegativeValue);
            Assert.IsTrue(exThrownNonPositiveCount);
            Assert.IsTrue(exThrownInputDoesNotFit);
        }

        [TestMethod]
        public void test_hexBytesToInt()
        {
            var zeroInput = new List<byte>() { 0x00, 0x00 };
            var leadingZerosInput = new List<byte>() { 0x00, 0x00, 0xC2, 0x03 };
            var noLeadingZerosSingleByteInput = new List<byte>() { 0xF1 };

            var emptyInput = new List<byte>();
            var tooManyBytesInput = new List<byte>() { 0x50, 0x00, 0x00, 0x00, 0x00 };
            
            // Positive tests:
            // - zero input
            // - typical input; leading zeros
            // - typical input; no leading zeros
            Assert.AreEqual<int>(0, ByteConverter.hexBytesToInt(zeroInput));
            Assert.AreEqual<int>(49667, ByteConverter.hexBytesToInt(leadingZerosInput));
            Assert.AreEqual<int>(241, ByteConverter.hexBytesToInt(noLeadingZerosSingleByteInput));

            // Error tests:
            // - empty input (no items)
            // - too many bytes supplied
            bool exThrownEmptyInput = false;
            bool exThrownTooManyBytesInInput = false;

            try
            {
                ByteConverter.hexBytesToInt(emptyInput);
            }
            catch
            {
                exThrownEmptyInput = true;
            }

            try
            {
                ByteConverter.hexBytesToInt(tooManyBytesInput);
            }
            catch
            {
                exThrownTooManyBytesInInput = true;
            }

            Assert.IsTrue(exThrownEmptyInput);
            Assert.IsTrue(exThrownTooManyBytesInInput);
        }

        private void assertByteList(List<byte> expected, List<byte> actual)
        {
            Assert.AreEqual<int>(expected.Count, actual.Count, "List count");

            for (int a = 0; a < expected.Count; ++a)
            {
                Assert.AreEqual<byte>(expected[a], actual[a], "List item");
            }
        }
    }
}
