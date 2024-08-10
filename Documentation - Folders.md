
Generally speaking, with Unity games it's a little easier to open up scenes and figure out what assets relate to some given Game Object, than it is to look at the raw assets and figure out what they do.

There's two useful features unity has when you right-click an asset in the Assets window
* Select Dependencies - shows if this asset is used in other assets
* Find references in scene - shows if this asset is used in the scene

Use these features to learn how the game works!
# Top-Level

* Assets - See [[#Assets]]
* ProjectSettings - Same as other Unity Projects

# Assets
* Anodyne - See [[#Anodyne]]
* Gizmos - This is Cinemachine-related. Idk
* LocalizationScripts - Some python scripts I made to output the unique characters in a different language, so as to assist in making font assets with TextMeshPro.
* Plugins - Plugins we used. Analgesic does not own any of the IP here, but everything's free to distribute. Cinemachine was used for cutscenes, Editor has a screenshot tool + plugin for Rider (my IDE), Kartridge was for integration with that storefront, PostProcessing was for various screen effects, ProCore was for ProBuilder/ProGrids which was used for some 3D level design, TextMesh Pro was what we used for displaying text, and Tilemap was the plugin used to make the 2D levels
* Resources - See [[#Resources]]
* Standard Assets - Some standard assets from Unity

# Anodyne


* **Fonts** - self-explanatory
* **Prefabs** - Contains prefabs for things like the enemies in 2D, or certain 3D objects in 3D (Save points... Doors... whatever the heck "pb_Mesh-1475698" is...). This folder is kind of confusing to browse, it's easier to see the prefabs in-context in the various Scenes.
* **Scenes** 
	* Main Game - contains all the game's levels. You probably want this. See the Level and Scene Guide (https://github.com/analgesicproductions/Anodyne-2-Open-Source/blob/main/Documentation%20-%20Level%20and%20Scene%20Guide.md) for notes on which scenes correspond to what levels
	* The other folders: Contain the post-game, debug, metaclean framework related scenes. Some of these are art tests, some are broken, some are old controls tests
* **Scripts**
	* The organization is not great here, but generally
	* Components - misc scripts - stuff that makes objects sparkable, or vacuumable
	* Controllers - Lets the player move in various contexts- 2D, 3D... or the "Shrinking to nanoscale" game
	* Editor - some tools I made for localizing / setting flags in-game
	* Entity - enemies, interactable 3D things, boss logic
	* global - Managing dialogue, helper functios, input management, global state management, save files
	* NPC - Stuff related to running events.
		* DialogueAno2 - this is the confusingly-named event engine for the game (this is what runs cutscenes). E.g. if you open CCC.unity and search "PrismDiveTrigger" you'll see the event script for that cutscene. This is a pretty complex but vital script to the game
		* NPCHelper - one-offs for npc logic in various 2d levels
		* NPCHelper3D - same as above but uh.. only for the drumbird?
	* One-offs - credits, the bad ending logic?
	* ShootGame - prototype code for something where you used to move and shoot in 3D i think
	* UI - Anything related to menus, dialogue
	* Utility - audio triggers, camera scrolling code, doors, scene metadata (SceneData, SceneData2D), sprite animation, other triggers,cutscene camera helpers - just a lot of misc. stuff to get the game working 
	* Visual - a lot of other misc scripts that are related to graphics, rotating objects, drawing fake shadows, the little audio visualizer in the UI, the "wormhole" effect when shrinking into levels.
* Terrain
	* The files used with Unity's terrain system for the 3D levels.
* Timeline
	* The assets used with Unity's Timeline system for some cutscenes like talking to the Chalaza or something. I didn't like this tool so I only used it for a few scenes, stuff like the bowing/jump sequence when nova shrinks into a character
* Visual
	* Mots of the 3D/2D art assets for the game, except mainly for sprites which are in Resources which needed to be instantiated during the game for various reasons. Stuff like materials, textures, models, animation controllers are here
	* In particular, the game's tilesets are all here.

# Resources
Generally, this folder has assets that needed to be loaded at arbitrary times or places during the game.

* Audio - Self explanatory - all the music and SFX
* Dialogue - all the game's text in each language
* Prefabs - More prefabs which needed to be instantiated, like poofs
	* For some reason this also contains important prefabs like "2D UI" (the 2d ui) or "MediumPlayer" (the controller for nova)
* Shaders - Various shaders used. of interest might be the hacky ones we used to add blend modes for some sprites (BG_Layer_Advanced)
* Visual - most of  the 2D sprites
