### Download

Latest official release ('MusicalKnights.exe') is located within the 'MusicalKnights...' folder. To run, make sure the entire 'MusicalKnights...' folder is downloaded, then execute the file from within.

### v0.2.0 Minor Update Notes
* **Conductor System/Audio:**
  * Updated soundtrack
  * Added support for count-in beats & initial lead offset; now the song won't immediately blare into players' ears as soon as the game loads
  * Added SFX to denote the beats that fall within the player's turn
  * Added SFX for troop attacking
* **Game Mechanics:**
  * Expanded upon lane-based fighting system
    * Troops now respect marker occupancy; only one troop can stand at a marker at a given time
    * When two opposing troops meet, the troop that more quickly follows a 'March' or a 'Charge' order from their Commander gets to attack the other troop
    * Troops cannot be spawned/deployed on the left- & right-most "spawner" lane markers of each lane if they are occupied, regardless of the allegiance of the occupying troop
  * Improved Adversary Commander AI; the Adversary Commander will now avoid spawning troops in occupied "spawner" markers
  * Added a strict limit as to the amount of mistakes the player is allowed to make in a single turn; if this limit is reached, the player immediately fails the turn
* **UI:**
  * Added pause menu
    * Added the ability to adjust input calibration
    * Added the ability to quit the game
  * Added the ability to show/hide the controls
  * Added the ability to toggle on/off various text-based debug info
* **Technical:**
  * Updated to Unity 2020.3.7f1
  * Mapped command inputs to actual keybinds to allow for easier rebinding in the future
    * Enabled the use of the directional arrow keys for command input
  * Consolidated various classes, including the Friendly & Enemy troop classes, to improve readability & workflow
  * Improved & reorganized how the Unity Inspector displayed various classes and their properties
  * Large overhaul of code across several classes & components

##### *Note: Full version history located within the 'VersionHistory' folder.*