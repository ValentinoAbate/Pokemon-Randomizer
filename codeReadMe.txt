===========================================
  Pokemon Emerald Randomizer Source Code
===========================================
v.2.2 -- 13 April 2014
Original code by Artemis251


Uhh...not quite sure what all to put in this. Hopefully the code is
easy enough to follow, and the comments, though likely somewhat sparse,
help you understand what's goin' on.

I realize there's not much elegance here, but that's because I kinda
built this whole thing modularly and didn't have a decent framework
planned before I started programming. Sorry 'bout that!

In any case, feel free to tweak the code to your heart's content. I do
have a few ideas of how to take the current program further, but I lack
the free time to really implement them. Feel free to do so on your own!
I'm not sure what the whole protocol is for open sourcin' code in terms
of gettin' a central repository (do people use some kinda globally-
accessible git/svn or somethin'?), but you're free to redistribute your
own changes and whatnot. It's all for the greater good of players!


-- Contents --
  0. What's included
  1. Language/Vocabulary in the code
  2. Weird things you should be aware of
  3. Known Bugs
  4. Possible Improvement Ideas


----------------------------------------
  0. What's included
----------------------------------------

So, what all are you gettin' here? Here's what should be given once
you've unzipped this bad boy:

 - code directory: This has all the files from my NetBeans project
        folder. It should load up alright in that IDE (I hope!).
        I'm not sure about the compatability with other IDEs like
        Eclipse, but there should be some feasible import capability
        that allows it.

 - misc Docs directory: This has a bunch of helpful information for
        coding. There are lists of pokemon, items, abilities, etc. in
        numerical and/or hex order. You'll find some developer notes
        as well riddled with addresses, parsing, and other tidbits
        that might help you get a lead on what you want to change.

 - derpbird.png: As its name implies.


----------------------------------------
  1. Language/Vocabulary in the code
----------------------------------------

Here're a few terms I've used throughout the code. They might make it
easier for you to understand what's goin' on!

 - Family: A pokemon family is its evolutionary line. This means that
           Bulbasaur, Ivysaur, and Venusaur make up a family, and that
           Eevee, Vaporeon, Umbreon, etc. are a family.

 - Sibling Color Data: Sometimes two palette color progressions use the
           same color for shading. Think about two different greens using
           the same dark green as their dark component. Sibling color
           data is recognized to allow this trait to exist after color
           randomization.

 - addy: I dunno why, but this is my abbreviation for 'address'.

 - Palette file syntax: The palette text file is coded with each color
           separated by / characters. Each color has lighter and darker
           hues, thus each section of color has comma-separated index
           numbers. As the palette colors may not have been in order,
           you should not expect the order to be necessarily numerical.
             Each pokemon has a palette, and they are in order (skipping
           the 25 empty slots). Moreover, the first listed color set is
           what I've deemed the 'main' color of the pokemon for those
           who want to only change color slightly. For example, Zubat's
           main color is its blue body, Tyranitar's is its green, and 
           Kyogre's is its blue.
             The sibling color data is coded as two sets of colors
           separated by a semicolon with a single number at the end
           that appears after a hyphen. That hyphened color is the shared
           color in both color progressions. For example, the entry
           '7,8,9;8,2,3-8' means that progressions 7,8,9 and 8,2,3 share
           palette slot 8.
             There are a few letter codes that appear after hyphens on
           some lines. These are used to better control the random color
           selection process so the end result sprites don't look too
           bizarre. Here's what they all mean:
             - L  - Make this color choice light only
             - D  - Make this color choice dark only
             - LN - Make this color choice light or normal only
             - DN - Make this color choice dark or normal only
             - B  - Make this color choice normal only
             - E  - Make this color's darkest shade(s) darker than normal


----------------------------------------
  2. Weird things you should be aware of
----------------------------------------

Pokemon Emerald has a few little tricks that you, as a budding randomizer
developer, should know. There's a reason some parts of the code have
bizarre cases and indexing.

 - Pokemon numbering
          As in most other pokemon games, the normal Dex order is not
        reflected in the game's code. The first 251 pokemon are in order,
        followed by 25 unused numbers, and tailed by the Hoenn pokemon...
        not in the normal order. There are a series of arrays in the code
        to help keep the order sensible, but you may be best off referring
        to a list with the weird coded numbers instead of the normal dex
        ordering.


 - Palette exceptions
          A few pokemon have weird unique palette coding. I've enumerated
        the ones I remember below -- my apologies if I've left one out
        or mistook its odd characteristic.

         - Castform
              Castforms many forms have different palettes, each of which
              has an oddly-coded algorithm. Because of this, Castform is
              ignored in the main palette loop. See the 'palSpecial.txt'
              file for more details. 
         - Missingno.
              For whatever reason, Missingno. is indexed at slot 252. Thus,
              that slot is ignored in some loops.
         - Shiny oddities
              Both Meditite and (I believe) Gastly have some weird coding
              for their shiny forms. Gastly, if memory serves, didn't end
              up being an issue since itschanging colors occurred before
              the anomaly. Meditite has its own special case hard-coded,
              however. Ctrl+F Meditite in the main code file to see what
              was done for this.
         - Palette slot sharing
              A few pokemon code the same palette slots that show up
              earlier in their palettes. Gastly and Goldeen have this for
              sure, but others may as well. I think I resolved this in my
              main palette loop, so this is just an FYI.

 - Pokemon byte coding
          There are a few places that use some weird indexing code to call
        pokemon. I don't know why it was used but did not allow for all 386
        pokemon to be used as replacements. In the same space, I've used a
        different set of assembly codes to allow all choices. For reference,
        here's what the old and new code did/do:

          - old:
               load value into register, logical shift left
          - new:
               load value into register, add new byte into register
               (0x00 if under index 256, 0xFF if over)

