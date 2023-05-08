import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem";
import { AbstractModManager } from "./AbstractModManager";
import { Helper } from "./Helper"
import { Constants } from "./Constants"


export class AmmoManager extends AbstractModManager
{
    protected configName: string = "AmmoConfig"

    protected afterPostDB(): void 
    {
        for(let itemKey in this.databaseTables.templates.items)
        {
            const item = this.databaseTables.templates.items[itemKey]

            if (item._props.ammoType)
            {
                this.setAmmoStack(item)
            }
        }

        Helper.iterateConfigItems(this.databaseTables, this.config)
        
        console.log(`${Constants.ModTitle}: Ammo changes applied!`)
    }

    private setAmmoStack(ammo: ITemplateItem): void 
    {
        if (!ammo._props.StackMaxSize)
        {
            return
        }

        ammo._props.StackMaxSize *= this.config.ammoStackMultiplier
    }
}