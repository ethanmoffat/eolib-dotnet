#!/usr/bin/env bash
#
# Assembles the versioned documentation site deployed to GitHub Pages.
#
# The input directory holds one folder of built HTML documentation per version (e.g. docs/0.1.0/index.html). With
# --releases, the docs archive attached to each published GitHub release is downloaded into it first. The site gets:
#
#   /<version>/     the docs for each version, with a version picker added to every page
#   /latest/        redirect pages to the same page of the newest release (the newest prerelease if there are no
#                   stable releases yet)
#   /versions.json  the list of versions, read by the version picker
#   /index.html     redirects to latest/
#
# To keep the site small, only the newest release of each minor version is published, along with the prereleases of
# versions that aren't released yet. For example, 1.0.2 replaces 1.0.0 and 1.0.1, and 1.1.0 replaces 1.1.0-rc.1. The
# docs of every release remain available in its docs archive. --all-versions publishes every version instead.
#
# This script is identical in eolib-cpp and eolib-dotnet.

set -euo pipefail

SCRIPT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"

INPUT_DIR=""
OUTPUT_DIR=""
TITLE=""
REPO=""
RELEASES=false
LATEST=""
BASE_PATH="/"
ALL_VERSIONS=false

function display_usage() {
    echo "Usage:"
    echo "  build-site.sh --input <dir> --output <dir> --title <name> [options]"
    echo ""
    echo "Options:"
    echo "  --input <dir>       Directory with one folder of HTML docs per version"
    echo "  --output <dir>      Directory to write the site to (replaced)"
    echo "  --title <name>      Project name shown on the redirect and not found pages"
    echo "  --releases          Download the docs archive (*-docs.tar.gz) of each published GitHub release into the"
    echo "                      input directory first, skipping versions that are already there. Requires gh."
    echo "  --repo <owner/name> Repository to download releases from [default: the current gh repository]"
    echo "  --latest <version>  Version that latest/ points to [default: the newest release]"
    echo "  --base-path <path>  URL path the site is served from, used by the not found page [default: /]"
    echo "  --all-versions      Publish every version, instead of only the newest release of each minor version"
    echo "  -h --help           Display this message"
}

while [[ $# -gt 0 ]]
do
    case "${1}" in
        --input)       INPUT_DIR="${2}"; shift ;;
        --output)      OUTPUT_DIR="${2}"; shift ;;
        --title)       TITLE="${2}"; shift ;;
        --releases)    RELEASES=true ;;
        --repo)        REPO="${2}"; shift ;;
        --latest)      LATEST="${2}"; shift ;;
        --base-path)   BASE_PATH="${2}"; shift ;;
        --all-versions) ALL_VERSIONS=true ;;
        -h|--help)     display_usage; exit 0 ;;
        *)
            >&2 echo "Error: unsupported option \"${1}\""
            display_usage
            exit 1
            ;;
    esac
    shift
done

if [[ -z "${INPUT_DIR}" || -z "${OUTPUT_DIR}" || -z "${TITLE}" ]]; then
    >&2 echo "Error: --input, --output and --title are required"
    display_usage
    exit 1
fi

mkdir -p "${INPUT_DIR}"

# Downloads the docs archive of each published release that isn't in the input directory yet.
function download_releases() {
    local repo_args=()
    if [[ -n "${REPO}" ]]; then
        repo_args=(--repo "${REPO}")
    fi

    local tag version work
    work="$(mktemp -d)"
    for tag in $(gh release list "${repo_args[@]}" --limit 1000 --exclude-drafts --json tagName --jq '.[].tagName'); do
        version="${tag#v}"
        if [[ -d "${INPUT_DIR}/${version}" ]]; then
            continue
        fi
        rm -rf "${work:?}"/*
        if ! gh release download "${tag}" "${repo_args[@]}" --pattern '*-docs.tar.gz' --dir "${work}" 2> /dev/null; then
            >&2 echo "warning: release ${tag} has no docs archive; skipping it"
            continue
        fi
        mkdir -p "${INPUT_DIR}/${version}"
        tar -xzf "${work}"/*-docs.tar.gz -C "${INPUT_DIR}/${version}" --strip-components=1
        echo "Downloaded the docs for ${tag}"
    done
    rm -rf "${work}"
}

# Prints a key that sorts versions in semver order (a prerelease sorts before its release), with the version appended.
function version_sort_key() {
    local version="${1}" core suffix major minor patch
    core="${version%%-*}"
    suffix=""
    if [[ "${version}" == *-* ]]; then
        suffix="${version#*-}"
    fi
    IFS=. read -r major minor patch <<< "${core}"
    if [[ -z "${suffix}" ]]; then
        printf '%05d.%05d.%05d 1 - %s\n' "${major:-0}" "${minor:-0}" "${patch:-0}" "${version}"
    else
        printf '%05d.%05d.%05d 0 %s %s\n' "${major:-0}" "${minor:-0}" "${patch:-0}" "${suffix}" "${version}"
    fi
}

# Prints a relative path from a file in the site to the site root, e.g. "../../" for <version>/api/page.html.
function root_path() {
    local relative="${1}" depth prefix=""
    depth="$(tr -cd '/' <<< "${relative}" | wc -c)"
    for ((i = 0; i < depth; i++)); do
        prefix="../${prefix}"
    done
    echo "${prefix}"
}

if [[ "${RELEASES}" == "true" ]]; then
    download_releases
fi

versions=()
while IFS= read -r version; do
    versions+=("${version}")
done < <(
    for dir in "${INPUT_DIR}"/*/; do
        [[ -f "${dir}index.html" ]] || continue
        version_sort_key "$(basename "${dir}")"
    done | sort -k1,1r -k2,2r -k3,3Vr | awk '{print $NF}'
)

