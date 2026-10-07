# Changelog

## 0.6.1 - 2026-10-06

- Made AntiRaidHeli independent of the server-wide `patrolhelicopterai.flee_damage_percentage` convar.
- Added a per-entity retirement guard that prevents only active AntiRaidHeli aircraft from fleeing at low health, without changing the server convar or affecting vanilla and other plugin helicopters.
- Preserved intentional plugin retirement for raid inactivity and cleanup by explicitly allowing aircraft already registered for controlled retirement.
- Prevented a naturally fleeing event helicopter from eventually disappearing and being misclassified as destroyed, which could incorrectly advance the response chain.

## 0.6.0 - 2026-10-06

- Added persistent controlled-test activation: `/antiraidhelistart` enables automatic raid protection until an administrator disables it, including across reloads and restarts.
- Added `/antiraidstop` to disable automatic protection and clean all active helicopters, pending rounds, map markers, hostility indicators, and saved raid progress. `/antiraidhelistop` remains as a compatibility alias.
- Repurposed `antiraidheli.start` and `antiraidheli.stop` console commands for the same persistent enable/disable behavior, and added `antiraidheli.test [level]` for manual console test responses.
- Changed fresh installations and one-time upgrades to begin disabled so the beta cannot start responding to live raids before an administrator explicitly enables it.
- Promoted the TEST server's controlled-testing health ladder to the v0.6.0 defaults: 3,000 / 6,000 / 9,000 / 12,000 fuselage health, with matching 9% main-rotor and 5% tail-rotor health.
- Disabled startup restoration of stale raid progress while the system is inactive and clear that saved progress, preventing an old response chain from returning when controlled testing is later enabled.
- Added configuration migration version 12 for persistent manual activation and the controlled beta health defaults.

## 0.5.1 - 2026-10-05

- Added cave-base safety: underground victim structures are excluded from automatic AntiRaidHeli responses by default because helicopters cannot reliably reach, see, or pressure raiders beneath terrain.
- Cave classification compares both the damaged structure and its connected Tool Cupboard against Rust's terrain surface, covering cave entrances whose outer pieces may be shallower than the protected base core.
- Added a configurable underground depth threshold, defaulting to eight metres, plus an enable/disable switch for servers that deliberately want cave raids protected.
- Kept administrator-created test incidents available underground so controlled diagnostics are still possible.
- Added configuration migration version 11 for the cave-base exclusion settings.

## 0.5.0 - 2026-10-05

- Rebalanced the production health ladder to 50,000 / 80,000 / 125,000 / 250,000 health across rounds 1–4; both final-round helicopters receive the full 250,000 health.
- Kept fuselage and rotor health configurable per response profile, with migration that replaces only the prior shipped defaults and preserves deliberate administrator customizations.
- Changed successful helicopter destruction into continued aggression: the next response round now deploys after a configurable five-second delay instead of waiting for more structural damage.
- Added helicopter-combat pressure. Sustained attacks on the response helicopter can trigger heavy rocket and napalm pressure at configurable remaining-health thresholds of 50% / 65% / 75% / 85% for rounds 1–4.
- Added shared threat scoring across raiders, defenders, and third parties who attack the helicopter, with split target assignment for multi-helicopter responses.
- Kept one four-round response chain per defended victim base, preventing separate teams from multiplying helicopter incidents in the same hotspot.
- Added active-response repair, upgrade, and attached-expansion blocking for identified aggressor raid bases, including BetterTC Repair All through its `OnStructureRepair` integration.
- Added optional NoEscape raid-block refresh when a confirmed raider attacks a response helicopter, closing repair gaps during extended fights and across respawns.
- Preserved heli hostility through player death and restored the HUD after respawn until the normal three-minute non-aggression timer expires.
- Armed non-raiders who are actually acquired as vanilla-style helicopter threats now receive the same visible hostility timer.
- Added a small `ROUND` caption to the HUD. The displayed round and map-marker level remain on the completed round during the inter-round delay, then advance when the next response actually deploys.
- Persisted pending escalation timing across plugin reloads and server restarts.
- Added configuration migration version 10 for response delay, combat-pressure windows, health thresholds, and the revised default health ladder.

## 0.4.3 - 2026-10-05

- Added persistent entity tags to newly created AntiRaidHeli label and radius map markers.
- Added reload-safe orphan cleanup during plugin initialization and after `/antiraidhelistop` or `antiraidheli.stop` removes the final incident.
- Added backward-compatible discovery of untagged legacy radius markers by pairing them with an AntiRaidHeli label at the same map position.
- Stale event markers from older reloads can now be removed without manually locating or killing their entities.

## 0.4.2 - 2026-10-05

- Added a compact current/total response indicator to the hostility HUD, such as `1/4` through `4/4`, immediately left of the countdown timer.
- The indicator follows the newest active hostility record when a player is involved in overlapping raid incidents.
- Rebalanced the HUD label widths to keep the helicopter icon, status, round indicator, and countdown readable without enlarging the panel.

## 0.4.1 - 2026-10-05

