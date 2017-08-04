# osu!HOPE
HoLLy's osu! Packet Editor

## Summary
osu!HOPE allows you to view and edit all traffic going to and coming from osu!Bancho, excluding the very first login request (for security reasons). This allows you to do all sorts of cool things, such as:
* Integrating osu!'s chat with external programs
* Dynamically blocking chat messages
* Creating chat macro's, where you can automatically replace parts of outgoing messages with other text
* Showing a notification ingame for important events
* Logging the user's Bancho status (eg. Idle, Playing x, Multiplaying, ...) to a file for use in stream overlays
* Debugging custom/private servers by being able to look at any packet going through bancho
* *and so much more...*

## Warning
While osu!HOPE in itself does not break any rules, it allows the user to create plugins that do. For this reason, peppy may or may not ban the use of this program, so I would not recommend using it on the official osu! servers.

## Dependencies:
* Uses [Titanium Web Proxy](https://github.com/justcoding121/Titanium-Web-Proxy) by justcoding121, licensed under the [MIT License](https://github.com/justcoding121/Titanium-Web-Proxy/blob/develop/LICENSE).
  * Titanium Web Proxy uses [BouncyCastle](https://www.bouncycastle.org/) by the Bouncy Castle Project, licensed under the [MIT License](https://www.bouncycastle.org/licence.html)
* Uses [HOPEless](https://github.com/HoLLy-HaCKeR/HOPEless) by me, licensed under the [MIT License](https://github.com/HoLLy-HaCKeR/HOPEless/blob/master/LICENSE.md)
