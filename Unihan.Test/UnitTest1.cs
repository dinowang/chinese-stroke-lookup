using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unihan.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestString()
        {
            var source = "測試中文字元筆劃數";
            var strokes = new[] { 12, 13, 4, 4, 6, 4, 12, 14, 15 };

            var result = StrokeLookup.Instance.GetStrokes(source);

            var totallyMatch = result.Select(x => x.Stroke).SequenceEqual(strokes);
            Assert.AreEqual(totallyMatch, true, "結果不符");
        }

        [TestMethod]
        public void TestChar()
        {
            var source = '堃';
            var stroke = 11;

            var result = StrokeLookup.Instance.GetStroke(source);

            Assert.AreEqual(result, stroke, "結果不符");
        }
    }
}
