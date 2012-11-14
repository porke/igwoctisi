namespace Client.Tests.Input.Controls
{
    using System;
    using Client.View.Controls;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Nuclex.UserInterface;

    [TestClass]
    public class WrappableListControlTests
    {
        private static readonly string[] _items = new string[]{ "12345 67890+", "abdcef ghijklm", "zażółcam gęślą jaźń" };
        private static Screen _screen = new Screen(100, 100);
        private WrappableListControl ctrl;

        [TestInitialize]
        public void SetUp()
        {
            ctrl = new WrappableListControl()
            {
                Bounds = new UniRectangle(new UniScalar(0), new UniScalar(0), new UniScalar(100), new UniScalar(100)),
                CharacterWidthInPx = 10
            };
            _screen.Desktop.Children.Clear();
        }

        [TestMethod]
        public void CreateList_ItemCountShouldEqual2()
        {
            _screen.Desktop.Children.Add(ctrl);
            ctrl.AddItems(_items);

            Assert.AreEqual(ctrl.GetItemCount(), 3);
        }

        [TestMethod]
        public void CreateListAndAddItemsWithoutDesktop_ShouldThrowException()
        {
            try
            {
                ctrl.AddItems(_items);
            }
            catch (InvalidOperationException)
            {
                // Ok, this is the exception we are expecting
            }
            catch
            {
                Assert.Fail("Wrong type of exception thrown!");
            }
        }

        [TestMethod]
        public void DeleteItem_ShouldLeave1ItemLess()
        {
            _screen.Desktop.Children.Add(ctrl);

            ctrl.AddItems(_items);
            Assert.AreEqual(ctrl.GetItemCount(), 3);

            ctrl.DeleteItem(2);

            Assert.AreEqual(ctrl.GetItemCount(), 2);
        }

        [TestMethod]
        public void GetItem_ShouldReturnCorrectValue()
        {
            _screen.Desktop.Children.Add(ctrl);

            ctrl.AddItems(_items);

            var item = ctrl.GetItem(0);
            Assert.AreEqual(_items[0], item);
        }
    }
}
