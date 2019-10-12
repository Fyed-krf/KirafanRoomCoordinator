using System.Drawing;
using System.Linq;
using Fyed.Kirafan;
using Xunit;

namespace Fyed.Kirafan.Test {
    public class RoomMasterTest {
        [Fact]
        public void TestGetPerimeter1() {
            var item = new RoomItem(0, "test item",
                new Point(0, 0));
            var master = new RoomMaster();
            var perimeter = master.GetPerimeter(item, new Point(0, 0), 1);
            var expected = new [] {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };

            Assert.Equal(expected, perimeter);
        }

        [Fact]
        public void TestGetPerimeter2() {
            var item = new RoomItem(0, "test item",
                new Point(0, 0));
            var master = new RoomMaster();
            var perimeter = master.GetPerimeter(item, new Point(0, 0), 5);
            var expected = new [] {
                new Point(0, 0),
                new Point(0, 5),
                new Point(5, 5),
                new Point(5, 0)
            };

            Assert.Equal(expected, perimeter);
        }

        [Fact]
        public void TestGetPerimeter3() {
            var item = new RoomItem(0, "test item",
                new Point(0, 0));
            var master = new RoomMaster();
            var perimeter = master.GetPerimeter(item, new Point(5, 7), 1);
            var expected = new [] {
                new Point(5, 7),
                new Point(5, 8),
                new Point(6, 8),
                new Point(6, 7)
            };

            Assert.Equal(expected, perimeter);
        }

        [Fact]
        public void TestGetPerimeter4() {
            var item = new RoomItem(0, "test item",
                new Point(0, 0));
            var master = new RoomMaster();
            var perimeter = master.GetPerimeter(item, new Point(5, 7), 3);
            var expected = new [] {
                new Point(5, 7),
                new Point(5, 10),
                new Point(8, 10),
                new Point(8, 7)
            };

            Assert.Equal(expected, perimeter);
        }

        [Fact]
        public void TestGetPerimeter5() {
            var item = new RoomItem(0, "test item",
                new Point(0, -1),
                new Point(1, -1));
            var master = new RoomMaster();
            var perimeter = master.GetPerimeter(item, new Point(0, 0), 1);
            var expected = new [] {
                new Point(0, -1),
                new Point(0, 0),
                new Point(2, 0),
                new Point(2, -1)
            };

            Assert.Equal(expected, perimeter);
        }

        [Fact]
        public void TestGetPerimeter6() {
            var item = new RoomItem(0, "test item",
                new Point(-2, -1),
                new Point(-1, -1),
                new Point(-2, 0),
                new Point(-1, 0),
                new Point(-2, 1),
                new Point(-1, 1));
            var master = new RoomMaster();
            var perimeter = master.GetPerimeter(item, new Point(10, 15), 4);
            var expected = new [] {
                new Point(2, 11),
                new Point(2, 23),
                new Point(10, 23),
                new Point(10, 11)
            };

            Assert.Equal(expected, perimeter);
            

                
        }

        // [Fact]
        // public void TestGetPerimeter10() {
        //     var item = new RoomItem(0, "test item",
        //         new Point(-1, 0),
        //         new Point(3, 0),
        //         new Point(4, 0),
        //         new Point(-1, 1),
        //         new Point(2, 1),
        //         new Point(3, 1),
        //         new Point(4, 1),
        //         new Point(-1, 2),
        //         new Point(0, 2),
        //         new Point(1, 2),
        //         new Point(2, 2),
        //         new Point(1, 3),
        //         new Point(0, 4),
        //         new Point(1, 4),
        //         new Point(0, 5));
        //     var master = new RoomMaster();
        //     var perimeter = master.GetPerimeter(item, new Point(-5, 3), 12);
        //     var expected = new [] {
        //         new Point(-17,3),
        //         new Point(-17,15),
        //         new Point(-17,27),
        //         new Point(-17,39),
        //         new Point(-5,39),
        //         new Point(7,39),
        //         new Point(7,51),
        //         new Point(-5,51),
        //         new Point(-5,63),
        //         new Point(-5,75),
        //         new Point(7,75),
        //         new Point(7,63),
        //         new Point(19,63),
        //         new Point(19,51),
        //         new Point(19,39),
        //         new Point(31,39),
        //         new Point(31,27),
        //         new Point(43,27),
        //         new Point(55,27),
        //         new Point(55,15),
        //         new Point(55,3),
        //         new Point(43,3),
        //         new Point(31,3),
        //         new Point(31,15),
        //         new Point(19,15),
        //         new Point(19,27),
        //         new Point(7,27),
        //         new Point(-5,27),
        //         new Point(-5,15),
        //         new Point(-5,3)
        //     };

        //     var perimeterArray = perimeter.ToArray();
        //     Assert.Equal(expected.Length, perimeterArray.Length);

        //     for (var i = 0; i < perimeterArray.Length; i++) {
        //         Assert.Equal(expected[i], perimeterArray[i]);
        //     }

        //     // Assert.Equal(expected, perimeter);
        // }
    }
}