import {ServerUser, User} from "./Common/User.js";
import {Server} from "./Common/Server.js";
import {Channel} from "./Common/Channel.js";
import {Post} from "./Common/Server/HttpConnections";

// Example Code :3
let user = new User("1", "New User", 0, 0);
let serverUser = user as ServerUser;
serverUser.nickname = null;
serverUser.permissionLevel = 0;

let channelOne = new Channel("1", "Channel One", null, []);
let channelTwo = new Channel("2", "Channel Two", null, []);

let server = new Server("1", "New Server", serverUser, [channelOne, channelTwo], [serverUser], [serverUser]);
channelOne.server = server;
channelTwo.server = server;

console.log(server);
console.log(channelOne)
console.log(channelTwo)
console.log(user);
console.log("Hello World!");

async function foo() {
    return Post<null, {}>("api/v1/channel/send_message", {})
}
console.log(await foo());