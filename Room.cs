using Godot;
using System;

public partial class Room : Node2D {
    [Export]
    public bool StartEnabled;

    public override void _Ready() {
        if (!StartEnabled) {
            OnPlayerExit();
        }
    }

    public void OnPlayerEnter() {
        foreach (var child in GetChildren()) {
            child.ProcessMode = ProcessModeEnum.Inherit;
        }
    }

    public void OnPlayerExit() {
        foreach (var child in GetChildren()) {
            child.ProcessMode = ProcessModeEnum.Disabled;
        }
    }
}
