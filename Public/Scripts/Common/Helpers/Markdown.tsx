import React from "react";
// @ts-ignore
import hljs from "highlight.js";

function parseHyperlinkText(text: string): JSX.Element[] {
    if (text == null || text === "")
        return [<></>];
    
    // (?:)                 - Makes it match everything only once
    // \[(.*?)\]\((.*?)\))  - Matches the hyperlink
    // ([^\[]+              - Matches everything else
    const regex =  /(\[(.*?)]\((.*?)\))|([^\[]+)/g
    const parts = text.match(regex);

    if (!parts)
        return [<></>];
    
    if (parts.length > 1) {
        const linkRegex = /\[(.*?)]\((.*?)\)/g
        
        return parts.map((part, index)=> {
            let subParts = part.split(linkRegex).filter(x => x != undefined && x != "");
            
            if (subParts.length > 1) {
                return <a key={index} href={subParts[1]}>{subParts[0]}</a>
            }
            return <span key={index} >{subParts[0]}</span>;
        });
    }
    
    return [<>{parts}</>];
}

function parseDefaultText(text: string): JSX.Element[] {
    if (text == null || text === "")
        return [<></>];

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
    
    if (!parts)
        return [<></>];
    
    return parts.map((part, index) => {
        const styles: string[] = [];
        
        if (part.match(/$\n/m))
            return <br key={index}></br>;
        if (part.match(/\*\*[^*]+\*\*/))
            styles.push("bold");
        else if (part.match(/\*[^*]+\*/))
            styles.push("italic");
        if (part.match(/\*\*\*[^*]+\*\*\*/))
            styles.push("italic");
        if (part.match(/__[^_]+__/))
            styles.push("underlined");
        if (part.match(/~~[^_]+~~/))
            styles.push("strikethrough");
        if (part.match(/\|\|[^|]+\|\|/))
            styles.push("spoiler")
        
        if (styles.length > 0) {
            part = part.replace(/\*\*/g, "").replace(/\*/g, "").replace(/__/g, "").replace(/~~/g, "").replace(/\|\|/g, "");
            let result = <>{parseHyperlinkText(part)}</>;
            
            styles.forEach(style => {
                if (style === "bold")
                    result = <b>{result}</b>;
                else if (style === "italic")
                    result = <i>{result}</i>;
                else if (style === "underlined")
                    result = <u>{result}</u>;
                else if (style === "strikethrough")
                    result = <s>{result}</s>;
                else if (style === "spoiler")
                    result = <span className={"spoiler"}>{result}</span>;
            });
            
            if (result)
                return result;
        }
        
        return <>{parseHyperlinkText(part)}</>;
    });
}

function parseCodeBlockText(text: string): JSX.Element[] {
    text = text.replaceAll("```", "");
    const languages = text.match(/^.*$/m);
    const language = languages ? languages[0] : ""; 
    const highlightedText: string = hljs.highlight(text, {language: language}).value;
    const regex = /$\n|[^\n]+/gm
    const parts = highlightedText.match(regex);

    if (!parts)
        return [<></>];

    const output = parts.map((part, index) => {
        if (part.match(/$\n/m)) {
            if (index == 0)
                return;
            return <br key={index}></br>;
        }
        
        // highlighter.js is trustworthy
        return <span dangerouslySetInnerHTML={{ __html: part, }}></span>;
    });

    return [<code>{output}</code>];
}

export function parseFormattedText(text: string): JSX.Element {
    if (text == null || text === "")
        return <span></span>

    // (?:)                     - Makes it match everything only once
    // ^\s*                     - Start of line with 0 or more whitespace
    // ```(?:[^`]|`(?!``))*```  - Code Block outside and get everything inside
    // [^`]+                    - Match everything else (so that text.match gets those too)
    const codeBoxRegex = /(```(?:[^`]|`(?!``))*```|[^`]+)/gm
    
    // ^-.+$\n                 - Matches lists
    // ^>.+$\n                  - Matches blockquotes
    // .+                       - Matches everything else
    const richTextRegex = /^-.+$\n|^>.+$\n|.+\n/gm
    const parts = text.match(codeBoxRegex);
    
    if (!parts)
        return <></>;
    
    let output = parts.map((part, index)=> {
        if (part.match(/```(?:[^`]|`(?!``))*```/))
            return <span key={index}>{parseCodeBlockText(part)}</span>;

        var newParts = part.match(richTextRegex)

        if (!newParts)
            return <></>;
        console.log(newParts)
        
        return newParts.map((part, index) => {
            if (part.match(/^-.+$\n/gm))
                return <li key={index}>{parseDefaultText(part.replace(/^- /, ""))}</li>;
            if (part.match(/^>.+$\n/gm))
                return <span className={"blockquote"} key={index}>{parseDefaultText(part.replace(/^> /, " "))}</span>;

            return <span key={index}>{parseDefaultText(part)}</span>;
        });
    });
    
    return <span>{output}</span>;
}