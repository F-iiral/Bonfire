import React from "react";

export function parseFormattedText(text: string): JSX.Element {
    if (text == null || text === "")
        return <span></span>
    
    // I know I will forget what this will do eventually so:
    // (?:                  - Makes it match everything only once
    // \*\*\*[^*]+\*\*\*    - Bold and Italic outside and get everything inside
    // \*\*[^*]+\*\*        - Bold outside and everything inside
    // \*[^*]+\*            - Italic outside and everything inside
    // [^*]+                - Match everything else (so that text.match gets those too)
    const regex = /(?:\*\*\*[^*]+\*\*\*|\*\*[^*]+\*\*|\*[^*]+\*|[^*]+)/g;
    const parts = text.match(regex);
    
    if (!parts)
       return <span></span>; 
    
    const output= parts.map((part, index) => {
        if (part.match(/\*\*\*[^*]+\*\*\*/)) {
            return <b><i key={index}>{part.replace(/\*\*\*/g, '')}</i></b>;
        }
        else if (part.match(/\*\*[^*]+\*\*/)) {
            return <b key={index}>{part.replace(/\*\*/g, '')}</b>;
        }
        else if (part.match(/\*[^*]+\*/)) {
            return <i key={index}>{part.replace(/\*/g, '')}</i>;
        }
        
        return <text key={index}>{part}</text>;
    });
    
    return <span>{output}</span>;
}