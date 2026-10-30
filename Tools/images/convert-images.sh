#!/usr/bin/env bash
# Shrinks the artwork for the web and writes it into the seed, where the ruleset migrator picks it up.
# The source files are only read, never changed.
#
# Usage: Tools/images/convert-images.sh [-o output folder] <source folder> [category …]
#   source: folders spells/, classes/, species/, backgrounds/, scenes/ and equipment/ (with subfolders such as
#           armor/, gear/, weapons/ and coins/) with PNG or JPEG files
#   category: spells, classes, species, backgrounds, scenes, items, coins; default every category found in the source
#   output: default Seed/TableTopSheets.Seed/Data/Images; only the converted categories are cleared and written again
#
# Each image becomes <category>/<key>.webp and <category>/<key>.thumb.webp:
#   spells, items, coins         512 x 512        thumbnail 128 x 128
#   classes                      900 px tall      thumbnail 256 px wide
#   species, backgrounds, scenes 1600 px wide, thumbnail 400 px wide
#
# File name -> key: a "-vN" suffix is a version (the highest one wins). Spells and scenes keep the rest of the name
# (fireball, dragon-battle); classes, species and backgrounds use the first word (wizard-3-high-elf-male -> wizard,
# acolyte-temple -> acolyte), half-orc is the orc species. Items are every subfolder of equipment/ but coins/; names
# that differ from the item key are listed in item-keys.txt. Coins drop "-piece" (gold-piece -> gold).
set -euo pipefail

repository=$(realpath "$(dirname "$0")/../..")
output_dir=$repository/Seed/TableTopSheets.Seed/Data/Images
aliases_file=$(dirname "$0")/item-keys.txt
quality=80
all_categories=(spells classes species backgrounds scenes items coins)

if [[ ${1:-} == -o ]]; then
  output_dir=$2
  shift 2
fi
if [[ $# -lt 1 || ! -d $1 ]]; then
  echo "Usage: $0 [-o output folder] <source folder> [category …]" >&2
  exit 2
fi

source_dir=$(realpath "$1")
shift
categories=("$@")

command -v magick >/dev/null || { echo "ImageMagick (magick) is required" >&2; exit 1; }

# folders with the images of a category
source_folders() {
  case $1 in
    items) find "$source_dir/equipment" -mindepth 1 -maxdepth 1 -type d ! -name coins 2>/dev/null | sort ;;
    coins) [[ -d $source_dir/equipment/coins ]] && echo "$source_dir/equipment/coins" ;;
    *) [[ -d $source_dir/$1 ]] && echo "$source_dir/$1" ;;
  esac
  return 0
}

# item keys of image names that differ from them
declare -A item_aliases=()
while read -r name keys; do
  [[ -z $name || $name == \#* ]] && continue
  item_aliases[$name]=$keys
done <"$aliases_file"

# key of a file name without extension and version
key_of() {
  local category=$1 name=$2
  case $category in
    spells | scenes | items) echo "$name" ;;
    coins) echo "${name%-piece}" ;;
    species) [[ $name == half-orc* ]] && echo orc || echo "${name%%-*}" ;;
    *) echo "${name%%-*}" ;;
  esac
}

convert_one() {
  local file=$1 target=$2 full=$3 thumb=$4
  magick "$file" -strip -resize "$full" -quality $quality "$target.webp"
  magick "$file" -strip -resize "$thumb" -quality $quality "$target.thumb.webp"
}

if [[ ${#categories[@]} -eq 0 ]]; then
  for category in "${all_categories[@]}"; do
    [[ -n $(source_folders "$category") ]] && categories+=("$category")
  done
fi

total=0
copies=()
for category in "${categories[@]}"; do
  [[ " ${all_categories[*]} " == *" $category "* ]] || { echo "unknown category $category" >&2; exit 2; }
  mapfile -t folders < <(source_folders "$category")
  [[ ${#folders[@]} -gt 0 ]] || { echo "skipped $category: no folder" >&2; continue; }
  case $category in
    spells | items | coins) full='512x512>' thumb='128x128>' ;;
    classes) full='x900>' thumb='256x>' ;;
    *) full='1600x>' thumb='400x>' ;;
  esac

  # highest version per key
  declare -A chosen=() versions=()
  while IFS= read -r -d '' file; do
    name=$(basename "${file%.*}")
    version=1
    if [[ $name =~ ^(.*)-v([0-9]+)$ ]]; then
      name=${BASH_REMATCH[1]}
      version=${BASH_REMATCH[2]}
    fi
    key=$(key_of "$category" "$name")
    if [[ -z ${versions[$key]:-} || $version -gt ${versions[$key]} ]]; then
      chosen[$key]=$file
      versions[$key]=$version
    fi
  done < <(find "${folders[@]}" -maxdepth 1 -type f \( -iname '*.png' -o -iname '*.jpg' -o -iname '*.jpeg' \) -print0)

  rm -rf "${output_dir:?}/$category"
  mkdir -p "$output_dir/$category"
  count=0
  for key in "${!chosen[@]}"; do
    targets=$key
    if [[ $category == items && -n ${item_aliases[$key]:-} ]]; then
      targets=${item_aliases[$key]}
      [[ $targets == - ]] && { echo "skipped item image $key: no item" >&2; continue; }
    fi
    first=${targets%% *}
    convert_one "${chosen[$key]}" "$output_dir/$category/$first" "$full" "$thumb" &
    count=$((count + 1))
    # one image for several keys (spell-scroll): copies after the conversion
    for other in ${targets#"$first"}; do
      copies+=("$category/$first:$category/$other")
      count=$((count + 1))
    done
    # a few conversions at a time
    while (($(jobs -r | wc -l) >= $(nproc))); do wait -n; done
  done
  wait
  for copy in "${copies[@]:-}"; do
    [[ -z $copy ]] && continue
    cp "$output_dir/${copy%%:*}.webp" "$output_dir/${copy#*:}.webp"
    cp "$output_dir/${copy%%:*}.thumb.webp" "$output_dir/${copy#*:}.thumb.webp"
  done
  echo "$category: $count images"
  total=$((total + count))
  unset chosen versions
  copies=()
done

echo "$total images, $(du -sh "$output_dir" | cut -f1) in $output_dir"
