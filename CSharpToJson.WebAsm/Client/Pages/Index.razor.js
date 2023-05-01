export function update(text) {
    const resultElement = document.querySelector("#highlighting-content");

    if (text[text.length - 1] === "\n") { // If the last character is a newline character
        text += " "; // Add a placeholder space character to the final line 
    }

    resultElement.innerHTML = text.replace(new RegExp("&", "g"), "&amp;").replace(new RegExp("<", "g"), "&lt;");
    Prism.highlightElement(resultElement);
}

export function syncScroll(element) {
    const resultElement = document.querySelector("#highlighting");
    resultElement.scrollTop = element.scrollTop;
    resultElement.scrollLeft = element.scrollLeft;
}

export function observe() {
    const resizeObserver = new window.ResizeObserver(elements => {
        const codeBlock = document.querySelector('#codeBlock');

        for (const elem of elements) {
            if (elem && codeBlock) {
                codeBlock.style.height = `${elem.borderBoxSize[0].blockSize}px`;
            }
        }
    });

    resizeObserver.observe(document.querySelector('#floatingTextarea'));
}

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
