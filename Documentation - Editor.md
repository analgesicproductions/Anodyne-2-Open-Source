

Note: I use GO as an abbreviation for Game Object.
First I'll go over Basic Tips, then I'll go over the basics of 3D/2D scenes.
# Bad Practices
* There's some coding/organizational practices to Anodyne 2 I wouldn't endorse now. Of these, the two that stick out most are

1. Using prefab instances for common objects like the player, UI, etc instead of having a separate, additively loaded scenes
2. Cutscenes being scripted *in the inspector window LOL* of the DialogueAno2 scripts, instead of being written in an external code editor, or event editing tool

Everything else is OK. Some stuff like the pause menu, the player controller, etc, could have been better organized across multiple partial C# files. The Dialogue Box is overly complicated, as is the implementation of localization which I think is easier with having text objects for every language rather than adjusting their properties through scripts. I don't think the way the vacuuming logic works was done particularly well.
# Mod Ideas
* Because you have full access to the game, modding is pretty easy especially if you've worked in Unity. You can replace FBXs (just check the animation controllers are still hooked up right), edit levels, add your own cutscene logic, etc, change the music.

1. Learn how doors work, then construct your own 3D spaces to ride around or jump in!
2. Change the character art or movement physics!
3. Learn how NPC scripts work, then try writing your own dialogue! 
4. Figure out where the dimension dive minigame data is stored, and make your own!
5. Create your own NPC areas to dive into! (Check out Wormhole.cs, and how it interacts with SparkGameController.cs . Check out SparkGame.unity's MediumPlayer -> SparkGameController -> Models for the diving data and what scenes they lead to.)
6. Make your own 2D areas by importing your own tilesets and creating levels!

...Etc!

# Basic Tips 
* Unfortunately this isn't a guide for using Unity, so I assume you have some familiarity with using it, like knowing what Play Mode is, or what a Scene is. That being said, you can still enjoy zooming around the 3D levels without any knowledge, just read a quick guide on the camera controls for Unity's scene view.

## Editing Dialogue
* Edit the english dialogue files - in Assets/Resources/Dialogue - "Ending, Intro, Misc, Desert, Dustbound, Misc, Ring" - then in Window -> My Tools, click "Make English Text" to compile the dialogue .xml file used in the game.
* Editing these files should be straightforward - the only nonintuitive thing is that the stuff like "PAL:" in Intro.txt - that points to the name table in Misc.txt when determining what character's name to show in game.

## Viewing Scenes
* Sometimes fog hides the entire scene - you can click the LAndscape-photo icon a few times at the top of the scene view to turn it off
* The weird XYZ thing in the top right of the scene can be clicked to toggle iso metric vs perspective mode. which you will need to do for 3d scenes.
* For 2D scenes, click the "2d" button at the top of the scene view
## Debug Area
* With Debug mode on, press SHIFT+D to enter the debug area making it easy to travel to most levels
## Fast Mode
* Hold 3, D, and press Left-Shift to activate debug mode (this is enabled by default when playing in Unity). Do the same to deactivate. Hold left-control to allow you to move very fast. (This might also be C? not sure)
## Setting Flags
* The game uses "Flags" to keep track of things that have happened. The **console** window will print a message any time a flag changes. 
* Use Window -> My Tools -> "Set Flag" (type in the flag name, then the value) to change flags during the game if you want to see something earlier.
* You can also tick "Auto Set" on any scene's instance of the Loader prefab's Data Loader script, then type in flag names, and all those flags will be set to 1 when starting the game in that scene.
* You can also set the "Progress Skip" value to a certain point in the story to update the game's flags to be at that point in the game.

Common Systems
* Cutscenes
* Audio/SFX
* Showing Dialogue
* Changing Scenes

# 3D Scenes

Scenes, of course, will have different things in them. But I tried to make some things consistent to make the game more manageable. We'll go through the hierarchy of Center City Cenote (CCC.unity) for the example 3D Scene.

GO = game object

* Cutscenes and Nanopoints - These deal with cutscenes, major NPCs like the ones you shrink in to. Usually these revolve on some scripts working together with the Event Script engine, "DialogueAno2". DialogueAno2 implements my own simple, custom cutscene language. It's pretty straightforward to reverse engineer and figure out if you can read C#, but if you have questions feel free to ask.
	* "VCs" refer to virtual cameras - GOs used to position the camera during cutscenes
* Chest3D - This is a treasure chest.
* SavePoint - I wonder what this is!
* MediumPlayer - Nova. This determines where she starts in the scene when editing the game, but not when playing from loading a save file.
* MedBigCam - the camera + control script. Note: This camera renders to a render texture - which is then displayed in the UI, which is how the downscaled pixely look effect works.
* SceneData - Contains SceneData and AudioHelper, which determine lighting data for a scene, and also handle looping/playing BG music.
	* AudioHelper, in each scene, generally has data on what music plays there. I would not recommend this organizational approach nowadays.
* Loader - handles player input, as well as has another AudioHelper (oh my god i'm remembering the hack here...) which contains the actual Audio Sources for SFX/music.
	* "DataLoader" manages scene transitions.. other stuff related to top-level game loop logic
	* Rewired Input Manager is where the controller support stuff would go (see the READMe for how to setup controller support again)
* UI - contains the UI for the 3d segments of the game, and pause menu
* UI Camera -  A camera object that's used so the UI has a camera to render to. This mostly never moves I think
* BigPlayer - the car
* NPCs - self-explanatory, each NPC usually has a script,trigger or two associated with it to run its logic
* Doors - Confusingly named. Some of these are DESTINATIONS from actual door triggers - so they're where you end up after exiting other levels, etc. Some of these like "DoortoCougherRoom" are actually door triggerse which take you to different levels.
* Lights - self explanatory
* Main Structure, Other, Market, Construction, Pig Alley, Park - 3D Art. I believe Marina wouldn't set things up like this nowadays
* AreaEnter-CCC - triggersthat are used to display the area's name when entering Cenote from other scenes
* BadEnd - stuff for bad ending
* Metacoins/metainfo - where the metacoins/metainfo were placed. I used a tool to place the metacoins while playing the game, then copy pasted them to this folder.



# 2D Scenes

We'll be using NanoOcean.unity as an example.
Once open, be sure to set the perspective to "Isometric" by clicking the "2D" button at the top of the scene view.

Other 2D levels have other objects as well, but these are the main ones!

* 2D Ano Player - the player. Move this to change the starting position.
* BG Layer Advanced 2D - the "ScrollParallaxPlane" is set here to specify a texture to display on top of the screen.
* UI Camera 2D - similar to UI Camera in 3D scenes.
* Loader - Same as 3D
* 2D 160px Camera - the camera that renders to the render texture, which is then displayed in the UI. This is the camera that actually scrolls around.
* 2D SceneData - same as 3D but with settings for 2D, like a hardcoded minimap
* Tilemaps - The editable tilemap for the level. Some levels have different layers, sometimes they use 8 or 16 px tiles.
* UI - Self explanatory
* NPCs - self explanatory. I guess it's worth noting that the Box Collider 2Ds on these that have "Is Trigger" selected are usually used for determining if the player's within talking range of the NPC.
* GridOverlay12 - these were used as reference when i was designing the level
* Design - contain objects relevant to the level design: enemies, gates, chests, etc. Unlike Anodyne 1, in Anodyne 2, all the enemies are just dumped into the same folder and they figure out for themselves which part of the screen they're in.