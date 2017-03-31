using ACKinshipFormatter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ACKinshipFormatterTest
{
    [TestClass]
    public class KinshipFormatterTest
    {
        public TestContext TestContext { get; set; }
        private KinshipFormatter formatter = new KinshipFormatter();

        [TestMethod]
        public void TestKinship_KinshipRule()
        {
            var expression = "KinshipRule::Expression";
            var result = "KinshipRule::Result";

            var kinshipRule = new KinshipRule(expression, result);
            Assert.AreEqual(expression, kinshipRule.Expression);
            Assert.AreEqual(result, kinshipRule.Result);
        }

        [TestMethod]
        public void TestKinship_KinshipTerm()
        {
            var term = "KinshipTerm::Term";
            var result = "KinshipTerm::Result";

            var kinshipRule = new KinshipTerm(term, result);
            Assert.AreEqual(term, kinshipRule.Term);
            Assert.AreEqual(result, kinshipRule.Result);
        }

        [TestMethod]
        public void TestKinship_NullOrEmpty()
        {
            var actual = formatter.Format(null);
            Assert.AreEqual("unrelated", actual);

            actual = formatter.Format("");
            Assert.AreEqual("unrelated", actual);
        }

        [TestMethod]
        public void TestKinship_NotKinship()
        {
            var actual = formatter.Format("12345");
            Assert.AreEqual("unrelated", actual);

            actual = formatter.Format("ABCDE");
            Assert.AreEqual("unrelated", actual);

            actual = formatter.Format("MM00FF");
            Assert.AreEqual("unrelated", actual);
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\KinshipFormatterTestData.csv", "KinshipFormatterTestData#csv", DataAccessMethod.Sequential)]
        [TestMethod]
        public void TestKinship()
        {
            var term = TestContext.DataRow["Term"] as string;
            var expected = TestContext.DataRow["Expected"] as string;

            if (expected != null)
            {
                var actual = formatter.Format(term);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
