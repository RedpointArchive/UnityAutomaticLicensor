# Unity Automatic Licensor

Unity doesn't support automatically licensing installations with Personal licenses. The only way to activate a Personal license is to interactively login and click through the licensing wizard.

This causes a problem for Windows build agents that are automated with Packer, or are otherwise dynamically spun up on public cloud infrastructure.

This tool allows you to license Unity with a Personal license from the command line. It requires you to have Unity 5.4.1f1 installed, but once you've licensed that version of Unity, any newer version of Unity should detect the license file for 5.x and automatically upgrade it to the newer license file version if needed.

## Usage

Download a release from the Releases page, and extract it somewhere on your Windows system.

Then run it like so:

```
.\UnityAutomaticLicensor.exe --username <your username> --password <your password> --unity-path "C:\Program Files\Unity\Editor\Unity.exe"
```

## Building from Source

You can build your own copy of the application with:

```
dotnet publish -c Release -r win10-x64
```

## License

This code is licensed under the MIT license.

## Support

There's absolutely NO SUPPORT for this software. Use it at your own risk.