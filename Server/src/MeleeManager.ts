import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem"
import { AbstractModManager } from "./AbstractModManager"
import { Constants } from "./Constants"
import { Helper } from "./Helper"


export class MeleeManager extends AbstractModManager
{
    protected configName: string = "MeleeConfig"

    protected afterPostDB(): void
    {
        Helper.iterateConfigItems(this.databaseTables, this.config, this.setMeleeItem)
        
        console.log(`${Constants.ModTitle}: Melee changes applied!`)
    }
    
    private setMeleeItem(item: ITemplateItem, config: any)
    {
        item._props.knifeHitSlashDam = config.slashDamage
        item._props.knifeHitStabDam = config.stabDamage
    }
}