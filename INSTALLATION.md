This is only tested on Windows 10, but Unity is pretty good about cross-platform so it probably works on Mac, too.

# Installation

1. Download this repo. It might take a while because there are like 10,000 tile files because unity stores every tile as a separate file because it's a silly engine.
2. [Install Unity 2017.4.31f1 (64-bit). ](https://unity.com/releases/editor/archive). I don't see any reason why Anodyne 2 wouldn't work in a newer Unity version or Mac OSX, but just FYI it's only ever been tested in this version.
3. Optionally, purchase and download ReWired if you want to re-integrate controller support
4. That's it. Wow! Actually you have to wait for unity to import all these files the first time you open the project, which takes a long time.

# Exporting the Game as a standalone EXE
1. This is only necessary if you want to export your own versions, mods, etc.
2. Open Build Settings (Ctrl Shift B), ensure all 62 scenes are there and checked. 
3. Change the Player Settings if you want to.
4. Hit Build! This will take a while due to shader compilation.
