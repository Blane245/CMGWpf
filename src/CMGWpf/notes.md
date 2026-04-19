# bugs
	~~* play problems
		~~* playback of stochastic is terminating at the generator stop time not as the clouds fade away. Partial fix. Looks like cloud wen too far.~~
		~~* play is not building the selected stochastic generator~~
		~~* stochastic audio playback is not matching soundroll. Test case is SW Duet and bass voice. Problem seem to be related to crossing time cell boundaries. (problem was bad systainvolenv values in the SF).~~
		~~* no spinning cursor when play invoked from genertor edit dialog~~
		~~* need to validate generator when Play is selected during edit. Currently, if the generator is invalid, it will not play or an exception occurs due to assumption that generator is valid but the user won't know why. Use the error control to display the error message and prevent play until the generator is valid.~~
		~~* remove reverb from all but composition level. It really makes no difference what level it is applied as it effect all voices the same at all levels. ~~
		~~* for large compositions a progress bar with cancel option is needed.~~
		~~* playing a single generator seems to have playback timeline problems. See thelandremains gong~~
		~~* thelandremains stochastic is getting an exception that the buffer is too small. May be related to muted voices.~~
		* sound scroll for each sound does not include the release portion of the sound.
	* tools windows not finished
		* stagger generators needs to have a specific sequence of secondarys to stagger, not the list as is now. Maybe a datagrid or itemgrid will work.
		* align generators is opening set genrators equal on OK pick
		* stagger generators is opening set generators equal on OK pick
		* set generators equal is not doing anything on OK picka and not closing
		* measure duration calculator in old format
		* oscillator frequency calculator in old format
	* messaging
		* no status message 
			generator edit completes with save or cancel
			play completes
	* generator
		~~* error message area not scrolling~~
		* changing starttime automatically changing stop time is not working. I've tried a couple of ways. Looks like I will need Inotification to make it happen
		~~* stochastic dialog opening problem. Appears to be mismatch between the read composition array and the voice array.
		~~* Old CMG format stores composition as flat comma-separated list, now properly reshaped to 2D array [timeCells][voices]~~
		* canceling a generator edit does not restore original Generator values to the UIGenerator. I tried in Cancel_Click but didn't completely work. Need to fix GeneratorDialog_Closing, too.
		* check noteShift handling. Looks like it got missed in the refactor.
		* check noise, tremolo, vibrato frequency units. Looks like they got missed in the refactor.
	* track
	* timeline
		* see thelandremains.cmg startup timeline. It has values like 4:80?
	* timeinterval
	* others
		the Orchesta Essentials soundfont has a problem with root key for the oboe for almost all of the key ranges, cause cent correction so in excess of 25 semitomes. This is causing the sound to be very distorted. I shoud notify the author of the soundfont and see if they can fix it. In the meantime, I will avoid the oboe.
# implementation
	* splash screen needs adjusting
	~~* reverb - ~~
		* keeping this simple, this will be a feedback-delay loop with a feedforward path for the original signal. There are two parameters, the reverb delay (number of seconds to delay the signal) and the reverb gain (how much of the output signal is feed back to the input of the delay). 
		* reverb on stochastic (cloud, voice, composition levels)
		* change units on delay time to msec rather than seconds. better resolution is needed 
	~~* reporting~~
	~~* recording (and scroll roll movie recording)~~
		~~* audio~~
			* WAV format implemented and working
			* MP3 format implemented and working (requires NAudio.Lame package)
		~~* video (audio and soundroll) ~~
			* MP4 format implemented and working (requires FFMpegCore package)
			* Uses virtual frame rendering at 30fps with synchronized audio
			* Generates frames by programmatically scrolling soundroll
			* No real-time requirement - can generate at any speed
	~~* documentation~~
		~~* user's guide~~
		~~* programmer's guide~~
# testing
	* do thorough testing of all generator parameters
	* put the DSP through its paces check all controls and algorithms.
	* Jump List Testing - Deferred due to file associations causing rebuild of test structure
# generator submit/cancel fiasco
	* the generator name and starttime/stoptime have special handling that complicates cancel and submit
	* the generator rename uses a newgenertorname filed on the UI. 
		* When a cancel occurs, the value of this property should not change as the UI.name should not change. When the UI is reverted back to the generator a property change shoudl be signaled on the UIgenerator to restore its values on the UI.
		* on submit, the UIgenerator name should change to newgeneratorname as is the generator's name. Both should have property change raised.
	* when the user changes the start time, the stop should change to maintain the duration. This is supported by a newStartTime property
		* when the user changes newStartTime, the the UI stoptime should change to maintain the interval and the UI startttime changed to the newstarttime value. A UIgenerator property change is raised to allow the stop time to update on the display
		* when the user cancels, the UIgenerator should be restored to the Generator and the newstartime changed to Generator.starttime. This will cause the UIGenerator stop time to change so it will have to be restored the the Generator.stoptime 
		* on submit, the UIgenerator is copied to the generator and the newstarttime to the generator. The newstarttime is copied to the UIgenerator. Both generator and UIgenrator get property changed signals. 
# buffer allocations and merging for stochastic generators
(completed)
	* at the bottom of the buffer chain is the instrument mono samples as they are built from the preset instruments. These sample are counted for 0 through the instrumentsample length. This length is determined by the gain envelope. The gain envelope is determined by the attack, decay, sustain, release parameters and the note duration.So the buffer size will be releaseTime * sampleRate. This will be >= duration * sampleRate. The real time location of the instrument's samples do not need to be known during their generation as they are just counted from 0 to the length of the sample. This is handled by the cloud builder. 
	* when the instrument samples are returned to the cloud builder, they must be placed in the cloud at their proper place. The cloud time is is based on time cell and extends to twice the time cell size to accomodate for sample that might extend into the next cell. the variable t1 ranges from 0 to cellsize, cellsize<=t2<2*cellsize. As the instruments are produced, they are placed in the cloud buffer at t1 * sampleRate. There may be an extensions of the cloudbuffer due to long release times.
		
		So the real time location of a cloud element is the start time of the generator plus the cell time plus the t1 time of the sample and its end time is this real time location plus the instrument's sample length divided by the sample rate.
	* Each cloud is sent as a 2-channel stereo buffer to the Get routine with possibly pan applied at the cloud level. This routine is assembling the clouds for all time cells for all voices into a composition (stereo) buffer. The composition buffer time is based on the generator start time and is initially allocated at one time cell larger than the length of the composition times 2 for stereo. This may be extended due to long releases from one or more clouds. The cloud buffers are added to the composition buffer at the proper location based on the the cell time. 

		So the real time location of the composition buffer is the start time of the generator and its end time is the composition start time plus the length of the composition buffer divided by the 2 * sample rate. 
	* Finally, the composition buffer added to the stereo buffer at the start time of the generator. The real time location of the stereo buffer is the start time of the generator and its end time is the start time plus the length of the stereo buffer divided by the sample rate.
		