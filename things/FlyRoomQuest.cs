using Godot;
using System;

public partial class FlyRoomQuest : Node {

    private FertilizerBlocker blocker;


    public override void _Ready() {
        var frog = GetNodeOrNull<Frog>("../Frog");
        if (frog == null) {
            GD.Print("Can't find frog");
        } else {
            frog.FrogStartsSleeping += on_frog_sleep;
        }

        blocker = GetNodeOrNull<FertilizerBlocker>("../Blocker");
        if (blocker == null) {
            GD.Print("Can't find blocker.");
        }
    }


    public void on_frog_sleep() {
        if (blocker != null) {
            blocker.make_removable();
        }
    }
}
