---
name: shared-package-local
description: Build and test the backend services against unpublished changes in Backend/Shared (InnoClinic.Core, InnoClinic.AspNetCore, InnoClinic.Contracts.Grpc, InnoClinic.Messaging) using a temporary local NuGet feed. Use whenever code in Backend/Shared changes and a service must compile or run against it.
---

# Test unpublished Shared changes via a local feed

Services consume `Backend/Shared/*` as **NuGet packages** (versions in `Backend/Directory.Packages.props`, feed in `Backend/nuget.config`), not as project references. Editing `Shared/` has no effect on a service until a new package version is restored.

Rules:
- Keep everything under the git-ignored `Backend/obj/localfeed/`. Use a separate `--packages` folder so the global NuGet cache (`~/.nuget/packages`) never gets a local build.
- **Never** run `dotnet nuget push` or publish to the GitHub feed. Publishing is the user's call.
- The `<Version>` and `Directory.Packages.props` edits are temporary. Revert them when done unless the user wants to keep the version bump.

All commands run from `Backend/`. `NUGET_USER`/`NUGET_TOKEN` must be set; the GitHub feed still serves the shared packages you didn't change.

## 1. Pick the local version(s)

Use a prerelease of the **next** version, e.g. Core `2.0.2` → `2.0.3-local.1`. A prerelease of the *same* version (`2.0.2-local.1`) sorts below `2.0.2` and fails restore with `NU1605 Detected package downgrade`, because other shared packages depend on `>= 2.0.2`.

Set it by editing `<Version>` in each changed shared csproj, e.g. `InnoClinic.Core.csproj` `2.0.2` → `2.0.3-local.1`. **Don't pass `-p:Version=...` to `dotnet pack`.** It's a global property that also applies to referenced projects. Packing `InnoClinic.AspNetCore` with `-p:Version=2.0.4-local.1` makes its package depend on `InnoClinic.Core 2.0.4-local.1`, which doesn't exist, so restore fails.

## 2. Pack the changed package(s), dependencies first

```bash
mkdir -p obj/localfeed/feed
dotnet pack Shared/InnoClinic.Core -c Release -o obj/localfeed/feed
dotnet pack Shared/InnoClinic.AspNetCore -c Release -o obj/localfeed/feed   # only if it changed or must pick up the Core change
```

`InnoClinic.AspNetCore` references `InnoClinic.Core` as a project reference, so its package depends on whatever `<Version>` Core's csproj has. If only Core changed, you don't need to repack AspNetCore: the published AspNetCore requires Core `>= 2.0.2`, which the local `2.0.3-local.1` satisfies. Repack AspNetCore (with its own `-local.N` version) only when it changed, or when it must compile against the changed Core API.

## 3. Local nuget.config (`obj/localfeed/nuget.config`)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/%NUGET_USER%/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
    <packageSource key="local"><package pattern="InnoClinic.*" /></packageSource>
    <packageSource key="github"><package pattern="InnoClinic.*" /></packageSource>
  </packageSourceMapping>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="%NUGET_USER%" />
      <add key="ClearTextPassword" value="%NUGET_TOKEN%" />
    </github>
  </packageSourceCredentials>
</configuration>
```

`value="feed"` is relative to this config file.

## 4. Point the services at the local version, then restore and build

Temporarily set the version(s) in `Directory.Packages.props` (e.g. `InnoClinic.Core` → `2.0.3-local.1`), then:

```bash
dotnet restore InnoClinic.<Service>.slnx --configfile obj/localfeed/nuget.config --packages obj/localfeed/packages --force
dotnet build InnoClinic.<Service>.slnx --no-restore
dotnet test --solution InnoClinic.Profiles.slnx --no-build   # Profiles only has tests
```

`--force` is required. An open Visual Studio or Rider auto-restores as soon as `Directory.Packages.props` changes, using the normal config, which fails with `NU1102`. Without `--force`, NuGet then treats those projects as already restored and skips them, so the build fails with `NU1102` for `>= 2.0.3-local.1`.

Do this for every service that uses the changed package. To confirm the local package was used everywhere, check that every `<Project>/obj/project.assets.json` lists `InnoClinic.Core/2.0.3-local.1` under `libraries`, with `obj/localfeed/packages` as its package folder.

## 5. Clean up

```bash
git restore -- Directory.Packages.props   # unless the user wants the bump kept
# Set each shared csproj <Version> back by hand (or to the real bump). Don't git restore those csproj files:
# that would also discard real changes made to them as part of the shared change.
dotnet restore InnoClinic.<Service>.slnx --force   # back to the normal feed and global cache; do this for every solution
rm -rf obj/localfeed
```

Then tell the user what to publish: which shared csproj `<Version>` values to bump, and which `Directory.Packages.props` entries to update after publishing.
