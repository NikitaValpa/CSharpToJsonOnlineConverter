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
}

export function copyToClipboard(jsonBlockElement) {
    if (jsonBlockElement.innerText) {
        navigator.clipboard.writeText(jsonBlockElement.innerText);
        console.log('copied');
    }
}
