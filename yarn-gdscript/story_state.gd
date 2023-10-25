extends Node

@export var dialogue_runner : Node
@export var view_wrapper : Node
@export var variable_wrapper : Node

var lines : PackedStringArray
var options : Array

var variables : Dictionary

const VARIABLE_LISTENERS = "variable_listeners"
const STATE_LISTENERS = "state_listeners"

var console_variables_dirty : bool = true
var _world_variables_lock : bool = false

#func _ready():
#	Console.add_command("story_variables_list_all", log_all_variables)
#	Console.add_command("story_variables_get", log_variable, 1)
#	Console.add_command("story_variables_set", override_variable, 2)
#	Console.add_command("story_go_to_node", go_to_node, 1)


#func log_variable(variable: String):
#	if variables.has(variable):
#		Console.log(variable + ": " + str(variables[variable]))
#	else:
#		Console.log(variable + " does not exist!")


#func log_all_variables():
#	for variable in variables.keys():
#		log_variable(variable)

func print_all_options():
	for option in options:
		print(option[0] + " " + option[1])

func notify_new_state():
	print("story state updated")

	for line in lines:
		print(line)

	print_all_options()

	if options.size() == 0:
		print("Story Finished!")

	get_tree().call_group(STATE_LISTENERS, "notify_new_state")


func notify_new_variables():
	# Quick hack to add variable autocomplete to the console
	# We need to wait for variables to come in
#	if console_variables_dirty:
#		console_variables_dirty = false
#		for variable_name in variables.keys():
#			Console.add_known_word(variable_name)

	# The Important part of the method
	print("story variables updated")

	if !_world_variables_lock:
		get_tree().call_group(VARIABLE_LISTENERS, "notify_new_variables")

func lock_world_variables():
	_world_variables_lock = true

func unlock_world_variables():
	_world_variables_lock = false
	get_tree().call_group(VARIABLE_LISTENERS, "notify_new_variables")


func override_variable(variable_name : String, value):
	if(variables.has(variable_name)):
		match typeof(variables[variable_name]):
			TYPE_BOOL: # Target is bool
				if typeof(value) == TYPE_STRING:
					@warning_ignore("unsafe_method_access")
					value = value.to_lower() == "true"
				@warning_ignore("unsafe_method_access")
				variable_wrapper.SetValueBool(variable_name, value)

			TYPE_FLOAT: # Target is float
				if typeof(value) == TYPE_STRING:
					@warning_ignore("unsafe_method_access")
					value = value.to_float()
				@warning_ignore("unsafe_method_access")
				variable_wrapper.SetValueFloat(variable_name, value)

			TYPE_STRING: # Target is string
				@warning_ignore("unsafe_method_access")
				variable_wrapper.SetValueString(variable_name, value)
			_:
				push_error("Value is not a supported yarn type: " + value)
	else:
		push_error("Variable is not a valid story variable: " + variable_name)

#	log_variable(variable_name)


func go_to_node(node_name : String):
	@warning_ignore("unsafe_method_access")
	dialogue_runner.Stop()
	@warning_ignore("unsafe_method_access")
	dialogue_runner.StartDialogue(node_name)

func get_current_node() -> String:
	@warning_ignore("unsafe_method_access")
	return dialogue_runner.GetCurrentNodeName()

func reset_variables():
	@warning_ignore("unsafe_method_access")
	dialogue_runner.SetInitialVariables(true)

func choose_option(index : int):
	if index < 0:
		push_warning("Choosing story index < 0")
		return


	print ("!!!Choosing option index: " + str(index))
	# Calling choose option should trigger the dialogue runner to
	# push new state to the wrapper instance
	@warning_ignore("unsafe_method_access")
	view_wrapper.ChooseOption(index)


func choose_option_fuzzy(input : String):
	var index: int = -1

	for i in options.size():
		var option = options[i]
		var option_id := option[0] as String
		if option_id.contains(input):
			if index > -1:
				push_error("Multiple options available for fuzzy id, so unable to choose one: " + input + ". See console for available options")
				print_all_options()
				return

			index = i

	if index > -1:
		choose_option(index)
	else:
		push_error("No option available for fuzzy id: " + input + ". See console for available options")
		print_all_options()

# Intended for when you want an option globally accessible
# There will be unintended side effects
# USE AT YOUR OWN RISK
func tunnel_option(node: String, option : String):
	push_warning("Using option tunnel: [" + node + ", " + option + "]")
	var current_node = get_current_node()
	go_to_node(node)
	choose_option_fuzzy(option)
	go_to_node(current_node)
