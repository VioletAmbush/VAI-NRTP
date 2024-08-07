import { AbstractModManager } from "./AbstractModManager";

import { Health as TypeHealth } from "@spt/models/eft/common/tables/IBotType"; 
import { Health as BaseHealth } from "@spt/models/eft/common/tables/IBotBase";
import { IProfileSides } from "@spt/models/eft/common/tables/IProfileTemplate";
import { Constants } from "./Constants"

export class HealthManager extends AbstractModManager
{
    protected configName: string = "HealthConfig"

    protected afterPostDB(): void
    {
        this.setBotsHealth()
        this.setPlayerHealth()

        console.log(`${Constants.ModTitle}: Health changes applied!`)
    }

    private setPlayerHealth(): void
    {
        for (let profileKey in this.databaseTables.templates.profiles)
        {
            const profile: IProfileSides = this.databaseTables.templates.profiles[profileKey]

            this.setBaseHealth(profile.bear.character.Health, this.config.player)
            this.setBaseHealth(profile.usec.character.Health, this.config.player)
        }
    }

    private setBotsHealth(): void
    {
        for (let botEntry in this.config.bots)
        {
            const bot = this.databaseTables.bots.types[botEntry]

            if (bot)
            {
                this.setTypeHealthConfig(bot.health, this.config.bots[botEntry])
            }
        }

        for (let bot in this.databaseTables.bots.types)
        {
            if (!this.config.bots[bot])
            {
                if (bot.includes("boss"))
                {
                    this.setTypeHealthMult(this.databaseTables.bots.types[bot].health, this.config.bossMultiplier)
                }
                else if (bot.includes("follower"))
                {
                    this.setTypeHealthMult(this.databaseTables.bots.types[bot].health, this.config.followerMultiplier)
                }
                else
                {
                    this.setTypeHealthMult(this.databaseTables.bots.types[bot].health, this.config.commonMultiplier)
                }
            }
        }
    }

    private setTypeHealthConfig(health: TypeHealth, configHealth: any): void
    {
        health.BodyParts.forEach(bp => { 
            bp.Head.max = configHealth["head"]
            bp.Head.min = bp.Head.max

            bp.Chest.max = configHealth["chest"]
            bp.Chest.min = bp.Chest.max

            bp.Stomach.max = configHealth["stomach"]
            bp.Stomach.min = bp.Stomach.max

            bp.LeftArm.max = configHealth["arm"]
            bp.LeftArm.min = bp.LeftArm.max
            bp.RightArm.max = bp.LeftArm.max
            bp.RightArm.min = bp.LeftArm.max

            bp.LeftLeg.max = configHealth["leg"]
            bp.LeftLeg.min = bp.LeftLeg.max
            bp.RightLeg.max = bp.LeftLeg.max
            bp.RightLeg.min = bp.LeftLeg.max })
    }

    private setTypeHealthMult(health: TypeHealth, multiplier: number): void
    {
        health.BodyParts.forEach(bp => { 
            bp.Head.max *= multiplier
            bp.Head.min *= multiplier

            bp.Chest.max *= multiplier
            bp.Chest.min *= multiplier

            bp.Stomach.max *= multiplier
            bp.Stomach.min *= multiplier

            bp.LeftArm.max *= multiplier
            bp.LeftArm.min *= multiplier
            bp.RightArm.max *= multiplier
            bp.RightArm.min *= multiplier

            bp.LeftLeg.max *= multiplier
            bp.LeftLeg.min *= multiplier
            bp.RightLeg.max *= multiplier
            bp.RightLeg.min *= multiplier })
    }

    private setBaseHealth(health: BaseHealth, configHealth: any)
    {
        health.BodyParts.Head.Health.Maximum = configHealth["head"]
        health.BodyParts.Head.Health.Current = configHealth["head"]
        
        health.BodyParts.Chest.Health.Maximum = configHealth["chest"]
        health.BodyParts.Chest.Health.Current = configHealth["chest"]
        
        health.BodyParts.Stomach.Health.Maximum = configHealth["stomach"]
        health.BodyParts.Stomach.Health.Current = configHealth["stomach"]
        
        health.BodyParts.LeftArm.Health.Maximum = configHealth["arm"]
        health.BodyParts.LeftArm.Health.Current = configHealth["arm"]
        health.BodyParts.RightArm.Health.Maximum = configHealth["arm"]
        health.BodyParts.RightArm.Health.Current = configHealth["arm"]
        
        health.BodyParts.LeftLeg.Health.Maximum = configHealth["leg"]
        health.BodyParts.LeftLeg.Health.Current = configHealth["leg"]
        health.BodyParts.RightLeg.Health.Maximum = configHealth["leg"]
        health.BodyParts.RightLeg.Health.Current = configHealth["leg"]
    }
}