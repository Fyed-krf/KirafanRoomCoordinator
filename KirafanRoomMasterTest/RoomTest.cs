using System;
using System.Drawing;
using Fyed.Kirafan;
using Xunit;

namespace Fyed.Kirafan.Test {
    public class RoomTest {
        [Fact]
        public void TestConstructor() {
            var room = new Room(3, 4);

            Assert.Equal(3, room.Width);

            Assert.Equal(4, room.Height);

            Assert.Empty(room.Dispositions);

            for (int x = 0; x < room.Width; x++) {
                for (int y = 0; y < room.Height; y++) {
                    var p = new Point(x, y);
                    Assert.Contains(p, room.FreeFloors);
                    Assert.Null(room.FloorSummary[x, y]);
                }
            }
        }

        [Fact]
        public void TestTryAdd1() {
            var room = new Room(9, 9);
            var item = new RoomItem(100, "test item",
                new Point(0, -1),
                new Point(1, -1));
            var newDisposition = new ItemDisposition(new Point(0, 1), item);
            Room addedRoom;
            var added = room.TryAddItem(newDisposition, out addedRoom);

            Assert.True(added);

            Assert.Single(addedRoom.Dispositions);
            var disposision = addedRoom.Dispositions[0];
            Assert.Equal(newDisposition, disposision);

            var freeFloors = addedRoom.FreeFloors;
            var floorSummary = addedRoom.FloorSummary;
            for (int x = 0; x < addedRoom.Width; x++) {
                for (int y = 0; y < addedRoom.Height; y++) {
                    var p = new Point(x, y);
                    var id = floorSummary[x, y];
                    if (x == 0 && y == 0) {
                        Assert.DoesNotContain(p, freeFloors);
                        Assert.Equal(100, id);
                    }
                    else if (x == 1 && y == 0) {
                        Assert.DoesNotContain(p, freeFloors);
                        Assert.Equal(100, id);
                    }
                    else {
                        Assert.Contains(p, freeFloors);
                        Assert.Null(id);
                    }
                }
            }
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(8, 0)]
        [InlineData(8, 8)]
        [InlineData(-1, 0)]
        [InlineData(10, 1)]
        [InlineData(3, 10)]
        public void TestTryAdd2(int x, int y) {
            var room = new Room(9, 9);
            var item = new RoomItem(0, "test item",
                new Point(0, -1),
                new Point(1, -1));
            var newDisposition = new ItemDisposition(new Point(x, y), item);
            Room addedRoom;
            var added = room.TryAddItem(newDisposition, out addedRoom);

            Assert.False(added);
        }

        [Fact]
        public void TestTryAdd3() {
            var room = new Room(9, 9);

            var item = new RoomItem(100, "test item1",
                new Point(0, -1),
                new Point(1, -1));
            var newDisposition = new ItemDisposition(new Point(0, 1), item);
            Room addedRoom;
            var added = room.TryAddItem(newDisposition, out addedRoom);

            Assert.True(added);

            Assert.Single(addedRoom.Dispositions);
            var disposision = addedRoom.Dispositions[0];
            Assert.Equal(newDisposition, disposision);

            var freeFloors = addedRoom.FreeFloors;
            var floorSummary = addedRoom.FloorSummary;
            for (int x = 0; x < addedRoom.Width; x++) {
                for (int y = 0; y < addedRoom.Height; y++) {
                    var p = new Point(x, y);
                    var id = floorSummary[x, y];
                    if (x == 0 && y == 0) {
                        Assert.DoesNotContain(p, freeFloors);
                        Assert.Equal(100, id);
                    }
                    else if (x == 1 && y == 0) {
                        Assert.DoesNotContain(p, freeFloors);
                        Assert.Equal(100, id);
                    }
                    else {
                        Assert.Contains(p, freeFloors);
                        Assert.Null(id);
                    }
                }
            }

            var item2 = new RoomItem(200, "test item2",
                new Point(1, -1),
                new Point(2, -1),
                new Point(1, 0),
                new Point(2, 0));
            var newDisposition2 = new ItemDisposition(new Point(0, 2), item2);
            Room addedRoom2;
            var added2 = addedRoom.TryAddItem(newDisposition2, out addedRoom2);

            Assert.True(added2);

            Assert.Equal(2, addedRoom2.Dispositions.Length);
            var disposision2 = addedRoom2.Dispositions[1];
            Assert.Equal(newDisposition2, disposision2);

            var freeFloors2 = addedRoom2.FreeFloors;
            var floorSummary2 = addedRoom2.FloorSummary;
            for (int x = 0; x < addedRoom2.Width; x++) {
                for (int y = 0; y < addedRoom2.Height; y++) {
                    var p = new Point(x, y);
                    var id = floorSummary2[x, y];

                    if (x == 0 && y == 0) {
                        Assert.DoesNotContain(p, freeFloors2);
                        Assert.Equal(100, id);
                    }
                    else if (x == 1 && y == 0) {
                        Assert.DoesNotContain(p, freeFloors2);
                        Assert.Equal(100, id);
                    }
                    else if (x == 1 && y == 1) {
                        Assert.DoesNotContain(p, freeFloors2);
                        Assert.Equal(200, id);
                    }
                    else if (x == 2 && y == 1) {
                        Assert.DoesNotContain(p, freeFloors2);
                        Assert.Equal(200, id);
                    }
                    else if (x == 1 && y == 2) {
                        Assert.DoesNotContain(p, freeFloors2);
                        Assert.Equal(200, id);
                    }
                    else if (x == 2 && y == 2) {
                        Assert.DoesNotContain(p, freeFloors2);
                        Assert.Equal(200, id);
                    }
                    else {
                        Assert.Contains(p, freeFloors2);
                        Assert.Null(id);
                    }
                }
            }
        }

        [Fact]
        public void TestToString() {
            var room = new Room(9, 9);

            var item = new RoomItem(0, "test item1",
                new Point(0, -1),
                new Point(1, -1));
            var newDisposition = new ItemDisposition(new Point(0, 1), item);
            Room addedRoom;
            var added = room.TryAddItem(newDisposition, out addedRoom);

            var item2 = new RoomItem(1, "test item2",
                new Point(1, -1),
                new Point(2, -1),
                new Point(1, 0),
                new Point(2, 0));
            var newDisposition2 = new ItemDisposition(new Point(0, 2), item2);
            Room addedRoom2;
            var added2 = addedRoom.TryAddItem(newDisposition2, out addedRoom2);

            var expected = string.Join(Environment.NewLine, new [] {
                "[0][0][ ][ ][ ][ ][ ][ ][ ]",
                "[#][1][1][ ][ ][ ][ ][ ][ ]",
                "[#][1][1][ ][ ][ ][ ][ ][ ]",
                "[ ][ ][ ][ ][ ][ ][ ][ ][ ]",
                "[ ][ ][ ][ ][ ][ ][ ][ ][ ]",
                "[ ][ ][ ][ ][ ][ ][ ][ ][ ]",
                "[ ][ ][ ][ ][ ][ ][ ][ ][ ]",
                "[ ][ ][ ][ ][ ][ ][ ][ ][ ]",
                "[ ][ ][ ][ ][ ][ ][ ][ ][ ]",
                ""
            });
            var actual = addedRoom2.ToString();
            Assert.Equal(expected, addedRoom2.ToString());
        }
    }
}