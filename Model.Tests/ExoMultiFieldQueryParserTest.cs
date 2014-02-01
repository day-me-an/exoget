using Exo.Exoget.Model.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lucene.Net.Analysis;
using Lucene.Net.Search;

namespace Exo.Exoget.Model.Tests
{
    
    
    /// <summary>
    ///This is a test class for ExoMultiFieldQueryParserTest and is intended
    ///to contain all ExoMultiFieldQueryParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ExoMultiFieldQueryParserTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void Parse_Boolean()
        {
            string[] fields = { "foo"};
            Analyzer analyzer = new ExoLuceneAnalyzer();
            ExoMultiFieldQueryParser target = new ExoMultiFieldQueryParser(fields, analyzer);
            string query = "hello - world";
            string expected = "(foo:hello) (foo:world)";

            Query actual = target.Parse(query);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void Parse_Exlclude()
        {
            string[] fields = { "foo" };
            Analyzer analyzer = new ExoLuceneAnalyzer();
            ExoMultiFieldQueryParser target = new ExoMultiFieldQueryParser(fields, analyzer);
            string query = "hello -world";
            string expected = "(foo:hello) -(foo:world)";

            Query actual = target.Parse(query);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void Parse_Escape()
        {
            string[] fields = { "foo" };
            Analyzer analyzer = new ExoLuceneAnalyzer();
            ExoMultiFieldQueryParser target = new ExoMultiFieldQueryParser(fields, analyzer);
            string query = "author:\"\\-Christopher\\-\"";
            string expected = "(foo:hello) -(foo:world)";

            Query actual = target.Parse(query);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual.ToString());
        }
    }
}
