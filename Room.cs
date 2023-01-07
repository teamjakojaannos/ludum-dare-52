using Godot;

public partial class Room : Node2D {
    [Export]
    public bool StartEnabled;

    [Export]
    public AudioStream Music;

    [Export]
    public float MusicPitch = 1.0f;

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
        GetParent().RemoveChild(this);
    }

    public void OnPlayerEnter() {
        Show();

        GetTree().CreateTimer(0.5f, false).Timeout += () => {
            foreach (var child in GetChildren()) {
                child.ProcessMode = ProcessModeEnum.Inherit;

                var boss = child as BigBoss;
                if (boss != null) {
                    boss.StartEncounter();
                }
            }
        };

        CallDeferred(nameof(PlayMusic));
    }

    private void PlayMusic() {
        var music_player = GetTree().Root.GetNode<Main>("Main").BackgroundMusicPlayer;
        music_player.PlayFadeIn(Music, MusicVolume, MusicPitch);
    }

    public void OnPlayerExit() {
        Hide();

        foreach (var child in GetChildren()) {
            child.ProcessMode = ProcessModeEnum.Disabled;
        }
    }
}
