# AntiRaidHeli

AntiRaidHeli detects active player base raids and deploys escalating patrol helicopters over the raid zone. Armed players inside the marked danger area receive vanilla-style helicopter treatment, while confirmed raid aggressors face persistent suppression and attacks against their raid base. Destroying a helicopter earns its normal wreckage and loot, but also triggers the next response round after a short configurable delay.

Version 0.6.9 remains administrator-controlled while live balancing continues. The configuration separately controls the raid coverage policy and whether automatic monitoring is currently running. Monitoring remains stopped after installation until an administrator runs `/antiraidhelistart`; that runtime choice is saved across reloads and restarts. `/antiraidstop` or `/antiraidhelistop` stops monitoring and immediately cleans every active AntiRaidHeli event, marker, timer, and hostility state. Administrator test incidents remain fully functional regardless of the configured coverage mode or monitoring state.

The `Raid coverage mode` accepts three values:

- `AllRaids` protects every qualifying raid.
- `OfflineRaidsOnly` protects a qualifying base only when none of its captured owners, Tool Cupboard or vehicle-privilege authorized players, or configured native-team/Clans/Friends associates are connected when the raid qualifies. The existing friendly-damage switches determine which optional relationships count. That decision is retained for the life of the incident so a response does not flicker as players connect or disconnect.
- `Disabled` prevents automatic monitoring from being started. Administrator tests are still available.

## Current response sequence

1. **Suppression** — 30,000 health, machine guns, light rocket/napalm pressure, and one loot crate.
2. **Escalation** — 50,000 health, 12 base rockets per pass, 45% base napalm chance, and one loot crate.
3. **Maximum Response** — 80,000 health, 16 base rockets per pass, 65% base napalm chance, and two loot crates.
4. **Final Response** — two helicopters with 120,000 health each, 20 base rockets per pass, 85% base napalm chance, and three loot crates per helicopter. This level targets only recently aggressive players.

All health, rotor health, gun, rocket, napalm, timing, targeting, marker, and announcement settings are configurable.

Gun accuracy and projectile speed scale with each response level. The default profile intentionally retains some misses while ensuring exposed aggressors face credible sustained fire.

When a recorded aggressor hides inside a confirmed raid base and continues attacking the victim base, the helicopter makes repeated rocket or napalm passes against that hostile shelter. Default rocket-pass cooldowns scale from 30 seconds at Suppression to 10 seconds at Final Response.

Rocket pressure adapts to both raid activity and resistance against the helicopter. Sustained structural damage increases rocket count, shortens the effective cooldown, and raises napalm probability. Continued helicopter damage triggers the heavy-pressure stage once the current response falls below its configurable remaining-health threshold—50%, 65%, 75%, and 85% by default across rounds 1–4. Two independent groups attacking the helicopter trigger sustained pressure and three trigger heavy pressure by default. Each additional group also improves effective and physical gun accuracy within configurable limits. Pressure decays when attackers stop, preserving a real surrender path.

Helicopter progression is defeat-based. Waiting causes the response to pause rather than count as a victory. Renewed structural damage restores the same level with its saved remaining health. Destroying a level proves continued aggression and automatically deploys the next response after a configurable five-second delay. Response state—including a pending inter-round deployment—is remembered for six hours by default and survives plugin reloads and server restarts. Defeating all four levels grants clearance for the continuing raid session.

Clearance is scoped to the same raiding party continuing against the same victim property. An unrelated group raiding another nearby base cannot inherit a completed response chain.

The configurable helicopter spawn mode supports `NearbyLand` for a short land-based arrival, or `OverRaidArea` to spawn immediately above the raid hotspot. Nearby-land mode prefers the configured 300-metre stand-off and searches progressively closer when a raid occurs on a small island or near the coast.

## Raid detection

The plugin has no required dependencies. It directly observes player damage to another player's building blocks, doors, simple building blocks, tool cupboards, and current player-built boat structures. Vehicle privilege authorization is respected for boat raids. A raid must reach the configured accumulated-damage threshold plus either the configured hit count or the heavy single-hit threshold inside the qualification window. This allows genuine explosive raids to qualify quickly without summoning a helicopter for a couple of stray rifle rounds.

