# bugs
	* play problems
		* playback of stochastic is terminating at the generator stop time not as the clouds fade away
	* tools windows not finished
		* stagger generators needs to have a specific sequence of secondarys to stagger, not the list as is now. Maybe a datagrid or itemgrid will work.
		* align generators is opening set genrators equal on OK pick
		* stagger generators is opening set generators equal on OK pick
		* set generators equal is not doing anything on OK picka and not closing
		* measure duration calculator in old format
		* oscillator frequency calculator in old format
	* messaging
	* generator
	* track 
	* algorithmic
	* timeinterval
# implementation
    * splash screen needs adjusting
	* reverb - 
		* keeping this simple, this will be a feedback-delay loop with a feedforward path for the original signal. There are two parameters, the reverb delay (number of seconds to delay the signal) and the reverb gain (how much of the output signal is feed back to the input of the delay). 
		* reverb on stochastic (cloud, voice, composition levels)
	* reporting
    * recording (and scroll roll movie recording)
		* audio
		* video (audio and soundroll)
	* documentation
		* user's guide
		* programmer's guide
# testing
	* do thorough testing of all generator parameters
	* put the DSP through its paces check all controls and algorithms.
	* Jump List Testing
	* Deferred due to file associations causing rebuild of test structure
