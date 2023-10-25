using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using YarnDonut;

public partial class dialogue_view_wrapper : Node, DialogueViewBase
{
	[Export]
	public Node storyState;

	public Action requestInterrupt { get; set; }

	private List<LocalizedLine> currentLinesYarn = new List<LocalizedLine>();

	private Action<int> onOptionSelected;

	#region Called by Dialogue Runner
	public void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
	{
		currentLinesYarn.Add(dialogueLine);
		onDialogueLineFinished?.Invoke();
	}

	public void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
	{
		string[] currentLinesGodot = new string[currentLinesYarn.Count];
		for(int i = 0; i < currentLinesGodot.Length; i++) {
			currentLinesGodot[i] = currentLinesYarn[i].RawText;
		}

		Godot.Collections.Array currentDialogueOptionsGodot = new Godot.Collections.Array();
		for(int i = 0; i < dialogueOptions.Length; i++) {
			currentDialogueOptionsGodot.Add(new Godot.Collections.Array() {
				dialogueOptions[i].Line.TextID,
				dialogueOptions[i].Line.RawText,
				dialogueOptions[i].IsAvailable
			} );
		}

		storyState.Set("lines", currentLinesGodot);
		storyState.Set("options", currentDialogueOptionsGodot);
		this.onOptionSelected = onOptionSelected;

		//CallDeferred(nameof(_PropagateToStoryState));
		_PropagateToStoryState();
		ResetState();
	}

	public void _PropagateToStoryState() {
		storyState.Call("notify_new_state");
	}

	public void DialogueStarted()
	{
		ResetState();
	}

	public void DialogueComplete()
	{
		string[] currentLinesGodot = new string[currentLinesYarn.Count];
		for(int i = 0; i < currentLinesGodot.Length; i++) {
			currentLinesGodot[i] = currentLinesYarn[i].RawText;
		}

		storyState.Set("lines", currentLinesGodot);
		storyState.Set("options", new Godot.Collections.Array());

		CallDeferred(nameof(_PropagateToStoryState));
		ResetState();
	}

	private void ResetState()
	{
		currentLinesYarn.Clear();
	}
	#endregion

	#region Called by GDScript Story State

	public void ChooseOption(int index){
		onOptionSelected.Invoke(index);
	}

	#endregion
}
