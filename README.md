# Unity Automatic Licensor

Unity doesn't support automatically licensing installations with Personal licenses. The only way to activate a Personal license is to interactively login and click through the licensing wizard.

This causes a problem for build agents that are automated with Packer, or are otherwise dynamically spun up on public cloud infrastructure.

This tool allows you to license Unity with a Personal license from the command line.

## Usage

Download a release from the Releases page, and extract it somewhere on your Windows system.

Then run it like so:

```
.\UnityAutomaticLicensor.exe --username <your username> --password <your password> --unity-path "C:\Program Files\Unity\Editor\Unity.exe"
```

Using the dotnet SDK, this application can be run on linux systems as well. Start the application via:
```
dotnet run --username <your username> --password <your password> --unity-path "/<path-to-unity>/Editor/Unity"
```

The location of the license file can be specified via
```
--unity-license-path <path-to-license-directory>
```

By default this application will try to license Unity v5.x versions. To activate newer versions which do not require to answer the activation survey, specify the correct version via
```
--unity-version <version> --unity-changeset <changeset>
```

## Building from Source

You can build your own copy of the application with:

```
dotnet publish -c Release
```


## License

This code is licensed under the MIT license.

## Support

There's absolutely NO SUPPORT for this software. Use it at your own risk.
