This is a complete repository of Anodyne 2, the 2019 game made by Analgesic Productions, compiled by original programmer and co-developer Melos Han-Tani in August 2024 for the game's 5th Anniversary. Find me on Twitter [https://twitter.com/han_tani](https://twitter.com/han_tani)! Check out this account's other projects for other things I've 99%-open sourced from my games. Note this is technically not Open Source because there's a clause preventing usage that would screw me over as an independent developer. But for most reasonable use cases it's effectively open source.

[Trailer](https://www.youtube.com/watch?v=yT6yQm3JM3E)

- Installation.md: Installation info
- Documentation - Level and Scene Guide.md: Info on which Unity Scenes are which levels
- Documentation - Editor.md: Explanation of using the Editor to make and change levels
- Documentation - Folders.md: Explanation of folder structure
- LICENSE.md and LICENSE_FOR_DERIVATIVES.md for licenses
- FAQ.md for common questions
- Talk to us on Discord ([https://discord.com/invite/analgesic](https://discord.com/invite/analgesic)) or e-mail ([hello@analgesic.productions](mailto:hello@analgesic.productions))

Note that while anyone is free to browse the game assets and codebase, only paid owners of the game may download and use the game assets (see below for detail.) Anyone can use the source code, though!

Purchase it [here on Steam](https://store.steampowered.com/app/877810/Anodyne_2_Return_to_Dust/). It's also on consoles like [Switch](https://www.nintendo.com/us/store/products/anodyne-2-return-to-dust-switch/), and other stores (PS4, Xbox, Itch, GOG, Epic, Humble).

Please check out our upcoming (at time of writing, 8/2024) game, [Angeline Era.](https://store.steampowered.com/app/2393920/Angeline_Era/)

# Changes from Base Game
## No Controller Support
* This version has no controller support, because the Rewired library is used, which is a paid Unity plugin. There are two easy ways to re-add controller support if you're familiar with C# coding:
* 1. Replace MyInput.cs with your controller library of choice. You simply need to assign the "just pressed ..." or ".. button is held down" bools in MyInput, with the proper data from your input library of choice.
* 2. Buy and set up ReWired yourself:
	* Use the zipped ControllerDataFiles.asset for Anodyne 2 in the repo.
	*  You'll also need to add the Rewired InputManager script to the 'Rewired Input Manager' GameObject that's in the Loader prefab.
	* Uncomment the "REWIRED_OPENSOURCE" stuff in MyInput.cs

## Technical
* Anodyne 2 has Kartridge integration (an old store) and Steam integration, however I'm not sure if I can distribute those freely so they've been removed. The calls to the Steam APIs are still included, though.
* This game has no console support (that was done by Ratalaika and we don't have that source code.)

## Other
* There are occasional occurences of old names of Marina and I's, that I didn't want to change bc it might break some things. We don't use these names so please ignore them!

# Legal Notes 
* We don't own anything in the Plugins folder, however, they are all freely available plugins so I left them there
* We don't own any of the Font assets. AFAIK these were all free to use, but just note we don't own those fonts and are only distributing them so the game doesn't display everything in times new roman or something
