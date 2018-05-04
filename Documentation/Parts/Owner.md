# Module: Owner

### owner announce
<details><summary markdown='span'>Expand for additional information</summary><p>

*Send a message to all guilds the bot is in.*

**Owner-only.**

**Aliases:**
`a, ann`

**Arguments:**

`[string...]` : *Message to send.*

**Examples:**

```
!owner announce SPAM SPAM
```
</p></details>

---

## Group: owner birthdays
<details><summary markdown='span'>Expand for additional information</summary><p>

*Birthday notifications management. If invoked without command, either lists or adds birthdays depending if argument is given.*

**Aliases:**
`birthday, bday, bd, bdays`

**Overload 1:**

`[user]` : *Birthday boy/girl.*

(optional) `[string]` : *Birth date.* (def: `None`)

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

**Overload 0:**

`[user]` : *Birthday boy/girl.*

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

(optional) `[string]` : *Birth date.* (def: `None`)

</p></details>

---

### owner birthdays add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a birthday to the database. If date is not specified, uses the current date as a birthday date. If the channel is not specified, uses the current channel.*

**Aliases:**
`+, a`

**Overload 1:**

`[user]` : *Birthday boy/girl.*

(optional) `[string]` : *Birth date.* (def: `None`)

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

**Overload 0:**

`[user]` : *Birthday boy/girl.*

(optional) `[channel]` : *Channel to send a greeting message to.* (def: `None`)

(optional) `[string]` : *Birth date.* (def: `None`)

**Examples:**

```
!owner birthday add @Someone
!owner birthday add @Someone #channel_to_send_message_to
!owner birthday add @Someone 15.2.1990
!owner birthday add @Someone #channel_to_send_message_to 15.2.1990
!owner birthday add @Someone 15.2.1990 #channel_to_send_message_to
```
</p></details>

---

### owner birthdays delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove status from running queue.*

**Aliases:**
`-, remove, rm, del`

**Arguments:**

`[user]` : *User whose birthday to remove.*

**Examples:**

```
!owner birthday delete @Someone
```
</p></details>

---

### owner birthdays list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all registered birthdays.*

**Aliases:**
`ls`

**Examples:**

```
!owner birthday list
```
</p></details>

---

## Group: owner blockedchannels
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate blocked channels. Bot will not listen for commands in blocked channels or react (either with text or emoji) to messages inside.*

**Aliases:**
`bc, blockedc, blockchannel, bchannels, bchannel, bchn`

**Overload 2:**

`[channel...]` : *Users to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[channel...]` : *Users to block.*

**Overload 0:**

`[channel]` : *Users to block.*

`[string...]` : *Reason (max 60 chars).*

</p></details>

---

### owner blockedchannels add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add channel to blocked channels list.*

**Aliases:**
`+, a`

**Overload 2:**

`[channel...]` : *Channels to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[channel...]` : *Channels to block.*

**Overload 0:**

`[channel]` : *Channel to block.*

`[string...]` : *Reason (max 60 chars).*

**Examples:**

```
!owner blockedchannels add #channel
!owner blockedchannels add #channel Some reason for blocking
!owner blockedchannels add 123123123123123
!owner blockedchannels add #channel 123123123123123
!owner blockedchannels add "This is some reason" #channel 123123123123123
```
</p></details>

---

### owner blockedchannels delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove channel from blocked channels list..*

**Aliases:**
`-, remove, rm, del`

**Arguments:**

`[channel...]` : *Channels to unblock.*

**Examples:**

```
!owner blockedchannels remove #channel
!owner blockedchannels remove 123123123123123
!owner blockedchannels remove @Someone 123123123123123
```
</p></details>

---

### owner blockedchannels list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all blocked channels.*

**Aliases:**
`ls`

**Examples:**

```
!owner blockedchannels list
```
</p></details>

---

## Group: owner blockedusers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate blocked users. Bot will not allow blocked users to invoke commands and will not react (either with text or emoji) to their messages.*

**Aliases:**
`bu, blockedu, blockuser, busers, buser, busr`

**Overload 2:**

