using Godot;
using System.Collections.Generic;

public partial class Dialogue : Control {

    private List<string> text_queue = new();

    [Export]
    public Label label;
    [Export]
    public Label previous_label;

    private int idx;

    [Signal]
    public delegate void DialogueFinishedEventHandler();

    [Signal]
    public delegate void DialogueAdvancedEventHandler(int idx);

    public override void _Ready() {
        this.label.Text = "";
        this.previous_label.Text = "";
    }

    public void clear_queue() {
        this.text_queue.Clear();
        this.previous_label.Text = "";
        idx = 0;

        update_label_text();
    }

    public void set_queue(List<string> lines) {
        Show();
        clear_queue();

        foreach (var line in lines) {
            this.text_queue.Add(line);
        }

        update_label_text();
    }

    public void on_ok_button_pressed() {
        advance_text_queue();
        update_label_text();
    }

    public void advance_text_queue() {;
        if (this.text_queue.Count != 0) {
            this.text_queue.RemoveAt(0);

            EmitSignal(nameof(DialogueAdvanced), ++idx);
        }

        if (this.text_queue.Count == 0) {
            this.label.Text = "";
            this.previous_label.Text = "";
            this.Hide();
            EmitSignal(nameof(DialogueFinished));
        }
    }

    public void update_label_text() {
        if (this.text_queue.Count == 0) {
            this.label.Text = "";
            return;
        }

        var text = this.text_queue[0];
        this.previous_label.Text = this.label.Text;
        this.label.Text = text;
    }
}
