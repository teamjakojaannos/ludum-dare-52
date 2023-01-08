using Godot;
using Godot.Collections;

public partial class IntroSequence : AnimatedSprite2D {
    [Export]
    public Array<string> FallDialogueLines = new Array<string>();

    [Export]
    public Array<string> FallDialogue2Lines = new Array<string>();

    private Dialogue dialogue;
    private Player player;
    private CameraFixedZoom camera;
    private Node2D overlay;
    private ShaderMaterial overlay_shader;

    private float bright_dist = 0.0f;
    private float darkness_dist_factor = 0.0f;

    private float target_bright_dist = 0.0f;
    private float target_darkness_dist_factor = 0.0f;

    private float target_camera_zoom = 1.5f;

    public override void _Ready() {
        player = GetTree().Root.GetNode<Player>("Main/player");
        dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        camera = GetTree().Root.GetNode<CameraFixedZoom>("Main/Camera");
        overlay = GetParent().GetNode<Node2D>("Overlay");
        overlay.Show();

        player.Hide();

        overlay_shader = overlay.Material as ShaderMaterial;
        overlay_shader.SetShaderParameter("bright_dist", 0.0f);
        overlay_shader.SetShaderParameter("darkness_dist_factor", 0.0f);

        target_camera_zoom = 1.5f;
        camera.ZoomFactor = 1.5f;
        camera.ResetSmoothing();
        Animation = "fall";
        Stop();


        if (Visible) {
            GetTree().CreateTimer(2.0f).Timeout += StartSequence;
        } else {
            player.Show();
            ReleasePlayer();
        }
    }

    public void StartSequence() {
        AnimationFinished += Fall;
        Play("fall");
    }

    private void Fall() {
        AnimationFinished -= Fall;

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
        dialogue.DialogueFinished -= ReleasePlayer;

        target_bright_dist = 200.0f;
        target_darkness_dist_factor = 10f;
        target_camera_zoom = 1.0f;
        camera.IntroOffset = Vector2.Zero;

        if (!player.is_game_started) {
            player.StartGame();
        }
    }

    public override void _Process(double delta) {
        bright_dist = Mathf.Lerp(bright_dist, target_bright_dist, 0.01f);
        darkness_dist_factor = Mathf.Lerp(darkness_dist_factor, target_darkness_dist_factor, 0.01f);
        camera.ZoomFactor = Mathf.Lerp(camera.ZoomFactor, target_camera_zoom, 0.01f);

        overlay_shader.SetShaderParameter("bright_dist", bright_dist);
        overlay_shader.SetShaderParameter("darkness_dist_factor", darkness_dist_factor);
    }
}
