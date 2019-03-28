# Command list

# Module: Administration

## Group: automaticroles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Automatic roles management. Automatic roles are automatically granted to a new member of the guild. Group call lists all automatic roles for the guild. Group call with an arbitrary amount of roles will add those roles to the automatic roles list for the guild, effective immediately.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`autoroles, automaticr, autorole, aroles, arole, arl, ar, aar`

**Overload 0:**

`[role...]` : *Roles to add.*

**Examples:**

```
!ar
!ar @Guests
```
</p></details>

---

### automaticroles add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add automatic role(s).*

**Requires user permissions:**
`Administrator`

**Aliases:**
`a, +, +=, <<, <`

**Arguments:**

`[role...]` : *Roles to add.*

**Examples:**

```
!ar add @Notifications
!ar add @Notifications @Role1 @Role2
```
</p></details>

---

### automaticroles delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove automatic role(s).*

**Requires user permissions:**
`Administrator`

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`

**Arguments:**

`[role...]` : *Roles to remove.*

**Examples:**

```
!ar delete @Notifications
!ar delete @Notifications @Role1 @Role2
```
</p></details>

---

### automaticroles deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all automatic roles for this guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da`

**Examples:**

```
!ar deleteall
```
</p></details>

---

### automaticroles list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all current automatic roles.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`print, show, ls, l, p`

**Examples:**

```
!ar list
```
</p></details>

---

## Group: channel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Channel administration. Group call prints channel information.*

**Aliases:**
`channels, chn, ch, c`

**Arguments:**

(optional) `[channel]` : *Channel to scan.* (def: `None`)

**Examples:**

```
!channel
!channel #general
```
</p></details>

---

### channel clone
<details><summary markdown='span'>Expand for additional information</summary><p>

*Clone a channel.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`copy, cp`

**Arguments:**

`[channel]` : *Channel to clone.*

(optional) `[string...]` : *Name for the cloned channel.* (def: `None`)

**Examples:**

```
!channel clone #general newname
```
</p></details>

---

### channel createcategory
<details><summary markdown='span'>Expand for additional information</summary><p>

*Create new channel category.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`addcategory, createcat, createc, ccat, cc, +category, +cat, +c, <c, <<c`

**Arguments:**

`[string...]` : *Name for the category.*

**Examples:**

```
!channel createcategory My New Category
```
</p></details>

---

### channel createtext
<details><summary markdown='span'>Expand for additional information</summary><p>

*Create new text channel. You can also specify channel parent, user limit and bitrate.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`addtext, addtxt, createtxt, createt, ctxt, ct, +, +txt, +t, <t, <<t`

**Overload 2:**

`[string]` : *Name for the channel.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[boolean]` : *NSFW?* (def: `False`)

**Overload 1:**

`[string]` : *Name for the channel.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

(optional) `[channel]` : *Parent category.* (def: `None`)

**Overload 0:**

`[channel]` : *Parent category.*

`[string]` : *Name for the channel.*

(optional) `[boolean]` : *NSFW?* (def: `False`)

**Examples:**

```
!channel addtext newtextchannel ParentCategory no
!channel addtext newtextchannel no
!channel addtext ParentCategory newtextchannel
```
</p></details>

---

### channel createvoice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Create new voice channel. You can also specify channel parent, user limit and bitrate.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`addvoice, addv, createv, cvoice, cv, +voice, +v, <v, <<v`

**Overload 2:**

`[string]` : *Name for the channel.*

(optional) `[channel]` : *Parent category.* (def: `None`)

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

**Overload 1:**

`[string]` : *Name for the channel.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

(optional) `[channel]` : *Parent category.* (def: `None`)

**Overload 0:**

`[channel]` : *Parent category.*

`[string]` : *Name for the channel.*

(optional) `[int]` : *User limit.* (def: `None`)

(optional) `[int]` : *Bitrate.* (def: `None`)

**Examples:**

```
!channel createtext "My voice channel" ParentCategory 0 96000
!channel createtext "My voice channel" 10 96000
!channel createtext ParentCategory "My voice channel" 10 96000
```
</p></details>

---

### channel delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete a given channel or category. If the channel isn't given, deletes the current one. You can also specify reason for deletion.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`-, del, d, remove, rm`

**Overload 1:**

(optional) `[channel]` : *Channel to delete.* (def: `None`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[string...]` : *Reason.*

**Examples:**

```
!channel delete
!channel delete "My voice channel"
!channel delete "My voice channel" Because I can!
```
</p></details>

---

### channel info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print information about a given channel. If the channel is not given, uses the current one.*

**Aliases:**
`i, information`

**Arguments:**

(optional) `[channel]` : *Channel.* (def: `None`)

**Examples:**

```
!channel info
!channel info "My voice channel"
```
</p></details>

---

### channel modify
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modify a given voice channel. Give 0 as an argument if you wish to keep the value unchanged.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`edit, mod, m, e`

**Overload 1:**

`[channel]` : *Voice channel to edit*

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

(optional) `[int]` : *User limit.* (def: `0`)

