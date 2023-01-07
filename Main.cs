using Godot;

public partial class Main : Node2D {
	[Export]
	public Control WinUI;

	[Export]
	public BackgroundMusic BackgroundMusicPlayer;
	
	public void Win() {
		WinUI.Show();
	}

	public void Lose() {
		// TODO
	}

	public void Reset() {
		WinUI.Hide();
	}

    public override void _Ready() {
        Reset();
    }
}
