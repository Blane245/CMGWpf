# enhancement
	* report writer does not need to produce signal samples, which takes lots of time. (soft need)
	* rewrite instrument. get in higher performance language. consider buildCloud too.
# bugs
	* report 
	* time line
		* time interval redefinition not working perperly. A clikc on th timeline outside the time interval border is causing a redefinition rather than a deletion.  
	* generators
	* track
	* DB Editor 
	* tools 
	* messaging
	* UI
		* Add generator is not blocked when playing
	* play
		* sound roll height not correct
		* playing single generator hangs up. not completing any tasks.
	* others
		* the Orchestra Essentials soundfont has a problem with root key for the oboe for almost all of the key ranges, cause cent correction so in excess of 25 semitomes. This is causing the sound to be very distorted. I shoud notify the author of the soundfont and see if they can fix it. In the meantime, I will avoid the oboe.
		* error checking of carnivalsque soundfont is preventing it from loading. Appears mesasesh_10 instrument has a bad end loop value. need some safe setting here
	* installation
		* The task bar icon is not the correct one. In fact, none of the icons are right except within the application
	* DSP
		* is sustain being handled properly?

# release
	* figure out where to put the installer. Windows Applications?