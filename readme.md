# Description

Procedural Pokemon Randomizer is a Pokemon randomizer I'm creating as a fun side project!

- This Randomizer focuses on using procedural generation and weighted random choices to create randomized ROMs that feel more natural
- Some of my favorite new features are:
	- Type-Variant Pokemon: Random type-variants of pokemon with corresponding color palettes, movesets, base stats, and other modifications!
	- Intelligent trainer pokemon choices: Challenge runs are more fun when you can plan for a fisherman to usually have WATER pokemon!
	- Intelligent wild pokemon choices: Find pokemon where they make sense (water pokemon when fishing, electric pokemon in the power plant, etc)!
	- Reoccuring trainer logic: trainers that you see multiple times (like rivals, etc.) will raise a team that keeps members but grows and evolves over the course of the game.
	- Battle type randomization: randomize the battle type of trainers you encounter! I prefer all double battles.
	- The Dunsparse Plague: anything can evolve into Dunsparse... half of the time. Affects NPCs too!
	- Bonus Moves: add bonus moves to pokemon movesets at a level that makes sense. Can draw moves from the pokemon's egg moves or make a random choice!
	- ...And much more!

Originally based off of source code from Artemis251's Pokemon Emerald Randomizer (v.2.2 -- 13 April 2014) but I pivoted to C# so now its just a reference.
Check out their website! http://artemis251.fobby.net/downloads/emerald/

Another project that's been a great reference for writing this is dabomstew's Universal pokemon randomizer.
Check out their project! https://github.com/Dabomstew/universal-pokemon-randomizer

# Rom Support

All unmodified Gen III english Roms:
- Emerald (BPEE0)
- Ruby/Sapphire v1.0, v1.1, and v1.2 (AXVE/AXPE 0/1/2)
- Fire Red / Leaf Green v1.0 and v1.1 (BPRE/BPGE 0/1)

# Releases

See the [GitHub Releases](https://github.com/ValentinoAbate/Pokemon-Randomizer/releases) tab

### OS Support

Windows

### Version Number Convention

Major.Minor.Patch

Major: this number will increment when a major update happens

Minor: this number will increment when a minor update happens

Patch: this number will increment when a fix has been made to a released build. If the version has no patches, this number will be omitted (e.g 2.1 == unpatched, 2.1.8 == version 2.1 patch 8)

# Install Instructions

1: Go to the Relases tab of this repository, and scroll to the "Assets" section of the latest release

2: Download the ProceduralPokemonRandomizer.[version].zip file, and extract its contents to whatever directory you'd like

3: Double click on the ProceduralPokemonRandomizer.exe file in the extracted folder to start the application

*Note: the Procedural Pokemon Randomizer runs on .Net 5. If you do not have the .Net 5 Desktop Runtime installed, the application will not run, and will prompt you to install the appropriate .Net Desktop Runtime.
 If you need to install .Net, you will need to restart your computer after installing the appropriate .Net Desktop Runtime* 


# Liscense

Licensed under the GNU General Public v3.0 license. See the LICENSE file for details!
