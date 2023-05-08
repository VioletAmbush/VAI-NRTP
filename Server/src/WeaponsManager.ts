import { AbstractModManager } from "./AbstractModManager"
import { Constants } from "./Constants"
import { Helper } from "./Helper"


export class WeaponsManager extends AbstractModManager
{
    protected configName: string = "WeaponsConfig"

    protected afterPostDB(): void
    {
        Helper.iterateConfigItems(this.databaseTables, this.config)
        
        console.log(`${Constants.ModTitle}: Weapons changes applied!`)
    }
}