`[user...]` : *Users to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[user...]` : *Users to block.*

**Overload 0:**

`[user]` : *Users to block.*

`[string...]` : *Reason (max 60 chars).*

</p></details>

---

### owner blockedusers add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add users to blocked users list.*

**Aliases:**
`+, a`

**Overload 2:**

`[user...]` : *Users to block.*

**Overload 1:**

`[string]` : *Reason (max 60 chars).*

`[user...]` : *Users to block.*

**Overload 0:**

`[user]` : *Users to block.*

`[string...]` : *Reason (max 60 chars).*

**Examples:**

```
!owner blockedusers add @Someone
!owner blockedusers add @Someone Troublemaker and spammer
!owner blockedusers add 123123123123123
!owner blockedusers add @Someone 123123123123123
!owner blockedusers add "This is some reason" @Someone 123123123123123
```
</p></details>

---

### owner blockedusers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove users from blocked users list..*

**Aliases:**
`-, remove, rm, del`

**Arguments:**

`[user...]` : *Users to unblock.*

**Examples:**

```
!owner blockedusers remove @Someone
!owner blockedusers remove 123123123123123
!owner blockedusers remove @Someone 123123123123123
```
</p></details>

---

### owner blockedusers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all blocked users.*

**Aliases:**
`ls`

**Examples:**

```
!owner blockedusers list
```
</p></details>

---

### owner botavatar
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set bot avatar.*

**Owner-only.**

**Aliases:**
`setbotavatar, setavatar`

**Arguments:**

`[string]` : *URL.*

**Examples:**

```
!owner botavatar http://someimage.png
```
</p></details>

---

### owner botname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set bot name.*

**Owner-only.**

**Aliases:**
`setbotname, setname`

**Arguments:**

`[string...]` : *New name.*

**Examples:**

```
!owner setname TheBotfather
```
</p></details>

---

### owner clearlog
<details><summary markdown='span'>Expand for additional information</summary><p>

*Clear application logs.*

**Owner-only.**

**Aliases:**
`clearlogs, deletelogs, deletelog`

**Examples:**

```
!owner clearlog
```
</p></details>

---

### owner dbquery
<details><summary markdown='span'>Expand for additional information</summary><p>

*Execute SQL query on the bot database.*

**Owner-only.**

**Aliases:**
`sql, dbq, q`

**Arguments:**

`[string...]` : *SQL Query.*

**Examples:**

```
!owner dbquery SELECT * FROM gf.msgcount;
```
</p></details>

---

### owner eval
<details><summary markdown='span'>Expand for additional information</summary><p>

*Evaluates a snippet of C# code, in context. Surround the code in the code block.*

**Owner-only.**

**Aliases:**
`compile, run, e, c, r`

**Arguments:**

`[string...]` : *Code to evaluate.*

**Examples:**

```
!owner eval ```await Context.RespondAsync("Hello!");```
```
</p></details>

---

### owner filelog
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggle writing to log file.*

**Owner-only.**

**Aliases:**
`setfl, fl, setfilelog`

**Arguments:**

(optional) `[boolean]` : *True/False* (def: `True`)

**Examples:**

```
!owner filelog yes
!owner filelog false
```
</p></details>

---

### owner generatecommandlist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Generates a markdown command-list. You can also provide a folder for the output.*

**Owner-only.**

**Aliases:**
`cmdlist, gencmdlist, gencmds, gencmdslist`

**Arguments:**

(optional) `[string...]` : *File path.* (def: `None`)

**Examples:**

```
!owner generatecommandlist
!owner generatecommandlist Temp/blabla.md
```
</p></details>

---

### owner leaveguilds
<details><summary markdown='span'>Expand for additional information</summary><p>

*Leaves the given guilds.*

**Owner-only.**

**Aliases:**
`leave, gtfo`

**Arguments:**

`[unsigned long...]` : *Guild ID list.*

**Examples:**

```
!owner leave 337570344149975050
!owner leave 337570344149975050 201315884709576708
```
</p></details>

---

## Group: owner priviledgedusers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate priviledged users. Priviledged users can invoke commands marked with RequirePriviledgedUsers permission.*

**Owner-only.**

**Aliases:**
`pu, privu, privuser, pusers, puser, pusr`

**Overload 0:**

`[user...]` : *Users to grant priviledge to.*

</p></details>

---

### owner priviledgedusers add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add users to priviledged users list.*

**Owner-only.**

**Aliases:**
`+, a`

**Arguments:**

`[user...]` : *Users to grant priviledge to.*

**Examples:**

```
!owner priviledgedusers add @Someone
!owner priviledgedusers add @Someone @SomeoneElse
```
</p></details>

---

### owner priviledgedusers delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove users from priviledged users list..*

**Owner-only.**

**Aliases:**
`-, remove, rm, del`

**Arguments:**

`[user...]` : *Users to revoke priviledges from.*

**Examples:**

```
!owner priviledgedusers remove @Someone
!owner priviledgedusers remove 123123123123123
!owner priviledgedusers remove @Someone 123123123123123
```
</p></details>

---

### owner priviledgedusers list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all priviledged users.*

**Owner-only.**

**Aliases:**
`ls`

**Examples:**

```
!owner priviledgedusers list
```
</p></details>

---

### owner sendmessage
<details><summary markdown='span'>Expand for additional information</summary><p>

*Sends a message to a user or channel.*

**Aliases:**
`send, s`

**Arguments:**

`[string]` : *u/c (for user or channel.)*

`[unsigned long]` : *User/Channel ID.*

`[string...]` : *Message.*

**Examples:**

```
!owner send u 303463460233150464 Hi to user!
!owner send c 120233460278590414 Hi to channel!
```
</p></details>

---

### owner shutdown
<details><summary markdown='span'>Expand for additional information</summary><p>

*Triggers the dying in the vineyard scene (power off the bot).*

**Aliases:**
`disable, poweroff, exit, quit`

**Overload 1:**

`[time span]` : *Time until shutdown.*

**Examples:**

```
!owner shutdown
```
</p></details>

---

## Group: owner statuses
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bot status manipulation. If invoked without command, either lists or adds status depending if argument is given.*

**Owner-only.**

**Aliases:**
`status, botstatus, activity, activities`

**Overload 0:**

`[ActivityType]` : *Activity type (Playing/Watching/Streaming/ListeningTo).*

`[string...]` : *Status.*

</p></details>

---

### owner statuses add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a status to running status queue.*

**Owner-only.**

**Aliases:**
`+, a`

**Arguments:**

`[ActivityType]` : *Activity type (Playing/Watching/Streaming/ListeningTo).*

`[string...]` : *Status.*

**Examples:**

```
!owner status add Playing CS:GO
!owner status add Streaming on Twitch
```
</p></details>

---

### owner statuses delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove status from running queue.*

**Owner-only.**

**Aliases:**
`-, remove, rm, del`

**Arguments:**

`[int]` : *Status ID.*

**Examples:**

```
!owner status delete 1
```
</p></details>

---

### owner statuses list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all bot statuses.*

**Owner-only.**

**Aliases:**
`ls`

**Examples:**

```
!owner status list
```
</p></details>

---

### owner statuses set
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set status to given string or status with given index in database. This sets rotation to false.*

**Owner-only.**

**Aliases:**
`s`

**Overload 1:**

`[ActivityType]` : *Activity type (Playing/Watching/Streaming/ListeningTo).*

`[string...]` : *Status.*

**Overload 0:**

`[int]` : *Status ID.*

**Examples:**

```
!owner status set Playing with fire
!owner status set 5
```
</p></details>

---

### owner statuses setrotation
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set automatic rotation of bot statuses.*

**Owner-only.**

**Aliases:**
`sr, setr`

**Arguments:**

(optional) `[boolean]` : *True/False* (def: `True`)

**Examples:**

```
!owner status setrotation
!owner status setrotation false
```
</p></details>

---

### owner sudo
<details><summary markdown='span'>Expand for additional information</summary><p>

*Executes a command as another user.*

**Aliases:**
`execas, as`

**Arguments:**

`[member]` : *Member to execute as.*

`[string...]` : *Command text to execute.*

**Examples:**

```
!owner sudo @Someone !rate
```
</p></details>

---

### owner toggleignore
<details><summary markdown='span'>Expand for additional information</summary><p>

*Toggle bot's reaction to commands.*

**Aliases:**
`ti`

**Examples:**

```
!owner toggleignore
```
</p></details>

---

