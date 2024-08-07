import { DependencyContainer } from "tsyringe"

import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod"
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod"
import { IPostSptLoadMod } from "@spt/models/external/IPostSptLoadMod"

import { AbstractModManager } from "./AbstractModManager";

import { AmmoManager } from "./AmmoManager"
import { GlobalsManager } from "./GlobalsManager"
import { HealingManager } from "./HealingManager"
import { HealthManager } from "./HealthManager"
import { MagazinesManager } from "./MagazinesManager"
import { MeleeManager } from "./MeleeManager"
import { RecoilManager } from "./RecoilManager"
import { FoodManager } from "./FoodManager"
import { StimulatorsManager } from "./StimulatorsManager"
import { ArmorManager } from "./ArmorManager"
import { WeaponsManager } from "./WeaponsManager"
import { EffectsManager } from "./EffectsManager"

export class ModManager implements IPreSptLoadMod, IPostDBLoadMod, IPostSptLoadMod
{
    private managers: AbstractModManager[] = []

    constructor()
    {
        this.managers.push(new AmmoManager)
        this.managers.push(new GlobalsManager)
        this.managers.push(new HealingManager)
        this.managers.push(new HealthManager)
        this.managers.push(new MagazinesManager)
        this.managers.push(new MeleeManager)
        this.managers.push(new RecoilManager)
        this.managers.push(new FoodManager)
        this.managers.push(new StimulatorsManager)
        this.managers.push(new ArmorManager)
        this.managers.push(new WeaponsManager)
        this.managers.push(new EffectsManager)
    }

    public preSptLoad(container: DependencyContainer): void
    {
        this.managers.forEach(m => m.preSptLoad(container))
    }
  
    public postDBLoad(container: DependencyContainer): void 
    {
        this.managers.forEach(m => m.postDBLoad(container))
    }

    public postSptLoad(container: DependencyContainer): void 
    {
        this.managers.forEach(m => m.postSptLoad(container))
    }
}

module.exports = { mod: new ModManager() }