# overview
The CMG app is currently rather restrictive in regards to its ability address multiple activities. This is due to the restrictions on the Web Api where the origianl application was developed as a client. The desire is to increase its flexibility and make it behave more like a windos app.
## task level 
* It should appear in the task bar with an CMG logo
* it can have more than one instance running at a time; however, if a CMG file is already open in an instance, it cannot be opened in another. The app will start with an empty file (or copy of the selected file, called *-copy) and a message that the requested file is already in use.
* The app can be opened with the name (full path) of a CMG file, selected from a list of recent files maintained in the task bar for the app.
* the app will open at the same workspace location and size that it was last closed.
* the app will open with a splash screen
## application level
* make app have the look and fell of a normal windows app. The Chrome of the all windows should include a top bar with a CMG logo, a menu (if needed), a centered title, and the minimize, maximize and close button on the far right. All windows should look this way. Maybe creating a user-control would work best.
* maximimum size for any window takes into account windows taskbar. It should never overlap the task bar. 
* within an instance of the app, a single file will be managed with a single list of tracks with their generators. 
* Each window will have a message area that has a scroll list of informative and/or error messages. This area is cleared each time the user performs a new interaction.
* all non-modal windows can be moved, resized, maximized, minimized, and exited at will. No blocking of other windows that are open.
* Most of the dialogs currently displayed will be turned into non-modals. One key feature is that mutiple tracks and generators can be editing at once allowing the user to coordinate their construction. In all cases, only a single non-modal for a function can be open for a given track or generator, e.g., one track volume per track, one generator edit per generator, etc. Only single comment editor and tools windows are allowed.
	* Non-modals
		* Menus
			* Edit->comment (but only one allowed)
			* Tools->(all) - model (tools that affect generators (align, stagger, equal duration) will have to update any open generator window with new generator start and stop times. This is asychronous with user updates of the generator)
		* Track
			* Track->rename (ony one per track allow. Selecting the option will activate the single rename window)
			* Track->Add generator (opens a non-modal generator editor in Add mode)
			* Track->Tools->Shift (only one per track. Selecting the option will activate the single shift window)
			* Track->Tools->Volume  (only one per track. Selecting the option will activate the single volume window)
		* Generator
			* Edit - (note interaction with Tools and track shift)
	* Modals
		* General 
			* The dirty dialog will be a messagebox
			* In fact, any confirmation or error handling will be modal messageboxes
		* Menus
			* file->open (a filedialog is used. need to block access to files that are open in other instances)
			* Edit->preferences (because of its pervasive scope)
			* Play->Play (changes cannot be have to the file while play is in progress. A popup is need to tell the user that generators are being edited can give the option to continue or abort)
			* Play->Report (changes cannot be have to the file while report is in progress. Popup similar to play)
		* Generator
			* Play - (see Play->Play above)
			* Move/Copy - (prevent move/copy if edit is active)
			* Delete - messagebox for confirmation
		* Genertor edit
			* Play - the user can select Play while a genrator is being edited. This will cause play to use the generator in a modified state before it is submitted for update to the track. 
## Processing and interdepencencies
* Each app instance must be aware of CMG files that are currently open to avoid having a file opened simulatneously by two different instances. This applies to File->Open and File->Recent.
* Track shifting will cause any open generators start and stop time change without user intervention. A message is displayed in the generator window to that affect.
* Generator alignh, stagger, and equal duration function will case any open generators affect to have start and stop times without user intervention.  A message is displayed in the generator window to that affect. 
* Exiting a window will check if the user has made modifications and request confirmation of the exit. 
* The Play function is the end result of all of the track and generator management by the user. It has its own special window that can be move, resized, minimized, maximized, and exited at will, but while it is active, no interaction with other CMG components is allowed. The app will appear to hang if the userminimizes the play window and tries to access other parts of the app instance.
* Exception handling - should an instance of the app terminate unexpectedly, there needs to be a way to 'forget' that the file it was handling is now avaiable for access. 
	

	
