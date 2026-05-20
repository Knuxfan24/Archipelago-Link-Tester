# Archipelago Link Tester

A program to help with testing Link implementations for Archipelago worlds.

Can currently test:

- TrapLink, specifying the name of the trap.
- DeathLink, specifying the cause (leaving the cause empty or setting it to null is also a valid option, as some games may not handle that well).
- RingLink, specifying the number to give (negative values will instead take Rings away).
- DamageLink, specifying the number of damage points to send.

### Usage

Generate a multiworld including the Link Tester apworld as one of the slots, then set the server address, player name and (if applicable) password to connect to that slot on your server. Then use the approriate tab for whichever Link you wish to test.