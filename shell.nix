{
  pkgs ? (
    let
      lock = builtins.fromJSON (builtins.readFile ./flake.lock);
    in
    import (builtins.fetchTarball {
      url = "https://github.com/NixOS/nixpkgs/archive/${lock.nodes.nixpkgs.locked.rev}.tar.gz";
      sha256 = lock.nodes.nixpkgs.locked.narHash;
    }) { }
  ),
}:

let
  dependencies = with pkgs; [
    dotnetCorePackages.sdk_8_0
  ];
in
pkgs.mkShell {
  name = "bepinex-devshell";
  packages = dependencies;
  shellHook = ''
    export GLIBC_TUNABLES=glibc.rtld.dynamic_sort=1
    export ROBUST_SOUNDFONT_OVERRIDE=${pkgs.soundfont-fluid}/share/soundfonts/FluidR3_GM2-2.sf2
    export XDG_DATA_DIRS=$GSETTINGS_SCHEMAS_PATH
    export LD_LIBRARY_PATH=${pkgs.lib.makeLibraryPath dependencies}
    export DOTNET_ROOT=${pkgs.dotnetCorePackages.sdk_8_0}
    export PATH="$PATH:/home/$(whoami)/.dotnet/tools"
  '';
}
