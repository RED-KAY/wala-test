Unity Developer Test:

===================================================================================

How to Submit:

- Make a private Fork of the Repository.
- Complete the Tasks and push the changes to your personal repository.
- Download the Source Code as .zig and mail it to us.

===================================================================================

Tasks:

    Bugs to Fix:

        - Game is Not playable from "IntroMenu" Scene.

        - Weapon "Weapon_Launcher" is not functioning.

        - "Enemy_Turret" is getting destroyed at the start of game.


    Features To Add:

        -Add the ability to assign color to compass makers according to enemy type.
 
        -Make the Player slam down on the ground if it starts crouching will using Jetpack.
	        # Player should receive damage according to its height, other then the fall damage.
	        # Add field to play a SFX on Slam.

        -Make enemy "Enemy_HoverBot_v2" move up or down to dodge whenever it takes damage.
	        # Fields:
		        Speed- How fast the bot will move.
		        Offset- Bot be at 3 positions, its initial position in prefab and +/- offset of it.
		        (It can dodge to any of the two position other the current)
		        Easing Curve- To be able to adjust the motion of the dodge.