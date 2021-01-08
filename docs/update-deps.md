# Updating .NET dependencies

The .NET ecosystem and the GitHub Dependabot have a few rough edges as far as updating dependencies go:
* [.NET dependency pinning/locking isn't straight forward](https://github.com/18F/piipan/pull/158)
* [Dependabot doesn't support lock files](https://github.com/18F/piipan/pull/165#issuecomment-752654442)
* [.NET tools don't update lock files when we'd expect them to](https://github.com/18F/piipan/pull/183#pullrequestreview-563530549)

One consequence is that Dependabot PRs can not be directly used to update our dependencies. They can only merely alert us that we must manually run the process below.

## Steps
1. For each affected source/test tree (e.g., directory with a `.csproj`), run: 
```
    dotnet list package --outdated
```
2. For each out-of-date package listed, run:
```
    dotnet add package <PACKAGE_NAME>
```
3. Update the package lockfile:
```
    dotnet restore --force-evaluate
```
4. Merge in updated `.csproj` and `packages.lock.json` files. The Dependabot PRs will automatically rebase and close themselves.

## Notes

*  Adding a completely brand new package with `dotnet add package` will update the project's corresponding lock file without any subsequent commands.

## References

* [`dotnet list package`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-list-package)
* [`dotnet add package`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-add-package)
