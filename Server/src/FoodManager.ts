import { ITemplateItem, IHealthEffect } from "@spt-aki/models/eft/common/tables/ITemplateItem"

import { AbstractModManager } from "./AbstractModManager"
import { Helper } from "./Helper"
import { ConfigMapper } from "./ConfigMapper"
import { Constants } from "./Constants"
import { IDatabaseTables } from "@spt-aki/models/spt/server/IDatabaseTables"
import { IProfileSides } from "@spt-aki/models/eft/common/tables/IProfileTemplate"


export class FoodManager extends AbstractModManager
{
    protected configName: string = "FoodConfig"

    protected afterPostDB(): void
    {
        Helper.iterateConfigItems(this.databaseTables, this.config, this.setFood)

        console.log(`${Constants.ModTitle}: Food changes applied!`)
    }

    private setFood(item: ITemplateItem, config: any, databaseTables: IDatabaseTables): void
    {
        if (config.buffs && config.buffs.length > 0)
        {
            Helper.setItemBuffs(databaseTables, item, config.buffs)
        }
        
        if (!config.hydrationCost || !config.energyCost)
        {
            return
        }

        const healthEffects: Record<string, Record<string, number>> = {}

        if (config.hydrationCost)
        {
            healthEffects["Hydration"] = { value: config.hydrationCost }
        }

        if (config.energyCost)
        {
            healthEffects["Energy"] = { value: config.energyCost }
        }

        item._props.effects_health = healthEffects
    }
}