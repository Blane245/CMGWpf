# Version 4.1.0
## bugs
	Changed duration of last note in an algorithmic generator to full length rather than truncating at the stop time. 
	Fixed handling of voice volume and velocity in stochastic generator
	Corrected DSP when decay time is short and when note interval is less than a specified duration
	Corrected loading and playing of compositions with sequencer algorithms
	Moved stochastic parameter 'events/sec' for the dynamic parameters to the voices. Each voice can now have a different cloud density.
	Improved and corrected the Ensemble Voice and Note Sequencer Edit UIs
	Implemented time snapping for generators
	Removed dead signal from audio signal leaving up to 1 second of silence at the end of a composition
	Cleaned up the user settings file to remove unused parameters