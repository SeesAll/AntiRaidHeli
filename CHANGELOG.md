# Changelog

## 0.3.2 - 2026-10-05

- Added a compact front-facing attack-helicopter image to the `HELI HOSTILE` indicator, using the same 13%-wide icon slot and dark-red visual treatment as NoEscape's raid-block icon.
- Embedded the optimized image directly in the plugin and registered it with Rust's own file storage, so it requires no external image host or ImageLibrary dependency. A code-drawn fallback remains available if registration ever fails.
- Shifted the indicator label to preserve clear spacing between the new icon, status text, and countdown.
- Fixed overlapping raid incidents being able to send a premature hostility-cleared message or overwrite the newest countdown.
- Pruned expired hostility records instead of checking them every second for the full response-memory period.
- Fixed manual stops and expired incidents leaving stale HUD state or stale persisted raid progress behind.
- Cleared both raid-progress and construction-history data on a new map save.
- Restricted final-response clearance to the same raiding party and victim property, preventing unrelated nearby raids from inheriting it.
- Continued tracking known aggressors who authorize on a captured Tool Cupboard, preventing TC capture from prematurely silencing an active response.
- Made a configured rocket count of zero disable rocket strafes as expected.
- Hardened configuration validation for delays, colors, marker channels, napalm bonuses, spawn-mode casing, and response-level ordering.
- Guaranteed response-profile arrays replace rather than append during configuration loading.
- Removed unused replacement-state, helicopter-destruction-time, and raid-damage-value code.

## 0.3.1 - 2026-10-05

- Added a configurable right-side `HELI HOSTILE` CUI countdown positioned below NoEscape's raid-block indicator by default.
- Added a brief green `HOSTILITY CLEARED` state while retaining the private “disarm, strip down, and get out” message.
- Added explicit weapon-category checks for belt inventory, so a surrendered or uninvolved player carrying guns is again considered armed even when Rust's transient threat score remains low.
- Kept belt-armed targeting separate from raid classification: carrying a gun can draw normal helicopter fire but does not expose the player's structures to rockets.
- Added UI cleanup on death, disconnect, plugin unload, and incident cleanup.

## 0.3.0 - 2026-10-05

- Added a private three-minute hostility countdown with three-, two-, and one-minute warnings plus a surrender message; renewed hostile activity resets the timer.
- Separated confirmed raid aggressors from ordinary armed players and temporary combatants. Armed bystanders receive vanilla-style targeting and can disarm to disengage without becoming raid-base owners.
- Added adaptive shelter pressure: sustained structural raiding increases rocket count, shortens pass cooldowns, and raises napalm probability; structure attacks stop shortly after raid damage stops.
- Replaced timer-based helicopter escalation with damage-triggered escalation. Destroying a helicopter arms the next level, which deploys immediately when structural raiding resumes.
- Waiting no longer counts as defeating a helicopter. Inactive responses pause and later return at the same level with saved helicopter and rotor health.
- Removed the default active-helicopter lifetime limit so continued raiding cannot simply outlast a response.
- Added configurable six-hour response-chain memory and persisted response level, completion state, protected/hostile buildings, and remaining helicopter health across reloads and restarts.
- Defeating all four response levels grants clearance for the continuing raid session; renewed damage extends that clearance until the configured response memory expires.

## 0.2.6 - 2026-10-05

- Kept admin-test aggressors active while they continue damaging the protected test building, even when both test structures share the same owner.
- Tightened rocket-pass cooldowns to 30/25/20/15 seconds across response levels so an actively used raid base remains under meaningful pressure.
- Preserved victim-building protection while refreshing test raid activity; the protected base is never reclassified as hostile.

## 0.2.5 - 2026-10-05

- Increased effective gun accuracy from 45/60/75/85% to 65/75/85/92% across response levels 1–4.
- Increased bullet speed from 250/300/350/400 to 300/350/400/450 to improve physical hit consistency without making every shot connect.
- Added a configuration migration so existing beta configs receive the revised accuracy profile automatically.
- Fixed Rust's patrol-helicopter initialization overriding the configured 300-metre land spawn and relocating the event helicopter to the vanilla map-edge entry point.

