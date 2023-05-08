import { Probability } from "@spt-aki/models/eft/common/IGlobals"
import { AbstractModManager } from "./AbstractModManager"
import { Constants } from "./Constants"


export class EffectsManager extends AbstractModManager
{
    protected configName: string = "EffectsConfig"

    protected afterPostDB(): void
    {
        const effects = this.databaseTables.globals.config.Health.Effects

        if (this.config.probChangesEnabled)
        {
            this.setProbability(effects.LightBleeding.Probability, this.config.lightBleed.prob)
            this.setProbability(effects.HeavyBleeding.Probability, this.config.heavyBleed.prob)
            this.setProbability(effects.Fracture.FallingProbability, this.config.fracture.fallProb)
            this.setProbability(effects.Fracture.BulletHitProbability, this.config.fracture.bulletProb)
        }

        effects.LightBleeding.DamageHealth *= this.config.lightBleed.damageMult
        effects.LightBleeding.RemovePrice *= this.config.lightBleed.priceMult

        effects.HeavyBleeding.DamageHealth *= this.config.heavyBleed.damageMult
        effects.HeavyBleeding.RemovePrice *= this.config.heavyBleed.priceMult

        effects.Fracture.RemovePrice *= this.config.fracture.priceMult

        effects.Intoxication.DamageHealth *= this.config.toxin.damageMult
        effects.Intoxication.RemovePrice *= this.config.toxin.priceMult

        console.log(`${Constants.ModTitle}: Effects changes applied!`)
    }

    private setProbability(prob: Probability, config: any)
    {
        prob.Threshold = config.T
        prob.K = config.K
        prob.B = config.B
    }
}