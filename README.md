# Basic how to build/run

- Clone this repo
- Install dotnet 8
- `dotnet build` (everything should just work, open an issue if not)
  - The post-build script might try to copy the dll to *my* install directory, change that in [VoidCrew.csproj](https://github.com/DEATHB4DEFEAT/Void-Crew-Cheat/blob/master/VoidCrew/VoidCrew.csproj#L30) if you want, it'll still be in the bin directory
- Install BepInEx (I use version `5.4.23.2` but any v5 should work)
- (If on Linux with Proton) Set your launch args to `WINEDLLOVERRIDES="winhttp=n,b" %command%` to load BepInEx
- Run the game to init BepInEx
- Put the compiled VoidCrew.dll into your BepInEx `Plugins` folder
- Run the game

# Features

- F1 to unlock every cosmetic
  - Achievements and DLCs will work, but won't stick around forever after you restart the game
- Infinite perk points (saves correctly, but you can't edit them or switch loadouts without the cheat active)
- No jetpack dash cooldown
- Instant cryptic success (the arrow minigame)
- Ignore enhancement cooldowns
- Infinite mutators (I'm bad at the game so idk if this is even necessary)
- Instant switches
- Instant levers
