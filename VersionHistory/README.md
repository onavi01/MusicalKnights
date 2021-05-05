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
    * Added input detection &amp; input accuracy
* **UI:**
  * Added simple flashing border