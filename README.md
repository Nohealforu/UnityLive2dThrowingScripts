# UnityLive2dThrowingScripts

Example project with some custom scripts for setting up a throwing system in Unity.

Custom "dumb" physics so that it's fairly efficient.

Can use 2d or 3d objects as projectiles with the projectile script and assign to projectile spawner empties.

Has a bunch of settings with tooltips.

Has jank sound, be careful with duration of sound effects/how many different sound sources used (if having a large mix of projectiles with different sounds).

Does not include Live2d sdk asset because no way I'm uploading all that.

Idk how to embed images properly in this.

[Throwing Stuff](Throwing%20Stuff.jpg)

Make sure when you use the script to set the script execution order properly or else parameter updates might be overridden (the hits are additive so should be last).

[Script Order](Script%20Execution%20Order.jpg)

Use this for whatever you want, this is mostly made so Vedal987 can use it if he wants.  I stress tested it and it shouldn't leak memory.

Includes a default asset model from VTube studio, rights respective to that creator.
