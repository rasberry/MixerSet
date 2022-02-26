# MixerSet
Adjust audio mixer levels from the command line

## Usage ##
```
Usage:
MixerSet (command) [options]
 Commands:
  (l)ist                   Lists apps in volume mixer
  (r)eset [n]              Resets all volumes to 0 or n
  (v)olume (appname) (n)   Sets appname volume to n
  (m)ute (appname)         Mutes or un-mutes appname
```

## TODO ##
* Consider using [CSCore](https://github.com/filoe/cscore)
* Figure out why mute/volume doesn't work for foobar2000
* Display which device is currently active (speakers, headphone, etc..)
