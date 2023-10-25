# yarn-godot-wrapper-example
An example of how I wrap the YarnDonut addon c# code with gdscript.

**This repo is for interested parties; it is explicitly not a claim that this is the right way to do things; and it certainly is not good clean code!** 

## Overview
### Goals
- Wrap YarnDonut so that all functionality can be used via gdscript
- Change the architecture to reduce reliance on actions / signals; let other components query the story for current state

### Implementation
- `dialogue_view_wrapper.cs` and `variable_storage_wrapper.cs` have been created to act as a bridges between C# and GDScript
- `dialogue_view_wrapper.cs` and `variable_storage_wrapper.cs` are assigned to a dialogue runner
- `story_state.gd` has been created to act as an entry point for any story related functionality
- `dialogue_view_wrapper.cs` and `variable_storage_wrapper.cs` push all their state to story_state.gd
  - `dialogue_view_wrapper.cs` greedily consumes all lines
- `story_state` directly calls `dialogue_view_wrapper` to select options
- `story_state` also has a direct reference to the Dialogue Runner to get node names etc
- it's an unholy tangled mass of classes but that mess is hidden from the rest of the game; other components just need to reference the StoryState Autoload
- Notes
  - The concept of "views" have been done away with in favour of a global state system. If desired, a similar wrapping solution could be used to implement gdscript. The main idea would be that `dialogue_view_wrapper.cs` acts as a bridge to a single GDScript orchestrator, and all the GDScript views would talk to that.
  - I haven't implemented commands or functions. I know at least one party was interested in that - sorry I have nothing to add at this point!

## Usage
- (to check the example) Run the game, check the console output, and press 1 or 2 to choose one of the options
- Assign yarn files to `story.tres` as usual
- See `choose_options.gd` for example usage
- Everything goes through story_state.gd
  - `choose_option`
  - `override_variable`
  - `options` can be read directly
  - `lines` can be read directly
  - There is a notification system, see `notify_new_variable` and `notify_new_state`. This is not intended to replace "views," it's intended for "world" changes; e.g. keep the visual tiem of day up to date


## Discussion and Review

TODO! (I don't have time)

## Notes
- The code is ripped almost straight from my current project; there are some lines in there that I've commented out
- I've included the version of YarnSpinner for Godot I'm using, which is a bit out date
  - Search for "ALEX" in `DialogueRunner.cs` to see where I have made changes
