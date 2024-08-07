import { ITemplateItem } from "@spt/models/eft/common/tables/ITemplateItem"
import { IDatabaseTables } from "@spt/models/spt/server/IDatabaseTables"
import { ConfigMapper } from "./ConfigMapper"
import { Constants } from "./Constants"


export class Helper 
{
    public static setItemBuffs(databaseTables: IDatabaseTables, item: ITemplateItem, buffsConfig: any): void
    {
        const buffName = `Buff${item._id}`

        databaseTables.globals.config.Health.Effects.Stimulator.Buffs[buffName] = ConfigMapper.mapBuffs(buffsConfig)

        item._props.StimulatorBuffs = buffName
    }

    public static trySetItemRarity(item: ITemplateItem, itemConfig: any): void
    {
        if (!itemConfig.rarity)
        {
            return
        }

        item._props.BackgroundColor = ConfigMapper.mapRarity(itemConfig.rarity)
    }

    public static iterateConfigItems(
        databaseTables: IDatabaseTables,
        config: any,
        action: (item: ITemplateItem, itemConfig: any, databaseTables: IDatabaseTables, config: any) => void = null,
        trySetRarity: boolean = true)
    {
        for (let itemKey in config.items)
        {
            const itemConfig = config.items[itemKey]
            const item = databaseTables.templates.items[itemConfig.id]

            if (!item)
            {
                console.log(`${Constants.ModTitle}: Item with id ${itemConfig.id} not found!`)
                continue
            }

            if (trySetRarity)
            {
                Helper.trySetItemRarity(item, itemConfig)
            }

            if (action != null)
            {
                action(item, itemConfig, databaseTables, config)
            }
        }
    }
} 