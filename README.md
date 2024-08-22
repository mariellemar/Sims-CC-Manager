![enter image description here](https://img.shields.io/github/downloads/sixstepsaway/Sims-CC-Sorter/total?style=for-the-badge)        ![enter image description here](https://img.shields.io/github/issues/sixstepsaway/sims-cc-sorter?style=for-the-badge)  ![enter image description here](https://img.shields.io/github/watchers/sixstepsaway/sims-cc-sorter?style=for-the-badge)  ![enter image description here](https://img.shields.io/github/v/tag/sixstepsaway/sims-cc-sorter?style=for-the-badge) 
# Sims CC Manager

My first real C# project, at least the first one that's turned into something that works. 

Now built in Godot 4.3.

![An image of the WIP app as it is right now.](https://64.media.tumblr.com/b122d4942504d2867a9957a2a833ab4b/a24a1a2064ec77fa-b0/s1280x1920/c0167e9431ee10027d236ca1ec39349cc50e0154.pnj)


## Premise 

An app that searches through Sims .package files and gets some info:

- The game version the package file is for. 
- Whether the file is broken.
- The name of the package file.
- What type of in-game item the package file has inside it.
- Whether the item is an override.
- If you have the mesh required by a recolor.

after that, you can decide which files you want enabled in your game and which you don't, and have separate profiles and instances for each.

## Use Cases

- You're a Sims youtuber. You play a private game sometimes too. Your YT profile will have Wonderful Whims and no adult content. Your private profile includes adult content and Wicked Whims.
- You have a medieval challenge going and want to completely overhaul your game medieval style but want to keep your modern day Pleasantview saves intact. 
- You and your sister both play Sims on the same computer. You can have an instance for your own mods and an instance for hers and keep both totally separate. You can even keep your settings separate.
- Your Sims 2 mods are named stupid things and you want to rename them all so you know what you're doing.
- You want to organize all your CC according to time period or style.
- You want to know which meshes don't have textures and vice versa and take those files out. 
- You want to manage Sims 2, Sims 3 and Sims 4 all in the same place. 
- You make CC or mods and need a clean environment to test your CC or mods out in, but don't want to lose your saves or usual set-up. 
- You want to be able to track which mods you've updated and which you haven't. 

## Current Capabilities

- Searches recursively through a folder given to it and outputs a list of which packages are *not* for the game you're trying to run them in. 
- Displays that information so you can do something with it. 
- Functioning UI shows packages in your instance folder.
- Gets Sims 2 package information! 

## Roadmap

- Add right-click menu so you can flag mods as root mods etc
- Add root capabilities lmao
- Add folder and file associations (some mods are folders not individual files)
- Add "rename from internal name", specifically for Sims 2 files, which was the whole point of this app to begin with... 

## License 

This is my first C# project worth a damn, but if you see something that's useful to you, feel free to use it in your own, just don't claim it's all yours. Link back!

## Acknowledgements 

Thanks to the following, which I have been learning from: 

- Lazy Duchess' CC Merger https://github.com/LazyDuchess/CC-Merger 
- This DBPF Editor https://github.com/noah-severyn/csDBPF 
- Delphy's Download Organizer's Source Code which made everything suddenly make sense. I'm mostly using this DBPF reader so far and retyping it myself to learn from and adjust.

