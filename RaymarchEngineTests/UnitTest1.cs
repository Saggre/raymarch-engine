using System.Linq;
using NUnit.Framework;
using RaymarchEngine.Core;

namespace RaymarchEngineTests
{
    /// <summary>
    /// Tests for the GameObject hierarchy
    /// </summary>
    public class Tests
    {
        /// <summary>
        /// Runs before each test
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Parenting a GameObject registers it on both sides, and removing it clears both
        /// </summary>
        [Test]
        public void Hierarchy()
        {
            GameObject parent = new GameObject();
            GameObject child = new GameObject();
                
            child.SetParent(parent);

            Assert.AreEqual(child.Parent, parent);
            Assert.Contains(child, parent.Children.ToArray());

            parent.RemoveChild(child);

            Assert.IsNull(child.Parent);
            Assert.IsEmpty(parent.Children.ToArray());
        }
    }
}