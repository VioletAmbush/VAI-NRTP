import { DependencyContainer } from "tsyringe"
import { AbstractModManager } from "./AbstractModManager"
import { ILocationConfig } from "@spt/models/spt/config/ILocationConfig"
import { ConfigTypes } from "@spt/models/enums/ConfigTypes"
import { ConfigServer } from "@spt/servers/ConfigServer"
import { Constants } from "./Constants"
import { ILocation } from "@spt/models/eft/common/ILocation"
import { ILocationBase } from "@spt/models/eft/common/ILocationBase"


export class GlobalsManager extends AbstractModManager
{
    protected configName: string = "GlobalsConfig"


    private ignoredLocations: string[] = ["hideout", "develop", "privatearea", "suburbs", "town", "terminal"]

    private locationConfig: ILocationConfig

    protected postSptInitialize(container: DependencyContainer): void 
    {
        super.postSptInitialize(container)

        const configServer = container.resolve<ConfigServer>("ConfigServer")
        this.locationConfig = configServer.getConfig<ILocationConfig>(ConfigTypes.LOCATION)
    }

    protected afterPostDB(): void
    {
        if (this.config.removeSkillFatigue)
        {
            this.databaseTables.globals.config.SkillFatigueReset = 0
        }

        if (this.config.removeInRaidItemRestrictions)
        {
            this.databaseTables.globals.config.RestrictionsInRaid = []
        }

        this.setEscapeTimeLimits()
        this.trySetConstructionTime()
        this.trySetExitsAlwaysAvailable()
        this.trySetAllExitsAvailable()
        this.tryRemoveRunThroughs()
    }

    protected afterPostSpt(): void
    {
        this.setLootMultipliers()

        console.log(`${Constants.ModTitle}: Globals config changes applied!`)
    }

    private setLootMultipliers(): void
    {
        for (let loc in this.locationConfig.staticLootMultiplier)
        {
            if (this.locationConfig.staticLootMultiplier[loc] &&
                this.locationConfig.looseLootMultiplier[loc] &&
                !this.ignoredLocations.includes(loc))
            {
                this.locationConfig.staticLootMultiplier[loc] = this.config.staticLootMultiplier
                this.locationConfig.looseLootMultiplier[loc] = this.config.looseLootMultiplier
            }
        }
    }

    private setEscapeTimeLimits(): void
    {
        for (let loc in this.databaseTables.locations)
        {
            if (this.databaseTables?.locations[loc]?.base &&
                !this.ignoredLocations.includes(loc))
            {
                this.databaseTables.locations[loc].base.EscapeTimeLimit *= this.config.raidTimeMultiplier
                this.databaseTables.locations[loc].base.EscapeTimeLimitCoop *= this.config.raidTimeMultiplier
            }
        }
    }

    private trySetConstructionTime(): void
    {
        if (this.config.removeConstructionTime != true)
        {
            return
        }

        for (let areaKey in this.databaseTables.hideout.areas)
        {
            const area = this.databaseTables.hideout.areas[areaKey]

            for (let stageKey in area.stages)
            {
                const stage = area.stages[stageKey]

                if (stage.constructionTime)
                {
                    stage.constructionTime = 0
                }
            }
        }
    }

    private trySetExitsAlwaysAvailable(): void
    {
        if (this.config.exitsAlwaysAvailable != true)
        {
            return
        }

        for (let locKey in this.databaseTables.locations)
        {
            const loc: ILocation = this.databaseTables.locations[locKey]

            if (loc?.base && !this.ignoredLocations.includes(locKey))
            {
                for (let exitKey in loc.allExtracts)
                {
                    loc.allExtracts[exitKey].Chance = 100
                }
            }
        }
    }

    private trySetAllExitsAvailable(): void
    {
        if (this.config.allExitsAvailable != true)
        {
            return
        }
        
        const locations = this.databaseTables.locations

        this.setAllExitsAvailable(locations.bigmap.base, "Customs,Boiler Tanks")
        this.setAllExitsAvailable(locations.interchange.base, "MallSE,MallNW")
        this.setAllExitsAvailable(locations.lighthouse.base, "Tunnel,North")
        this.setAllExitsAvailable(locations.shoreline.base, "Village,Riverside")
        this.setAllExitsAvailable(locations.tarkovstreets.base, "E1_2,E2_3,E3_4,E4_5,E5_6,E6_1")
        this.setAllExitsAvailable(locations.woods.base, "House,Old Station")
    }

    private setAllExitsAvailable(location: ILocationBase, entryPoints: string): void
    {
        for (let exit of location.exits)
        {
            exit.EntryPoints = entryPoints
        }
    }

    private tryRemoveRunThroughs(): void
    {
        if (this.config.removeRunThroughs != true)
        {
            return
        }

        this.databaseTables.globals.config.exp.match_end.survived_exp_requirement = 0
        this.databaseTables.globals.config.exp.match_end.survived_seconds_requirement = 0
    }
}