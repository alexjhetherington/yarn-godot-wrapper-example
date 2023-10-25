extends Node

func _input(event : InputEvent) -> void:
	if (event is InputEventKey):
		if (event.get_physical_keycode_with_modifiers() == KEY_1):
			if (event.pressed):
				StoryState.choose_option(0)
		if (event.get_physical_keycode_with_modifiers() == KEY_2):
			if (event.pressed):
				StoryState.choose_option(1)
