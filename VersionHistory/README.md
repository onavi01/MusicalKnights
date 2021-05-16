## Version History

### v0.2.20 Patch Notes 
* Fixed a bug that caused player command chains to be accepted even if the internal mistakes per turn limit was reached
* Adjusted which GUI elements were toggled on/off by controls
  * The basic controls are no longer shown/hidden by 'Help'
  * The command strings above each Commander's head are no longer shown/hidden by 'Debug' and are no longer considered debug information
  * The lane marker icons can now be shown/hidden by 'Debug' and are considered debug information
* Massive code overhaul across all components in preparation for the future updates; should not visibly affect gameplay
* Updated to Unity 2020.3.8f1

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

### v0.1.20 Patch Notes 
* Fixed a bug that allowed the player to input more than one commands in a single beat
* Improved code across several components
    * Transferred the responsibility of calculating the distance between lane markers to the main lane interface
    * Large overhaul of player commander AI
    * Allowed other game elements to more easily determine whose turn it is

### v0.1.15 Patch Notes 
* Improved code across several components
    * Refactored how friendly & enemy troops were spawned
    * Refactored how friendly & enemy troops keep track of their current lane and position while they march
    * Refactored & unionized how game elements keep track & follow the internal metronome

### v0.1.10 Patch Notes
* Added text UI to display input accuracy
* Refactored how children GameObjects were created

### v0.1.0 Minor Update Notes
* **Conductor System/Audio:**
  * Added internal metronome
  * Added calibration support (currently inaccessible)
  * Added main music tracks (warning track currently unused)
* **Game Mechanics:**
  * Added lanes
  * Added basic soldiers
  * Added simple soldier deployment
  * Added main command system
    * Added input detection & input accuracy
* **UI:**
  * Added simple flashing border