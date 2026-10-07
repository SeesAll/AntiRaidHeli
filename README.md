# AntiRaidHeli

AntiRaidHeli detects active player base raids and deploys escalating patrol helicopters over the raid zone. Armed players inside the marked danger area receive vanilla-style helicopter treatment, while confirmed raid aggressors face persistent suppression and attacks against their raid base. Destroying a helicopter earns its normal wreckage and loot, but also triggers the next response round after a short configurable delay.

Version 0.6.0 is a controlled public-test build. Automatic raid detection is disabled after installation or upgrade until an administrator runs `/antiraidhelistart`. That choice is saved across reloads and restarts. `/antiraidstop` disables protection again and immediately cleans every active AntiRaidHeli event, marker, timer, and hostility state.

## Current response sequence

1. **Suppression** — 3,000 health, machine guns, light rocket/napalm pressure, and one loot crate.
2. **Escalation** — 6,000 health, 12 base rockets per pass, 45% base napalm chance, and one loot crate.
3. **Maximum Response** — 9,000 health, 16 base rockets per pass, 65% base napalm chance, and two loot crates.
4. **Final Response** — two helicopters with 12,000 health each, 20 base rockets per pass, 85% base napalm chance, and three loot crates per helicopter. This level targets only recently aggressive players.

All health, rotor health, gun, rocket, napalm, timing, targeting, marker, and announcement settings are configurable.

Gun accuracy and projectile speed scale with each response level. The default profile intentionally retains some misses while ensuring exposed aggressors face credible sustained fire.

When a recorded aggressor hides inside a confirmed raid base and continues attacking the victim base, the helicopter makes repeated rocket or napalm passes against that hostile shelter. Default rocket-pass cooldowns scale from 30 seconds at Suppression to 10 seconds at Final Response.

Rocket pressure adapts to both raid activity and resistance against the helicopter. Sustained structural damage increases rocket count, shortens the effective cooldown, and raises napalm probability. Continued helicopter damage triggers the heavy-pressure stage once the current response falls below its configurable remaining-health threshold—50%, 65%, 75%, and 85% by default across rounds 1–4. Pressure decays when attackers stop, preserving a real surrender path.

Helicopter progression is defeat-based. Waiting causes the response to pause rather than count as a victory. Renewed structural damage restores the same level with its saved remaining health. Destroying a level proves continued aggression and automatically deploys the next response after a configurable five-second delay. Response state—including a pending inter-round deployment—is remembered for six hours by default and survives plugin reloads and server restarts. Defeating all four levels grants clearance for the continuing raid session.

Clearance is scoped to the same raiding party continuing against the same victim property. An unrelated group raiding another nearby base cannot inherit a completed response chain.

The configurable helicopter spawn mode supports `NearbyLand` for a short land-based arrival, or `OverRaidArea` to spawn immediately above the raid hotspot. Nearby-land mode prefers the configured 300-metre stand-off and searches progressively closer when a raid occurs on a small island or near the coast.

## Raid detection

The plugin has no required dependencies. It directly observes player damage to another player's building blocks, doors, simple building blocks, and tool cupboards. A raid must reach the configured accumulated-damage threshold plus either the configured hit count or the heavy single-hit threshold inside the qualification window. This allows genuine explosive raids to qualify quickly without summoning a helicopter for a couple of stray rifle rounds.

Underground cave bases are excluded from automatic helicopter responses by default. The plugin compares both the damaged structure and its connected Tool Cupboard with Rust's terrain surface; a configurable eight-metre depth threshold identifies a cave base without treating ordinary foundations and shallow bunkers as underground. Cave raids therefore remain open game instead of creating unreachable helicopters, terrain-blocked attacks, or endless patrol behavior. Administrators can disable this exclusion or change the threshold, and explicit administrator test events remain available underground.

Nearby structural damage is merged into one incident. Continued qualifying structure damage refreshes the incident. Ordinary PvP does not keep a raid alive.

Damage by the structure owner, authorized Tool Cupboard users, native teammates, and—when installed—members recognized by Clans or Friends can be ignored independently. Clans, Friends, SmartRecon, and NoEscape are not required dependencies.

## Targeting rules

- Only the helicopter created for a particular raid is modified.
- Vanilla patrol helicopters are not changed.
- Confirmed raiders and temporary combatants remain hostile for three minutes after their last hostile action—even through death and respawn—and receive private countdown messages.
- Raid-hostile players receive a configurable right-side CUI countdown below NoEscape's default raid-block indicator, with a matching compact helicopter silhouette, a small `ROUND` caption, and current/total response indicator such as `1/4` through `4/4`, followed by a brief cleared state and private retreat message.
- Ordinary armed players use Rust's native threat rules and can disarm to disengage without being classified as raiders.
- Firearms and other weapon-category items carried in the belt explicitly count as armed even when Rust's transient native threat score is low.
- Attacking the helicopter or another player creates temporary combat hostility but does not by itself expose an unrelated player's structures to rocket damage. Threat scoring prioritizes active raiders and major damage sources while allowing multi-helicopter rounds to split targets.
- Targets must also be alive, connected, visible, outside safe zones, inside the danger zone, and satisfy the configured threat rules.
- Solid terrain, buildings, and deployables break line of sight.
- SmartRecon invisibility is supported when SmartRecon is installed.
- Rocket strafes target recorded raid aggressors rather than uninvolved defenders.
- A recent aggressor hiding in a confirmed hostile raid base can trigger a structure strafe; protected victim and ambiguous property still cannot be damaged by the event helicopter.
- Victim-owned building pieces, defenses, and deployables are protected from plugin helicopter damage.
- Aggressor-owned structures remain damageable even without a Tool Cupboard.
- Recently constructed third-party raid bases can be classified as hostile when aggressors actively occupy them.

