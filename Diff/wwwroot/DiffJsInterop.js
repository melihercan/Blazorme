// Loads the diff libraries this package ships, so a consuming app does not have to add script
// tags of its own. Before this existed, Blazorme.Diff silently required three CDN references in
// index.html and failed at runtime with a JS interop error if any were missing.
//
// jsdiff and diff2html are classic UMD bundles that assign globals rather than ES modules, so
// they cannot be imported directly; this module injects them and waits for them to settle.

const base = './_content/Blazorme.Diff/';

let ready;

function loadScript(src, globalName) {
    // An app that still has the old CDN <script> tags keeps working: the global is already
    // there, so nothing is injected and no second copy is fetched.
    if (window[globalName]) {
        return Promise.resolve();
    }

    const existing = document.querySelector(`script[data-blazorme-diff="${globalName}"]`);
    if (existing) {
        return existing.dataset.blazormeLoaded
            ? Promise.resolve()
            : new Promise((resolve, reject) => {
                existing.addEventListener('load', () => resolve());
                existing.addEventListener('error', () => reject(new Error(`Failed to load ${src}`)));
            });
    }

    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = src;
        script.dataset.blazormeDiff = globalName;
        script.addEventListener('load', () => {
            script.dataset.blazormeLoaded = 'true';
            resolve();
        });
        script.addEventListener('error', () => reject(new Error(`Failed to load ${src}`)));
        document.head.appendChild(script);
    });
}

function loadStylesheet(href) {
    // Only guards against this module adding it twice. An app that already links diff2html's
    // stylesheet itself ends up with two copies of identical rules, which is harmless.
    if (document.querySelector('link[data-blazorme-diff="css"]')) {
        return;
    }

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    link.dataset.blazormeDiff = 'css';
    document.head.appendChild(link);
}

function ensureReady() {
    // Cached, so concurrent calls share one load rather than racing to inject duplicates.
    ready ??= (async () => {
        await loadScript(base + 'diff.min.js', 'Diff');
        await loadScript(base + 'diff2html.min.js', 'Diff2Html');
        loadStylesheet(base + 'diff2html.min.css');
    })();

    return ready;
}

export async function createTwoFilesPatch(firstTitle, secondTitle, firstInput, secondInput) {
    await ensureReady();
    return Diff.createTwoFilesPatch(firstTitle, secondTitle, firstInput, secondInput);
}

export async function html(diff, configuration) {
    await ensureReady();
    return Diff2Html.html(diff, configuration);
}
