# Unity Automatic Licensor

Licensing Unity Personal via commandline is currently not supported. The only way to activate a Personal license is to interactively login and click through the licensing wizard.

Using build agents this is not always a feasible option, since you are not always able(or inclined) to do this manually. Especially for volatile systems like GitHub Actions this is a problem.

This tool allows you to license Unity with a Personal license from the command line.

## Usage

On a Windows System you can simply download, build and run the licensor natively.

```
.\UnityAutomaticLicensor.exe --username <your username> --password <your password> --unity-path <path-to-unity>\Editor\Unity.exe"
```

Using the dotnet SDK, this application can be run on linux systems as well. Start the application via:
```
dotnet run --username <your username> --password <your password> --unity-path "/<path-to-unity>/Editor/Unity"
```

If you are on a headless system and using Unity 2017 or newer you should run the licensor via ``xvfb-run``. The license agreement will not be confirmed properly if otherwise.

### Optional arguments

By default the license will be saved in the "C:\ProgramData\Unity" directory. You can specify the folder manually(e.g. for linux systems) via:
```
--unity-license-path <path-to-license-directory>
```

By default this application will try to license Unity 5.x versions. To activate newer versions which do not require to answer the activation survey, specify the correct version via
```
--unity-version <version> --unity-changeset <changeset>
```

Usually Unity will be started after obtaining a license to verify if it is valid. You can disable this behaviour via
```
--nocheck
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
