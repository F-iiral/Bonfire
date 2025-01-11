import {parseFormattedText} from "./Common/Helpers/Markdown.js";
import {Get, Query} from "./Common/Server/HttpConnections.js";
import ReactDOM from "react-dom";
import {SelfUserContent} from "./Content/Account/SelfUser";
import {GetMessagesContext} from "./Context/Channel/GetMessages.js";

const startTime = performance.now()

// Example Code :3
const inputText = "This is **bold** text. " +
    "This is *italic* text. " +
    "This is __underlined__ text. " +
    "This is ~~strikethrough~~ text. " +
    "This is ***bold and italic*** text. " +
    "This is __**bold and underlined**__ text. " +
    "This is ~~**bold and strikethrough**~~ text. " +
    "This is __*italic and underlined*__ text. " +
    "This is ~~*italic and strikethrough*~~ text. " +
    "This is __~~strikethrough and underlined~~__ text. " +
    "This is __***bold, italic and underlined***__ text. " +
    "This is ~~***bold, italic and strikethrough***~~ text. " +
    "This is __~~***all of it***~~__ text. " +
    "This is a ||hidden|| text. " +
    "Here should be a\n break." +
    "\n" +
    "- This is a list\n" +
    "- This is still in the list :3\n" +
    "\n" +
    "> This is a blockquote" +
    "\n" +
    "Oh hey look at this [Example Link](https://www.example.com)!" +
    "\n" +
    "```ts\n" + 
    "function parseCodeBlockText(text: string): JSX.Element[] {\n" +
    "    text = text.replaceAll(\"``\", \"\");\n" +
    "    const languages = text.match(/^.*$/m);\n" +
    "    const language = languages ? languages[0] : \"\"; \n" +
    "    const highlightedText: string = hljs.highlight(text, {language: language}).value;\n" +
    "    const regex = /$\\n|[^\\n]+/gm\n" +
    "    const parts = highlightedText.match(regex);\n" +
    "\n" +
    "    if (!parts)\n" +
    "        return [<></>];\n" +
    "\n" +
    "    const output = parts.map((part, index) => {\n" +
    "        if (part.match(/$\\n/m)) {\n" +
    "            if (index == 0)\n" +
    "                return;\n" +
    "            return <br key={index}></br>;\n" +
    "        }\n" +
    "        return <span dangerouslySetInnerHTML={{ __html: part, }}></span>;\n" +
    "    });\n" +
    "\n" +
    "    // highlighter.js is trustworthy\n" +
    "    return [<code>{output}</code>];\n" +
    "}\n" +
    "```"
;
const outputElement = parseFormattedText(inputText);
const textPart = document.createElement('p');
ReactDOM.render(outputElement, textPart);

const parent = document.getElementById("content")
if (parent) {
    parent.appendChild(textPart);
}

const endTime = performance.now()
console.log(`Took ${endTime - startTime}ms`)

// @ts-ignore
const messages = await Query<[], GetMessagesContext>("/api/v1/channel/get_messages", new GetMessagesContext(4177598349313))
console.log(messages)
if (messages != null) {
    for (const message of messages) {
        // @ts-ignore
        const outputElement = parseFormattedText(message.Content);
        const textPart = document.createElement('p');
        ReactDOM.render(outputElement, textPart);

        const parent = document.getElementById("content")
        if (parent) {
            parent.appendChild(textPart);
        }
    }
}

const accountData = await Get<SelfUserContent>("/api/v1/account/get_self_user")
console.log(accountData)