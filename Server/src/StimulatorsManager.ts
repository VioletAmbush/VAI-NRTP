import { ITemplateItem } from "@spt/models/eft/common/tables/ITemplateItem"
import { AbstractModManager } from "./AbstractModManager"
import { Helper } from "./Helper"
import { Constants } from "./Constants"
import { IDatabaseTables } from "@spt/models/spt/server/IDatabaseTables"


export class StimulatorsManager extends AbstractModManager
{
    protected configName: string = "StimulatorsConfig"

    protected afterPostDB(): void
    {
        Helper.iterateConfigItems(this.databaseTables, this.config, this.setStimulator)

        console.log(`${Constants.ModTitle}: Stimulators changes applied!`)
    }

    private setStimulator(item: ITemplateItem, config: any, databaseTables: IDatabaseTables): void
    {
        if (config.buffs)
        {
            Helper.setItemBuffs(databaseTables, item, config.buffs)
        }

        if (!item._props.effects_damage)
        {
            return
        }

        for (let effKey in item._props.effects_damage)
        {
            const effect = item._props.effects_damage[effKey]

            if (config.painkillerDuration && effKey === "Pain")
            {
                effect.duration = config.painkillerDuration
            }

            if (config.contusionDuration && effKey === "Contusion")
            {
                effect.duration = config.contusionDuration
            }

            if (config.intoxicationDuration && effKey === "Intoxication")
            {
                effect.duration = config.intoxicationDuration
            }

            if (config.radExposureDuration && effKey === "RadExposure")
            {
                effect.duration = config.radExposureDuration
            }
        }

        if (config.removeGoldenStarContusion)
        {
            const effects: any = {}

            for (let effect in item._props.effects_damage)
            {
                if (effect !== "Contusion")
                {
                    effects[effect] = item._props.effects_damage[effect]
                }
            }

            item._props.effects_damage = effects
        }
    }
}