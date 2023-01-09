using Godot;

public partial class Room : Node2D {
    [Export]
    public bool StartEnabled;

    [Export]
    public AudioStream Music;

    [Export]
    public float MusicPitch = 1.0f;

    public int CameraLimitLeft {
        get {
            var shape = GetNodeOrNull<Area2D>("Bounds")?.GetNodeOrNull<CollisionShape2D>("Shape");
            var room_size = (shape?.Shape as RectangleShape2D)?.Size ?? new Vector2(16 * 32, 9 * 32);
            var offset = shape?.Position ?? Vector2.Zero;
            return Mathf.FloorToInt(room_size.x / -2.0f) + Mathf.FloorToInt(offset.x);
        }
    }
    public int CameraLimitRight {
        get {
            var shape = GetNodeOrNull<Area2D>("Bounds")?.GetNodeOrNull<CollisionShape2D>("Shape");
            var room_size = (shape?.Shape as RectangleShape2D)?.Size ?? new Vector2(16 * 32, 9 * 32);
            var offset = shape?.Position ?? Vector2.Zero;
            return Mathf.FloorToInt(room_size.x / 2.0f) + Mathf.FloorToInt(offset.x);
        }
    }
    public int CameraLimitTop {
        get {
            var shape = GetNodeOrNull<Area2D>("Bounds")?.GetNodeOrNull<CollisionShape2D>("Shape");
            var room_size = (shape?.Shape as RectangleShape2D)?.Size ?? new Vector2(16 * 32, 9 * 32);
            var offset = shape?.Position ?? Vector2.Zero;
            return Mathf.FloorToInt(room_size.y / -2.0f) + Mathf.FloorToInt(offset.y);
        }
    }
    public int CameraLimitBottom {
        get {
            var shape = GetNodeOrNull<Area2D>("Bounds")?.GetNodeOrNull<CollisionShape2D>("Shape");
            var room_size = (shape?.Shape as RectangleShape2D)?.Size ?? new Vector2(16 * 32, 9 * 32);
            var offset = shape?.Position ?? Vector2.Zero;
            return Mathf.FloorToInt(room_size.y / 2.0f) + Mathf.FloorToInt(offset.y);
        }
    }

    [Export]
    public float MusicVolume = -15.0f;

    public override void _Ready() {
        if (StartEnabled) {
            OnPlayerEnter();
        } else {
            OnPlayerExit();
            CallDeferred(new StringName(nameof(Detach)));
        }
    }

    private void Detach() {
        GD.Print($"Detaching {this.Name}");
        GetParent().RemoveChild(this);
    }

    public virtual void OnPlayerEnter() {
        Show();

        var player = GetTree().Root.GetNode<Player>("Main/player");
        player.ProcessMode = ProcessModeEnum.Disabled;
        GetTree().CreateTimer(0.5f, false).Timeout += () => {
            player.ProcessMode = ProcessModeEnum.Inherit;
            foreach (var child in GetChildren()) {
                child.ProcessMode = ProcessModeEnum.Inherit;
            }
        };

        CallDeferred(nameof(PlayMusic));
    }

    private void PlayMusic() {
        var music_player = GetTree().Root.GetNode<Main>("Main").BackgroundMusicPlayer;
        music_player.PlayFadeIn(Music, MusicVolume, MusicPitch);
    }

    public virtual void OnPlayerExit() {
        Hide();

        foreach (var child in GetChildren()) {
            child.ProcessMode = ProcessModeEnum.Disabled;
        }
    }
}
