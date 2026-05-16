#!/usr/bin/env bash
set -euo pipefail

silent=false
skip_nstrip=false

for arg in "$@"; do
    case "$arg" in
        --silent)
            silent=true
            ;;
        --skip-nstrip)
            skip_nstrip=true
            ;;
        *)
            echo "Unknown argument: $arg" >&2
            echo "Usage: $0 [--silent] [--skip-nstrip]" >&2
            exit 2
            ;;
    esac
done

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "$script_dir/.." && pwd)"
managed_libs_folder="$repo_root/.ManagedLibs"
cvr_path="${CVRPATH:-}"
cvr_executable="ChilloutVR.exe"

default_paths=(
    "$HOME/.local/share/Steam/steamapps/common/ChilloutVR"
    "$HOME/.steam/steam/steamapps/common/ChilloutVR"
    "$HOME/.var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/ChilloutVR"
)

if [[ -n "$cvr_path" && -f "$cvr_path/$cvr_executable" ]]; then
    echo
    echo "Found the ChilloutVR folder at: $cvr_path"
else
    cvr_path=""
    for path in "${default_paths[@]}"; do
        if [[ -f "$path/$cvr_executable" ]]; then
            cvr_path="$path"
            export CVRPATH="$cvr_path"
            echo "Found ChilloutVR at: $cvr_path"
            echo "Set CVRPATH for this shell session."
            break
        fi
    done
fi

if [[ -z "$cvr_path" ]]; then
    echo "[ERROR] ChilloutVR.exe not found in CVRPATH or the common Linux Steam locations." >&2
    echo "        Export CVRPATH to the ChilloutVR folder, then run this script again." >&2
    exit 1
fi

mkdir -p "$managed_libs_folder"

echo
echo "Copying DLLs from CVR and MelonLoader into .ManagedLibs"

required_files=(
    "$cvr_path/MelonLoader/net35/0Harmony.dll"
    "$cvr_path/MelonLoader/net35/MelonLoader.dll"
    "$cvr_path/MelonLoader/net35/Mono.Cecil.dll"
)

for file in "${required_files[@]}"; do
    if [[ ! -f "$file" ]]; then
        echo "[ERROR] Missing required file: $file" >&2
        exit 1
    fi
    cp "$file" "$managed_libs_folder/"
done

managed_data_path="$cvr_path/ChilloutVR_Data/Managed"
if [[ ! -d "$managed_data_path" ]]; then
    echo "[ERROR] Missing managed data folder: $managed_data_path" >&2
    exit 1
fi

cp "$managed_data_path"/*.dll "$managed_libs_folder/"

references_file="$repo_root/References.Items.props"
ignore_names=(
    netstandard
    Mono.Cecil
    Unity.Burst.Cecil
    Microsoft.Win32.Registry
)

is_ignored() {
    local name="$1"
    local ignored
    for ignored in "${ignore_names[@]}"; do
        [[ "$name" == "$ignored" ]] && return 0
    done
    return 1
}

append_reference() {
    local name="$1"
    local file_name="$2"
    {
        printf '    <Reference Include="%s">\n' "$name"
        printf '      <HintPath>$(MsBuildThisFileDirectory)/.ManagedLibs/%s</HintPath>\n' "$file_name"
        printf '      <Private>False</Private>\n'
        printf '    </Reference>\n'
    } >> "$references_file"
}

{
    printf '<Project>\n'
    printf '  <ItemGroup>\n'
} > "$references_file"

append_reference "0Harmony" "0Harmony.dll"
append_reference "MelonLoader" "MelonLoader.dll"
append_reference "Mono.Cecil" "Mono.Cecil.dll"

while IFS= read -r file; do
    base_name="$(basename "$file" .dll)"
    if ! is_ignored "$base_name"; then
        append_reference "$base_name" "$base_name.dll"
    fi
done < <(find "$managed_data_path" -maxdepth 1 -type f -name '*.dll' | sort)

{
    printf '  </ItemGroup>\n'
    printf '</Project>\n'
} >> "$references_file"

echo
echo "Generated References.Items.props containing the common ManagedLibs references."

dlls_to_strip=(
    Assembly-CSharp.dll
    Assembly-CSharp-firstpass.dll
    AVProVideo.Runtime.dll
    Unity.TextMeshPro.dll
    MagicaCloth.dll
    MagicaClothV2.dll
)

if [[ "$skip_nstrip" == true ]]; then
    echo
    echo "Skipping NStrip because --skip-nstrip was provided."
    exit 0
fi

if [[ "$silent" != true ]]; then
    echo
    read -r -n 1 -s -p "Press any key to strip the DLLs using NStrip"
    echo
fi

nstrip_path=""
if [[ -f "$repo_root/NStrip.exe" ]]; then
    nstrip_path="$repo_root/NStrip.exe"
elif [[ -f "$script_dir/NStrip.exe" ]]; then
    nstrip_path="$script_dir/NStrip.exe"
elif command -v NStrip.exe >/dev/null 2>&1; then
    nstrip_path="$(command -v NStrip.exe)"
elif command -v NStrip >/dev/null 2>&1; then
    nstrip_path="$(command -v NStrip)"
fi

if [[ -z "$nstrip_path" ]]; then
    echo "Could not find NStrip in this directory or PATH." >&2
    echo "Install it from https://github.com/BepInEx/NStrip/releases/latest" >&2
    exit 1
fi

run_nstrip() {
    local dll_path="$1"
    if [[ "$nstrip_path" == *.exe && -x "$nstrip_path" ]]; then
        "$nstrip_path" -p -n "$dll_path" "$dll_path"
    elif [[ "$nstrip_path" == *.exe && $(command -v mono || true) ]]; then
        mono "$nstrip_path" -p -n "$dll_path" "$dll_path"
    elif [[ "$nstrip_path" != *.exe ]]; then
        "$nstrip_path" -p -n "$dll_path" "$dll_path"
    else
        echo "Found $nstrip_path, but it is not executable and mono is not installed." >&2
        exit 1
    fi
}

echo
echo "NStrip converts private/protected symbols to public. Some mods will not compile without it."

for dll_file in "${dlls_to_strip[@]}"; do
    dll_path="$managed_libs_folder/$dll_file"
    if [[ -f "$dll_path" ]]; then
        run_nstrip "$dll_path"
    else
        echo "Skipping missing DLL: $dll_path"
    fi
done

echo
echo "Copied libraries and stripped DLLs."
