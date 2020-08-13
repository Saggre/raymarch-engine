using System.Linq;
using NUnit.Framework;
using RaymarchEngine.Core;

namespace RaymarchEngineTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

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