----------------------------------------
  3. Known Bugs
----------------------------------------

 - Wally's battle
          I'm pretty sure that, even though I revisited this issue a
        lot, Wally's battle is still buggy. There's something funky
        about how I pick the pokemon...likely fixable, but hard to test.
        I did like 30 tries last time of randomizing that battle, and
        all came back fine. Still, players have revealed their own

 - Color randomizations
          Certain colors don't shade quite as well as others. This is
        due to how we can't tell shades as easily for all colors.
        I tried my best to equalize the colors, but I still think the
        algorythm could use work. The problem colors are usually yellow,
        cyan, and magenta, though sometimes red, green, and blue can
        be a little rough, too. These colors are all around the max/min
        values of colors -- yellow is #FFFF00, cyan #00FFFF, magenta
        #FF00FF, red #FF0000, etc.


----------------------------------------
  4. Possible Improvement Ideas 
----------------------------------------

Not sure what to start tweaking? Why not these?

 - Pokemon moveset and typing synergy
          The idea here is to make a pokemon have a greater chance to
        learn attacks that follow its typing. The additional STAB is
        a very helpful thing! Make sure there's some variance, though,
        so you don't JUST end up havin' those attacks.
          This idea would likely require a static list of attacks that
        could be sorted by type, power, damaging vs. non-damaging, etc.
        Shouldn't be that tough to make a separate class that loads
        them all in and has a function to get random attacks.

 - Sort learned attacks by power
          Once a randomized moveset is selected, it might flow better
        through the game to have the strong attacks get placed on the
        higher levels. Not sure what the best course of action is for
        non-damaging moves, though. Maybe leave the non-damaging wherever
        they happen to be generated and just resort the attacks?
          This would benefit from the static attack class as mentioned
        above as well. Once that's made, this ought to be fairly easy
        to do as well.

 - Smarter TM move randomization
          TMs, when randomzied, have a great chance to be...erm...
        useless. Who wants Splash and Tackle as TM options? Also, there
        is a chance that you'll never see a TM for any given type that
        the player may be interested in. If you could guarantee certain
        attack options always appear, TMs will be far more useful, as they
        should be.
          My thoughts on this were to create different categories of
        attacks, then break the ratio of TM choices down into those
        categories. Something like Damaging, Stat changers, Status effects,
        and Healing were my thoughts, with a break down of like 70/15/10/5
        respectively. Each category would be broken down into strengths,
        and each selection would grab certain frequencies of those strengths.
          For example, Damaging moves between 10-55 base power could be
        'weak', 60-95 'medium', and 100+ 'strong'. Then, break the strength
        picks down like 20/60/20. Judging the other categories is up to the
        coder, but you can easily base the other types on accuracy and
        usefulness. For exmaple, Stat changers would put things like Growl
        as 'weak', Meditate as 'medium', and Calm Mind as 'strong'. You
        get the idea.
          For further usefulness, ensure each typing has a sure chance to
        be picked. This would be easiest to implement with that static
        attack class I keep beating to death in every one of these
        suggestions!

 - 'Keep evolution Forms Intact' for trainer pokemon
          Basically, make sure that Joey on Route 1 doesn't have a Blaziken
        while the Elite Four have Caterpies and Zigzagoon.

 - Make sure starter pokemon attack types can hit all other starters
          Ever start with a random pokemon that has a ghost-type move and
        realize that the first unescapable encounter is against a normal-type?
        Not a fun time. This might be a little more tricky, as you'll likely
        have to revisit movesets for random pokemon and keep starters and the
        first encounter (normally a Poochyena) in mind.

 - Mess with the shiny rate
          I couldn't find anyone professing to know where to change the rate
        for shinies, but it'd be cool to set it to whatever you want. :)

 - Make random pokemon locations sensical
          I'm not sure about the best way to do this, but make sure that the
        randomized pokemon in a given area aren't ridiculously typed for their
        surroundings. Some players hate seeing, say, Magmar in the middle of
        the ocean. (Well, I guess he IS a duck, but still!)
          This idea would have to bypass global substitutions and whatnot to
        pave the way for route-by-route analysis of viable pokemon types.
        Furthermore, you'd have to generate a list when the program is executed
        of all pokemon and what types they are to fit them into the surroundings.
        Remember, types may be randomized! Things might get hairy if you want
        to ensure all pokemon appear as you go through the location assigning
        process...This whole idea is going to be super-tricky. Best of luck if
        you are trying this one!

 - Implement a central random seed to replicate randomzations
          The awesome Universal Pokemon Randomizer does this. It uses one
        instance of Random to get a base seed, then uses that same seed for all
        randomizations to ensure that if the same seed were used to randomize
        again, the results would be identical. This allows people to have the
        same game as streamers, friends, etc. without needing the patches they
        used.
          This should be doable by all means, but it would require a lot of
        replacement since I was silly when I made this and created a lot of
        new Random instances whenever something got randomized.




