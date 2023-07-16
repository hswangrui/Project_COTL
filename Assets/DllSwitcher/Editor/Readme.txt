What's the plugin for?
For some reason,  you may need to replace the dll file of you project with source code(Or replace source code with dll file) . 
But when you done this, Unity will loose references to related scripts, and this plugin is help you to fix this problem.

How to use it?
1. Change a couple of settings in the editor(Edit->Project Settings->Editor, Requires Unity Pro)
	Version Control: Meta files
	Asset Serialization: Force text
2. Open our plugin window(Window->DllSwitcher), Set the dll file's path, source code 's directory and the target dictory you need to replace.
3. Click the replace button.

Warn:
1. It's important to backup your project before use this plugin.
2. About the dll Dependency: Also the only case you have to put you dependented dll file int the same directory is that 
   some class in your dll inherit from an class of other dll, But we suggest you always put the dependency together.