using System.Drawing;
using System.Linq;

namespace Fyed.Kirafan {
    public class ActionPointsReviewer : IRoomReviewer {
        private bool avoidDuplicatePoints;
        private bool avoidDeadPoints;

        public ActionPointsReviewer(bool avoidDuplicatePoints, bool avoidDeadPoints) {
            this.avoidDuplicatePoints = avoidDuplicatePoints;
            this.avoidDeadPoints = avoidDeadPoints;
        }

        public ActionPointsReviewer() : this(true, true) {

        }

        public bool IsAcceptable(Room room) {
            if (avoidDuplicatePoints) {
                var duplicated = room.Dispositions
                    .GroupBy(x => x.Location)
                    .Any(g => g.Count() > 1);
                if (duplicated) {
                    return false;
                }
            }

            if (avoidDeadPoints) {
                var freeFloors = room.FreeFloors;
                var dead = room.Dispositions
                    .Where(x => !room.FreeFloors.Contains(x.Location))
                    .Where(x => !x.Item.Occupation.Contains(Point.Empty))
                    .Any();
                if (dead) {
                    return false;
                }
            }

            return true;
        }
    }
}