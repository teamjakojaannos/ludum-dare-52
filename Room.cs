using Godot;

public partial class Room : Node2D {
    [Export]
    public bool StartEnabled;

    public override void _Ready() {
        if (StartEnabled) {
            OnPlayerEnter();
        } else {
            OnPlayerExit();
        }
    }

    public void OnPlayerEnter() {
        Show();

        GetTree().CreateTimer(0.5f, false).Timeout += () => {
            foreach (var child in GetChildren()) {
                child.ProcessMode = ProcessModeEnum.Inherit;
            }
        };
    }

    public void OnPlayerExit() {
        Hide();

        foreach (var child in GetChildren()) {
            child.ProcessMode = ProcessModeEnum.Disabled;
        }
    }
}