Underground cave bases are excluded from automatic helicopter responses by default. The plugin compares both the damaged structure and its connected Tool Cupboard with Rust's terrain surface; a configurable eight-metre depth threshold identifies a cave base without treating ordinary foundations and shallow bunkers as underground. Cave raids therefore remain open game instead of creating unreachable helicopters, terrain-blocked attacks, or endless patrol behavior. Administrators can disable this exclusion or change the threshold, and explicit administrator test events remain available underground.

Nearby structural damage is merged into one incident. Continued qualifying structure damage refreshes the incident. Ordinary PvP does not keep a raid alive.

Damage by the structure owner, authorized Tool Cupboard users, native teammates, and—when installed—members recognized by Clans or Friends can be ignored independently. Clans, Friends, SmartRecon, and NoEscape are not required dependencies.

## Targeting rules

- Only the helicopter created for a particular raid is modified.
- Vanilla patrol helicopters are not changed.
- The server-wide `patrolhelicopterai.flee_damage_percentage` value is ignored only for AntiRaidHeli aircraft. Event helicopters therefore cannot retreat at low health and falsely advance a response round; vanilla and other plugin helicopters retain the server owner's setting.
- Confirmed raiders and temporary combatants remain hostile for three minutes after their last hostile action—even through death and respawn—and receive private countdown messages.
- Raid-hostile players receive a configurable right-side CUI countdown below NoEscape's default raid-block indicator, with a matching compact helicopter silhouette, a small `ROUND` caption, and current/total response indicator such as `1/4` through `4/4`, followed by a brief cleared state and private retreat message.
- Ordinary armed players use Rust's native threat rules and can disarm to disengage without being classified as raiders.
- Firearms and other weapon-category items carried in the belt explicitly count as armed even when Rust's transient native threat score is low.
- Attacking the helicopter or another player creates temporary combat hostility. A helicopter attacker exposes only property associated with that attacker inside the danger zone; unrelated property remains protected. Threat scoring prioritizes active raiders and major damage sources while allowing multi-helicopter rounds to split targets.
- Targets must also be alive, connected, visible, outside safe zones, inside the danger zone, and satisfy the configured threat rules.
- Solid terrain, buildings, and deployables break line of sight.
- SmartRecon invisibility is supported when SmartRecon is installed.
- Rocket strafes target recorded raid aggressors and other players who actively attack the event helicopter, rather than uninvolved defenders or bystanders.
- A recent combatant hiding in an associated base can trigger a structure strafe. Ambiguous and unrelated property still cannot be damaged by the event helicopter.
- Victim-owned building pieces, defenses, and deployables remain protected unless a victim owner, authorized occupant, teammate, clan member, or recognized friend attacks the response helicopter from inside the danger zone.
- Aggressor-owned structures remain damageable even without a Tool Cupboard.
- Recently constructed third-party raid bases can be classified as hostile when aggressors actively occupy them.

## Raid-base identification and property protection

The plugin maintains a compact, configurable construction history grouped by building ID. The default history is six hours, with a one-hour fast-classification window. A recent building within 100 metres of the raided base is considered only after at least three pieces have been placed. It becomes a hostile raid base when recorded aggressors remain inside, on top of, or immediately around it for the configured confirmation time.

Direct aggressor, native team, clan, friend, and Tool Cupboard associations do not depend on building age. Victim ownership takes precedence over hostile classification, and ambiguous structures are protected by default. Recent construction history can be persisted across plugin reloads and server restarts.

## Escalation

