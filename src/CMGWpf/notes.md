* integrate DB editor with main application
* fix messaging system
* make voice ensembles list "hot" to display ensemble when clicked on
* make tag note_sequences list "hot" to display note_sequence when clicked on
* error checking of carnivalsque soundfont is prevent it from loading. Appears mesasesh_10 instrument has a bad end loop value. need some safe setting here

# bugs

	* DB lists are not loading onto the UI thread during ensemebel and notsequence dialog startup
	* ensemblepanel and notesequencepanel are not necessary
	* tools 
		* when a new file is loaded the tools dialog should be retarted to refesh the generator lists
		* make secondary generator list scrollable with a max number of displayed entries as 10
	* messaging
		* no status message 
			generator edit completes with save or cancel
			play completes
	* UI
		* Add genertor is not blocked when playing
	* others
		the Orchestra Essentials soundfont has a problem with root key for the oboe for almost all of the key ranges, cause cent correction so in excess of 25 semitomes. This is causing the sound to be very distorted. I shoud notify the author of the soundfont and see if they can fix it. In the meantime, I will avoid the oboe.
	* installation
		* The task bar icon is not the correct one. In fact, none of the icons are right except withint the application
	* DSP
		* appears to be a coupling betwen markov algorithms of different generators. Related to copy generator and track. Problem resolves when file is reloaded.
# release
	* figure out where to put the installer. Windows Applications?