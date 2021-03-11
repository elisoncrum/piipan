#!/bin/bash
#
# Update package dependencies for C# projects (.csproj), recursively
# from the indicated path.
# Only minor updates are considered, unless --highest-major is specified.
#
# usage: update-packages.bash <path> [--highest-major]

#source $(dirname "$0")/../tools/common.bash || exit
source /Users/ryanmhofschneider/projects/scratch/piipan/tools/common.bash || exit

main () {
  start_path="$1"
  options="--highest-minor"
  if [ "$#" = "2" ]; then
    if [ "$2" = "--highest-major" ]; then
      options=""
    fi
  fi

  projects=$(find "$start_path" -name "*.csproj")

  while IFS=, read csproj; do
    dn=$(dirname $csproj)

    pushd "$dn" > /dev/null
      # Some projects will complain that "No assets file was found..."
      # if you try to find outdated packages before running restore.
      dotnet restore > /dev/null

      # Don't want grep to result in error code for whole pipeline if no
      # new packages found; see https://unix.stackexchange.com/a/581991
      list=$(dotnet list package --outdated $options | tr -s ' ' | { grep '>' || test $? = 1; } | cut -f 3,6 -d ' ')
      while IFS=, read line; do
        trimmed=$(echo "$line" | tr -d '[:space:]')
        if [ "$trimmed" != "" ]; then
          package=$(echo $line | cut -f1 -d ' ')
          version=$(echo $line | cut -f2 -d ' ')

          dotnet add package --version $version $package
        fi
      done <<< "$list"

      dotnet restore --force-evaluate
    popd > /dev/null
  done <<< "$projects"

  script_completed
}

main "$@"
