# Module: Music

## connect
*Connect the bot to a voice channel. If the channel is not given, connects the bot to the same channel you are in.*

**Owner-only.**

**Requires permissions:**
`Use voice chat`

**Aliases:**
`con, conn, enter`

**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

**Examples:**

```
!connect
!connect Music
```
---

## disconnect
*Disconnects the bot from the voice channel.*

**Owner-only.**

**Requires permissions:**
`Use voice chat`

**Aliases:**
`dcon, dconn, discon, disconn, dc`

**Examples:**

```
!disconnect
```
---

## Group: play
*Commands for playing music. If invoked without subcommand, plays given URL or searches YouTube for given query and plays the first result.*

**Requires bot permissions:**
`Speak`

**Aliases:**
`music, p`

**Arguments:**

`[string...]` : *URL or YouTube search query.*

**Examples:**

```
!play https://www.youtube.com/watch?v=dQw4w9WgXcQ
!play what is love?
```
---

### play file
*Plays an audio file from the server filesystem.*

**Owner-only.**

**Requires bot permissions:**
`Speak`

**Aliases:**
`f`

**Arguments:**

`[string...]` : *Full path to the file to play.*

**Examples:**

```
!play file test.mp3
```
---

## skip
*Skip current voice playback.*

**Owner-only.**

**Requires permissions:**
`Use voice chat`

**Examples:**

```
!skip
```
---

## stop
*Stops current voice playback.*

**Owner-only.**

**Requires permissions:**
`Use voice chat`

**Examples:**

```
!stop
```
---

