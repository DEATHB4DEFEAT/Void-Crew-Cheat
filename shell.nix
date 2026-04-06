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
    export LD_LIBRARY_PATH=${pkgs.lib.makeLibraryPath dependencies}
    export DOTNET_ROOT=${pkgs.dotnetCorePackages.sdk_8_0}
    export PATH="$PATH:/home/$(whoami)/.dotnet/tools"
  '';
}
