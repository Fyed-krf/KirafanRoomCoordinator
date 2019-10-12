using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fyed.Kirafan {
    public class RoomMaster {
        public Task FindRoomCoordinationAsync(Room startRoom, RoomItem[] items, IRoomReviewer[] reviewers, Action<Room> roomReceiver, Action<Room> halfwayReceiver, CancellationToken cancel) {
            //var taskCount = Math.Max(1, Environment.ProcessorCount - 1);
            var taskCount = Math.Max(1, Environment.ProcessorCount);
            var popCount = 10;
            var wait = TimeSpan.FromMilliseconds(100);

            var stack = new ConcurrentStack<Tuple<Room, RoomItem[]>>();
            stack.Push(Tuple.Create(startRoom, items));

            var waitingCount = 0;


            return Task.Run(() => {
                var workers = new Task[taskCount];
                for (int i = 0; i < workers.Length; i++) {
                    workers[i] = Task.Factory.StartNew(() => {
                        var buffer = new Tuple<Room, RoomItem[]>[popCount];
                        var waiting = false;
                        int counter = 0;
                        Tuple<Room, RoomItem[]>[] newRooms = null;
                        while (true) {
                            cancel.ThrowIfCancellationRequested();

                            var itemCount = stack.TryPopRange(buffer);
                            if (itemCount > 0) {
                                if (waiting) {
                                    Interlocked.Decrement(ref waitingCount);
                                    waiting = false;
                                }

                                counter++;
                                if (counter % 100 == 0) {
                                    counter = 0;
                                    halfwayReceiver(buffer[0].Item1);
                                }

                                if (newRooms == null) {
                                    var newRoomsLength = (buffer[0].Item1.Width + 2) * (buffer[0].Item1.Height + 2) * 2;
                                    newRooms = new Tuple<Room, RoomItem[]>[newRoomsLength];
                                }


                                for (var i = 0; i < itemCount; i++) {
                                    var roomItemPair = buffer[i];
                                    var room = roomItemPair.Item1;
                                    var items = roomItemPair.Item2;
                                    if ((items.Length == 0)) {
                                        roomReceiver(room);
                                    }
                                    else {
                                        var item = items[0];
                                        var flippedItem = item.Flip();
                                        var newItems = items[1..^0];

                                        var newRoomsCount = 0;

                                        for (var y = room.Height; y >= -1; y--) {
                                            for (var x = room.Height; x >= -1; x--) {
                                                var p = new Point(x, y);

                                                var disposition = new ItemDisposition(p, item);
                                                Room newRoom;
                                                if (room.TryAddItem(disposition, out newRoom)) {
                                                    if (newRoom.IsAcceptableWith(reviewers)) {
                                                        newRooms[newRoomsCount] = Tuple.Create(newRoom, newItems);
                                                        newRoomsCount++;
                                                    }
                                                }

                                                var disposition2 = new ItemDisposition(p, flippedItem);
                                                Room newRoom2;
                                                if (room.TryAddItem(disposition2, out newRoom2)) {
                                                    if (newRoom2.IsAcceptableWith(reviewers)) {
                                                        newRooms[newRoomsCount] = Tuple.Create(newRoom2, newItems);
                                                        newRoomsCount++;
                                                    }
                                                }
                                            }
                                        }
                                        if (newRoomsCount > 0) {
                                            stack.PushRange(newRooms, 0, newRoomsCount);
                                        }
                                    }
                                }
                            }
                            else {
                                if (!waiting) {
                                    Interlocked.Increment(ref waitingCount);
                                    waiting = true;
                                }
                                if (waitingCount == taskCount) {
                                    break;
                                }
                                Thread.Sleep(wait);
                            }
                        }
                    }, cancel, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
                Task.WaitAll(workers, cancel);
            }, cancel);
        }
    
        public IEnumerable<Point> GetPerimeter(RoomItem item, Point startPoint, int sideLength) {
            // itemは長方形を想定
            // 長方形以外のitemの外周取得は工夫が必要なので保留

            var minX = item.Occupation
                .Select(p => p.X)
                .Min();
            var maxX = item.Occupation
                .Select(p => p.X)
                .Max();
            var minY = item.Occupation
                .Select(p => p.Y)
                .Min();
            var maxY = item.Occupation
                .Select(p => p.Y)
                .Max();
            
            // left top
            yield return new Point(startPoint.X + minX * sideLength, startPoint.Y + minY * sideLength);
           // left bottom
            yield return new Point(startPoint.X + minX * sideLength, startPoint.Y + (maxY + 1) * sideLength);
            // right botton
            yield return new Point(startPoint.X + (maxX + 1) * sideLength, startPoint.Y + (maxY + 1) * sideLength);
            // right top
            yield return new Point(startPoint.X + (maxX + 1) * sideLength, startPoint.Y + minY * sideLength);
 



            // var movableMap = new Dictionary<Point, HashSet<Point>>();
            // var nodePool = item.Occupation
            //     .SelectMany(p => {
            //         var lt = new Point(startPoint.X + p.X * sideLength, startPoint.Y + p.Y * sideLength);
            //         var rt = new Point(startPoint.X + (p.X + 1) * sideLength, startPoint.Y + p.Y * sideLength);
            //         var lb = new Point(startPoint.X + p.X * sideLength, startPoint.Y + (p.Y + 1) * sideLength);
            //         var rb = new Point(startPoint.X + (p.X + 1) * sideLength, startPoint.Y + (p.Y + 1) * sideLength);

            //         HashSet<Point> movablePoints;
            //         if (!movableMap.TryGetValue(lt, out movablePoints)) {
            //             movablePoints = new HashSet<Point>();
            //             movableMap.Add(lt, movablePoints);
            //         }
            //         movablePoints.Add(rt);
            //         movablePoints.Add(lb);

            //         if (!movableMap.TryGetValue(rt, out movablePoints)) {
            //             movablePoints = new HashSet<Point>();
            //             movableMap.Add(rt, movablePoints);
            //         }
            //         movablePoints.Add(lt);
            //         movablePoints.Add(rb);

            //         if (!movableMap.TryGetValue(lb, out movablePoints)) {
            //             movablePoints = new HashSet<Point>();
            //             movableMap.Add(lb, movablePoints);
            //         }
            //         movablePoints.Add(lt);
            //         movablePoints.Add(rb);

            //         if (!movableMap.TryGetValue(rb, out movablePoints)) {
            //             movablePoints = new HashSet<Point>();
            //             movableMap.Add(rb, movablePoints);
            //         }
            //         movablePoints.Add(rt);
            //         movablePoints.Add(lb);

            //         return new [] { lt, rt, lb, rb };
            //     })
            //     // .SelectMany(p => new [] {
            //     //     new Point(startPoint.X + p.X * sideLength, startPoint.Y + p.Y * sideLength),
            //     //     new Point(startPoint.X + (p.X + 1) * sideLength, startPoint.Y + p.Y * sideLength),
            //     //     new Point(startPoint.X + p.X * sideLength, startPoint.Y + (p.Y + 1) * sideLength),
            //     //     new Point(startPoint.X + (p.X + 1) * sideLength, startPoint.Y + (p.Y + 1) * sideLength)
            //     // })
            //     .Distinct()
            //     .ToList();

            // var minX = nodePool
            //     .Select(p => p.X)
            //     .Min();
            // var leftNodes = nodePool
            //     .Where(p => p.X == minX)
            //     .ToArray();
            // var minYLeftNodes = leftNodes
            //     .Select(p => p.Y)
            //     .Min();
            // var startNode = leftNodes
            //     .Where(p => p.Y == minYLeftNodes)
            //     .First();
            
            // var currentNode = startNode;
            // yield return currentNode;

            // nodePool.Remove(currentNode);

            // while (true) {
            //     var nextNodeCanditate = new Point(currentNode.X - sideLength, currentNode.Y);
            //     if (movableMap[currentNode].Contains(nextNodeCanditate) && nodePool.Remove(nextNodeCanditate)) {
            //         currentNode = nextNodeCanditate;
            //         yield return currentNode;
            //         continue;
            //     }

            //     nextNodeCanditate = new Point(currentNode.X, currentNode.Y + sideLength);
            //     if (movableMap[currentNode].Contains(nextNodeCanditate) && nodePool.Remove(nextNodeCanditate)) {
            //         currentNode = nextNodeCanditate;
            //         yield return currentNode;
            //         continue;
            //     }

            //     nextNodeCanditate = new Point(currentNode.X + sideLength, currentNode.Y);
            //     if (movableMap[currentNode].Contains(nextNodeCanditate) && nodePool.Remove(nextNodeCanditate)) {
            //         currentNode = nextNodeCanditate;
            //         yield return currentNode;
            //         continue;
            //     }

            //     nextNodeCanditate = new Point(currentNode.X, currentNode.Y - sideLength);
            //     if (movableMap[currentNode].Contains(nextNodeCanditate) && nodePool.Remove(nextNodeCanditate)) {
            //         currentNode = nextNodeCanditate;
            //         yield return currentNode;
            //         continue;
            //     }

            //     break;
            // }
        }
    }
}