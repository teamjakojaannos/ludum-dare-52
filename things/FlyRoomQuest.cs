using Godot;
using System;

public partial class FlyRoomQuest : Node {


    private RoomTransition transition;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        var frog = GetNodeOrNull<Frog>("../Frog");
        if (frog == null) {
            GD.Print("Can't find frog");
        } else {
            frog.FrogStartsSleeping += on_frog_sleep;
        }

        transition = GetNodeOrNull<RoomTransition>("../BackToSpawnRoom");
        if (transition == null) {
            GD.Print("Level transition not found");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    public void on_frog_sleep() {
		GD.Print("Frog is sleeping");
        if (transition != null) {
            // activate transition
        }
    }
}