## 0.2.4 - 2026-10-05

- Added a configurable helicopter spawn mode: `NearbyLand` or `OverRaidArea`.
- Nearby-land mode prefers a dry position at the configured 300-metre stand-off and progressively searches closer on small islands or coastal raid locations.
- Removed reliance on custom-map coastline and ocean-patrol data that could place the helicopter far offshore or beyond the visible map.

## 0.2.3 - 2026-10-05

- Restricted helicopter player targeting to recorded raid-zone aggressors by default.
- Aggressor status is recorded from victim-structure damage, PvP attacks within the active raid zone, and attacks against the event helicopter.
- Preserved rocket and napalm attacks against confirmed aggressor raid bases while protecting victim and ambiguous property.

## 0.2.2 - 2026-10-05

- Reduced the default local response spawn distance from 500 to 350 metres.
- Added nearby ocean-shoreline detection so helicopters can enter naturally from the closest practical coast without creating a long cross-map response delay.
- Added configurable shoreline preference and offshore stand-off distance with safe local-spawn fallback behavior.
- Added scaled rocket and napalm capability to every response level, beginning with light pressure at level 1 and increasing through level 4.
- Restricted player-targeted rocket strafes to recorded raid aggressors.
- Added rocket attacks against confirmed aggressor shelters when a recent attacker hides from the helicopter; victim and ambiguous property remain protected.
- Strengthened gun target assignment and visibility refresh so eligible exposed targets are actively reacquired by the turrets.

## 0.2.1 - 2026-10-05

- Fixed helicopters firing briefly and then failing to consistently reacquire a visible raid aggressor.
- Added native target-acquisition bookkeeping and continuous line-of-sight timestamp refresh for eligible targets.
- Recently active aggressors remain valid targets even if Rust's transient native threat value drops during the fight.
- Increased the gun target-retention window while preserving obstruction, safe-zone, invisibility, range, and raid-zone checks.

## 0.2.0 - 2026-10-05

- Added victim-property protection for building pieces, SAM sites, turrets, traps, doors, and other owned deployables damaged by AntiRaidHeli helicopters.
- Added ownership-aware damage rules that allow damage only to confirmed aggressor property or classified raid bases.
- Added persistent, compact construction history grouped by building ID.
- Added a six-hour default history, one-hour fast-classification window, 100-metre candidate radius, minimum-piece threshold, and active-use confirmation settings.
- Added permanent incident-level association for aggressors and their native teams, with dynamic Clans and Friends recognition when available.
- Added an administrator-only `/antiraidhelitest [level]` command for single-player victim-versus-raid-base testing.
- Kept ambiguous and unrelated property protected by default.

## 0.1.0 - 2026-10-05

- Added independent player-base raid detection with cumulative-damage, hit-count, and heavy single-hit qualification thresholds.
- Added configurable owner, Tool Cupboard authorization, native team, Clans, and Friends exclusions for friendly structure damage.
- Added merging, inactivity expiration, and concurrent raid-zone limits.
- Added four configurable helicopter response levels from Suppression through Final Response.
- Added proportional body and rotor health scaling.
- Added configurable gun damage, speed, accuracy, burst behavior, and range.
- Added level-controlled rockets, napalm chance, rocket damage scaling, and attack cooldowns.
- Added a five-minute replacement window that requires continued qualifying raid damage.
- Preserved normal patrol-helicopter destruction, wreckage, and loot behavior.
- Added raid-zone targeting for armed and visible players on either side of a raid.
- Added stricter recent-aggressor targeting for the Final Response.
- Added solid-cover line-of-sight checks and SmartRecon invisibility compatibility.
- Added escalation-colored radius and label markers.
- Added global initial, escalation, final-defeat, and all-clear announcements.
- Added administrator start and complete cleanup commands.
- Allowed server-console test starts with no connected players by using the map center as the incident location.
- Isolated all behavior to AntiRaidHeli-owned helicopters so vanilla helicopters remain unchanged.
