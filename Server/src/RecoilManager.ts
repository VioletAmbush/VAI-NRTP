import { ITemplateItem } from "@spt-aki/models/eft/common/tables/ITemplateItem"
import { AbstractModManager } from "./AbstractModManager"
import { Constants } from "./Constants"


export class RecoilManager extends AbstractModManager
{
    protected configName: string = "RecoilConfig"

    protected afterPostDB(): void
    {
        for (let itemKey in this.databaseTables.templates.items)
        {
            const item = this.databaseTables.templates.items[itemKey]

            if (item._props.RecoilForceBack && 
                item._props.RecoilForceUp &&
                item._props.CameraRecoil)
            {
                if (item._props.weapFireType.includes("fullauto"))
                {
                    this.setGunRecoil(item, this.config.recoilAutoMultiplier)
                }
                else if (item._props.weapFireType.includes("single") && item._props.BoltAction == false)
                {
                    this.setGunRecoil(item, this.config.recoilSingleMultiplier)
                }
            }
        }
        
        console.log(`${Constants.ModTitle}: Recoil changes applied!`)
    }

    private setGunRecoil(item: ITemplateItem, recoilMultiplier: number)
    {
        item._props.RecoilForceBack *= recoilMultiplier
        item._props.RecoilForceUp *= recoilMultiplier
        item._props.CameraRecoil *= recoilMultiplier
    }
}