if [[ ${#versions[@]} -eq 0 ]]; then
    >&2 echo "Error: no documentation versions found in ${INPUT_DIR}"
    exit 1
fi

if [[ -z "${LATEST}" ]]; then
    for version in "${versions[@]}"; do
        if [[ "${version}" != *-* ]]; then
            LATEST="${version}"
            break
        fi
    done
    LATEST="${LATEST:-${versions[0]}}"
fi
if [[ ! -d "${INPUT_DIR}/${LATEST}" ]]; then
    >&2 echo "Error: the latest version ${LATEST} is not in ${INPUT_DIR}"
    exit 1
fi

# Keeps the newest release of each minor version, and the prereleases newer than it. The versions are sorted newest
# first, so a version is kept if no release of its minor version has been seen yet. The latest version is always kept.
if [[ "${ALL_VERSIONS}" != "true" ]]; then
    kept=()
    released=" "
    for version in "${versions[@]}"; do
        core="${version%%-*}"
        minor="${core%.*}"
        if [[ "${version}" == "${LATEST}" || "${released}" != *" ${minor} "* ]]; then
            kept+=("${version}")
        else
            echo "Skipping ${version}, which is replaced by a newer release"
        fi
        if [[ "${version}" != *-* ]]; then
            released+="${minor} "
        fi
    done
    versions=("${kept[@]}")
fi

rm -rf "${OUTPUT_DIR}"
mkdir -p "${OUTPUT_DIR}"

for version in "${versions[@]}"; do
    cp -R "${INPUT_DIR}/${version}" "${OUTPUT_DIR}/${version}"
    while IFS= read -r -d '' file; do
        relative="${file#"${OUTPUT_DIR}"/}"
        script="<script src=\"$(root_path "${relative}")version-picker.js\" defer></script>"
        # Literal replacement of the first </head>, without sed escaping issues in the paths.
        awk -v tag="${script}" '!done && sub(/<\/head>/, tag "</head>") { done = 1 } { print }' "${file}" \
            > "${file}.tmp" && mv "${file}.tmp" "${file}"
    done < <(find "${OUTPUT_DIR}/${version}" -name '*.html' -print0)
done

# latest/ has a small redirect page for each page of the latest version, rather than a copy of it: GitHub Pages doesn't
# support symlinks (actions/upload-pages-artifact copies their targets).
while IFS= read -r -d '' file; do
    page="${file#"${INPUT_DIR}/${LATEST}"/}"
    target="$(root_path "latest/${page}")${LATEST}/${page}"
    mkdir -p "$(dirname "${OUTPUT_DIR}/latest/${page}")"
    cat > "${OUTPUT_DIR}/latest/${page}" << EOF
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<title>${TITLE} documentation</title>
<link rel="canonical" href="${target}">
<script>window.location.replace("${target}" + window.location.search + window.location.hash);</script>
<meta http-equiv="refresh" content="0; url=${target}">
</head>
<body><p>Redirecting to <a href="${target}">${TITLE} ${LATEST}</a>.</p></body>
</html>
EOF
done < <(find "${INPUT_DIR}/${LATEST}" -name '*.html' -print0)

{
    echo "["
    for i in "${!versions[@]}"; do
        version="${versions[${i}]}"
        aliases="[]"
        if [[ "${version}" == "${LATEST}" ]]; then
            aliases='["latest"]'
        fi
        prerelease=false
        if [[ "${version}" == *-* ]]; then
            prerelease=true
        fi
        separator=","
        if [[ ${i} -eq $((${#versions[@]} - 1)) ]]; then
            separator=""
        fi
        printf '  {"version": "%s", "title": "%s", "aliases": %s, "prerelease": %s}%s\n' \
            "${version}" "${version}" "${aliases}" "${prerelease}" "${separator}"
    done
    echo "]"
} > "${OUTPUT_DIR}/versions.json"

cp "${SCRIPT_ROOT}/site/version-picker.js" "${OUTPUT_DIR}/version-picker.js"
for page in index.html 404.html; do
    sed -e "s|@TITLE@|${TITLE}|g" -e "s|@LATEST@|${LATEST}|g" -e "s|@BASE_PATH@|${BASE_PATH%/}/|g" "${SCRIPT_ROOT}/site/${page}" > "${OUTPUT_DIR}/${page}"
done

echo "Built the ${TITLE} docs site in ${OUTPUT_DIR}: ${versions[*]} (latest: ${LATEST})"
