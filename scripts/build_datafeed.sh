#!/usr/bin/env bash
set -euo pipefail

configuration="${1:-Release}"
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/.." && pwd)"
cvr_path="${CVRPATH:-}"
cvr_executable="ChilloutVR.exe"

default_paths=(
    "$HOME/.local/share/Steam/steamapps/common/ChilloutVR"
    "$HOME/.steam/steam/steamapps/common/ChilloutVR"
    "$HOME/.var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/ChilloutVR"
)

if [[ -z "$cvr_path" || ! -f "$cvr_path/$cvr_executable" ]]; then
    cvr_path=""
    for path in "${default_paths[@]}"; do
        if [[ -f "$path/$cvr_executable" ]]; then
            cvr_path="$path"
            break
        fi
    done
fi

if [[ -z "$cvr_path" ]]; then
    echo "[ERROR] ChilloutVR.exe not found in CVRPATH or the common Linux Steam locations." >&2
    echo "        Export CVRPATH to the ChilloutVR folder, then run this script again." >&2
    exit 1
fi

dotnet build "$repo_root/DataFeed/DataFeed.csproj" \
    -c "$configuration" \
    -p:OutputPath="$cvr_path/Mods/" \
    -p:SolutionDir="$repo_root/"
