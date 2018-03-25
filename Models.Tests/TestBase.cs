using System;
using System.Linq;
using AquariumMonitor.Models.Validation;
using AquariumMonitor.APIModels;
using Xunit;

namespace Models.Tests
{
    public abstract class TestBase
    {
        protected IAPIModel item;

        protected void CheckResult(object objectToValidate, bool isValid, string errorMessage)
        {
            if (isValid)
                CheckNullResult(objectToValidate, errorMessage);
            else
                CheckNotNullResult(objectToValidate, errorMessage);
        }

        protected void CheckNotNullResult(object objectToValidate, string errorMessage)
        {
            var results = EntityValidator.Validate(objectToValidate);
            Assert.NotNull(results.SingleOrDefault(r => r.ErrorMessage == errorMessage));
        }

        protected void CheckNullResult(object objectToValidate, string errorMessage)
        {
            var results = EntityValidator.Validate(objectToValidate);
            Assert.Null(results.SingleOrDefault(r => r.ErrorMessage == errorMessage));
        }

        protected string GenerateString(int length)
        {
            string baseWord = "UNICORN";

            return string.Join("", Enumerable.Repeat(baseWord, (int)Math.Floor((decimal)length / baseWord.Length) + 1).ToArray())
                .Substring(0, length);
        }
    }
}
