using NUnit.Framework;
using Daybreak.Config;

namespace Daybreak.Config.Tests
{
    [TestFixture]
    public class Tests
    {
        private class TestSection
        {
            [CfgField] public bool TestBoolean = false;
            [CfgField] public string TestString = "test string";
            [CfgField] public int TestInt = -1;
        }

        private class TestConfig : BaseConfig
        {
            [CfgField("test_value")] public int RemappedValue = -1;
            [CfgSection] public TestSection TestSection = new TestSection();
        }

        [Test]
        public void ValuesAreSet()
        {
            var cfg = new TestConfig();
            cfg.LoadFromString(@"
[test_section]
test_boolean = true
test_string = ""testier string""
test_int = 21");
            Assert.AreEqual(cfg.TestSection.TestBoolean, true);
            Assert.AreEqual(cfg.TestSection.TestString, "testier string");
            Assert.AreEqual(cfg.TestSection.TestInt, 21);
        }

        [Test]
        public void RemappedValue_Parsed()
        {
            var cfg = new TestConfig();
            Assert.AreEqual(-1, cfg.RemappedValue);

            cfg.LoadFromString(@"test_value = 42");
            Assert.AreEqual(42, cfg.RemappedValue);
        }

        [Test]
        public void HandlesInvalidValues()
        {
            var cfg = new TestConfig();
            Assert.Throws<Exceptions.InvalidValueException>(() => cfg.LoadFromString(@"
[test_section]
test_boolean = invalid"));
        }
    }
}