- Added explicit multi-helicopter flight deconfliction with separate concentric orbit lanes for each helicopter.
- Added configurable orbit spacing, defaulting to 45 metres per additional helicopter.
- Added configurable attack-timing staggering, defaulting to four seconds per additional helicopter, so rocket and strafe passes do not begin in lockstep.
- Retained 70-metre approach-formation spacing and independent randomized crash destinations.
- Added configuration migration version 9 for the new flight-separation controls.

## 0.4.0 - 2026-10-05

- Added configurable multi-helicopter response groups with a hard safety limit of three helicopters per response level.
- Enabled a two-helicopter Final Response by default. Both helicopters retain the complete level-four weapons profile and independently drop three crates, for six crates if the raiders defeat the full final group.
- Changed escalation and final victory handling so a response group is not defeated until every helicopter in that group is destroyed.
- Added independent formation spacing, movement, targeting, rocket pressure, cleanup, randomized crash destinations, and inactivity retirement for every helicopter in a group.
- Persisted each group member's fuselage and rotor health across inactivity, plugin reloads, and server restarts.
- Added true per-response physical gun-spread scaling without modifying vanilla or other plugin helicopters: 1.00/0.75/0.50/0.30 aim-cone scales for levels 1–4.
- Added configuration migration version 8 so existing installations receive the two-helicopter final response and aim defaults without rebuilding their configuration.

## 0.3.9 - 2026-10-05

- Replaced Rust's monument-biased death-flight choice for AntiRaidHeli helicopters with a per-helicopter randomized dry-land crash destination.
- Added configurable minimum and maximum crash distances from the defended raid, defaulting to a 300–600 metre ring.
- Added randomized area-uniform selection plus a coastal fallback sweep, so nearby valid land is still found without repeatedly favoring the same direction.
- Scoped the death-flight patch exclusively to AntiRaidHeli-managed helicopters; vanilla and other plugin helicopters retain their normal crash behavior.
- Added configuration migration version 7 for the new crash-destination controls without requiring config deletion.

## 0.3.8 - 2026-10-05

- Fixed legacy persisted `EscalationAlert` language templates throwing a `FormatException` during helicopter death and interrupting response-transition cleanup.
- Added fresh message keys for the revised escalation-standby and final-victory announcements so existing installations receive the current wording without deleting their language file.
- Hardened all chat-message formatting so an incompatible customized or stale language template logs one warning instead of breaking event logic.

## 0.3.7 - 2026-10-05

- Darkened the embedded helicopter silhouette on the hostility indicator for clearer contrast against the red panel and closer visual alignment with NoEscape's Raid Block icon.
- Bound earned final-response clearance to the exact connected Rust building ID whenever one is available.
- A separately constructed base now starts a fresh four-helicopter response chain even when it belongs to the same victim and is near a previously cleared raid; owner matching remains only as a fallback for standalone deployables without a building ID.

## 0.3.6 - 2026-10-05

- Fixed continued structural damage after defeating the fourth helicopter recreating `HELI HOSTILE` even though the raiding party had earned clearance.
- Completed response chains now refresh their earned raid clearance without recording new heli hostility or attempting to deploy a nonexistent fifth response level.

## 0.3.5 - 2026-10-05

- Changed helicopter loot progression to 1/1/2/3 crates across response levels 1–4, limiting a fully defeated chain to seven crates instead of sixteen.
- Removed raid-hostile records and the `HELI HOSTILE` indicator immediately when the fourth and final helicopter is defeated.
- Expanded the final victory announcement with: “You may raid in peace now—you earned it.”
- Fixed hidden aggressors remaining in the native target list preventing repeated shelter-pressure passes; active raiders inside confirmed raid bases can now be strafed even while the helicopter still remembers them as a target.
- Increased response levels 2–4 to 12/16/20 base rockets per pass, shortened their base pass cooldowns to 18/14/10 seconds, and increased their napalm chances to 45/65/85 percent.
- Increased level 2–4 rocket damage scales to 1.25/1.75/2.5 and tightened their intra-volley rocket timing, while retaining activity-based scaling for sustained and heavy raids.
- Added configuration migration version 6 so existing installations receive the revised escalation and loot defaults without rebuilding their configuration.

## 0.3.4 - 2026-10-05

- Changed the default initial helicopter response delay from 15 seconds to immediate deployment. The helicopter still stages 300 meters from the raid and must fly into the response zone, giving raiders only its approach time to react.
- Retained the response-delay setting so server owners can deliberately restore a delay if desired, with the config label explicitly documenting that `0` means instant and positive values are seconds.
- Added automatic repair for configs affected by legacy response-profile duplication, trimming excess entries back to the intended four escalation levels.
- Added a second legacy repair for configs whose surviving four entries were all copies of the level-one profile, restoring the intended behavior and armament for levels 2–4.

## 0.3.3 - 2026-10-05

- Fixed `/antiraidhelistop` and `antiraidheli.stop` leaving a stale `HELI HOSTILE` countdown on some players after stopping multiple or overlapping incidents.
- Batch cleanup now explicitly removes the AntiRaidHeli hostility panel from every connected player after all incidents have ended.

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
