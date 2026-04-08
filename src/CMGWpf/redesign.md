# overview
The CMG app is currently rather restrictive in regards to its ability address multiple activities. This is due to the restrictions on the Web Api where the origianl application was developed as a client. The desire is to increase its flexibility and make it behave more like a windos app.
## task level 
* the app will open with a splash screen (**done**)
* It should appear in the task bar with an CMG logo (**done**)
* it can have more than one instance running at a time (**done**)
* the app will open at the same workspace location and size that it was last closed. (**done**)
* If a CMG file is already open in an instance, it cannot be opened in another. The app will start with an empty file (or copy of the selected file, called *-copy) and a message that the requested file is already in use. (**done - blocking implemented, copy feature deferred**)
* The app can be opened with the name (full path) of a CMG file, selected from a jump list of files maintained in the task bar for the app. (**done - command-line support added** not yet tested)
## application level
* make app have the look and fell of a normal windows app. The Chrome of the all windows should include a top bar with a CMG logo in the upper left-hand corner, a optional menu, a centered title (to be set by the caller), and the minimize, maximize and close button on the far right. All windows should look this way. Maybe creating a user-control would work best. (**done**)
* maximimum size for any window takes into account windows taskbar. It should never overlap the task bar. (**done during taks level changes**)
* within an instance of the app, a single file will be managed with a single list of tracks with their generators. (**done**) 
* Each window will have a message area that has a scroll list of informative and/or error messages. This area is cleared each time the user performs a new interaction.
* all windows can be moved, resized, maximized, minimized, and exited at will. No blocking of other windows that are open. (**done**)
* Most of the dialogs currently displayed will be turned into non-modals. One key feature is that mutiple tracks and generators can be editing at once allowing the user to coordinate their construction. In all cases, only a single non-modal for a function can be open for a given track or generator, e.g., one track volume per track, one generator edit per generator, etc. Only single comment editor and tools windows are allowed.
	* Non-modals
		* Menus
			* Tools->(all) - modal except align, stagger, and equal which could affect genertors with open dialogs. (**done**) 
		* Track
			* Track->rename (ony one per track allow. Selecting the option will activate the single rename window) (**done**)
			* Track->Tools->Shift (only one per track. Selecting the option will activate the single shift window) (**done**)
			* Track->Tools->Volume  (only one per track. Selecting the option will activate the single volume window) (**done**)
		* Generator
			* Edit - (note interaction with Tools and track shift, these function now check that no generator is being edited before starting) (**done**)
	* Modals
		* General 
			* The dirty dialog will be a warning messagebox (**done**)
			* In fact, any confirmation or error handling will be modal messageboxes. Some errors and some warnings (**done**)
		* Menus
			* file->open (a filedialog is used. need to block access to files that are open in other instances) (**done**)
			* Edit->preferences (because of its pervasive scope) (**done**)
			* Edit->comment (but only one allowed) (**done**)
			* Tools-> Align, stagger, Equal - these dialog cannot open if ther are any active generator dialogs (**need debugging**)
			* Play->Play (changes cannot be made to the file while play is in progress. A popup is needed to tell the user that generators are being edited and give the option to continue or abort)
			* Play->Report (changes cannot be have to the file while report is in progress. Popup similar to play)
		* Track
			* Track->Add generator (opens a modal generator editor in Add mode to prevent additional simulataneous additions, which would cause nonunique generator names) (**done**)
		* Generator
			* Play - (see Play->Play above)
			* Move/Copy - (prevent move/copy if edit is active) (**done**)
			* Delete - (prevent is edit is active) messagebox for confirmation (**done*)
		* Generator edit
			* Play - the user can select Play while a generator is being edited. This will cause play to use the generator in a modified state before it is submitted for update to the track. 
## Processing and interdepencencies
* Each app instance must be aware of CMG files that are currently open to avoid having a file opened simulatneously by two different instances. This applies to File->Open and File->Recent. (**done**)
* Track shifting will cause any open generators start and stop time change without user intervention. A message is displayed in the generator window to that affect.
* Generator align, stagger, and equal duration function will cause any open generators affect to have start and stop times without user intervention.  A message is displayed in the generator window to that affect. (**done**)
* Exiting CMG will check if the user has made modifications and request confirmation of the exit. (**done**)
* The Play function is the end result of all of the track and generator management by the user. It has its own special window that can be move, resized, minimized, maximized, and exited at will, but while it is active, no interaction with other CMG components is allowed. The app will appear to hang if the userminimizes the play window and tries to access other parts of the app instance.
* Exception handling - should an instance of the app terminate unexpectedly, there needs to be a way to 'forget' that the file it was handling is now available for access. (**done**)

# Side effects that need to be fixed
	

	
