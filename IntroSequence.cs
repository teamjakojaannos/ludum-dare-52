using Godot;
using Godot.Collections;

public partial class IntroSequence : AnimatedSprite2D {
    [Export]
    public Array<string> HistoryLessonLines = new();

    [Export]
    public Array<string> FallDialogueLines = new();

    [Export]
    public Array<string> FallDialogue2Lines = new();

    private Dialogue dialogue;
    private Player player;
    private CameraFixedZoom camera;
    private Node2D overlay;
    private Sprite2D camera_overlay;
    private ShaderMaterial overlay_shader;

    private AnimationPlayer animation;

    private float bright_dist = 0.0f;
    private float darkness_dist_factor = 0.0f;

    private float target_bright_dist = 0.0f;
    private float darkness_lerp = 0.01f;
    private float target_darkness_dist_factor = 0.0f;

    private float target_camera_zoom = 1.5f;

    private bool dialogue_ready = false;
    private bool animation_ready = false;

    public override void _Ready() {
        player = GetTree().Root.GetNode<Player>("Main/player");
        dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        camera = GetTree().Root.GetNode<CameraFixedZoom>("Main/Camera");
        overlay = GetParent().GetNode<Node2D>("Overlay");
        overlay_shader = overlay.Material as ShaderMaterial;
        camera_overlay = GetParent().GetNode<Sprite2D>("CameraOverlay");
        overlay.Hide();
        camera_overlay.Show();
        camera_overlay.Modulate = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        player.Hide();
        animation = GetNode<AnimationPlayer>("../IntroAnimationPlayer");

        if (Visible) {
            overlay_shader.SetShaderParameter("bright_dist", 0.0f);
            overlay_shader.SetShaderParameter("darkness_dist_factor", 0.0f);

            target_camera_zoom = 1.5f;
            camera.ZoomFactor = 1.5f;
            camera.OverridePlayerPosition = false;
            camera.IntroOffset = Vector2.Up * 40.0f;
            camera.Offset = Vector2.Right * 200f;
            camera.ResetSmoothing();
            Animation = "fall";
            Stop();

            GetTree().CreateTimer(2.0f).Timeout += StartSequence;
        } else {
            player.Show();
            ReleasePlayer();
        }
    }

    public void StartSequence() {
        camera.ResetSmoothing();
        target_bright_dist = 200.0f;
        target_darkness_dist_factor = 10f;
        darkness_lerp = 1.0f;

        animation.Play("Intro");
        animation.AnimationFinished += (_) => {
            animation_ready = true;
            AttemptStartFallSequence();
        };

        dialogue.DialogueFinished += StartDialogueFinished;
        dialogue.set_queue(new System.Collections.Generic.List<string>(HistoryLessonLines));
    }

    private void StartDialogueFinished() {
        dialogue_ready = true;
        dialogue.DialogueFinished -= StartDialogueFinished;

        AttemptStartFallSequence();
    }

    private void AttemptStartFallSequence() {
        if (dialogue_ready && animation_ready) {
            StartFallSequence();
        }
    }

    private void StartFallSequence() {
        GetTree().CreateTimer(2.5f).Timeout += () => {
            var lines = new System.Collections.Generic.List<string>();
            lines.Add("Until one day...");
            lines.Add("A brave little tomato...");
            lines.Add("Would embark on an adventure");
            lines.Add("which would seal the fate");
            lines.Add("of the entire Greenhouse");

            overlay.Show();
            target_bright_dist = 0.0f;
            target_darkness_dist_factor = 0.0f;
            darkness_lerp = 0.05f;

            dialogue.DialogueFinished += PlayFall;
            dialogue.set_queue(lines);
        };
    }

    public void PlayFall() {
        dialogue.DialogueFinished -= PlayFall;
        AnimationFinished += Fall;
        Play("fall");
    }

    private void Fall() {
        AnimationFinished -= Fall;
        camera_overlay.Hide();


        dialogue.DialogueFinished += FallDialogueFinished;
        dialogue.set_queue(new System.Collections.Generic.List<string>(FallDialogueLines));
    }

    private void FallDialogueFinished() {
        dialogue.DialogueFinished -= FallDialogueFinished;

        GetTree().CreateTimer(2.0f).Timeout += () => {
            target_bright_dist = 75.0f;
            target_darkness_dist_factor = 0.33f;

            AnimationFinished += GetUp;
            Play("get_up");
        };
    }

    private void GetUp() {
        AnimationFinished -= GetUp;
        player.Show();

        var player_sprite = player.GetNode<AnimatedSprite2D>("Sprite");

        player_sprite.Play("default");
        player_sprite.AnimationFinished += Monologue;

        Play("idle");
    }

    private void Monologue() {
        var player_sprite = player.GetNode<AnimatedSprite2D>("Sprite");
        player_sprite.AnimationFinished -= Monologue;

        dialogue.DialogueFinished += ReleasePlayer;
        dialogue.set_queue(new System.Collections.Generic.List<string>(FallDialogue2Lines));
    }

    private void ReleasePlayer() {
        camera_overlay.Hide();
        dialogue.DialogueFinished -= ReleasePlayer;

        target_bright_dist = 200.0f;
        target_darkness_dist_factor = 10f;
        darkness_lerp = 0.01f;
        target_camera_zoom = 1.0f;
        camera.IntroOffset = Vector2.Zero;
        camera.Offset = Vector2.Zero;

        if (!player.is_game_started) {
            player.StartGame();
        }
    }

    public override void _Process(double delta) {
        bright_dist = Mathf.Lerp(bright_dist, target_bright_dist, darkness_lerp);
        darkness_dist_factor = Mathf.Lerp(darkness_dist_factor, target_darkness_dist_factor, darkness_lerp);
        camera.ZoomFactor = Mathf.Lerp(camera.ZoomFactor, target_camera_zoom, 0.01f);

        overlay_shader.SetShaderParameter("bright_dist", bright_dist);
        overlay_shader.SetShaderParameter("darkness_dist_factor", darkness_dist_factor);
    }
}
