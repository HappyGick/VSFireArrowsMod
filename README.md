# Fire Arrows
This mod I made specifically for me and my girlfriend's server. She loves to hunt, fight monsters, and use spears and arrows (fell in love with the bow as soon as she got one), and said that it would be nice if the game had stuff to light caves from a distance (besides throwing torches to the ground). That reminded me of how Terraria had light-emitting arrows that could be used to light caves, so I set out to make a simple mod for this: Fire Arrows!

The mod simply adds Fire Arrows that you can craft with grid crafting and require little resources besides a lit torch and basic arrowmaking ingredients. The recipe you can find in the Handbook.

In real life, fire arrows were used in past wars to light structures on fire and to lower enemy morale. They weren't used for hitting enemies due to excessive weight (which makes them slow), and the fact that they can't really penetrate armor due to lack of sharpness. So the arrows I added are about as useful as crude arrows for combat, except they break a lot less, but they don't inflict fire ticking or anything. They're meant to be used for setting temporary (they last aprox. 60s) light sources from a distance.

I'll maybe perfect this mod in my free time (for example with better visuals), in the meantime, any features I add are specifically for my girlfriend and I.

## In this repo
This is a monorepo containing The [Fire Arrows](https://mods.vintagestory.at/show/mod/44358) and [Fire Arrows Combat Overhaul Compat](https://mods.vintagestory.at/show/mod/46062) mods. The reason Combat Overhaul compatibility was split up was because Vintage Story queries all types when sandboxing the mods, so you can't make use of the .NET Runtime's lazy loading for building a single mod 

## TODOs:
- [x] ~~Make the arrows turn off when they touch water~~ *Added in v0.0.2*
- [x] ~~Have an in-world method of lighting them (currently you just consume a torch to make 1, 4, 8, or 16 arrows). My plan is to have the player craft unlit arrows and then hold a torch in their left hand to light them on fire~~ *Added in v0.0.2*
- [ ] ~~Make the arrows behave similar to torches when held~~ and/or when loading up the bow *Partially added in v0.0.2*
- [x] ~~Make a better model for the arrows (currently reusing the game's copper arrow model, which is shared across all arrows in the game). I want to make it so it looks like the flammable material is wrapped around the tip~~ *Added in v0.0.2*
- [ ] I think the arrows are more affected by gravity than normal arrows, but I'm not sure if it's balanced or if the gravity modifier actually works. As far as I could read from the game's code, it works
- [ ] Split this into tiers and add modifiers to the arrows. Using resin, for example, could make them last longer. Currently it's just dry grass and they last 60 seconds, but I want to make this more nuanced
- [ ] Light certain entities on fire if you hit them with these lol
- [ ] Secret easter egg that my girlfriend requested
- [x] ~~Add some images to the mod page lmao~~ *Done*
