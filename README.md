**Original repo at https://github.com/DEATHB4DEFEAT/Void-Crew-Cheat**

# Basic how to build/run

- Clone this repo
- Install dotnet 8
- `dotnet build` (everything should just work, open an issue if not)
  - The post-build script might try to copy the dll to *my* install directory, change that in [VoidCrew.csproj](https://github.com/DEATHB4DEFEAT/Void-Crew-Cheat/blob/master/VoidCrew/VoidCrew.csproj#L33-L34) if you want, it'll still be in the bin directory
- Install BepInEx (I use version `5.4.23.2` but any v5 should work)
- Enable hiding the game object in BepInEx
- (If on Linux with Proton) Set your launch args to `WINEDLLOVERRIDES="winhttp=n,b" %command%` to load BepInEx
- Run the game to init BepInEx (probably don't *need* to do this)
- Put the compiled VoidCrew.dll into your BepInEx `Plugins` folder
  - Or, if developing, you can get the BepInEx.Debug ScriptEngine and put the VoidCrew.dll (and VoidCrew.pdb for some reason) in the `scripts` folder
- Run the game

# Features

- F1 to unlock every cosmetic
  - Achievements and DLCs will work, but won't stick around forever after you restart the game
- F2 to toggle highlighting every interactable (defaults on)
- Larger interaction distance (cba to figure out why it's not infinite)
- Infinite mutators
- Infinite perk points (saves correctly, but you can't edit them or switch loadouts without the cheat active)
- No jetpack dash cooldown
- No ability cooldowns (it looks like there is but there isn't)
- Infinite ability duration
- Allow early canceling of abilities
- Grapple items out of sockets/storage
- Instantly complete the arrow minigame
- No ship enhancement cooldowns
- Instant switches
- Instant levers

# Changelog

## 2.0.0

- Commented code
- Works with ScriptEngine, make sure you enable hiding the game object in BepInEx
  - Properly patches/unpatches when it should
- Interactable highlighting toggle (defaults on)
- Larger interaction distance
- Fixed instant switch activation
- No ability cooldowns (it looks like there is but there isn't)
- Infinite ability duration
- Allow early canceling of abilities
- Grapple items out of sockets/storage
- Complete arrow combos even more instantly (don't have to press esc or wait to fail)
- Slightly better instant activation of levers/circles
