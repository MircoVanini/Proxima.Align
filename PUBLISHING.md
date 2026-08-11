# Publishing checklist

The repository and GitHub workflow produce a Marketplace-ready VSIX but do not
upload or publish it automatically.

## Build and verify

```powershell
dotnet restore .\Proxima.Align.slnx
dotnet test .\Proxima.Align.slnx -c Release --no-restore --nologo
dotnet build .\Proxima.Align.slnx -c Release --no-restore --nologo
.\scripts\Verify-Vsix.ps1
```

Upload this file when ready:

```text
src\Proxima.Align\bin\Release\net8.0-windows8.0\Proxima.Align.vsix
```

## Suggested Marketplace fields

| Field | Value |
| --- | --- |
| Publisher | Proxima Software |
| Internal name | proxima-align-assignments |
| Display name | Proxima Align Assignments |
| Type | Tools |
| Categories | Coding Tools, Productivity |
| Pricing | Free |
| Supported editions | Community, Professional, Enterprise |
| Supported versions | Visual Studio 17.14 and later |
| Repository | https://github.com/MircoVanini/Proxima.Align |
| Tags | alignment, assignments, formatting, productivity |

Use [MARKETPLACE.md](MARKETPLACE.md) as the overview and
`docs/images/marketplace-banner.png` as the listing image.
Confirm that the banner is present on the public `develop` branch before
copying the overview, so its absolute image URL resolves on Marketplace.

## Marketplace steps

1. Sign in at https://marketplace.visualstudio.com/vs.
2. Open **Publish extensions** and select the `Proxima Software` publisher.
3. Choose **New extension > Visual Studio**.
4. Upload the verified Release VSIX.
5. Complete the listing using the fields above, the overview, and the banner.
6. Save the listing privately and install it from Marketplace for a final check.
7. Use **Make Public** only when the private listing has been validated.

For updates, keep the VSIX ID unchanged and increment `Version.props` before
building a new package.
