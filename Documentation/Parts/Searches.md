# Module: Searches

## Group: gif
<details><summary markdown='span'>Expand for additional information</summary><p>

*GIPHY commands. If invoked without a subcommand, searches GIPHY with given query.*

**Aliases:**
`giphy`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```xml
!gif wat
```
</p></details>

---

### gif random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return a random GIF.*

**Aliases:**
`r, rand, rnd`

</p></details>

---

### gif trending
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return an amount of trending GIFs.*

**Aliases:**
`t, tr, trend`

**Arguments:**

(optional) `[int]` : *Number of results (1-10).* (def: `5`)

**Examples:**

```xml
!gif trending 
!gif trending 3
```
</p></details>

---

## Group: goodreads
<details><summary markdown='span'>Expand for additional information</summary><p>

*Goodreads commands. Group call searches Goodreads books with given query.*

**Aliases:**
`gr`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```xml
!goodreads Ender's Game
```
</p></details>

---

### goodreads book
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search Goodreads books by title, author, or ISBN.*

**Aliases:**
`books, b`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```xml
!goodreads book Ender's Game
```
</p></details>

---

## Group: imdb
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search Open Movie Database. Group call searches by title.*

**Aliases:**
`movies, series, serie, movie, film, cinema, omdb`

**Arguments:**

`[string...]` : *Title.*

**Examples:**

```xml
!imdb Airplane
```
</p></details>

---

### imdb id
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search by IMDb ID.*

**Arguments:**

`[string]` : *ID.*

**Examples:**

```xml
!imdb id tt4158110
```
</p></details>

---

### imdb search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Searches IMDb for given query and returns paginated results.*

**Aliases:**
`s, find`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```xml
!imdb search Sharknado
```
</p></details>

---

### imdb title
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search by title.*

**Aliases:**
`t, name, n`

**Arguments:**

`[string...]` : *Title.*

**Examples:**

```xml
!imdb title Airplane
```
</p></details>

---

## Group: imgur
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search imgur. Group call retrieves top ranked images from given subreddit.*

**Aliases:**
`img, im, i`

**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 0:**

`[string]` : *Subreddit.*

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

**Examples:**

```xml
!imgur aww
!imgur 10 aww
!imgur aww 10
```
</p></details>

---

### imgur latest
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return latest images from given subreddit.*

**Aliases:**
`l, new, newest`

**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 0:**

`[string]` : *Subreddit.*

`[int]` : *Number of images to print [1-10].*

**Examples:**

```xml
!imgur latest aww
!imgur latest 10 aww
!imgur latest aww 10
```
</p></details>

---

### imgur top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Return amount of top rated images in the given subreddit for given timespan.*

**Aliases:**
`t`

**Overload 3:**

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Overload 2:**

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[string]` : *Subreddit.*

(optional) `[int]` : *Number of images to print [1-10].* (def: `1`)

**Overload 1:**

`[int]` : *Number of images to print [1-10].*

`[TimeWindow]` : *Timespan in which to search (day/week/month/year/all).*

`[string...]` : *Subreddit.*

**Overload 0:**

`[int]` : *Number of images to print [1-10].*

`[string...]` : *Subreddit.*

**Examples:**

```xml
!imgur top day 10 aww
!imgur top 10 day aww
!imgur top 5 aww
!imgur top day aww
```
</p></details>

---

## Group: joke
<details><summary markdown='span'>Expand for additional information</summary><p>

*Group for searching jokes. Group call returns a random joke.*

**Aliases:**
`jokes, j`

</p></details>

---

### joke search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search for the joke containing the given query.*

**Aliases:**
`s`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```xml
!joke search blonde
```
</p></details>

---

### joke yourmom
<details><summary markdown='span'>Expand for additional information</summary><p>

*Yo mama so...*

**Aliases:**
`mama, m, yomomma, yomom, yomoma, yomamma, yomama`

</p></details>

---

## Group: reddit
<details><summary markdown='span'>Expand for additional information</summary><p>

*Reddit commands. Group call prints hottest posts from given sub.*

**Aliases:**
`r`

**Arguments:**

(optional) `[string]` : *Subreddit.* (def: `all`)

**Examples:**

```xml
!reddit 
!reddit aww
```
</p></details>

---

### reddit controversial
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest controversial posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```xml
!reddit controversial aww
```
</p></details>

---

### reddit gilded
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest gilded posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```xml
!reddit gilded aww
```
</p></details>

---

### reddit hot
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest hot posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```xml
!reddit hot aww
```
</p></details>

---

### reddit new
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest posts for a subreddit.*

**Aliases:**
`newest, latest`

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```xml
!reddit new aww
```
</p></details>

---

### reddit rising
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get newest rising posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```xml
!reddit rising aww
```
</p></details>

---

### reddit subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add new feed for a subreddit.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```xml
!reddit subscribe aww
```
</p></details>

---

### reddit top
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get top posts for a subreddit.*

**Arguments:**

`[string]` : *Subreddit.*

**Examples:**

```xml
!reddit top aww
```
</p></details>

---

### reddit unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a subreddit feed using subreddit name or subscription ID (use command ``feed list`` to see IDs).*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`

**Overload 1:**

`[string]` : *Subreddit.*

