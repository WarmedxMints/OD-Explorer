# OD-Explorer

OD Explorer is a companion app for Elite Dangerous, which aids commanders in exploration and exobiology research.

[Click here for the latest release](https://github.com/WarmedxMints/OD-Explorer/releases)

![](ODExplorerRouteView.png)

![](ODExplorerChecklist.png)
OD Explorer is not affiliated with [Frontier Developments](https://www.frontier.co.uk/), the developers of [Elite Dangerous](https://www.elitedangerous.com/).


OD Explorer requires .NET 8 x64 destop runtime which can be obtained [here](https://download.visualstudio.microsoft.com/download/pr/53e9e41c-b362-4598-9985-45f989518016/53c5e1919ba2fe23273f2abaff65595b/dotnet-runtime-8.0.11-win-x64.exe)

# Linux notes
### From [Hursofid](https://github.com/Hursofid)

If you're playing Elite Dangerous on Linux, you may get OD Explorer running using Wine.

- Install wine, [winetricks](https://github.com/Winetricks/winetricks) and [.NET 8](https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/8.0.22/windowsdesktop-runtime-8.0.22-win-x64.exe)

- Ensure corefonts are installed; otherwise, OD Explorer will crash. Use the `winetricks corefonts` command

- If you have Elite Dangerous on a separate game library, you'd need to create a symlink similar to this:
`ln -s /games/steam/steamapps/compatdata/359320/pfx/drive_c/users/steamuser/Saved\ Games/Frontier\ Developments ~/.wine/drive_c/users/<YOUR_USERNAME_HERE>/Saved\ Games/.`
