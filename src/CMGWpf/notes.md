# bugs
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
# testing
	* do thorough testing of all generator parameters
	* put the DSP through its paces check all controls and algorithms.
	* Jump List Testing - Deferred due to file associations causing rebuild of test structure
# review documentation
# feature changes
	* ~~consider removing the silent generator. It does not have much purpose anymore as the way that the audio is generted has chenged. ~~
	* clean up code related to the silent generator and remove it from the UI. It is not needed anymore as the way that audio is generated has changed.
	* implement help about and user guide link