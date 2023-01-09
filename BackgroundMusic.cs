using Godot;
using System;

public partial class BackgroundMusic : Node {
    public const float SILENCE_VOLUME = -50.0f;

    private AudioStreamPlayer current;
    private AudioStreamPlayer fade;

    private float target_volume;

    public override void _Ready() {
        current = new AudioStreamPlayer();
        fade = new AudioStreamPlayer();

        AddChild(current);
        AddChild(fade);
    }

    public void PlayFadeIn(AudioStream track, float volume = -15.0f, float pitch = 1.0f) {
        fade.Stop();
        var tmp = current;
        current = fade;
        fade = tmp;

        target_volume = volume;
        current.VolumeDb = SILENCE_VOLUME;
        current.Stream = track;
        current.PitchScale = pitch;
        current.Play();
    }

    public void Stop() {
        fade.Stop();
        var tmp = current;
        current = fade;
        fade = tmp;

        target_volume = SILENCE_VOLUME;
    }

    public override void _Process(double delta) {
        if (current.Playing) {
            current.VolumeDb = Mathf.Lerp(current.VolumeDb, target_volume, 0.01f);
        }
        if (fade.Playing) {
            fade.VolumeDb = Mathf.Lerp(fade.VolumeDb, SILENCE_VOLUME, 0.005f);

            if (Mathf.Abs(fade.VolumeDb - SILENCE_VOLUME) < 0.01f) {
                fade.Stop();
            }
        }
    }
}
