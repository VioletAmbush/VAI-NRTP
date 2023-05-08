import { DependencyContainer } from "tsyringe"

import { ILogger } from "@spt-aki/models/spt/utils/ILogger"
import { JsonUtil } from "@spt-aki/utils/JsonUtil"

import { IPreAkiLoadMod } from "@spt-aki/models/external/IPreAkiLoadMod"
import { IPostDBLoadMod } from "@spt-aki/models/external/IPostDBLoadMod"
import { IPostAkiLoadMod } from "@spt-aki/models/external/IPostAkiLoadMod"
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer"
import { IDatabaseTables } from "@spt-aki/models/spt/server/IDatabaseTables"

export abstract class AbstractModManager implements IPreAkiLoadMod, IPostDBLoadMod, IPostAkiLoadMod
{
    protected abstract configName: string
    protected config: any

    protected container: DependencyContainer
    protected jsonUtil: JsonUtil
    protected logger: ILogger

    protected databaseTables: IDatabaseTables

    protected preAkiInitialized: boolean = false
    protected postDBInitialized: boolean = false
    protected postAkiInitialized: boolean = false

    public preAkiLoad(container: DependencyContainer): void
    {
        if (!this.preAkiInitialized)
        {
            this.preAkiInitialize(container)
        }

        if (this.config.enabled != true)
        {
            return
        }

        this.afterPreAki()

    }

    public postDBLoad(container: DependencyContainer): void
    {
        if (!this.preAkiInitialized)
        {
            this.preAkiInitialize(container)
        }

        if (this.config.enabled != true)
        {
            return
        }

        if (!this.postDBInitialized)
        {
            this.postDBInitialize(container)
        }

        this.afterPostDB()
    }

    public postAkiLoad(container: DependencyContainer): void 
    {
        if (!this.preAkiInitialized)
        {
            this.preAkiInitialize(container)
        }

        if (this.config.enabled != true)
        {
            return
        }

        if (!this.postDBInitialized)
        {
            this.postDBInitialize(container)
        }

        if (!this.postAkiInitialized)
        {
            this.postAkiInitialize(container)
        }

        this.afterPostAki()
    }

    protected preAkiInitialize(container: DependencyContainer): void
    {
        this.config = require(`../config/${this.configName}.json`)
        
        this.preAkiInitialized = true
    }

    protected postDBInitialize(container: DependencyContainer): void
    {
        this.container = container

        this.jsonUtil = this.container.resolve<JsonUtil>("JsonUtil")
        this.logger = this.container.resolve<ILogger>("WinstonLogger")

        const databaseServer = this.container.resolve<DatabaseServer>("DatabaseServer")
        this.databaseTables = databaseServer.getTables()

        this.postDBInitialized = true
    }

    protected postAkiInitialize(container: DependencyContainer): void
    {

    }

    protected afterPreAki(): void {}

    protected afterPostDB(): void {}

    protected afterPostAki(): void {}
}