When a helicopter is destroyed, it selects a randomized crash destination 50–300 metres from the defended raid by default instead of repeatedly favoring the nearest monument. Selection prefers dry land, then configurable shallow water, and can fall back to deeper water when an offshore raid has no valid nearby shore. This keeps boat and ocean-base responses from becoming stuck without changing vanilla or other-plugin helicopters. Loot scales at 1/1/2 crates across the first three levels. The default Final Response deploys two full-strength helicopters, each dropping three crates; the final round is not defeated until both are destroyed. A one-to-three-helicopter count is configurable inside every response profile, with `1` meaning a normal single-aircraft round. Destroying the current group starts the next round after five seconds by default; the HUD and marker continue to show the completed round until deployment. Inactivity pauses rather than defeats the current helicopter group, preserving every fuselage and rotor's health for the resumed response. After all configured levels are defeated, the original raiding party earns clearance for that continuing raid against that exact connected building or protected player-built boat until its response memory expires, and its hostility indicator is removed immediately. Attacking another separately constructed property begins a fresh response chain, including when it belongs to the same victim.

While a response chain is active, identified aggressor raid bases cannot be repaired, upgraded, or expanded from an attached building piece. The repair hook also blocks BetterTC's Repair All operation. If NoEscape is installed, confirmed raiders who attack the response helicopter have their raid block refreshed so an extended air fight or respawn cannot create an unintended repair window.

## Configuration organization

The generated configuration is intentionally divided into related sections:

- `Raid detection` controls what qualifies as a raid, cave-base exclusion, incident merging, inactivity, response memory, concurrency, and friendly-damage exclusions.
- `Raid base identification and structure protection` controls recent-building history and how occupied raid bases become valid helicopter targets.
- `Player targeting` controls the danger zone, armed-player behavior, and the raid-hostility duration.
- `Raid hostility screen indicator` controls the NoEscape-style HUD alert and its position/colors.
- `Continued raid activity escalation` controls the shared sustained/heavy pressure multipliers and the activity thresholds that continued raiding or helicopter combat can trigger. Independent attacker groups can trigger these same pressure stages through the next section.
- `Independent attacker-group escalation` controls the added pressure and accuracy applied when unrelated groups cooperate against the response helicopter.
- `Helicopter spawn, patrol, and crash behavior` controls spawn mode, approach, patrol area, optional lifetime, and the randomized crash-distance ring.
- `Map marker` and `Announcements` control player-facing map/chat information.
- `Escalating response profiles` contains the ordered health and weapon settings for every response level.

The response-profile list order is the round order. Each profile has `Number of helicopters deployed for this specific round (1-3; 1 = single helicopter)`, so the value applies only to the named profile containing it. The defaults use one helicopter for rounds 1–3 and two for round 4; the hard maximum is three. Multi-helicopter groups use a spaced approach formation, separate concentric orbit lanes, staggered attack windows, shared threat ranking with split gun targets, and independent crash destinations; orbit spacing and attack staggering are configurable under `Helicopter spawn, patrol, and crash behavior`. Each profile also controls fuselage health, rotor health, and the remaining-health percentage that enables combat-driven heavy rocket pressure. `Gun aim cone scale (0.05-3)` controls physical bullet spread, with lower values producing tighter aim. `Successful bullet damage chance percent (0-100)` is the final hit-acceptance control, allowing physical spread and effective damage accuracy to be tuned separately.

## Configuration guide

AntiRaidHeli writes a complete configuration after its first load. Existing configurations are merged and migrated automatically when settings are renamed or added; administrators should not need to delete and rebuild the file. The example below shows the major control settings and one complete response profile. Rounds 2–4 use the same fields with progressively stronger default values.