## Raid-base identification and property protection

The plugin maintains a compact, configurable construction history grouped by building ID. The default history is six hours, with a one-hour fast-classification window. A recent building within 100 metres of the raided base is considered only after at least three pieces have been placed. It becomes a hostile raid base when recorded aggressors remain inside, on top of, or immediately around it for the configured confirmation time.

Direct aggressor, native team, clan, friend, and Tool Cupboard associations do not depend on building age. Victim ownership takes precedence over hostile classification, and ambiguous structures are protected by default. Recent construction history can be persisted across plugin reloads and server restarts.

## Escalation

When a helicopter is destroyed, it selects a randomized dry-land crash destination 300–600 metres from the defended raid by default instead of repeatedly favoring the nearest monument. This creates a deliberate choice between continuing the raid and leaving to contest the helicopter loot. The distance ring and the feature itself are configurable, and the behavior applies only to AntiRaidHeli helicopters. Loot scales at 1/1/2 crates across the first three levels. The default Final Response deploys two full-strength helicopters, each dropping three crates; the final round is not defeated until both are destroyed. Multi-helicopter mode and its one-to-three-helicopter count are configurable per response profile. Destroying the current group starts the next round after five seconds by default; the HUD and marker continue to show the completed round until deployment. Inactivity pauses rather than defeats the current helicopter group, preserving every fuselage and rotor's health for the resumed response. After all configured levels are defeated, the original raiding party earns clearance for that continuing raid against that exact connected building until its response memory expires, and its hostility indicator is removed immediately. Attacking another separately constructed base begins a fresh response chain, including when the other base belongs to the same victim.

While a response chain is active, identified aggressor raid bases cannot be repaired, upgraded, or expanded from an attached building piece. The repair hook also blocks BetterTC's Repair All operation. If NoEscape is installed, confirmed raiders who attack the response helicopter have their raid block refreshed so an extended air fight or respawn cannot create an unintended repair window.

## Configuration organization

The generated configuration is intentionally divided into related sections:

- `Raid detection` controls what qualifies as a raid, cave-base exclusion, incident merging, inactivity, response memory, concurrency, and friendly-damage exclusions.
- `Raid base identification and structure protection` controls recent-building history and how occupied raid bases become valid helicopter targets.
- `Player targeting` controls the danger zone, armed-player behavior, and the raid-hostility duration.
- `Raid hostility screen indicator` controls the NoEscape-style HUD alert and its position/colors.
- `Adaptive anti-raid pressure` controls how continued raiding and attacks on the helicopter increase rocket and napalm pressure.
- `Helicopter patrol` controls spawn mode, approach, patrol area, optional lifetime, and the randomized dry-land crash-distance ring.
- `Map marker` and `Announcements` control player-facing map/chat information.
- `Escalating response profiles` contains the ordered health and weapon settings for every response level.

Each response profile can independently enable a multi-helicopter group and select one to three helicopters. The defaults use one helicopter for levels 1–3 and two for level 4. Multi-helicopter groups use a spaced approach formation, separate concentric orbit lanes, staggered attack windows, shared threat ranking with split gun targets, and independent crash destinations; orbit spacing and attack staggering are configurable under `Helicopter patrol`. Each profile also controls fuselage health, rotor health, and the remaining-health percentage that enables combat-driven heavy rocket pressure. `Gun aim cone scale` controls physical bullet spread: lower values are more accurate, with defaults of `1.00`, `0.75`, `0.50`, and `0.30` across the four levels. `Bullet accuracy percent` remains the final hit-acceptance control, allowing spread and effective accuracy to be tuned separately.

Section-level `Enabled` values are deliberate feature switches; the top-level `Enabled` value is the plugin-wide detection switch. Existing configuration files are merged with new defaults on load, and response-profile arrays are replaced rather than appended so updates cannot duplicate response levels.

AntiRaidHeli tags every marker it creates and removes orphaned markers when the plugin loads or the final incident is stopped. Legacy markers created before tagging was introduced are also recognized by their AntiRaidHeli label and matching radius position.

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

`/antiraidhelistart` enables automatic protection and saves that state across plugin reloads and server restarts. `/antiraidstop` disables automatic protection and terminates all active helicopters, pending replacements, markers, hostility indicators, and saved raid progress. The older `/antiraidhelistop` spelling remains as a compatibility alias. For a single-player property-protection test, look at the simulated victim building and run `/antiraidhelitest [level]`; the targeted building is protected while the administrator's other buildings are treated as aggressor property. The console-only `antiraidheli.test [level]` starts a manual response at an online player's position, or the map center when nobody is connected.

## Installation

Place `AntiRaidHeli.cs` in the server's `oxide/plugins` directory. The configuration is generated at `oxide/config/AntiRaidHeli.json` and is safely extended with default values when new settings are introduced. Version 0.6.0 deliberately migrates existing installations to `Enabled: false` once and applies the TEST-server beta health ladder so live testing begins under administrator supervision.

The HUD helicopter image is embedded in the plugin and stored through Rust's own file storage while loaded. It does not require ImageLibrary or an external image host and is removed from file storage when the plugin unloads.

## Performance design

Raid damage is filtered with inexpensive checks before an incident is created. Construction is stored once per building rather than once per entity, expired construction and hostility records are pruned, and data is written in batches. Active incidents are maintained once per second, target scanning is limited to active players, and only plugin-owned helicopters receive custom behavior. The default maximum is two simultaneous raid zones.
