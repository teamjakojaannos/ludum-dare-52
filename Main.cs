using Godot;

public partial class Main : Node2D {
	[Export]
	public Control WinUI;

	[Export]
	public Dialogue DialogueUI;

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
		DialogueUI.Hide();
	}

    public override void _Ready() {
        Reset();
    }

	public void TeleportToGreenhouse() {
		var player = GetNode<Player>("player");
		var transition = GetNode<RoomTransition>("InitialRoomTransition");
		transition.IsFirstRoom = false;
		transition.TransitionToRoom(player);
	}
}
