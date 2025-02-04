// @ts-ignore
import hljs from "highlight.js";

function parseHyperlinkText(text: string): HTMLSpanElement[] {
    if (!text) return [];
    
    // (?:)                 - Makes it match everything only once
    // \[(.*?)\]\((.*?)\))  - Matches the hyperlink
    // ([^\[]+              - Matches everything else
    const regex =  /(\[(.*?)]\((.*?)\))|([^\[]+)/g
    const parts = text.match(regex);
    if (!parts) return [];
    
    return parts.map(part => {
        const linkRegex = /\[(.*?)]\((.*?)\)/g;
        let subParts = part.split(linkRegex).filter(x => x);
        
        if (subParts.length > 1) {
            const anchor = document.createElement("a");
            anchor.href = subParts[1];
            anchor.textContent = subParts[0];
            return anchor;
        }
        
        const span = document.createElement("span");
        span.textContent = subParts[0];
        return span;
    });
}

function parseDefaultText(text: string): HTMLElement[] {
    if (!text) return [];

    // (?:)                 - Makes it match everything only once
    // $\n                  - Newlines
    // \*\*\*[^*]+\*\*\*    - Bold and Italic outside and get everything inside (needed since they use the same character)
    // \*\*[^*]+\*\*        - Bold outside and get everything inside
    // \*[^*]+\*            - Italic outside and everything inside
    // __[^_]+__            - Underlined outside and everything inside
    // ~~[^~]+~~            - Strikethrough outside and everything inside
    // \|\|[^|]+\|\|        - Spoiler outside and everything inside
    // [^*_~|\n]+|\*|_|~    - Match everything else (so that text.match gets those too)
    // -gm flags            - Needs to be both global (so it matches everything) and multiline (so we can render it multiline)
    const regex = /$\n|\*\*\*[^*]+\*\*\*|\*\*[^*]+\*\*|\*[^*]+\*|__[^_]+__|~~[^~]+~~|\|\|[^|]+\|\||[^*_~\|\n]+|\*|_|~|\|/gm
    const parts = text.match(regex);
    if (!parts) return [];

    return parts.map(part => {
        const styles = [];

        if (part.match(/\n/)) return document.createElement("br");
        if (part.match(/\*\*[^*]+\*\*/)) styles.push("b");
        if (part.match(/\*[^*]+\*/)) styles.push("i");
        if (part.match(/__[^_]+__/)) styles.push("u");
        if (part.match(/~~[^_]+~~/)) styles.push("s");
        if (part.match(/\|\|[^|]+\|\|/)) styles.push("spoiler");

        part = part.replace(/[\*~_|]/g, "");
        let element = parseHyperlinkText(part);
        
        styles.forEach(style => {
            let wrapper = document.createElement(style === "spoiler" ? "span" : style);
            if (style === "spoiler") wrapper.className = "spoiler";
            wrapper.append(...element);
            element = [wrapper];
        });
        
        return element;
    }).flat();
}

function parseCodeBlockText(text: string): HTMLElement[] {
    text = text.replaceAll("```", "");
    const language = text.split('\n')[0];
    const highlightedText = hljs.highlight(text, { language }).value;

    const codeElement = document.createElement("code");
    codeElement.innerHTML = highlightedText;
    return [codeElement];
}

export function parseFormattedText(text: string): HTMLSpanElement {
    if (!text) return document.createElement("span");

    // (?:)                     - Makes it match everything only once
    // ^\s*                     - Start of line with 0 or more whitespace
    // ```(?:[^`]|`(?!``))*```  - Code Block outside and get everything inside
    // [^`]+                    - Match everything else (so that text.match gets those too)
    const codeBoxRegex = /(```(?:[^`]|`(?!``))*```|[^`]+)/gm
    
    // ^-.+$\n                 - Matches lists
    // ^>.+$\n                  - Matches blockquotes
    // .+                       - Matches everything else
    const richTextRegex = /^-.+$\n|^>.+$\n|.+\n?/gm
    const parts = text.match(codeBoxRegex);
    
    if (!parts)
        return document.createElement("span");

    const container = document.createElement("span");
    parts.forEach(part => {
        if (part.match(/```(?:[^`]|`(?!``))*```/)) {
            container.append(...parseCodeBlockText(part));
            return;
        }

        let newParts = part.match(richTextRegex);
        if (!newParts) return;
        
        newParts.forEach(part => {
            if (part.match(/^-.+$\n/gm)) {
                const li = document.createElement("li");
                li.append(...parseDefaultText(part.replace(/^- /, "")));
                container.append(li);
            } else if (part.match(/^>.+$\n/gm)) {
                const blockquote = document.createElement("span");
                blockquote.className = "blockquote";
                blockquote.append(...parseDefaultText(part.replace(/^> /, " ")));
                container.append(blockquote);
            } else {
                container.append(...parseDefaultText(part));
            }
        });
    });
    
    return container;
}