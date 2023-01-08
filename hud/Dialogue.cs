using Godot;
using System.Collections.Generic;

public partial class Dialogue : Control {

    private List<string> text_queue = new();

    [Export]
    public Label label;

    [Signal]
    public delegate void DialogueFinishedEventHandler();

    public void clear_queue() {
        this.text_queue.Clear();

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

    public void advance_text_queue() {
        if (this.text_queue.Count != 0) {
            this.text_queue.RemoveAt(0);
        }

        if (this.text_queue.Count == 0) {
            EmitSignal(nameof(DialogueFinished));
            this.Hide();
        }
    }

    public void update_label_text() {
        if (this.text_queue.Count == 0) {
            this.label.Text = "";
            return;
        }

        var text = this.text_queue[0];
        this.label.Text = text;
    }
}