**Overload 0:**

`[int]` : *Subscription ID.*

**Examples:**

```xml
!reddit unsubscribe aww
!reddit unsubscribe 12
```
</p></details>

---

## Group: steam
<details><summary markdown='span'>Expand for additional information</summary><p>

*Steam commands. Group call searches steam profiles for a given ID.*

**Aliases:**
`s, st`

**Examples:**

```xml
!steam 123456123
```
</p></details>

---

### steam profile
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get Steam user information for user based on his ID.*

**Aliases:**
`id, user`

**Arguments:**

`[unsigned long]` : *ID.*

**Examples:**

```xml
!steam profile 123456123
```
</p></details>

---

## Group: urbandict
<details><summary markdown='span'>Expand for additional information</summary><p>

*Urban Dictionary commands. Group call searches Urban Dictionary for a given query.*

**Aliases:**
`ud, urban`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```xml
!urbandict blonde
```
</p></details>

---

## Group: weather
<details><summary markdown='span'>Expand for additional information</summary><p>

*Weather search commands. Group call returns weather information for given query.*

**Aliases:**
`w`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```xml
!weather london
```
</p></details>

---

### weather forecast
<details><summary markdown='span'>Expand for additional information</summary><p>

*Get weather forecast for the following days (def: 7).*

**Aliases:**
`f`

**Overload 1:**

`[int]` : *Amount of days to fetch the forecast for.*

`[string...]` : *Query.*

**Overload 0:**

`[string...]` : *Query.*

**Examples:**

```xml
!weather forecast london
!weather forecast 5 london
```
</p></details>

---

## Group: wikipedia
<details><summary markdown='span'>Expand for additional information</summary><p>

*Wikipedia search. If invoked without a subcommand, searches Wikipedia with given query.*

**Aliases:**
`wiki`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```xml
!wikipedia Linux
```
</p></details>

---

### wikipedia search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search Wikipedia for a given query.*

**Aliases:**
`s, find`

**Arguments:**

`[string...]` : *Query.*

**Examples:**

```xml
!wikipedia search Linux
```
</p></details>

---

## Group: xkcd
<details><summary markdown='span'>Expand for additional information</summary><p>

*Search xkcd. Group call returns random comic or, if an ID is provided, a comic with given ID.*

**Aliases:**
`x`

**Overload 1:**

`[int]` : *Comic ID.*

**Examples:**

```xml
!xkcd 
!xkcd 650
```
</p></details>

---

### xkcd id
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves comic with given ID from xkcd.*

**Arguments:**

(optional) `[int]` : *Comic ID.* (def: `None`)

**Examples:**

```xml
!xkcd id 
!xkcd id 650
```
</p></details>

---

### xkcd latest
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves latest comic from xkcd.*

**Aliases:**
`fresh, newest, l`

</p></details>

---

### xkcd random
<details><summary markdown='span'>Expand for additional information</summary><p>

*Retrieves a random comic.*

**Aliases:**
`rnd, r, rand`

</p></details>

---

## Group: youtube
<details><summary markdown='span'>Expand for additional information</summary><p>

*Youtube search commands. Group call searches YouTube for given query.*

**Aliases:**
`y, yt, ytube`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```xml
!youtube never gonna give you up
```
</p></details>

---

### youtube search
<details><summary markdown='span'>Expand for additional information</summary><p>

*Advanced youtube search.*

**Aliases:**
`s`

**Arguments:**

`[int]` : *Amount of results. [1-20]*

`[string...]` : *Search query.*

**Examples:**

```xml
!youtube search 5 rick astley
```
</p></details>

---

### youtube searchchannel
<details><summary markdown='span'>Expand for additional information</summary><p>

*Advanced youtube search for channels only.*

**Aliases:**
`sc, searchc, channel`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```xml
!youtube searchchannel rick astley
```
</p></details>

---

### youtube searchp
<details><summary markdown='span'>Expand for additional information</summary><p>

*Advanced youtube search for playlists only.*

**Aliases:**
`sp, searchplaylist, playlist`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```xml
!youtube searchp rick astley
```
</p></details>

---

### youtube searchvideo
<details><summary markdown='span'>Expand for additional information</summary><p>

*Advanced youtube search for videos only.*

**Aliases:**
`sv, searchv, video`

**Arguments:**

`[string...]` : *Search query.*

**Examples:**

```xml
!youtube searchvideo rick astley
```
</p></details>

---

### youtube subscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Add a new subscription for a YouTube channel.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`add, a, +, sub`

**Arguments:**

`[string]` : *Channel URL.*

(optional) `[string]` : *Friendly name.* (def: `None`)

**Examples:**

```xml
!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO
!youtube subscribe https://www.youtube.com/user/RickAstleyVEVO rick
```
</p></details>

---

### youtube unsubscribe
<details><summary markdown='span'>Expand for additional information</summary><p>

*Remove a YouTube channel subscription.*

**Requires user permissions:**
`Manage guild`

**Aliases:**
`del, d, rm, -, unsub`

**Arguments:**

`[string]` : *Channel URL or subscription name.*

**Examples:**

```xml
!youtube unsubscribe https://www.youtube.com/user/RickAstleyVEVO
!youtube unsubscribe rick
```
</p></details>

---

