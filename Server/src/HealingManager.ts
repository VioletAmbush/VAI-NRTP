import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem"
import { AbstractModManager } from "./AbstractModManager"
import { Helper } from "./Helper"
import { Constants } from "./Constants"
import { IDatabaseTables } from "@spt-aki/models/spt/server/IDatabaseTables"


export class HealingManager extends AbstractModManager
{
    protected configName: string = "HealingConfig"

    protected afterPostDB(): void
    {
        Helper.iterateConfigItems(this.databaseTables, this.config, this.setHealingItem)

        console.log(`${Constants.ModTitle}: Healing items changes applied!`)
    }

    private setHealingItem(item: ITemplateItem, itemConfig: any, databaseTables: IDatabaseTables)
    {
        item._props.MaxHpResource = itemConfig.MaxHpResource
        item._props.hpResourceRate = itemConfig.HPResourceRate

        if (itemConfig.LightBleedingCost)
        {
            item._props.effects_damage.LightBleeding.cost = itemConfig.LightBleedingCost
        }

        if (itemConfig.HeavyBleedingCost)
        {
            item._props.effects_damage.HeavyBleeding.cost = itemConfig.HeavyBleedingCost
        }

        if (itemConfig.FractureCost)
        {
            item._props.effects_damage.Fracture.cost = itemConfig.FractureCost
        } 

        if (itemConfig.Regen)
        {
            const buffName = `Buff${item._id}`

            databaseTables.globals.config.Health.Effects.Stimulator.Buffs[buffName] = 
            [
                {
                    BuffType: "HealthRate",
                    Chance: 1,
                    Delay: 1,
                    Duration: itemConfig.Regen.Time,
                    Value: itemConfig.Regen.Rate,
                    AbsoluteValue: true,
                    SkillName: ""
                },
            ]

            item._props.StimulatorBuffs = buffName
            item._props.effects_damage = []
            item._parent = "5448f3a14bdc2d27728b4569"
        }

        if (itemConfig.useTime)
        {
            item._props.medUseTime = itemConfig.useTime
        }
    }
}