```json
{
  "Raid coverage mode (AllRaids, OfflineRaidsOnly, or Disabled)": "AllRaids",
  "Automatic protection monitoring enabled (managed by admin start and stop commands)": false,
  "Raid detection": {
    "Minimum qualifying hits within the qualification window": 2,
    "Minimum accumulated structure damage within the qualification window": 100.0,
    "Single structure hit damage that can satisfy the hit-count requirement": 100.0,
    "Qualification window seconds": 20.0,
    "Initial helicopter response delay seconds (0 = instant)": 0.0,
    "Seconds between defeated response rounds (0 = immediate)": 5.0,
    "Pause response after no raid damage or helicopter combat for seconds (progress remains remembered)": 300.0,
    "Remember paused response progress minutes (0 = until wipe)": 360.0
  },
  "Player targeting": {
    "Danger zone radius meters": 125.0,
    "Allow vanilla threat targeting of armed non-aggressors in the danger zone": true,
    "Minimum native threat level (0-1)": 0.5,
    "Raid and combat hostility duration seconds": 180.0
  },
  "Helicopter spawn, patrol, and crash behavior": {
    "Spawn mode (NearbyLand or OverRaidArea)": "NearbyLand",
    "Nearby-land spawn distance from raid meters": 300.0,
    "Orbit radius around raid meters": 75.0,
    "Minimum crash distance from raid meters": 50.0,
    "Maximum crash distance from raid meters": 300.0
  },
  "Escalating response profiles": [
    {
      "Name": "Round 1 - Suppression",
      "Health": 30000.0,
      "Main rotor health": 2700.0,
      "Tail rotor health": 1500.0,
      "Escalate rockets and napalm at remaining health percent": 50.0,
      "Loot crates dropped when destroyed": 1,
      "Number of helicopters deployed for this specific round (1-3; 1 = single helicopter)": 1,
      "Bullet damage": 20.0,
      "Bullet speed": 300,
      "Successful bullet damage chance percent (0-100)": 65.0,
      "Gun aim cone scale (0.05-3; lower = tighter physical spread)": 1.0,
      "Gun fire rate seconds": 0.125,
      "Burst length seconds": 3.0,
      "Seconds between bursts": 3.0,
      "Maximum target range meters": 300.0,
      "Maximum speed": 42.0,
      "Maximum rotation speed scale": 1.0,
      "Enable rockets": true,
      "Maximum rockets per attack": 3,
      "Seconds between rockets": 0.2,
      "Rocket damage scale": 1.0,
      "Minimum seconds between rocket attacks": 30.0,
      "Enable napalm": true,
      "Napalm chance percent per rocket attack": 10.0
    }
  ]
}
```

### Core controls

- `Raid coverage mode` determines which naturally detected raids qualify. `AllRaids` covers online and offline raids, `OfflineRaidsOnly` responds only when no recognized defender is connected at qualification time, and `Disabled` prevents automatic monitoring from starting. Admin tests always remain available.
- `Automatic protection monitoring enabled` records the persistent state controlled by `/antiraidhelistart` and `/antiraidhelistop`. Editing it directly is supported, although the commands are safer during live operation because stop also performs cleanup.
- Qualification settings prevent stray gunfire from creating an event. A raid qualifies after meeting the accumulated-damage requirement plus either the normal hit-count requirement or the heavy single-hit threshold inside the qualification window.
- `Pause response...` pauses a response after both qualifying raid damage and combat against its helicopter have been quiet for the configured period. It does not award victory or skip the round. `Remember paused response progress` controls how long that round and its remaining health can resume.
- `Danger zone radius` is the player-targeting and map-warning radius. Confirmed aggressors remain hostile for the configured duration; non-aggressors still follow Rust's normal armed/threatening behavior.
- `Spawn mode` accepts `NearbyLand` or `OverRaidArea`. Crash-distance settings form a randomized ring around the defended property and can fall back to water for offshore incidents.

### Response profile fields

Round 1 above demonstrates every field shared by all four ordered response profiles:

- `Name` is used in announcements and marker text. The array position—not a separately editable level number—determines the round.
- `Health`, `Main rotor health`, and `Tail rotor health` control the aircraft and weak-point durability for each helicopter in that round.
- `Escalate rockets and napalm at remaining health percent` enables combat-driven heavy pressure when attackers reduce that round's helicopter to the stated remaining-health percentage.
- `Loot crates dropped when destroyed` applies to each destroyed helicopter. A two-helicopter round with `3` therefore produces up to six crates.
- `Number of helicopters deployed for this specific round` applies only to the profile block containing it. `1` means a normal single-helicopter round; `2` or `3` enables a coordinated multi-helicopter group for that round.
- `Bullet damage` is damage before ordinary Rust mitigation. `Bullet speed` controls projectile velocity. `Successful bullet damage chance` is a final 0–100% hit-acceptance check, while `Gun aim cone scale` controls physical spread; lower aim-cone values are more accurate.
- Fire rate, burst length, and delay between bursts control machine-gun cadence. Maximum target range remains constrained by the configured danger and patrol boundaries.
- Rocket count is per attack pass, not the total ammunition for the helicopter. Dynamic pressure can multiply the count and shorten the cooldown. Rocket damage scale multiplies normal rocket damage.
- Napalm chance is evaluated per rocket attack. Sustained/heavy pressure can add the configured napalm bonuses, capped at 100%.

