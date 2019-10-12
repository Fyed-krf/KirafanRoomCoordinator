using System.Drawing;
using Fyed.Kirafan;
using Xunit;

namespace Fyed.Kirafan.Test {
    public class RoomReviewerTest {
        [Fact]
        public void TestIsAcceptable1() {
            var room = new Room(9, 9);

            var reviewer = new ActionPointsReviewer();
            Assert.True(reviewer.IsAcceptable(room));

            var item = new RoomItem(100, "test item",
                new Point(0, -1),
                new Point(1, -1));
            var newDisposition = new ItemDisposition(new Point(0, 1), item);
            Room addedRoom;
            var added = room.TryAddItem(newDisposition, out addedRoom);

            Assert.True(added);
            Assert.True(reviewer.IsAcceptable(addedRoom));

            var item2 = new RoomItem(200, "test item2",
                new Point(1, -1),
                new Point(2, -1),
                new Point(1, 0),
                new Point(2, 0));
            var newDisposition2 = new ItemDisposition(new Point(0, 2), item2);
            Room addedRoom2;
            var added2 = addedRoom.TryAddItem(newDisposition2, out addedRoom2);

            Assert.True(added2);
            Assert.True(reviewer.IsAcceptable(addedRoom2));
        }

         [Fact]
        public void TestIsAcceptable2() {
             var room = new Room(9, 9);

            var reviewer = new ActionPointsReviewer();
            Assert.True(reviewer.IsAcceptable(room));

            var item = new RoomItem(100, "test item",
                new Point(0, -1));
            var newDisposition = new ItemDisposition(new Point(0, 1), item);
            Room addedRoom;
            var added = room.TryAddItem(newDisposition, out addedRoom);

            Assert.True(added);
            Assert.True(reviewer.IsAcceptable(addedRoom));
            Assert.True(addedRoom.IsAcceptableWith(reviewer));

            item = new RoomItem(200, "test item",
                new Point(0, 1));
            newDisposition = new ItemDisposition(new Point(0, 1), item);
            Room addedRoom2;
            added = addedRoom.TryAddItem(newDisposition, out addedRoom2);

            Assert.True(added);
            Assert.False(reviewer.IsAcceptable(addedRoom2));
            Assert.False(addedRoom2.IsAcceptableWith(reviewer));

            item = new RoomItem(300, "test item",
                new Point(1, 0));
            newDisposition = new ItemDisposition(new Point(0, 1), item);
            Room addedRoom3;
            added = addedRoom2.TryAddItem(newDisposition, out addedRoom3);

            Assert.True(added);
            Assert.False(reviewer.IsAcceptable(addedRoom3));
            Assert.False(addedRoom3.IsAcceptableWith(reviewer));
        }

        [Fact]
        public void TestIsAcceptable3() {
            var reviewer = new ReachabilityReviewer();
            var room = new Room(5, 5);
            
            Assert.True(room.IsAcceptableWith(reviewer));

            var item = new RoomItem(0, "test item",
                new Point(0, 0),
                new Point(0, 1),
                new Point(2, 1),
                new Point(3, 1),
                new Point(1, 2),
                new Point(1, 3),
                new Point(3, 3),
                new Point(3, 4),
                new Point(4, 4));
            Room addedRoom;
            var added = room.TryAddItem(new ItemDisposition(new Point(0, 0), item), out addedRoom);

            Assert.True(added);
            Assert.True(addedRoom.IsAcceptableWith(reviewer));

            var item2 = new RoomItem(0, "test item",
                new Point(0, 0));
            Room addedRoom2;
            var added2 = addedRoom.TryAddItem(new ItemDisposition(new Point(2, 4), item2), out addedRoom2);

            Assert.True(added2);
            Assert.False(addedRoom2.IsAcceptableWith(reviewer));
        }
   }
}