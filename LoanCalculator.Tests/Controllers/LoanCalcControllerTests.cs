using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoanCalculator.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanCalculator.Controllers.Tests
{
    [TestClass()]
    public class LoanCalcControllerTests
    {
        [TestMethod()]
        public void CalculatePaymentAmountTest()
        {
            // Arrange
            var principal = 20000f;
            var numberOfPayments = 60;
            var interestRateYearly = 0.00625f;

            // Act
            var paymentAmount = LoanCalcController.CalculatePaymentAmount(principal, numberOfPayments, interestRateYearly);

            // Assert
            Assert.AreEqual(400.76d, paymentAmount, 0.01d);
        }
    }
}