Recommended practice is to change one category at a time and benchmark before and after. Health changes affect encounter length; accuracy and damage affect player lethality; rocket count, cooldown, and multi-helicopter counts have the greatest effect on destruction and server workload.

Section-level `Enabled` values are deliberate feature switches. `Automatic protection monitoring enabled` is the persistent runtime state managed by the start/stop commands, while `Raid coverage mode` determines whether all raids, only offline raids, or no organic raids qualify. Existing configuration files are migrated without duplicating response levels, and the removed legacy fields are translated into their replacement settings before the cleaned configuration is saved.

AntiRaidHeli tags every marker it creates and removes orphaned markers when the plugin loads or the final incident is stopped. Legacy markers created before tagging was introduced are also recognized by their AntiRaidHeli label and matching radius position.

AntiRaidHeli response aircraft must be genuinely destroyed to advance the escalation chain. Native damage-threshold retirement is suppressed per event helicopter without changing any global Rust convar. Plugin-controlled retirement for raid inactivity, cleanup, unloading, and administrator stop remains available.

## Permissions and commands

Permission:

```text
antiraidheli.admin
```

Chat commands:

```text
/antiraidhelistart
/antiraidstop
/antiraidhelitest [level]
/antiraidhelistop
```

Console commands:

```text
antiraidheli.start
antiraidheli.stop
antiraidheli.test [level]
```

`/antiraidhelistart` starts automatic monitoring and saves that state across plugin reloads and server restarts. It refuses to start while the configured coverage mode is `Disabled` and tells the administrator what must be changed. `/antiraidstop` or `/antiraidhelistop` stops monitoring and terminates all active helicopters, pending replacements, markers, hostility indicators, and saved raid progress. For a controlled property-protection test, look at the simulated victim building and run `/antiraidhelitest [level]`. The exact targeted building (or player-built boat) remains protected for the complete test, including when CopyPaste assigns it the same owner as the administrator. Any player who damages it is learned as an aggressor, and that player's other associated structures can become hostile raid-base targets through the normal classification rules. The test bypasses both the coverage mode and automatic-monitoring state. The console-only `antiraidheli.test [level]` starts a manual response at an online player's position, or the map center when nobody is connected.

## Installation

Place `AntiRaidHeli.cs` in the server's `oxide/plugins` directory. The configuration is generated at `oxide/config/AntiRaidHeli.json` and is safely migrated when settings are renamed or reorganized. Version 0.6.10 retains the existing runtime enabled/stopped choice, defaults coverage to `AllRaids`, and keeps fresh installations stopped until an administrator deliberately starts monitoring. Its RaidProtection integration tracks event rockets and napalm through their complete lifetime so only confirmed aggressor/combatant structures bypass external protection.

The HUD helicopter image is embedded in the plugin and stored through Rust's own file storage while loaded. It does not require ImageLibrary or an external image host and is removed from file storage when the plugin unloads.

## Performance design

Raid damage is filtered with inexpensive checks before an incident is created. Construction is stored once per building rather than once per entity, expired construction and hostility records are pruned, and data is written in batches. Active incidents are maintained once per second, target scanning is limited to active players, and only plugin-owned helicopters receive custom behavior. Combat-group evaluation is cached, pooled collections are used in recurring classification work, and health diagnostics aggregate projectile hits instead of allocating and scheduling one callback per shot. The default maximum is two simultaneous raid zones.
