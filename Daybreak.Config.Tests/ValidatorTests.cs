using NUnit.Framework;

namespace Daybreak.Config.Tests
{
    [TestFixture]
    public class ValidatorTests
    {
        private class TestConfig : BaseConfig
        {
            [CfgField] [Validation.LengthBetween(4, 8)]
            public string LengthString = "12345";

            [CfgField] [Validation.Between(0, 10)] public int ClampedInt = 1;
        }

        [Test]
        public void InvalidValuesFail()
        {
            var cfg = new TestConfig();
            Assert.Throws<Exceptions.InvalidValueException>(() => cfg.LoadFromString(@"
length_string = ""123"""));
            Assert.Throws<Exceptions.InvalidValueException>(() => cfg.LoadFromString(@"
clamped_int = 10"));
        }

        [Test]
        public void ValidValuesPass()
        {
            var cfg = new TestConfig();
            Assert.DoesNotThrow(() => cfg.LoadFromString(@"
length_string = ""123456"""));
            Assert.DoesNotThrow(() => cfg.LoadFromString(@"
clamped_int = 2"));
            Assert.AreEqual("123456", cfg.LengthString);
            Assert.AreEqual(2, cfg.ClampedInt);
        }
    }
}