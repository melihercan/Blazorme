// Loads the Split.js bundle this package ships, so a consuming app does not have to add a script
// tag of its own. Before this existed, Blazorme.Split silently required a CDN reference in
// index.html and failed at runtime with a JS interop error without it.
//
// Split.js is a classic UMD bundle that assigns a global rather than an ES module, so it cannot
// be imported directly; this module injects it and waits for it to settle.

const base = './_content/Blazorme.Split/';

let ready;

function loadScript(src, globalName) {
    // An app that still has the old CDN <script> tag keeps working: the global is already there,
    // so nothing is injected and no second copy is fetched.
    if (window[globalName]) {
        return Promise.resolve();
    }

    const existing = document.querySelector(`script[data-blazorme-split="${globalName}"]`);
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
        script.dataset.blazormeSplit = globalName;
        script.addEventListener('load', () => {
            script.dataset.blazormeLoaded = 'true';
            resolve();
        });
        script.addEventListener('error', () => reject(new Error(`Failed to load ${src}`)));
        document.head.appendChild(script);
    });
}

function ensureReady() {
    // Cached, so two Split components rendering together share one load.
    ready ??= loadScript(base + 'split.min.js', 'Split');
    return ready;
}

// Named "create" rather than "Split" so it cannot shadow the global this module depends on.
export async function create(elements, options) {
    await ensureReady();
    Split(elements, options);
}
