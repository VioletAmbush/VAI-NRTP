import { ITemplateItem } from "@spt/models/eft/common/tables/ITemplateItem"
import { AbstractModManager } from "./AbstractModManager"
import { Helper } from "./Helper"
import { Constants } from "./Constants"


export class MagazinesManager extends AbstractModManager
{
    protected configName: string = "MagazinesConfig"

    protected afterPostDB(): void
    {
        for(let itemKey in this.databaseTables.templates.items)
        {
            const item = this.databaseTables.templates.items[itemKey]

            if (item._props.Cartridges &&
                item._props.Cartridges.length > 0 &&
                item._props.ReloadMagType !== "InternalMagazine")
            {
                this.setMagLoadModifier(item)
            }
        }

        Helper.iterateConfigItems(this.databaseTables, this.config, this.setMag)
        
        console.log(`${Constants.ModTitle}: Magazines changes applied!`)
    }

    private setMagLoadModifier(mag: ITemplateItem)
    {
        const maxCount = mag._props.Cartridges[0]._max_count

        if (maxCount < 50)
        {
            mag._props.LoadUnloadModifier += this.config.magLoadPercent

            if (mag._props.LoadUnloadModifier < 0)
            {
                mag._props.LoadUnloadModifier *= this.config.magLoadMultiplier;
            }
        }
        else
        {
            mag._props.LoadUnloadModifier += this.config.largeMagLoadPercent

            if (mag._props.LoadUnloadModifier < 0)
            {
                mag._props.LoadUnloadModifier *= this.config.largeLoadMultiplier;
            }
        }

        if (mag._props.LoadUnloadModifier < -70)
        {
            mag._props.LoadUnloadModifier = -70;
        }
    }

    private setMag(item: ITemplateItem, config: any): void
    {
        item._props.LoadUnloadModifier = config.magLoadPercent
    }
}