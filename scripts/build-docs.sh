#!/usr/bin/env bash
#
# Builds the HTML API reference for one version of eolib-dotnet with DocFX.
#
# The DocFX configuration and pages (docs/) always come from this repository, while the code comes from --source.
# This lets the release workflow build the docs of older release tags that predate the docs configuration.
#
# scripts/serve-docs.sh builds and serves the docs site on localhost.

set -euo pipefail

SCRIPT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"
REPO_ROOT="$(dirname "${SCRIPT_ROOT}")"

SOURCE_DIR="${REPO_ROOT}"
OUTPUT_DIR=""
BUILD_DIR=""
VERSION=""
WARNINGS_AS_ERRORS=false
ARCHIVE=""

function display_usage() {
    echo "Usage:"
    echo "  build-docs.sh [options]"
    echo ""
    echo "Options:"
    echo "  --source <dir>          eolib-dotnet source tree to document [default: this repository]"
    echo "  --output <dir>          Directory to write the HTML docs to (replaced) [default: <build-dir>/html]"
    echo "  --build-dir <dir>       Working directory for the DocFX configuration and metadata"
    echo "                          [default: <source>/build/docs]"
    echo "  --version <version>     Version shown in the docs [default: AssemblyInformationalVersion of the source]"
    echo "  --archive <file>        Also create a .tar.gz of the docs, e.g. Moffat.EndlessOnline.SDK-<version>-docs.tar.gz"
    echo "  --warnings-as-errors    Fail if DocFX reports any warnings"
    echo "  -h --help               Display this message"
}

while [[ $# -gt 0 ]]
do
    case "${1}" in
        --source)               SOURCE_DIR="$(cd "${2}" && pwd)"; shift ;;
        --output)               OUTPUT_DIR="${2}"; shift ;;
        --build-dir)            BUILD_DIR="${2}"; shift ;;
        --version)              VERSION="${2}"; shift ;;
        --archive)              ARCHIVE="${2}"; shift ;;
        --warnings-as-errors)   WARNINGS_AS_ERRORS=true ;;
        -h|--help)              display_usage; exit 0 ;;
        *)
            >&2 echo "Error: unsupported option \"${1}\""
            display_usage
            exit 1
            ;;
    esac
    shift
done

PROJECT="${SOURCE_DIR}/Moffat.EndlessOnline.SDK/Moffat.EndlessOnline.SDK.csproj"
if [[ ! -f "${PROJECT}" ]]; then
    >&2 echo "Error: ${PROJECT} not found"
    exit 1
fi

if [[ -z "${VERSION}" ]]; then
    VERSION="$(sed -n 's/^\[assembly: AssemblyInformationalVersion("\(.*\)")\]\r\{0,1\}$/\1/p' \
        "${SOURCE_DIR}/Moffat.EndlessOnline.SDK/Properties/AssemblyInfo.cs")"
fi

BUILD_DIR="${BUILD_DIR:-${SOURCE_DIR}/build/docs}"
CONFIG_DIR="${BUILD_DIR}/docs-config"
rm -rf "${CONFIG_DIR}"
mkdir -p "${CONFIG_DIR}"
CONFIG_DIR="$(cd "${CONFIG_DIR}" && pwd)"
OUTPUT_DIR="${OUTPUT_DIR:-${BUILD_DIR}/html}"
rm -rf "${OUTPUT_DIR}"
mkdir -p "${OUTPUT_DIR}"
OUTPUT_DIR="$(cd "${OUTPUT_DIR}" && pwd)"

# DocFX reads the API from the built assembly and its XML documentation, since it doesn't run the source generator
# that creates the protocol code when it loads the project. Publishing copies the dependencies next to the assembly,
# so DocFX can resolve the types they define.
ASSEMBLY_DIR="${BUILD_DIR}/assembly"
rm -rf "${ASSEMBLY_DIR}"
echo "Building ${PROJECT}..."
dotnet publish "${PROJECT}" -c Release -nologo -v quiet -o "${ASSEMBLY_DIR}"
ASSEMBLY_DIR="$(cd "${ASSEMBLY_DIR}" && pwd)"

cp -R "${REPO_ROOT}/docs/." "${CONFIG_DIR}/"
for file in "${CONFIG_DIR}/docfx.json" "${CONFIG_DIR}/index.md"; do
    sed -i.bak \
        -e "s|@EOLIB_DOCS_VERSION@|${VERSION}|g" \
        -e "s|@EOLIB_DOCS_ASSEMBLY_DIR@|${ASSEMBLY_DIR}|g" \
        -e "s|@EOLIB_DOCS_OUTPUT_DIR@|${OUTPUT_DIR}|g" \
        -e "s|@EOLIB_DOCS_WARN_AS_ERROR@|${WARNINGS_AS_ERRORS}|g" \
        "${file}"
    rm "${file}.bak"
done

(cd "${REPO_ROOT}" && dotnet tool restore > /dev/null)
echo "Building the docs for Moffat.EndlessOnline.SDK ${VERSION}..."
(cd "${REPO_ROOT}" && dotnet tool run docfx "${CONFIG_DIR}/docfx.json")
echo "${VERSION}" > "${OUTPUT_DIR}/version.txt"

if [[ -n "${ARCHIVE}" ]]; then
    # The archive has a single top-level folder named after the archive, which build-site.sh strips when extracting.
    name="$(basename "${ARCHIVE}" .tar.gz)"
    staging="$(mktemp -d)"
    cp -R "${OUTPUT_DIR}" "${staging}/${name}"
    tar -czf "${ARCHIVE}" -C "${staging}" "${name}"
    rm -rf "${staging}"
    echo "Created ${ARCHIVE}"
fi

echo "Built the docs in ${OUTPUT_DIR}"
