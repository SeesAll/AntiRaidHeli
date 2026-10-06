# AntiRaidHeli

AntiRaidHeli detects active player base raids and deploys escalating patrol helicopters over the raid zone. Armed players inside the marked danger area receive vanilla-style helicopter treatment, while confirmed raid aggressors face persistent suppression and attacks against their raid base. Destroying a helicopter earns its normal wreckage and loot, but renewed structural raiding immediately summons the next response level.

## Current response sequence

1. **Suppression** — 100,000 health, machine guns, and light, infrequent rocket/napalm pressure.
2. **Escalation** — 250,000 health with stronger guns and moderately increased rocket/napalm pressure.
3. **Maximum Response** — 500,000 health, stronger targeting, and heavy rocket/napalm pressure.
4. **Final Response** — 1,000,000 health and the most aggressive weapon profile. This level targets only recently aggressive players.

All health, rotor health, gun, rocket, napalm, timing, targeting, marker, and announcement settings are configurable.

Gun accuracy and projectile speed scale with each response level. The default profile intentionally retains some misses while ensuring exposed aggressors face credible sustained fire.

When a recorded aggressor hides inside a confirmed raid base and continues attacking the victim base, the helicopter makes repeated rocket or napalm passes against that hostile shelter. Default rocket-pass cooldowns scale from 30 seconds at Suppression to 15 seconds at Final Response.

Rocket pressure adapts to raid activity. Sustained structural damage increases rocket count, shortens the effective cooldown, and raises napalm probability. When structural damage stops, hostile-structure attacks stop after the configured pressure window while the helicopter remains on patrol.

Helicopter progression is defeat-based. Waiting causes the response to pause rather than count as a victory. Renewed structural damage restores the same level with its saved remaining health; destroying a level arms the next response for immediate deployment on the next raid hit. Response state is remembered for six hours by default and survives plugin reloads and server restarts. Defeating all four levels grants clearance for the continuing raid session.

Clearance is scoped to the same raiding party continuing against the same victim property. An unrelated group raiding another nearby base cannot inherit a completed response chain.

The configurable helicopter spawn mode supports `NearbyLand` for a short land-based arrival, or `OverRaidArea` to spawn immediately above the raid hotspot. Nearby-land mode prefers the configured 300-metre stand-off and searches progressively closer when a raid occurs on a small island or near the coast.

## Raid detection

The plugin has no required dependencies. It directly observes player damage to another player's building blocks, doors, simple building blocks, and tool cupboards. A raid must reach the configured accumulated-damage threshold plus either the configured hit count or the heavy single-hit threshold inside the qualification window. This allows genuine explosive raids to qualify quickly without summoning a helicopter for a couple of stray rifle rounds.

Nearby structural damage is merged into one incident. Continued qualifying structure damage refreshes the incident. Ordinary PvP does not keep a raid alive.

Damage by the structure owner, authorized Tool Cupboard users, native teammates, and—when installed—members recognized by Clans or Friends can be ignored independently. Clans, Friends, SmartRecon, and NoEscape are not required dependencies.

## Targeting rules

- Only the helicopter created for a particular raid is modified.
- Vanilla patrol helicopters are not changed.
- Confirmed raiders and temporary combatants remain hostile for three minutes after their last hostile action and receive private countdown messages.
- Raid-hostile players receive a configurable right-side CUI countdown below NoEscape's default raid-block indicator, with a matching compact helicopter silhouette, followed by a brief cleared state and private retreat message.
- Ordinary armed players use Rust's native threat rules and can disarm to disengage without being classified as raiders.
- Firearms and other weapon-category items carried in the belt explicitly count as armed even when Rust's transient native threat score is low.
- Attacking the helicopter or another player creates temporary combat hostility but does not expose that player's structures to rocket damage.
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

When a helicopter is destroyed, its normal crash behavior and loot remain intact. The next response level is armed but does not deploy until qualifying structural raiding resumes. Inactivity pauses rather than defeats the current helicopter, preserving its body and rotor health for the resumed response. After all configured levels are defeated, the original raiding party earns clearance for that continuing raid session until its response memory expires.

## Configuration organization

The generated configuration is intentionally divided into related sections:

- `Raid detection` controls what qualifies as a raid, incident merging, inactivity, response memory, concurrency, and friendly-damage exclusions.
- `Raid base identification and structure protection` controls recent-building history and how occupied raid bases become valid helicopter targets.
- `Player targeting` controls the danger zone, armed-player behavior, and the raid-hostility duration.
- `Raid hostility screen indicator` controls the NoEscape-style HUD alert and its position/colors.
- `Adaptive anti-raid pressure` controls how continued raiding increases rocket and napalm pressure.
- `Helicopter patrol` controls spawn mode, approach, patrol area, and optional lifetime.
- `Map marker` and `Announcements` control player-facing map/chat information.
- `Escalating response profiles` contains the ordered health and weapon settings for every response level.

Section-level `Enabled` values are deliberate feature switches; the top-level `Enabled` value is the plugin-wide detection switch. Existing configuration files are merged with new defaults on load, and response-profile arrays are replaced rather than appended so updates cannot duplicate response levels.

## Permissions and commands

Permission:

```text
antiraidheli.admin
```

Chat commands:

```text
/antiraidhelistart [level]
/antiraidhelitest [level]
/antiraidhelistop
```

Console commands:

```text
antiraidheli.start [level]
antiraidheli.stop
```

The start command is intended for controlled testing and starts the requested response level at the administrator's position. From the server console it uses an online player's position, or the map center when nobody is connected. For a single-player property-protection test, look at the simulated victim building and run `/antiraidhelitest [level]`; the targeted building is protected while the administrator's other buildings are treated as aggressor property. The stop command terminates all active helicopters, pending replacements, markers, and raid state.

## Installation

Place `AntiRaidHeli.cs` in the server's `oxide/plugins` directory. The configuration is generated at `oxide/config/AntiRaidHeli.json` and is safely extended with default values when new settings are introduced.

The HUD helicopter image is embedded in the plugin and stored through Rust's own file storage while loaded. It does not require ImageLibrary or an external image host and is removed from file storage when the plugin unloads.

## Performance design

Raid damage is filtered with inexpensive checks before an incident is created. Construction is stored once per building rather than once per entity, expired construction and hostility records are pruned, and data is written in batches. Active incidents are maintained once per second, target scanning is limited to active players, and only plugin-owned helicopters receive custom behavior. The default maximum is two simultaneous raid zones.
