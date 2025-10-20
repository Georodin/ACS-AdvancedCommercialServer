# Advanced Commercial Servers (ACS) Mod

Generate resources with data-center flair. Build server racks, install servers, feed them power, manage the heat, and watch the goods roll in.

![Preview Image](/About/preview.jpg)

---

## What’s New — October 2025

- **RimWorld 1.6 support**, with an **updated 1.5 build** maintained.
- **Core code refactor** of the generation system for cleaner architecture and fewer edge-case bugs.
- **Custom resource list is now persistent** across saves/restarts (no more disappearing choices).
- **Resource settings fixed** — values apply and serialize reliably.

> ⚠️ I don’t have much time to maintain ACS right now. If you like the mod, please consider jumping in on GitHub. **PRs beat issues**—small, focused commits are super helpful.  
> • Repo: https://github.com/Georodin/ACS-AdvancedCommercialServer  
> • Pick an [enhancement](https://github.com/Georodin/ACS-AdvancedCommercialServer/labels/enhancement) or bug and open a PR 🙏

---

## Generatable Resources

Using the servers, the following resources can be generated (and more via Mod Settings):

![Silver](/Source/TexturesPreview/Silver.png)
![Gold](/Source/TexturesPreview/Gold.png)
![Jade](/Source/TexturesPreview/Jade.png)
![Plasteel](/Source/TexturesPreview/Plasteel.png)
![Steel](/Source/TexturesPreview/Steel.png)
![Industrial Components](/Source/TexturesPreview/ComponentIndustrial.png)
![Spacer Components](/Source/TexturesPreview/ComponentSpacer.png)
![Neutroamine](/Source/TexturesPreview/Neutroamine.png)
![Uranium](/Source/TexturesPreview/Uranium.png)

- Silver  
- Gold  
- Jade  
- Plasteel  
- Steel  
- Industrial Components  
- Spacer Components  
- Neutroamine  
- Uranium

---

## How It Works

Server Racks hold up to **12 servers**. Each installed server adds to the rack’s:

- **Power draw** (Watts)
- **Calculating power** (THz) — increases generation speed
- **Heat output** — scales with power draw

Generation speed **scales with the summed THz** in the rack. Output time is balanced against **market value**, so expensive items naturally take longer.

> Heads-up: Some items (quest/unique content, etc.) don’t behave well when spawned. If something seems off, remove it from your custom list.

---

## Servers

There are three server tiers. Mix and match—everything stacks inside the rack.

### Basic Server
- **Description:** Entry-level compute module for typical workloads  
- **Cost:** 50 Steel, 3 Industrial Components  
- **Power Consumption:** 200 W  
- **Calculating Power:** 1 THz  
- **Research Prerequisites:** `ServerBasic`

### Advanced Server
- **Description:** More throughput, more draw  
- **Cost:** 70 Steel, 5 Industrial Components, 2 Spacer Components  
- **Power Consumption:** 500 W  
- **Calculating Power:** 3 THz  
- **Research Prerequisites:** `ServerAdvanced`

### Glitterworld Server
- **Description:** Top-shelf compute from the glittering elite  
- **Cost:** 50 Plasteel, 5 Industrial Components, 5 Spacer Components  
- **Power Consumption:** 1200 W  
- **Calculating Power:** 8 THz  
- **Research Prerequisites:** `ServerGlitterworld`

---

## Heat & Power

Heat production is **directly tied to power consumption**. High-tier stacks run hot—plan cooling or expect toastier colonists.

---

## Boosting Research Speed

All operating Server Racks contribute their **combined** research speed. Place a rack **adjacent** to a **High-Tech Research Bench** to tap into the networked bonus.

---

## Configuration (Mod Settings)

- **Custom Resource List:** Add nearly any item to generate.  
  _Now persists correctly across sessions and saves._
- **Balance Controls:** Adjust generation multipliers and behavior.

> If an item behaves strangely when spawned, it probably isn’t intended to be generated. Remove it from your list.

---

## Compatibility

- **RimWorld:** 1.6 (current) and 1.5 (maintained build)
- As always, back up your saves before reshuffling mods. When updating from older ACS versions, let the game load fully so settings migrate.

---

## Changelog

### October 2025
- Added RimWorld 1.6 support; maintained 1.5 build
- Refactored core generation system (cleaner, faster, fewer edge cases)
- Fixed: custom resource list persistence
- Fixed: resource settings application/serialization

### April 2024
- Updated to version 1.5
- Customizable resource list available in Mod Settings
- Fixed heat pusher issue (heat now scales with rack power usage)
- Adjusted generation relative to market value

---

## Acknowledgments

Inspired by **Spess Carp’s Commercial Server Mod**. Enormous gratitude to the RimWorld Discord **#mod-development** crew!

Special appreciation to **Jamaican Castle**, **Erdelf**, and **Aelanna** for their help and guidance:
- [Jamaican Castle's Mods](https://steamcommunity.com/profiles/76561197998915712/myworkshopfiles/)
- [Erdelf's Mods](https://steamcommunity.com/id/erdelf/myworkshopfiles/)
- [Aelanna's Mods](https://steamcommunity.com/id/aelanna/myworkshopfiles/)

---

## Contributing

I’m short on time. If you want ACS to thrive, the best help is **direct contributions**:

1. Pick an [enhancement](https://github.com/Georodin/ACS-AdvancedCommercialServer/labels/enhancement) or bug  
2. Submit a PR with a brief rationale and testing notes  
3. Keep code tidy—small, focused commits help a lot

> Repo: https://github.com/Georodin/ACS-AdvancedCommercialServer

---

## Feedback and Bug Reports

Issues, ideas, or balance feedback are welcome as **Issues** or **Enhancements** on GitHub.

- GitHub: https://github.com/Georodin/ACS-AdvancedCommercialServer  
- Enhancements label: https://github.com/Georodin/ACS-AdvancedCommercialServer/labels/enhancement  
- Steam feedback thread: https://steamcommunity.com/sharedfiles/filedetails/?id=3040482484

Enjoy building your colony’s first high-heat money printer. Stay cool. 😅