(optional) `[int]` : *Bitrate.* (def: `0`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!channel modify "My voice channel" 20 96000 Some reason
```
</p></details>

---

### channel rename
<details><summary markdown='span'>Expand for additional information</summary><p>

*Rename given channel. If the channel is not given, renames the current one.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`r, name, setname, rn`

**Overload 2:**

`[string]` : *Reason.*

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

**Overload 1:**

`[channel]` : *Channel to rename.*

`[string...]` : *New name.*

**Overload 0:**

`[string...]` : *New name.*

**Examples:**

```
!channel rename New name for this channel
!channel rename "My voice channel" "My old voice channel"
!channel rename "My reason" "My voice channel" "My old voice channel"
```
</p></details>

---

### channel setnsfw
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set whether this channel is NSFW or not. You can also provide a reason for the change.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`nsfw`

**Overload 2:**

`[boolean]` : *Set NSFW?*

(optional) `[channel]` : *Channel.* (def: `None`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 1:**

`[channel]` : *Channel.*

`[boolean]` : *Set NSFW?*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

(optional) `[channel]` : *Channel.* (def: `None`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!channel setnsfw #general
!channel setnsfw false #general
```
</p></details>

---

### channel setparent
<details><summary markdown='span'>Expand for additional information</summary><p>

*Change the given channel's parent. If the channel is not given, uses the current one. You can also provide a reason.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`setpar, par, parent`

**Overload 1:**

`[channel]` : *Child channel.*

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[channel]` : *Parent category.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!channel setparent "My channel" ParentCategory
!channel setparent ParentCategory I set a new parent for this channel!
```
</p></details>

---

### channel setposition
<details><summary markdown='span'>Expand for additional information</summary><p>

*Change the position of the given channel in the guild channel list. If the channel is not given, repositions the current one. You can also provide reason.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`setpos, pos, position`

**Overload 2:**

`[channel]` : *Channel to reposition.*

`[int]` : *New position.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 1:**

`[int]` : *Position.*

`[channel]` : *Channel to reorder.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[int]` : *Position.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!channel setposition 4
!channel setposition "My channel" 1
!channel setposition "My channel" 4 I changed position :)
```
</p></details>

---

### channel setratelimit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the per-user ratelimit for given channel. Setting the value to 0 will disable ratelimit.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`setrl, setrate, setrlimit`

**Overload 1:**

`[channel]` : *Channel to affect.*

`[int]` : *New ratelimit.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[int]` : *New ratelimit.*

`[channel]` : *Channel to affect.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[int]` : *New ratelimit.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!channel setratelimit #general 5
!channel setratelimit 5 #general Reason
```
</p></details>

---

### channel settopic
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set channel topic. If the channel is not given, uses the current one.*

**Requires permissions:**
`Manage channels`

**Aliases:**
`t, topic, sett`

**Overload 2:**

`[string]` : *Reason.*

`[channel]` : *Channel.*

`[string...]` : *New topic.*

**Overload 1:**

`[channel]` : *Channel.*

`[string...]` : *New Topic.*

**Overload 0:**

`[string...]` : *New Topic.*

**Examples:**

```
!channel settopic New channel topic
!channel settopic "My channel" New channel topic
```
</p></details>

---

### channel viewperms
<details><summary markdown='span'>Expand for additional information</summary><p>

*View permissions for a member or role in the given channel. If the member is not given, lists the sender's permissions. If the channel is not given, uses the current one.*

**Requires bot permissions:**
`Administrator`

**Aliases:**
`tp, perms, permsfor, testperms, listperms`

**Overload 3:**

(optional) `[member]` : *Member.* (def: `None`)

(optional) `[channel]` : *Channel.* (def: `None`)

**Overload 2:**

`[channel]` : *Channel.*

(optional) `[member]` : *Member.* (def: `None`)

**Overload 1:**

`[role]` : *Role.*

(optional) `[channel]` : *Channel.* (def: `None`)

**Overload 0:**

`[channel]` : *Channel.*

`[role]` : *Role.*

**Examples:**

```
!channel viewperms @Someone
!channel viewperms Admins
!channel viewperms #private everyone
!channel viewperms everyone #private
```
</p></details>

---

## Group: commandrules
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manage command rules. You can specify a rule to block a command in a certain channel, or allow a command to be executed only in specific channel. Group call lists all commandrules for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`cmdrules, crules, cr`

</p></details>

---

### commandrules allow
<details><summary markdown='span'>Expand for additional information</summary><p>

*Allow a command to be executed only in specific channel(s).*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`a, only`

**Arguments:**

`[string]` : *Command to forbid.*

`[channel...]` : *Channels where to forbid.*

**Examples:**

```
!commandrules allow
```
</p></details>

---

### commandrules forbid
<details><summary markdown='span'>Expand for additional information</summary><p>

*Forbid a command to be executed in a specific channel(s) (or globally if no channel is provided).*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`f, deny`

**Arguments:**

`[string]` : *Command to forbid.*

`[channel...]` : *Channels where to forbid.*

**Examples:**

```
!commandrules allow
```
</p></details>

---

### commandrules list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all command rules for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ls, l`

**Examples:**

```
!commandrules list
```
</p></details>

---

## Group: emoji
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manipulate guild emoji. Standalone call lists all guild emoji or prints information about given emoji.*

**Aliases:**
`emojis, e`

**Overload 0:**

`[emoji]` : *Emoji to print information about.*

**Examples:**

```
!emoji
!emoji :some_emoji:
```
</p></details>

---

### emoji add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add emoji specified via URL or as an attachment. If you have Discord Nitro, you can also pass emojis from another guild as arguments instead of their URLs.*

**Requires permissions:**
`Manage emoji`

**Aliases:**
`addnew, create, install, a, +, +=, <, <<`

**Overload 3:**

`[string]` : *Name for the emoji.*

(optional) `[URL]` : *Image URL.* (def: `None`)

**Overload 2:**

`[URL]` : *Image URL.*

`[string]` : *Name for the emoji.*

**Overload 1:**

`[string]` : *Name for the emoji.*

`[emoji]` : *Emoji from another server to steal.*

**Overload 0:**

`[emoji]` : *Emoji from another server to steal.*

`[string]` : *Name.*

**Examples:**

```
!emoji add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg
!emoji add pepe [ATTACHED IMAGE]
!emoji add pepe :pepe_from_other_server:
```
</p></details>

---

### emoji delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove guild emoji. Note: Bots can only delete emojis they created!*

**Requires permissions:**
`Manage emoji`

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`

**Arguments:**

`[emoji]` : *Emoji to delete.*

**Examples:**

```
!emoji delete pepe
```
</p></details>

---

### emoji info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prints information for given guild emoji.*

**Aliases:**
`details, information, i`

**Arguments:**

`[emoji]` : *Emoji.*

**Examples:**

```
!emoji info :pepe:
```
</p></details>

---

### emoji list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all emojis for this guild.*

**Aliases:**
`print, show, l, p, ls`

**Examples:**

```
!emoji list
```
</p></details>

---

### emoji modify
<details><summary markdown='span'>Expand for additional information</summary><p>

*Edit name of an existing guild emoji.*

**Requires permissions:**
`Manage emoji`

**Aliases:**
`edit, mod, e, m, rename`

**Overload 1:**

`[emoji]` : *Emoji to rename.*

`[string]` : *New name.*

**Overload 0:**

`[string]` : *New name.*

`[emoji]` : *Emoji to rename.*

**Examples:**

```
!emoji modify :pepe: newname
!emoji modify newname :pepe:
```
</p></details>

---

## Group: filter
<details><summary markdown='span'>Expand for additional information</summary><p>

*Message filtering commands. If invoked without subcommand, either lists all filters or adds a new filter for the given word list. Filters are regular expressions.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`f, filters`

**Overload 0:**

`[string...]` : *Filter list. Filter is a regular expression (case insensitive).*

**Examples:**

```
!filter fuck fk f+u+c+k+
```
</p></details>

---

### filter add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add filter to guild filter list.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`addnew, create, a, +, +=, <, <<`

**Arguments:**

`[string...]` : *Filter list. Filter is a regular expression (case insensitive).*

**Examples:**

```
!filter add fuck f+u+c+k+
```
</p></details>

---

### filter delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes filter either by ID or plain text match.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`

**Overload 1:**

`[int...]` : *Filters IDs to remove.*

**Overload 0:**

`[string...]` : *Filters to remove.*

**Examples:**

```
!filter delete fuck f+u+c+k+
!filter delete 3 4
```
</p></details>

---

### filter deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all filters for the current guild.*

**Requires user permissions:**
`Administrator, Manage guild`

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da`

**Examples:**

```
!filter clear
```
</p></details>

---

### filter list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all filters for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ls, l`

**Examples:**

```
!filter list
```
</p></details>

---

## Group: forbiddennames
<details><summary markdown='span'>Expand for additional information</summary><p>

*Manage forbidden names for this guild. Group call shows all the forbidden nicknames for this guild.*

**Requires permissions:**
`Manage nicknames`

**Requires user permissions:**
`Manage guild`

**Aliases:**
`forbiddenname, forbiddennicknames, fn, disallowednames`

**Overload 0:**

`[string...]` : *Forbidden name list (can be regexes)*

**Examples:**

```
!forbiddennames f+u+c+k
```
</p></details>

---

### forbiddennames add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add nicknames to the forbidden list (can be a regex).*

**Requires permissions:**
`Manage nicknames`

**Requires user permissions:**
`Manage guild`

**Aliases:**
`addnew, create, a, +, +=, <, <<`

**Arguments:**

`[string...]` : *Name list.*

**Examples:**

```
!forbiddennames add fuck f+u+c+k+
```
</p></details>

---

### forbiddennames delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Removes forbidden name either by ID or plain text match.*

**Requires permissions:**
`Manage nicknames`

**Requires user permissions:**
`Manage guild`

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`

**Overload 1:**

`[int...]` : *Forbidden name IDs to remove.*

**Overload 0:**

`[string...]` : *Forbidden name IDs to remove.*

**Examples:**

```
!forbiddennames delete 3 4
!forbiddennames delete SomeName
```
</p></details>

---

### forbiddennames deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all forbidden names for the current guild.*

**Requires permissions:**
`Manage nicknames`

**Requires user permissions:**
`Administrator, Manage guild`

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da`

**Examples:**

```
!forbiddennames clear
```
</p></details>

---

### forbiddennames list
<details><summary markdown='span'>Expand for additional information</summary><p>

*Show all forbidden names for this guild.*

**Requires permissions:**
`Manage nicknames`

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ls, l`

**Examples:**

```
!forbiddennames list
```
</p></details>

---

## Group: guild
<details><summary markdown='span'>Expand for additional information</summary><p>

*Miscellaneous guild control commands. Group call prints guild information.*

**Aliases:**
`server, g`

**Examples:**

```
!guild
```
</p></details>

---

## Group: guild configure
<details><summary markdown='span'>Expand for additional information</summary><p>

*Allows manipulation of guild settings for this bot. If invoked without subcommands, lists the current guild configuration.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`configuration, config, cfg`

**Examples:**

```
!guild configure
```
</p></details>

---

## Group: guild configure antiflood
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prevents guild raids (groups of users purposely flooding the guild). You can specify the action, sensitivity (number of users allowed to join before the action is performed) as well as the cooldown (timespan after which the user is removed from the watch. For example, an active watch with sensitivity 5 and cooldown of 10s will execute action if 5 or more users join the guild in period of 10s. The action is applied to all of the users that are currently under watch.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`antiraid, ar, af`

**Overload 5:**

`[boolean]` : *Enable?*

`[short]` : *Sensitivity (number of users allowed to join within a given timespan).*

(optional) `[PunishmentActionType]` : *Action type.* (def: `Kick`)

(optional) `[time span]` : *Cooldown.* (def: `None`)

**Overload 4:**

`[boolean]` : *Enable?*

`[PunishmentActionType]` : *Action type.*

(optional) `[short]` : *Sensitivity (number of users allowed to join within a given timespan).* (def: `5`)

(optional) `[time span]` : *Cooldown.* (def: `None`)

**Overload 3:**

`[boolean]` : *Enable?*

`[PunishmentActionType]` : *Action type.*

(optional) `[time span]` : *Cooldown.* (def: `None`)

(optional) `[short]` : *Sensitivity (number of users allowed to join within a given timespan).* (def: `5`)

**Overload 2:**

`[boolean]` : *Enable?*

`[time span]` : *Cooldown.*

(optional) `[PunishmentActionType]` : *Action type.* (def: `Kick`)

(optional) `[short]` : *Sensitivity (number of users allowed to join within a given timespan).* (def: `5`)

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg antiflood
!guild cfg antiflood on
!guild cfg antiflood on kick 5s
```
</p></details>

---

### guild configure antiflood action
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the action to execute on the users when they flood/raid the guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setaction, a`

**Arguments:**

`[PunishmentActionType]` : *Action type.*

**Examples:**

```
!guild cfg antiflood action mute
!guild cfg antiflood action temporaryban
```
</p></details>

---

### guild configure antiflood cooldown
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the antiflood sensitivity. Antiflood action will be executed if the specified amount of users join the guild in the given cooldown period.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setcooldown, setcool, cool, c`

**Arguments:**

`[time span]` : *Cooldown.*

**Examples:**

```
!guild cfg antiflood cooldown 9s
```
</p></details>

---

### guild configure antiflood sensitivity
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the antiflood sensitivity. Antiflood action will be executed if the specified amount of users join the guild in the given cooldown period.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setsensitivity, setsens, sens, s`

**Arguments:**

`[short]` : *Sensitivity (number of users allowed to join within a given timespan).*

**Examples:**

```
!guild cfg antiflood sensitivity 9
```
</p></details>

---

## Group: guild configure antispam
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prevents users from posting more than specified amount of same messages.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`as`

**Overload 3:**

`[boolean]` : *Enable?*

`[short]` : *Sensitivity (max repeated messages).*

(optional) `[PunishmentActionType]` : *Action type.* (def: `TemporaryMute`)

**Overload 2:**

`[boolean]` : *Enable?*

`[PunishmentActionType]` : *Action type.*

(optional) `[short]` : *Sensitivity (max repeated messages).* (def: `5`)

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg antispam
!guild cfg antispam on
!guild cfg antispam on mute
!guild cfg antispam on 5
!guild cfg antispam on 6 kick
```
</p></details>

---

### guild configure antispam action
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the action to execute when the antispam quota is hit.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setaction, a`

**Arguments:**

`[PunishmentActionType]` : *Action type.*

**Examples:**

```
!guild cfg antispam action mute
!guild cfg antispam action temporaryban
```
</p></details>

---

### guild configure antispam exempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disable the antispam watch for some entities (users, channels, etc).*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ex, exc`

**Overload 2:**

`[member...]` : *Members to exempt.*

**Overload 1:**

`[role...]` : *Roles to exempt.*

**Overload 0:**

`[channel...]` : *Channels to exempt.*

**Examples:**

```
!guild cfg antispam exempt @Someone
!guild cfg antispam exempt #spam
!guild cfg antispam exempt Role
```
</p></details>

---

### guild configure antispam sensitivity
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the antispam sensitivity - max amount of repeated messages before an action is taken.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setsensitivity, setsens, sens, s`

**Arguments:**

`[short]` : *Sensitivity (max repeated messages).*

**Examples:**

```
!guild cfg antispam sensitivity 9
```
</p></details>

---

### guild configure antispam unexempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove an exempted entity and allow antispam watch for that entity.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`unex, uex`

**Overload 2:**

`[member...]` : *Members to unexempt.*

**Overload 1:**

`[role...]` : *Roles to unexempt.*

**Overload 0:**

`[channel...]` : *Channels to unexempt.*

**Examples:**

```
!guild cfg antispam unexempt @Someone
!guild cfg antispam unexempt #spam
!guild cfg antispam unexempt Category
```
</p></details>

---

## Group: guild configure instantleave
<details><summary markdown='span'>Expand for additional information</summary><p>

*Automatically bans users which leave in certain timespan after joining.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`joinleave, instaleave, il, jl`

**Overload 2:**

`[boolean]` : *Enable?*

`[short]` : *Cooldown (join-leave max seconds).*

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg instaleave
!guild cfg instaleave on
!guild cfg instaleave on 5
```
</p></details>

---

### guild configure instantleave cooldown
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the instant leave sensitivity. User will be banned if he leaves within the given time window (in seconds).*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setcooldown, setcool, cool, c`

**Arguments:**

`[short]` : *Cooldown (in seconds).*

**Examples:**

```
!guild cfg instaleave cooldown 5
```
</p></details>

---

### guild configure leave
<details><summary markdown='span'>Expand for additional information</summary><p>

*Allows user leaving message configuration.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`exit, drop, lvm, lm, l`

**Overload 2:**

`[boolean]` : *Enable leave messages?*

(optional) `[channel]` : *Channel.* (def: `None`)

(optional) `[string...]` : *Leave message.* (def: `None`)

**Overload 1:**

`[channel]` : *Channel.*

`[string...]` : *Leave message.*

**Overload 0:**

`[string...]` : *Leave message.*

**Examples:**

```
!guild cfg leave
!guild cfg leave on #general
!guild cfg leave Welcome, %user%!
!guild cfg leave off
```
</p></details>

---

## Group: guild configure linkfilter
<details><summary markdown='span'>Expand for additional information</summary><p>

*Linkfilter configuration. Group call prints current configuration, or enables/disables linkfilter if specified.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`lf, linkf, linkremove, filterlinks`

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg linkfilter
!guild cfg linkfilter on
```
</p></details>

---

### guild configure linkfilter booters
<details><summary markdown='span'>Expand for additional information</summary><p>

*Enable or disable DDoS/Booter website filtering.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ddos, boot, dos`

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg linkfilter booters
!guild cfg linkfilter booters on
```
</p></details>

---

### guild configure linkfilter disturbingsites
<details><summary markdown='span'>Expand for additional information</summary><p>

*Enable or disable shock website filtering.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`disturbing, shock, shocksites`

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg linkfilter disturbing
!guild cfg linkfilter disturbing on
```
</p></details>

---

### guild configure linkfilter invites
<details><summary markdown='span'>Expand for additional information</summary><p>

*Enable or disable Discord invite filters.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`invite, inv, i`

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg linkfilter invites
!guild cfg linkfilter invites on
```
</p></details>

---

### guild configure linkfilter iploggers
<details><summary markdown='span'>Expand for additional information</summary><p>

*Enable or disable filtering of IP logger websites.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ip, loggers, ipleech`

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg linkfilter iploggers
!guild cfg linkfilter iploggers on
```
</p></details>

---

### guild configure linkfilter shorteners
<details><summary markdown='span'>Expand for additional information</summary><p>

*Enable or disable filtering of URL shortener websites.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`urlshort, shortenurl, urlshorteners`

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg linkfilter shorteners
!guild cfg linkfilter shorteners on
```
</p></details>

---

## Group: guild configure logging
<details><summary markdown='span'>Expand for additional information</summary><p>

*Action logging configuration.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`log, modlog`

**Overload 1:**

`[boolean]` : *Enable?*

(optional) `[channel]` : *Log channel.* (def: `None`)

**Examples:**

```
!guild cfg log
!guild cfg log on #log
!guild cfg log off
```
</p></details>

---

### guild configure logging exempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disable the logs for some entities (users, channels, etc).*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ex, exc`

**Overload 2:**

`[member...]` : *Members to exempt.*

**Overload 1:**

`[role...]` : *Roles to exempt.*

**Overload 0:**

`[channel...]` : *Channels to exempt.*

**Examples:**

```
!guild cfg log exempt @Someone
!guild cfg log exempt #spam
!guild cfg log exempt Role
```
</p></details>

---

### guild configure logging unexempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove an exempted entity and allow logging for actions regarding that entity.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`unex, uex`

**Overload 2:**

`[member...]` : *Members to unexempt.*

**Overload 1:**

`[role...]` : *Roles to unexempt.*

**Overload 0:**

`[channel...]` : *Channels to unexempt.*

**Examples:**

```
!guild cfg log unexempt @Someone
!guild cfg log unexempt #spam
!guild cfg log unexempt Role
```
</p></details>

---

## Group: guild configure ratelimit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prevents users from posting more than specified amount of messages in 5s.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`rl, rate`

**Overload 3:**

`[boolean]` : *Enable?*

`[short]` : *Sensitivity (messages per 5s to trigger action).*

(optional) `[PunishmentActionType]` : *Action type.* (def: `PermanentMute`)

**Overload 2:**

`[boolean]` : *Enable?*

`[PunishmentActionType]` : *Action type.*

(optional) `[short]` : *Sensitivity (messages per 5s to trigger action).* (def: `5`)

**Overload 1:**

`[boolean]` : *Enable?*

**Examples:**

```
!guild cfg ratelimit
!guild cfg ratelimit on
!guild cfg ratelimit on mute
!guild cfg ratelimit on 5
!guild cfg ratelimit on 6 kick
```
</p></details>

---

### guild configure ratelimit action
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the action to execute when the ratelimit is hit.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setaction, a`

**Arguments:**

`[PunishmentActionType]` : *Action type.*

**Examples:**

```
!guild cfg ratelimit action mute
!guild cfg ratelimit action temporaryban
```
</p></details>

---

### guild configure ratelimit exempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Disable the ratelimit watch for some entities (users, channels, etc).*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`ex, exc`

**Overload 2:**

`[member...]` : *Members to exempt.*

**Overload 1:**

`[role...]` : *Roles to exempt.*

**Overload 0:**

`[channel...]` : *Channels to exempt.*

**Examples:**

```
!guild cfg ratelimit exempt @Someone
!guild cfg ratelimit exempt #spam
!guild cfg ratelimit exempt Role
```
</p></details>

---

### guild configure ratelimit sensitivity
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set the ratelimit sensitivity. Ratelimit will be hit if member sends more messages in 5 seconds than given sensitivity number.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`setsensitivity, setsens, sens, s`

**Arguments:**

`[short]` : *Sensitivity (messages per 5s to trigger action).*

**Examples:**

```
!guild cfg ratelimit sensitivity 9
```
</p></details>

---

### guild configure ratelimit unexempt
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove an exempted entity and allow ratelimit watch for that entity.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`unex, uex`

**Overload 2:**

`[member...]` : *Members to unexempt.*

**Overload 1:**

`[role...]` : *Roles to unexempt.*

**Overload 0:**

`[channel...]` : *Channels to unexempt.*

**Examples:**

```
!guild cfg ratelimit unexempt @Someone
!guild cfg ratelimit unexempt #spam
!guild cfg ratelimit unexempt Category
```
</p></details>

---

### guild configure setmuterole
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gets or sets mute role for this guild.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`muterole, mr, muterl, mrl`

**Arguments:**

(optional) `[role]` : *New mute role.* (def: `None`)

**Examples:**

```
!guild cfg muterole
!guild cfg muterole MuteRoleName
```
</p></details>

---

### guild configure setup
<details><summary markdown='span'>Expand for additional information</summary><p>

*Starts an interactive wizard for configuring the guild settings.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`wizard`

**Examples:**

```
!guild cfg setup
```
</p></details>

---

### guild configure suggestions
<details><summary markdown='span'>Expand for additional information</summary><p>

*Command suggestions configuration.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`suggestion, cmdsug, sugg, sug, cs, s`

**Overload 1:**

`[boolean]` : *Enable suggestions?*

**Examples:**

```
!guild cfg suggestions
!guild cfg suggestions on
```
</p></details>

---

### guild configure verbose
<details><summary markdown='span'>Expand for additional information</summary><p>

*Configuration of bot's responding options.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`fullresponse, verbosereact, verboseresponse, v, vr`

**Overload 1:**

`[boolean]` : *Enable silent response?*

**Examples:**

```
!guild cfg verbose
!guild cfg verbose on
```
</p></details>

---

### guild configure welcome
<details><summary markdown='span'>Expand for additional information</summary><p>

*Allows user welcoming configuration.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`enter, join, wlc, wm, w`

**Overload 2:**

`[boolean]` : *Enable welcoming?*

(optional) `[channel]` : *Channel.* (def: `None`)

(optional) `[string...]` : *Welcome message.* (def: `None`)

**Overload 1:**

`[channel]` : *Channel.*

(optional) `[string...]` : *Welcome message.* (def: `None`)

**Overload 0:**

`[string...]` : *Welcome message.*

**Examples:**

```
!guild cfg welcome
!guild cfg welcome on #general
!guild cfg welcome Welcome, %user%!
!guild cfg welcome off
```
</p></details>

---

### guild getbans
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get guild ban list.*

**Requires permissions:**
`View audit log`

**Aliases:**
`banlist, viewbanlist, getbanlist, bans, viewbans`

**Examples:**

```
!guild banlist
```
</p></details>

---

### guild info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print guild information.*

**Aliases:**
`i, information`

**Examples:**

```
!guild info
```
</p></details>

---

### guild log
<details><summary markdown='span'>Expand for additional information</summary><p>

*View guild audit logs. You can also specify an amount of entries to fetch.*

**Requires permissions:**
`View audit log`

**Aliases:**
`auditlog, viewlog, getlog, getlogs, logs`

**Arguments:**

(optional) `[int]` : *Amount of entries to fetch* (def: `10`)

**Examples:**

```
!guild logs
```
</p></details>

---

### guild memberlist
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the guild member list.*

**Aliases:**
`listmembers, lm, members`

**Examples:**

```
!guild memberlist
```
</p></details>

---

### guild prune
<details><summary markdown='span'>Expand for additional information</summary><p>

*Prune guild members who weren't active in the given amount of days [1-30].*

**Requires permissions:**
`Kick members`

**Requires user permissions:**
`Administrator`

**Aliases:**
`p, clean, purge`

**Arguments:**

(optional) `[int]` : *Days.* (def: `7`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!guild prune 5
```
</p></details>

---

### guild rename
<details><summary markdown='span'>Expand for additional information</summary><p>

*Rename guild.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`r, name, setname`

**Arguments:**

`[string...]` : *New name.*

**Examples:**

```
!guild rename New guild name
!guild rename "Reason for renaming" New guild name
```
</p></details>

---

### guild seticon
<details><summary markdown='span'>Expand for additional information</summary><p>

*Change icon of the guild.*

**Requires permissions:**
`Manage guild`

**Aliases:**
`icon, si`

**Arguments:**

`[URL]` : *New icon URL.*

**Examples:**

```
!guild seticon http://imgur.com/someimage.png
```
</p></details>

---

## Group: message
<details><summary markdown='span'>Expand for additional information</summary><p>

*Commands for manipulating messages.*

**Aliases:**
`m, msg, msgs, messages`

</p></details>

---

### message attachments
<details><summary markdown='span'>Expand for additional information</summary><p>

*View all message attachments. If the message is not provided, scans the last sent message before command invocation.*

**Aliases:**
`a, files, la`

**Arguments:**

(optional) `[message]` : *Message.* (def: `None`)

**Examples:**

```
!message attachments
!message attachments 408226948855234561
```
</p></details>

---

## Group: message delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes messages from the current channel. Group call deletes given amount of most recent messages.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`-, prune, del, d`

**Arguments:**

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!messages delete 10
!messages delete 10 Cleaning spam
```
</p></details>

---

### message delete after
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes given amount messages after a specified message ID.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`aft, af`

**Arguments:**

`[message]` : *Message after which to delete.*

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!messages delete before 123456789132 20 Cleaning spam
```
</p></details>

---

### message delete before
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes given amount messages before a specified message ID.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`bef, bf`

**Arguments:**

`[message]` : *Message before which to delete.*

(optional) `[int]` : *Amount.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!messages delete before 123456789132 20 Cleaning spam
```
</p></details>

---

### message delete from
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes given amount of most recent messages from the given member.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`f, frm`

**Overload 1:**

`[member]` : *User whose messages to delete.*

(optional) `[int]` : *Message range.* (def: `5`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[int]` : *Amount.*

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!messages delete from @Someone 10 Cleaning spam
!messages delete from 10 @Someone Cleaning spam
```
</p></details>

---

### message delete reactions
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes all reactions from the given message.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`react, re`

**Arguments:**

(optional) `[message]` : *Message.* (def: `None`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!messages delete reactions 408226948855234561
```
</p></details>

---

### message delete regex
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deletes given amount of most-recent messages that match a given regular expression withing a given message amount.*

**Requires permissions:**
`Manage messages`

**Requires user permissions:**
`Administrator`

**Aliases:**
`r, rgx, regexp, reg`

**Overload 1:**

`[string]` : *Pattern (Regex).*

(optional) `[int]` : *Amount.* (def: `100`)

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[int]` : *Amount.*

`[string]` : *Pattern (Regex).*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!messages delete regex s+p+a+m+ 10 Cleaning spam
!messages delete regex 10 s+p+a+m+ Cleaning spam
```
</p></details>

---

### message flag
<details><summary markdown='span'>Expand for additional information</summary><p>

*Flags the message given by ID for deletion vote. If the message is not provided, flags the last sent message before command invocation.*

**Requires bot permissions:**
`Manage messages`

**Aliases:**
`f`

**Arguments:**

(optional) `[message]` : *Message.* (def: `None`)

(optional) `[time span]` : *Voting timespan.* (def: `None`)

**Examples:**

```
!message flag
!message flag 408226948855234561
```
</p></details>

---

### message listpinned
<details><summary markdown='span'>Expand for additional information</summary><p>

*List pinned messages in this channel.*

**Aliases:**
`lp, listpins, listpin, pinned`

**Examples:**

```
!messages listpinned
```
</p></details>

---

### message modify
<details><summary markdown='span'>Expand for additional information</summary><p>

*Modify the given message.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`edit, mod, e, m`

**Arguments:**

`[message]` : *Message.*

`[string...]` : *New content.*

**Examples:**

```
!messages modify 408226948855234561 modified text
```
</p></details>

---

### message pin
<details><summary markdown='span'>Expand for additional information</summary><p>

*Pins the message given by ID. If the message is not provided, pins the last sent message before command invocation.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`p`

**Arguments:**

(optional) `[message]` : *Message.* (def: `None`)

**Examples:**

```
!messages pin
!messages pin 408226948855234561
```
</p></details>

---

### message unpin
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unpins the message at given index (starting from 1) or message ID. If the index is not given, unpins the most recent one.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`up`

**Overload 1:**

`[message]` : *Message.*

**Overload 0:**

(optional) `[int]` : *Index (starting from 1).* (def: `1`)

**Examples:**

```
!messages unpin 12345645687955
!messages unpin 10
```
</p></details>

---

### message unpinall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unpins all pinned messages in this channel.*

**Requires permissions:**
`Manage messages`

**Aliases:**
`upa`

**Examples:**

```
!messages unpinall
```
</p></details>

---

## Group: roles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Miscellaneous role control commands. Group call lists all the roles in this guild or prints information about a given role.*

**Aliases:**
`role, rl`

**Overload 0:**

`[role]` : *Role.*

</p></details>

---

### roles create
<details><summary markdown='span'>Expand for additional information</summary><p>

*Create a new role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`new, add, a, c, +, +=, <, <<`

**Overload 2:**

`[string]` : *Name.*

(optional) `[color]` : *Color.* (def: `None`)

(optional) `[boolean]` : *Hoisted (visible in online list)?* (def: `False`)

(optional) `[boolean]` : *Mentionable?* (def: `False`)

**Overload 1:**

`[color]` : *Color.*

`[string...]` : *Name.*

**Overload 0:**

`[string...]` : *Name.*

**Examples:**

```
!roles create Role
!roles create "My role" #C77B0F no no
!roles create #C77B0F My new role
```
</p></details>

---

### roles delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Create a new role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`del, remove, rm, d, -, >, >>`

**Arguments:**

`[role]` : *Role.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!role delete My role
!role delete @admins
```
</p></details>

---

### roles info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get information about a given role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`i`

**Arguments:**

`[role]` : *Role.*

**Examples:**

```
!role info Admins
```
</p></details>

---

### roles mention
<details><summary markdown='span'>Expand for additional information</summary><p>

*Mention the given role. This will bypass the mentionable status for the given role.*

**Requires user permissions:**
`Administrator`

**Requires bot permissions:**
`Manage roles`

**Aliases:**
`mentionall, @, ma`

**Arguments:**

`[role]` : *Role.*

**Examples:**

```
!role mentionall Admins
```
</p></details>

---

### roles setcolor
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set a color for the role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`clr, c, sc, setc`

**Overload 1:**

`[role]` : *Role.*

`[color]` : *Color.*

**Overload 0:**

`[color]` : *Color.*

`[role]` : *Role.*

**Examples:**

```
!role setcolor #FF0000 Admins
!role setcolor Admins #FF0000
```
</p></details>

---

### roles setmentionable
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set role mentionable var.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`mentionable, m, setm`

**Overload 1:**

`[role]` : *Role.*

(optional) `[boolean]` : *Mentionable?* (def: `True`)

**Overload 0:**

`[boolean]` : *Mentionable?*

`[role]` : *Role.*

**Examples:**

```
!role setmentionable Admins
!role setmentionable Admins false
!role setmentionable false Admins
```
</p></details>

---

### roles setname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set a name for the role.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`name, rename, n`

**Overload 1:**

`[role]` : *Role.*

`[string...]` : *New name.*

**Overload 0:**

`[string]` : *New name.*

`[role]` : *Role.*

**Examples:**

```
!role setname @Admins Administrators
!role setname Administrators @Admins
```
</p></details>

---

### roles setvisible
<details><summary markdown='span'>Expand for additional information</summary><p>

*Set role hoisted var (visibility in online list).*

**Requires permissions:**
`Manage roles`

**Aliases:**
`separate, h, seth, hoist, sethoist`

**Overload 1:**

`[role]` : *Role.*

(optional) `[boolean]` : *Hoisted (visible in online list)?* (def: `False`)

**Overload 0:**

`[boolean]` : *Hoisted (visible in online list)?*

`[role]` : *Role.*

**Examples:**

```
!role setvisible Admins
!role setvisible Admins false
!role setvisible false Admins
```
</p></details>

---

## Group: selfassignableroles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Self-assignable roles management. A member can grant himself a self-assignable roleusing ``giveme`` command. Group call lists all self-assignable roles for the guild. Group call with an arbitrary amount of roles will add those roles to the self-assignable roles list for this guild, effective immediately.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`sar, selfroles, selfrole`

**Overload 0:**

`[role...]` : *Roles to add.*

**Examples:**

```
!sar
!sar @Announcements
```
</p></details>

---

### selfassignableroles add
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a self-assignable role(s).*

**Requires user permissions:**
`Administrator`

**Aliases:**
`a, +, +=, <<, <`

**Arguments:**

`[role...]` : *Roles to add.*

**Examples:**

```
!sar add @Notifications
!sar add @Notifications @Role1 @Role2
```
</p></details>

---

### selfassignableroles delete
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove self-assignable role(s).*

**Requires user permissions:**
`Administrator`

**Aliases:**
`remove, rm, del, d, -, -=, >, >>`

**Arguments:**

`[role...]` : *Roles to remove.*

**Examples:**

```
!sar delete @Notifications
!sar delete @Notifications @Role1 @Role2
```
</p></details>

---

### selfassignableroles deleteall
<details><summary markdown='span'>Expand for additional information</summary><p>

*Delete all self-assignable roles for the current guild.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`removeall, rmrf, rma, clearall, clear, delall, da`

**Examples:**

```
!sar clear
```
</p></details>

---

### selfassignableroles list
<details><summary markdown='span'>Expand for additional information</summary><p>

*List all current self-assignable roles.*

**Requires user permissions:**
`Administrator`

**Aliases:**
`print, show, ls, l, p`

**Examples:**

```
!sar list
```
</p></details>

---

## Group: user
<details><summary markdown='span'>Expand for additional information</summary><p>

*Miscellaneous user control commands. Group call prints information about given user.*

**Aliases:**
`users, u, usr`

**Overload 1:**

(optional) `[member]` : *Guild member.* (def: `None`)

**Overload 0:**

(optional) `[user]` : *User.* (def: `None`)

</p></details>

---

### user addrole
<details><summary markdown='span'>Expand for additional information</summary><p>

*Assign a role to a member.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`+role, +r, ar, addr, +roles, addroles, giverole, giveroles, grantrole, grantroles, gr`

**Overload 1:**

`[member]` : *Member.*

`[role...]` : *Roles to grant.*

**Overload 0:**

`[role]` : *Role.*

`[member]` : *Member.*

**Examples:**

```
!user addrole @User Admins
!user addrole Admins @User
```
</p></details>

---

### user avatar
<details><summary markdown='span'>Expand for additional information</summary><p>

*View user's avatar in full size.*

**Aliases:**
`a, pic, profilepic`

**Arguments:**

`[user]` : *User whose avatar to show.*

**Examples:**

```
!user avatar @Someone
```
</p></details>

---

### user ban
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bans the user from the guild.*

**Requires permissions:**
`Ban members`

**Aliases:**
`b`

**Arguments:**

`[member]` : *Member to ban.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user ban @Someone
!user ban @Someone Troublemaker
```
</p></details>

---

### user banid
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bans the ID from the guild.*

**Requires permissions:**
`Ban members`

**Aliases:**
`bid`

**Arguments:**

`[unsigned long]` : *ID.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user banid 154956794490845232
!user banid 154558794490846232 Troublemaker
```
</p></details>

---

### user deafen
<details><summary markdown='span'>Expand for additional information</summary><p>

*Deafen or undeafen a member.*

**Requires permissions:**
`Deafen voice chat members`

**Aliases:**
`deaf, d, df`

**Arguments:**

`[boolean]` : *Deafen?*

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user deafen on @Someone
!user deafen off @Someone
```
</p></details>

---

### user info
<details><summary markdown='span'>Expand for additional information</summary><p>

*Print the information about the given user.*

**Aliases:**
`i, information`

**Overload 1:**

(optional) `[member]` : *Guild member.* (def: `None`)

**Overload 0:**

(optional) `[user]` : *User.* (def: `None`)

**Examples:**

```
!user info @Someone
```
</p></details>

---

### user kick
<details><summary markdown='span'>Expand for additional information</summary><p>

*Kicks the member from the guild.*

**Requires permissions:**
`Kick members`

**Aliases:**
`k`

**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user kick @Someone
!user kick @Someone Troublemaker
```
</p></details>

---

### user mute
<details><summary markdown='span'>Expand for additional information</summary><p>

*Mute or unmute a member.*

**Requires permissions:**
`Mute voice chat members`

**Aliases:**
`m`

**Overload 1:**

`[boolean]` : *Mute?*

`[member]` : *Member to mute.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[member]` : *Member to mute.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user mute off @Someone
!user mute on @Someone Trashtalk
```
</p></details>

---

### user mutevoice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Mute or unmute a member in the voice channels.*

**Requires permissions:**
`Mute voice chat members`

**Aliases:**
`mv, voicemute, vmute, mutev, vm`

**Overload 0:**

`[boolean]` : *Mute?*

`[member]` : *Member to mute.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[member]` : *Member to mute.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user mutevoice off @Someone
!user mutevoice on @Someone Trashtalk
```
</p></details>

---

### user removeallroles
<details><summary markdown='span'>Expand for additional information</summary><p>

*Revoke all roles from user.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`remallroles, -ra, -rall, -allr`

**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user removeallroles @Someone
```
</p></details>

---

### user removerole
<details><summary markdown='span'>Expand for additional information</summary><p>

*Revoke a role from member.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`remrole, rmrole, rr, -role, -r, removeroles, revokerole, revokeroles`

**Overload 2:**

`[member]` : *Member.*

`[role...]` : *Roles to revoke.*

**Overload 1:**

`[member]` : *Member.*

`[role]` : *Role.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[role]` : *Role.*

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user removerole @Someone Admins
!user removerole Admins @Someone
```
</p></details>

---

### user setname
<details><summary markdown='span'>Expand for additional information</summary><p>

*Gives someone a new nickname in the current guild.*

**Requires permissions:**
`Manage nicknames`

**Aliases:**
`nick, newname, name, rename`

**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *Nickname.* (def: `None`)

**Examples:**

```
!user setname @Someone Newname
```
</p></details>

---

### user softban
<details><summary markdown='span'>Expand for additional information</summary><p>

*Bans the member from the guild and then unbans him immediately.*

**Requires permissions:**
`Ban members`

**Aliases:**
`sb, sban`

**Arguments:**

`[member]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user sban @Someone
!user sban @Someone Troublemaker
```
</p></details>

---

### user tempban
<details><summary markdown='span'>Expand for additional information</summary><p>

*Temporarily bans the user from the server and then unbans him after given timespan.*

**Requires permissions:**
`Ban members`

**Aliases:**
`tb, tban, tmpban, tmpb`

**Overload 3:**

`[time span]` : *Time span.*

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 2:**

`[member]` : *User.*

`[time span]` : *Time span.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 1:**

`[user]` : *User (doesn't have to be a member).*

`[time span]` : *Time span.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[time span]` : *Time span.*

`[user]` : *User (doesn't have to be a member).*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user tempban @Someone 3h4m
!user tempban 5d @Someone Troublemaker
!user tempban @Someone 5h30m30s Troublemaker
```
</p></details>

---

### user tempmute
<details><summary markdown='span'>Expand for additional information</summary><p>

*Temporarily mutes the user and unmutes him after the given timespan.*

**Requires permissions:**
`Manage roles`

**Aliases:**
`tm, tmute, tmpmute, tmpm`

**Overload 1:**

`[time span]` : *Time span.*

`[member]` : *Member.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[member]` : *User.*

`[time span]` : *Time span.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user tempmute @Someone 3h4m
!user tempmute 5d @Someone Spammer
!user tempmute @Someone 5h30m30s Spammer
```
</p></details>

---

### user unban
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unbans the user from the server.*

**Requires permissions:**
`Ban members`

**Aliases:**
`ub`

**Overload 1:**

`[user]` : *User.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Overload 0:**

`[unsigned long]` : *ID.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user unban 154956794490845232
```
</p></details>

---

### user unmute
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unmute a member.*

**Requires permissions:**
`Mute voice chat members`

**Aliases:**
`um`

**Arguments:**

`[member]` : *Member to unmute.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user unmute @Someone
```
</p></details>

---

### user unmutevoice
<details><summary markdown='span'>Expand for additional information</summary><p>

*Unmute a member in the voice channels.*

**Requires permissions:**
`Mute voice chat members`

**Aliases:**
`umv, voiceunmute, vunmute, unmutev, vum`

**Arguments:**

`[member]` : *Member to unmute.*

(optional) `[string...]` : *Reason.* (def: `None`)

**Examples:**

```
!user unmutevoice @Someone
```
</p></details>

---

### user warn
<details><summary markdown='span'>Expand for additional information</summary><p>

*Warn a member in private message by sending a given warning text.*

**Requires permissions:**
`Kick members`

**Aliases:**
`w`

**Arguments:**

`[member]` : *Member.*

(optional) `[string...]` : *Warning message.* (def: `None`)

**Examples:**

```
!user warn @Someone Stop spamming or kick!
```
</p></details>

---

