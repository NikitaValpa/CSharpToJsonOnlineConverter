export function stringify(json) {
    try {
        const validJson = JSON.parse(json);
        return JSON.stringify(validJson, null, 4);
    } catch (e) {
        console.error(e);
        return "Generated json is invalid";
    }
}

export function highlightJson(json) {
    const jsonOut = document.querySelector("#jsonOutput");
    jsonOut.innerHTML = json;
    Prism.highlightElement(jsonOut);
    document.querySelector('#copy-button').style.display = 'block';
}

export function disableCopyToClipboardButton() {
    document.querySelector('#copy-button').style.display = 'none';
}

export async function copyToClipboard(jsonBlockElement) {
    const tooltip = bootstrap.Tooltip.getInstance('#copy-button');
    
    if (jsonBlockElement.innerText) {
        navigator.clipboard.writeText(jsonBlockElement.innerText);
        await tooltip.setContent({ '.tooltip-inner': 'Copied' });
        setTimeout(async () => { await tooltip.setContent({ '.tooltip-inner': 'Copy to clipboard' }) }, 1000);
    }
}

export function initialize() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

    Prism.highlightElement(document.querySelector("#example-csharp"));
    Prism.highlightElement(document.querySelector("#example-json"));
}
