import { AbstractModManager } from "./AbstractModManager"
import { Constants } from "./Constants"
import { Helper } from "./Helper"


export class ArmorManager extends AbstractModManager
{
    protected configName: string = "ArmorConfig"

    protected afterPostDB(): void
    {
        Helper.iterateConfigItems(this.databaseTables, this.config)
        
        console.log(`${Constants.ModTitle}: Armor changes applied!`)
    }
}