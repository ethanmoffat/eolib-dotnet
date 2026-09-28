// Adds a version picker to every page of the versioned docs site, with a link to the latest version on pages of other
// versions. It floats in the corner of the page, so it works with any docs generator's layout. Added to each page by scripts/build-site.sh, which also writes versions.json next to this script.
(function () {
    "use strict";

    const root = new URL(".", document.currentScript.src);

    function currentLocation() {
        const relative = decodeURIComponent(window.location.href.substring(root.href.length));
        const separator = relative.indexOf("/");
        return separator < 0
            ? { version: relative, page: "" }
            : { version: relative.substring(0, separator), page: relative.substring(separator + 1) };
    }

    async function navigate(version, page) {
        const target = new URL(version + "/" + page, root);
        try {
            const response = await fetch(target, { method: "HEAD" });
            window.location.href = response.ok ? target.href : new URL(version + "/", root).href;
        } catch (e) {
            window.location.href = new URL(version + "/", root).href;
        }
    }

    function addStyles() {
        const style = document.createElement("style");
        style.textContent = `
            .eolib-version-picker {
                position: fixed; right: 16px; bottom: 40px; z-index: 1000; max-width: 320px; padding: 6px 10px;
                font: 13px/1.4 system-ui, sans-serif; color: #24292f; background: #f6f8fa;
                border: 1px solid #d0d7de; border-radius: 6px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
            }
            .eolib-version-picker select { margin-left: 6px; font: inherit; }
            .eolib-version-picker.eolib-version-old { background: #fff8c5; border-color: #d4a72c; }
            .eolib-version-notice { margin-top: 4px; }
            .eolib-version-notice a { color: inherit; font-weight: 600; }
            @media (prefers-color-scheme: dark) {
                .eolib-version-picker { color: #e6edf3; background: #161b22; border-color: #30363d; }
                .eolib-version-picker.eolib-version-old { background: #3b2e00; border-color: #9e6a03; }
            }`;
        document.head.appendChild(style);
    }

    function addPicker(versions, latest, current) {
        const container = document.createElement("div");
        container.className = "eolib-version-picker";
        const label = document.createElement("label");
        label.textContent = "Version";
        const select = document.createElement("select");
        for (const entry of versions) {
            const option = document.createElement("option");
            option.value = entry.version;
            option.textContent = entry.version === latest ? entry.title + " (latest)" : entry.title;
            option.selected = entry.version === current.version || (current.version === "latest" && entry.version === latest);
            select.appendChild(option);
        }
        select.addEventListener("change", () => navigate(select.value, current.page));
        label.appendChild(select);
        container.appendChild(label);
        document.body.appendChild(container);
        return container;
    }

    function addNotice(container, latest, current) {
        if (current.version === "latest" || current.version === latest) {
            return;
        }
        container.classList.add("eolib-version-old");
        const notice = document.createElement("div");
        notice.className = "eolib-version-notice";
        notice.append("This is not the latest version. ");
        const link = document.createElement("a");
        link.href = new URL("latest/", root).href;
        link.textContent = "Go to " + latest;
        link.addEventListener("click", (e) => {
            e.preventDefault();
            navigate("latest", current.page);
        });
        notice.appendChild(link);
        container.appendChild(notice);
    }

    async function init() {
        const response = await fetch(new URL("versions.json", root));
        if (!response.ok) {
            return;
        }
        const versions = await response.json();
        const latestEntry = versions.find((entry) => entry.aliases.includes("latest"));
        const latest = latestEntry ? latestEntry.version : versions[0].version;
        const current = currentLocation();
        addStyles();
        const container = addPicker(versions, latest, current);
        addNotice(container, latest, current);
    }

    init().catch(() => {});
})();
