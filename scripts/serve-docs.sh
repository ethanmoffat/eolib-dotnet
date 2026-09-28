#!/usr/bin/env bash
#
# Builds the docs for the working tree and serves the docs site on localhost, to check changes before a release.
#
# The working tree is shown as "<version>-local", and latest/ points to it. With --releases, the docs of the published
# releases are downloaded (and cached in build/docs/versions) so the version picker can be checked too.
#
# This script is identical in eolib-cpp and eolib-dotnet, apart from TITLE.

set -euo pipefail

SCRIPT_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"
REPO_ROOT="$(dirname "${SCRIPT_ROOT}")"

TITLE="eolib-dotnet"
PORT=8000
RELEASES=false
BUILD=true
DOCS_DIR="${REPO_ROOT}/build/docs"

function display_usage() {
    echo "Usage:"
    echo "  serve-docs.sh [options]"
    echo ""
    echo "Options:"
    echo "  --port <port>   Port to serve the site on [default: ${PORT}]"
    echo "  --releases      Include the docs of the published releases (requires gh)"
    echo "  --no-build      Serve the docs from the previous build instead of rebuilding them"
    echo "  -h --help       Display this message"
}

while [[ $# -gt 0 ]]
do
    case "${1}" in
        --port)        PORT="${2}"; shift ;;
        --releases)    RELEASES=true ;;
        --no-build)    BUILD=false ;;
        -h|--help)     display_usage; exit 0 ;;
        *)
            >&2 echo "Error: unsupported option \"${1}\""
            display_usage
            exit 1
            ;;
    esac
    shift
done

if [[ "${BUILD}" == "true" ]]; then
    "${SCRIPT_ROOT}/build-docs.sh" --output "${DOCS_DIR}/html"
fi
if [[ ! -f "${DOCS_DIR}/html/index.html" ]]; then
    >&2 echo "Error: no docs found in ${DOCS_DIR}/html. Run without --no-build first."
    exit 1
fi

local_version="$(cat "${DOCS_DIR}/html/version.txt")-local"

versions_dir="${DOCS_DIR}/versions"
mkdir -p "${versions_dir}"
find "${versions_dir}" -mindepth 1 -maxdepth 1 -name '*-local' -exec rm -rf {} +
cp -R "${DOCS_DIR}/html" "${versions_dir}/${local_version}"

site_args=(--input "${versions_dir}" --output "${DOCS_DIR}/site" --title "${TITLE}" --latest "${local_version}")
if [[ "${RELEASES}" == "true" ]]; then
    site_args+=(--releases)
fi
"${SCRIPT_ROOT}/build-site.sh" "${site_args[@]}"

if ! command -v python3 > /dev/null; then
    echo "python3 was not found. Serve ${DOCS_DIR}/site with any static file server, e.g. npx serve."
    exit 0
fi
echo "Serving the docs at http://localhost:${PORT}/ (Ctrl+C to stop)"
python3 -m http.server "${PORT}" --bind 127.0.0.1 --directory "${DOCS_DIR}/site"
