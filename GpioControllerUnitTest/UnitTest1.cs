// Petri Kannisto
// Tampere University of Technology
// Department of Automation Science and Engineering
// File created: 11/2016
// Last modified: 11/2016

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tut.Ase.TraxsterRobotApp;

namespace GpioControllerUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async System.Threading.Tasks.Task readPins1()
        {
            // Test how byte is converted to bool array

            byte dataToReturn = 0x28; // 0010 1000
            var objectUnderTest = new GpioController(new GertbotUartController(dataToReturn), new MotorController());

            var retval = await objectUnderTest.readPins();
            
            Assert.AreEqual<int>(8, retval.Length);

            Assert.IsFalse(retval[0]);
            Assert.IsFalse(retval[1]);
            Assert.IsFalse(retval[2]);
            Assert.IsTrue(retval[3]);

            Assert.IsFalse(retval[4]);
            Assert.IsTrue(retval[5]);
            Assert.IsFalse(retval[6]);
            Assert.IsFalse(retval[7]);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task readPins2()
        {
            // Test how byte is converted to bool array

            byte dataToReturn = 0xD7; // 1101 0111
            var objectUnderTest = new GpioController(new GertbotUartController(dataToReturn), new MotorController());

            var retval = await objectUnderTest.readPins();

            Assert.AreEqual<int>(8, retval.Length);

            Assert.IsTrue(retval[0]);
            Assert.IsTrue(retval[1]);
            Assert.IsTrue(retval[2]);
            Assert.IsFalse(retval[3]);

            Assert.IsTrue(retval[4]);
            Assert.IsFalse(retval[5]);
            Assert.IsTrue(retval[6]);
            Assert.IsTrue(retval[7]);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task writePins1()
        {
            // Test how bool array is converted to a byte

            var input = new bool[]
            {
                // LS: 1101 = 13 = D
                true,
                false,
                true,
                true,

                // MS: 1000 = 8
                false,
                false,
                false,
                true
            };

            var uartController = new GertbotUartController(0x00);
            var objectUnderTest = new GpioController(uartController, new MotorController());

            await objectUnderTest.writePins(input);

            Assert.AreEqual<byte>(0x8D, uartController.LatestPinBitsSent);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task writePins2()
        {
            // Test how bool array is converted to a byte

            var input = new bool[]
            {
                // LS: 0010 = 2
                false,
                true,
                false,
                false,

                // MS: 0111 = 7
                true,
                true,
                true,
                false
            };

            var uartController = new GertbotUartController(0x00);
            var objectUnderTest = new GpioController(uartController, new MotorController());

            await objectUnderTest.writePins(input);

            Assert.AreEqual<byte>(0x72, uartController.LatestPinBitsSent);
        }
    }
}
