﻿# the-godfather

Just another general-purpose Discord bot. 
Written in C# using [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).

---

TheGodfather only listens for commands inside the guilds, and nowhere else.
The commands are invoked by sending a message starting with a "prefix" or by mentioning the bot at the start of the message.
The default prefix for the bot is ``!``, however you can change it using ``prefix`` command (affects just the guild in which it is invoked). 
Also you can trigger commands by mentioning the bot. 

For example, valid command calls are: 
```!ping```
```@TheGodfather ping```


## :page_facing_up: Command list

Command list is available at the [Documentation](Documentation/README.md) directory.
It is advised to read the explanation below in order to use TheGodfather in his full potential.


## :page_facing_up: Command groups:

Commands are divided into command groups due to large number of commands. 
For example, command group ``user`` contains commands which are used for administrative commands on Discord users. Some subcommands of thise group are ``kick`` , ``ban`` etc. 
In order to call the ``kick`` command for example, one should always provide the command group name and then the actual command name, in this case: ``!user kick @Someone``.


## :page_facing_up: Command arguments:

Some commands require additional information, from now on called **command arguments**.
For example, the ``kick`` command requires a user to be passed to it, so the bot can know who to kick from the guild.

Commands that require arguments also specify the type of the argument. 
For example, you need to pass a user to ``kick`` command and not some random text.

Argument types can be one of the following: 
* ``int`` : Integer (a single whole number). Valid examples: ``25`` , ``-64``.
* ``double`` : Floating point number, can also be an integer. Valid examples: ``5.64`` , ``-3.2`` , ``5``.
* ``string`` : A string of of Unicode characters WITHOUT spaces. If you want to include spaces, then surround the string with quotes. Valid examples: ``testtest``, ``T3S7``, ``"I need quotes for spaces!"``
* ``string...`` : Unicode text, can include spaces. Valid examples: ``This is a text so I do not need quotes``.
* ``boolean`` : ``true`` or ``false`` (can be converted from ``yes`` or ``no`` in various forms, see: [CustomBoolConverter](TheGodfather/Extensions/Converters/CustomBoolConverter.cs)). Valid examples: ``true`` , ``yes`` , ``no``.
* ``user`` : Discord user, given by mention, username or UID (User ID). Valid examples: ``@Someone`` , ``Someone`` , ``123456789123456``.
* ``channel`` : Discord channel, given by mention, channel name or CID (Channel ID). Valid examples: ``#channel`` , ``MyChannel`` , ``123456789123456``.
* ``role`` : An existing role, given by mention, role name or RID (Role ID). Valid examples: ``@Admins`` , ``Admins`` , ``123456789123456``.
* ``emoji`` : Emoji, either in Unicode or Discord representation (using ``:``). Valid examples: ``😂`` , ``:joy:``.
* ``id`` : ID of a Discord entity (could be a message, user, channel, role etc.). Can only be seen by enabling the ``Developer appearance`` option Discord appearance settings.
* ``color`` : A hexadecimal or RGB color representation. Valid examples: ``FF0000`` , ``(255, 0, 0)``.
* ``time span`` : A time span in form **DDd HHh MMm SSs** Valid examples: ``3d 5m 30s`` etc. 

Arguments can be marked as ``(optional)`` in the documentation. When this is the case, you can omit that argument.

For example, the aforementioned ``kick`` command also accepts a ``string`` corresponding to a reason for the kick. However, since it is marked as optional, both of the following invocations will succeed:
```!user kick @Someone```
```!user kick @Someone I have kicked him because I can!```


## :page_facing_up: Command aliases

Aliases are the synonyms for a command.
Aliases are usually shorter than regular names and are meant for faster invocation of the commands. Some people like it short and some people like it descriptive.

For example, the ``user`` command group has an alias ``u``. This means that if you wish to call a subcommand from that group, for example ``kick``, you can also call it using an alias for the group: ``u kick``.


## :page_facing_up: Command overloads:

Each command can be invoked in various ways, each of those ways being called an **overload** in the documentation. 

For example, let's consider the ``bank transfer`` command. The logic of this command is to transfer currency from your account to another user's account. 
One way to use it is to provide a ``user`` to pass the currency to and an ``int`` which corresponds to the amount of credits to transfer. 
The ordering of these arguments can sometimes be hard to remember. This is where overloads come in. The purpose of an overload is to give alternate ways of invoking the same command.
In this example, another way to use the ``bank transfer`` command is to pass the amount first and the user second.
This way, the ordering of the arguments does not matter.

:exclamation: **Note:** ``string...`` argument always comes last because it captures raw text until the end of the message.

:exclamation: **Note:** It is always preferred to surround arguments of type ``string`` with quotes. 
This eliminates the misinterpretation in case two strings are required as arguments (if quotes are not used, the space will be seen as a separator and the passed text will be interpreted as multiple strings, which is not usually a behaviour that the user expects).
